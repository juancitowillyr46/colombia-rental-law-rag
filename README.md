# colombia-rental-law-rag

A RAG assistant that answers Colombian residential rental law questions (who pays for a repair, tenant and landlord duties) using only official legal texts, always citing the exact article.

> **Status:** work in progress — Stage 1 (in-memory vector index), Phase 1 (retrieval).
> Runs locally on demand with Ollama.

## How it works (short)

1. The corpus (Ley 820 de 2003, Código Civil, Decreto 1077 de 2015) is split **one article per chunk** and embedded with `bge-m3`.
2. A question is embedded with the same model; the top in-force articles are retrieved by cosine similarity.
3. Below a similarity threshold, the assistant says it found no basis — without calling the chat model.
4. The chat model writes a short answer in Spanish; **the code** drops any citation that was not retrieved, and the cited text always comes from the corpus.

## Stack

.NET 10 · ASP.NET Core Minimal API · Microsoft.Extensions.AI · Ollama (OllamaSharp) · xUnit

## Roadmap

- **Stage 1** (current): in-memory index, `POST /ask`, citation validation, evals, `v0.1.0`.
- **Stage 2**: PostgreSQL + pgvector, hybrid search, `v0.2.0` only if evals improve.

## Disclaimer

General information, not legal advice.

## License

[MIT](LICENSE)
