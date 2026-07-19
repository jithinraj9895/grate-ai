import json
import logging
import os

import pika
import psycopg
from dotenv import load_dotenv
from sentence_transformers import SentenceTransformer

load_dotenv()

# -----------------------------
# Logging
# -----------------------------
logging.basicConfig(
    level=logging.INFO,
    format="[%(asctime)s ] [ %(levelname)s ] [ %(message)s ]"
)

logger = logging.getLogger(__name__)

# -----------------------------
# Load Embedding Model
# -----------------------------
logger.info("Loading embedding model...")

model = SentenceTransformer(os.getenv("MODEL_NAME"))

logger.info("Embedding model loaded.")

# -----------------------------
# PostgreSQL Connection
# -----------------------------
pg_conn = psycopg.connect(
    host=os.getenv("DB_HOST"),
    port=os.getenv("DB_PORT"),
    dbname=os.getenv("DB_NAME"),
    user=os.getenv("DB_USER"),
    password=os.getenv("DB_PASSWORD")
)

logger.info("Connected to PostgreSQL.")

# -----------------------------
# RabbitMQ Connection
# -----------------------------
rabbit = pika.BlockingConnection(
    pika.ConnectionParameters(
        host=os.getenv("RABBITMQ_HOST"),
        port=int(os.getenv("RABBITMQ_PORT"))
    )
)

channel = rabbit.channel()

channel.queue_declare(
    queue="embedding-queue",
    durable=True
)

channel.basic_qos(prefetch_count=1)

logger.info("Connected to RabbitMQ.")

# -----------------------------
# Message Handler
# -----------------------------
def process_message(ch, method, properties, body):

    try:

        message = json.loads(body)
        print(message)
        product_id = message["ProductId"]
        name = message.get("Name", "")
        description = message.get("Description", "")

        text = f"{name}. {description}".strip()

        logger.info(f"Generating embedding for Product {product_id}")

        embedding = model.encode(text).tolist()

        with pg_conn.cursor() as cursor:

            cursor.execute(
                """
                INSERT INTO product_embeddings
                (
                    product_id,
                    text,
                    embedding
                )
                VALUES
                (
                    %s,
                    %s,
                    %s
                )
                ON CONFLICT (product_id)
                DO UPDATE
                SET
                text = EXCLUDED.text,
                embedding = EXCLUDED.embedding;
                """,
                (
                    product_id,
                    text,
                    embedding
                )
            )

        pg_conn.commit()

        logger.info(f"Embedding stored for Product {product_id}")

        ch.basic_ack(
            delivery_tag=method.delivery_tag
        )

    except Exception:

        logger.exception("Failed to process message")

        pg_conn.rollback()

        ch.basic_nack(
            delivery_tag=method.delivery_tag,
            requeue=True)

# -----------------------------
# Start Worker
# -----------------------------
logger.info("Worker started. Waiting for messages...")

channel.basic_consume(
    queue="embedding-queue",
    on_message_callback=process_message
)

channel.start_consuming()