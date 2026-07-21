# Ever wondered how a distributed system works? Me neither.

I was just practicing my .NET skills for an interview.

The original idea was pretty simple, build a small e-commerce application with a React frontend and a .NET backend. Nothing fancy—just something that would let me brush up on APIs, Entity Framework, and a few frontend concepts.

Then I reached the point where I needed a product search.

Seems easy, right?

Just search by product name.

## The rabbit hole begins

Like any developer, my first thought was optimization.

How do I make searching fast?

A little ChatGPT, a little Google, and I ended up implementing backend pagination. Then I thought, why stop there? Infinite scrolling would make the experience much smoother. Since products already have IDs, I could use cursor-based pagination instead of offset pagination.

Great.

Problem solved.

Or so I thought.

Then another question popped into my head.

> Why am I only searching product names?

Every product has a description. Sometimes the description is more important than the title itself.

Imagine a product called:

**"Organic Green Powder"**

The title doesn't really tell you much.

But the description says:

> Rich in vitamins, supports immunity, improves digestion and overall health.

Now if someone searches:

> "something good for health"

A normal SQL query isn't going to find that product unless those exact words exist somewhere.

That felt... limiting.

## Keyword search isn't understanding

Traditional search engines work mostly on matching words.

If I search:

> healthy food

but the description contains:

> nutritious meal

SQL doesn't magically know that *healthy* and *nutritious* are closely related.

They're different words.

Humans understand they're almost the same.

Databases don't.

So I started looking into ways to make search actually understand meaning.

That's when I came across **semantic search**.

## Wait... what's semantic search?

The idea is surprisingly simple.

Instead of comparing words...

...compare meaning.

To do that, we convert text into numbers.

For example,

```text
"Give me a product which can be good for health"
```

becomes something like

```text
[-0.17, 0.54, 0.02, ..., 0.81]
```

This array is called an **embedding** (or vector).

Another sentence like

```text
"I need something nutritious"
```

might become

```text
[-0.16, 0.51, 0.04, ..., 0.79]
```

The numbers are different, but they end up very close to each other in vector space because they mean similar things.

Now instead of searching for words...

...we search for the **nearest vector**.

That's the magic.

## But where do these vectors come from?

I didn't create them manually.

I used a small embedding model from Sentence Transformers.

Whenever a product is created or updated, the description is sent to a Python service.

The Python service generates an embedding and stores it inside PostgreSQL using the `pgvector` extension.

Now every product has:

* Product details in the relational database
* A vector representation inside PostgreSQL

Searching becomes:

1. Convert the user's query into a vector.
2. Compare it with all stored vectors.
3. Return the closest matches.

No keyword matching.

No LIKE queries.

Just similarity.

## This is where things got interesting

Initially my architecture looked like this:

```text
React
    │
.NET API
    │
SQL Server
```

Simple.

Then semantic search entered the picture.

Suddenly I needed a Python service because the embedding model is much easier to use from Python.

But I didn't want my API waiting for embeddings every time a product was created.

That sounded like a bad idea.

So I introduced RabbitMQ.

Now the flow became:

```text
React
    │
.NET API
    │
RabbitMQ
    │
Python Worker
    │
PostgreSQL (pgvector)
```

Whenever a product is added:

* The .NET API stores it in SQL Server.
* A message is published to RabbitMQ.
* The Python worker consumes the message.
* It generates an embedding.
* The embedding is stored in PostgreSQL.

The API doesn't wait.

Everything happens asynchronously.

Searching is handled by another lightweight FastAPI service that generates the query embedding and performs a nearest-neighbor search on PostgreSQL.

## Did I need a distributed system?

Honestly?

Probably not.

I could have built everything inside a single application.

But that's exactly why I enjoyed building it.

Every new question led to another technology.

Need semantic search?

Learn embeddings.

Need embeddings?

Learn Python.

Need asynchronous processing?

Learn RabbitMQ.

Need vector storage?

Learn PostgreSQL with pgvector.

Need multiple services?

Learn Docker.

Without realizing it, I had gone from a simple CRUD application to a small distributed system.

## Lessons learned

This project taught me much more than semantic search.

It taught me that software architecture evolves because of requirements, not because someone decided microservices were cool.

I also learned that distributed systems introduce new challenges:

* Service-to-service communication
* Event-driven processing
* Docker networking
* Data synchronization
* Handling failures
* Keeping services independent

These are things you don't usually encounter when building a standard CRUD application.

## What's next?

There are still a lot of improvements I'd like to make:

* Authentication and authorization
* Caching with Redis
* Better ranking and hybrid search
* Background retries and dead-letter queues
* CI/CD with GitHub Actions
* Deploying the entire stack to a VPS using Docker Compose

This started as interview preparation.

It ended up becoming one of the most fun side projects I've built in a while.

Sometimes all it takes is asking one more "why?".
