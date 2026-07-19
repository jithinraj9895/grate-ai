from fastapi import FastAPI, HTTPException
from sentence_transformers import SentenceTransformer
from dotenv import load_dotenv
from pgvector.psycopg import register_vector
import psycopg
import os
import logging

# -----------------------------
# Load Environment Variables
# -----------------------------
load_dotenv()

# -----------------------------
# Logging
# -----------------------------
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s | %(levelname)s | %(message)s"
)

logger = logging.getLogger(__name__)

# -----------------------------
# FastAPI
# -----------------------------
app = FastAPI(title="Semantic Search API")

# -----------------------------
# Load Model
# -----------------------------
logger.info("Loading embedding model...")

model = SentenceTransformer(
    os.getenv("MODEL_NAME", "sentence-transformers/all-MiniLM-L6-v2")
)

logger.info("Embedding model loaded.")

# -----------------------------
# PostgreSQL Connection
# -----------------------------
logger.info("Connecting to PostgreSQL...")

pg_conn = psycopg.connect(
    host=os.getenv("DB_HOST"),
    port=os.getenv("DB_PORT"),
    dbname=os.getenv("DB_NAME"),
    user=os.getenv("DB_USER"),
    password=os.getenv("DB_PASSWORD")
)

register_vector(pg_conn)

logger.info("Connected to PostgreSQL.")

# -----------------------------
# Health Check
# -----------------------------
@app.get("/")
def home():
    return {
        "status": "Running"
    }

# -----------------------------
# Search
# -----------------------------
@app.get("/search")
def search(query:str):
    
    try:

        logger.info(f"Searching : {query}")

        # Generate embedding
        query_embedding = model.encode(query).tolist()

        with pg_conn.cursor() as cursor:

            cursor.execute(
                """
                SELECT
                        product_id,
                        text,
                        1 - (embedding <=> %s::vector) AS similarity
                    FROM product_embeddings
                    ORDER BY embedding <=> %s::vector
                    LIMIT 10
                """,
                (
                    query_embedding,
                    query_embedding
                )
            )

            rows = cursor.fetchall()

        logger.info(f"Found {len(rows)} results")

        results = []

        for row in rows:

            results.append(
                {
                    "productId": row[0],
                    "text": row[1],
                    "similarity": float(row[2])
                }
            )

        return {
            "results": results
        }

    except Exception as ex:

        logger.exception("Search failed")

        raise HTTPException(
            status_code=500,
            detail=str(ex)
        )