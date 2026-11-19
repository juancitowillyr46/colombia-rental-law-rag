# AGENTS.md

> Single source of instructions for coding agents (Claude Code, Copilot, Cursor, Codex...).
> CLAUDE.md only imports this file; edit this one.

## Project
colombia-rental-law-rag: a RAG assistant that answers Colombian residential rental law questions
(who pays for repairs, tenant and landlord duties) using only official legal texts as sources,
always citing the article. Stage 1 = in-memory vector index. Stage 2 (PostgreSQL + pgvector) is NOT in scope yet.
Full plan: HANDOFF.md.

## Golden rule
The model writes, the code decides what is valid. No citation reaches the user unless it came from retrieval.
The article text shown to the user always comes from the corpus, never from the model.

## Stack
- .NET 10, ASP.NET Core Minimal API, Microsoft.Extensions.AI abstractions.
- Ollama (local) via OllamaSharp, which implements IChatClient and IEmbeddingGenerator.
- Embeddings: bge-m3. Chat: qwen2.5 (size configurable).

## Conventions
- All code, file names, identifiers, comments, logs and commit messages in English. Standard C# conventions
  (PascalCase types/methods/files, camelCase locals, _camelCase private fields, Async suffix, I-prefixed interfaces).
- User-facing texts in Spanish, centralized in src/RentalLawAssistant.Core/Messages/SpanishMessages.cs.
- Data files (corpus metadata, index file, eval files) use snake_case keys. HTTP API JSON uses camelCase.
- Nullable enabled, warnings as errors.
- Core has no ASP.NET Core dependencies.

## Commands
- Build: dotnet build
- Test: dotnet test
- Format check: dotnet format --verify-no-changes
- Ingest corpus: dotnet run --project src/RentalLawAssistant.Ingestion
- Run API: dotnet run --project src/RentalLawAssistant.Api
- Run evals: dotnet run --project tools/RentalLawAssistant.Evals
- Ollama must be running locally with bge-m3 and the chat model pulled (ollama list).

## Working agreement
- Small, verifiable steps; after each one, say how to test it.
- Run build and tests before proposing a commit.
- Ask before adding a NuGet package or changing scope.
- Verify Microsoft.Extensions.AI / Ollama APIs and package names against current docs; they change often.
- Public repo: never commit secrets, real personal data or generated index files.
