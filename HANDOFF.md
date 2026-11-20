# Handoff: `colombia-rental-law-rag` – Etapa 1: asistente RAG de arriendos en memoria

> Documento para iniciar el proyecto en VS Code con **Claude Code**. Léelo completo antes de escribir código.
> Brief completo del proyecto: https://claude.ai/artifact/5mPVy8K8XnN6suujkiU8gG

**Repositorio:** `colombia-rental-law-rag` (público, portafolio como desarrollador de IA).
**Stack:** .NET 10 (C#), ASP.NET Core Minimal API, Microsoft.Extensions.AI, Ollama.
**Etapa actual:** **Etapa 1** (fases 1 a 4): índice vectorial **en memoria**, hasta publicar la `v0.1.0`. PostgreSQL + pgvector es la etapa 2 (sección 14) y **no se implementa ahora**.
**Modo de ejecución:** local, solo para demos. No hay despliegue en la nube.

---

## 0. Primer paso para Claude Code: crear `CLAUDE.md`

Claude Code lee automáticamente el archivo `CLAUDE.md` de la raíz del repositorio en cada sesión. Crearlo **antes que cualquier otra cosa**, con este contenido (en inglés, porque es parte técnica del repo):

```markdown
# CLAUDE.md

## Project
colombia-rental-law-rag: a RAG assistant that answers Colombian residential rental law questions
(who pays for repairs, tenant and landlord duties) using only official legal texts as sources,
always citing the article. Stage 1 = in-memory vector index. Stage 2 (PostgreSQL + pgvector) is NOT in scope yet.
Full plan: HANDOFF.md.

## Golden rule
The model writes, the code decides what is valid. No citation reaches the user unless it came from retrieval.
The article text shown to the user always comes from the corpus, never from the model.

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
```

---

## 1. Convenciones (resumen)

| Elemento | Convención | Ejemplo |
|---|---|---|
| Solución y proyectos | inglés, `PascalCase` con puntos | `RentalLawAssistant.sln`, `RentalLawAssistant.Core` |
| Carpetas dentro de proyectos | inglés, `PascalCase` | `Corpus/`, `Retrieval/`, `Answering/` |
| Carpetas de la raíz | inglés, minúsculas | `src/`, `tests/`, `tools/`, `data/`, `evals/`, `docs/` |
| Archivos, tipos, métodos | inglés, `PascalCase`; asíncronos con `Async` | `CitationValidator.cs`, `AskAsync()` |
| Variables y parámetros | inglés, `camelCase` | `questionEmbedding` |
| Claves en archivos de datos | inglés, `snake_case` | `article_number`, `in_force`, `expected_article_ids` |
| JSON de la API HTTP | `camelCase` (por defecto en ASP.NET Core) | `{ "question": "..." }` |
| Comentarios, logs, commits | inglés; commits en imperativo | `Add citation validator` |
| Textos para el usuario final | **español**, en `SpanishMessages.cs` | aviso legal, "no encontré base" |
| Texto legal del corpus | **español**, tal como está en la norma | contenido de los artículos |

## 1.1 Repositorio público de portafolio

**Seguridad**
- En la etapa 1 no hay secretos: Ollama corre local sin clave. Si en algún momento se usa un modelo con API key, va en User Secrets, nunca en `appsettings.json`.
- `.gitignore` estándar de .NET (`dotnet new gitignore`) más `data/index/` (el índice es generado y puede pesar varios MB).
- Activar *secret scanning* y *push protection* en GitHub.

**Presentación**
- `README.md` en español (decisión del autor; opcionalmente un `README.en.md`) con:
  1. Una línea del problema.
  2. GIF de 3 preguntas: una clara, una con palabras cotidianas ("calentador") y una sin base que el asistente rechaza.
  3. Nota: *"Runs locally on demand with Ollama."*
  4. Diagrama de arquitectura en Mermaid.
  5. Decisiones técnicas: fragmentación por artículo, validación de citas en código, umbral de recuperación.
  6. Tabla de evaluaciones (ajuste y prueba lado a lado).
  7. Cómo correrlo.
  8. Roadmap por etapas y fases (sección 14).
  9. Aviso: *"General information, not legal advice."*
- `LICENSE` MIT.
- *Topics*: `rag`, `retrieval-augmented-generation`, `llm`, `ollama`, `dotnet`, `csharp`, `vector-search`, `embeddings`, `legal-tech`.

**Calidad visible**
- xUnit, `.editorconfig` + `dotnet format`, `Directory.Build.props` con `Nullable` y `TreatWarningsAsErrors`.
- GitHub Actions (`.github/workflows/ci.yml`): build, verificación de formato y pruebas. Las pruebas de CI **no** llaman a Ollama: usan dobles de prueba para `IChatClient` e `IEmbeddingGenerator`.

---

## 2. Contexto y objetivo

- Segundo proyecto del portafolio. El primero, `academy-whatsapp-bot`, ya demuestra clasificación de intención con reglas + LLM, evaluaciones y webhooks. Este suma **RAG**: fragmentación, embeddings, búsqueda vectorial, respuestas con citas y control de alucinaciones.
- Caso: dudas de arrendamiento de vivienda urbana en Colombia, sobre todo **quién paga una reparación**. La ley separa las reparaciones locativas (inquilino) de las necesarias (propietario).
- Conexión futura: será la pieza de conocimiento del bot de incidentes para inmobiliarias (fase 6).

**Ejemplo esperado**

Pregunta: *"Se dañó el calentador por viejo, ¿lo paga el inquilino o el propietario?"*
Respuesta: una explicación corta en español, la cita del Código Civil con el texto real del artículo, el aviso legal y la fecha de corte del corpus.

## 3. Alcance de la etapa 1

**Incluye**
- Corpus en Markdown, un archivo por norma, con un encabezado por artículo y su vigencia.
- Consola de ingesta: parsea por artículo, genera embeddings con `bge-m3` y guarda el índice en un archivo.
- API con `POST /ask`: carga el índice en memoria al arrancar, recupera, genera y valida.
- Umbral de recuperación y rechazo explícito ("no encontré base").
- Validación de citas en código.
- Evaluaciones de recuperación y de respuesta, con conjuntos de ajuste y de prueba.
- README, GIF y release `v0.1.0`.

**Fuera de alcance (no hacer sin preguntar)**
- PostgreSQL, pgvector, Docker o búsqueda híbrida (etapa 2).
- WhatsApp o integración con `academy-whatsapp-bot` (fase 6).
- Memoria de conversación o preguntas de seguimiento.
- Arriendos comerciales, jurisprudencia y conceptos de entidades.
- Interfaz web: basta Swagger/OpenAPI o un archivo `.http`.
- Despliegue en la nube.

## 4. Stack

- **.NET 10** (LTS) y **ASP.NET Core Minimal API**.
- **Microsoft.Extensions.AI**: `IEmbeddingGenerator<string, Embedding<float>>` para embeddings e `IChatClient` para generación, ambos contra **Ollama** (como en `academy-whatsapp-bot`; reutilizar ese conocimiento).
- **Modelos en Ollama:**
  - Embeddings: `bge-m3` (multilingüe, vectores de 1.024 dimensiones).
  - Chat: `qwen2.5`, en el tamaño que tu máquina maneje bien (por ejemplo `qwen2.5:7b`, o `qwen2.5:3b` si es lento). Configurable.
- **Búsqueda en memoria:** implementación propia de `IArticleStore` con similitud coseno de `System.Numerics.Tensors` (`TensorPrimitives.CosineSimilarity`). Es una interfaz propia para que en la etapa 2 se cambie por PostgreSQL sin tocar la API.
- **Serialización:** `System.Text.Json` (`JsonNamingPolicy.SnakeCaseLower` para los archivos de datos).
- **Pruebas:** xUnit y `WebApplicationFactory`.
- **Evaluaciones:** consola propia; en la fase 3, opcionalmente `Microsoft.Extensions.AI.Evaluation` para la fidelidad.

## 5. Estructura de la solución

```
colombia-rental-law-rag/
├── .github/workflows/ci.yml
├── CLAUDE.md
├── HANDOFF.md
├── README.md
├── LICENSE
├── .editorconfig
├── .gitignore                              # dotnet new gitignore + data/index/
├── Directory.Build.props
├── RentalLawAssistant.sln
├── data/
│   ├── raw/                                # original downloads (PDF/HTML), optional, for traceability
│   ├── corpus/
│   │   ├── ley-820-2003.md
│   │   ├── codigo-civil-arrendamiento.md
│   │   ├── decreto-1077-2015-reparaciones.md
│   │   └── SOURCES.md                      # URL + download date for each law
│   └── index/                              # generated by ingestion, git-ignored
│       └── articles.index.json
├── evals/
│   └── questions.json
├── docs/
│   └── architecture.md
├── src/
│   ├── RentalLawAssistant.Core/            # class library, no ASP.NET dependencies
│   │   ├── Corpus/
│   │   │   ├── LawArticle.cs               # record
│   │   │   └── CorpusParser.cs             # Markdown → LawArticle list
│   │   ├── Indexing/
│   │   │   ├── IndexedArticle.cs           # LawArticle + embedding + hash + model
│   │   │   ├── IArticleStore.cs            # SearchAsync(embedding, topK), GetByIdAsync(id)
│   │   │   ├── InMemoryArticleStore.cs     # loads index file, cosine similarity
│   │   │   ├── IndexFile.cs                # read/write articles.index.json
│   │   │   └── Ingestor.cs                 # parse → hash → embed (only changed) → save
│   │   ├── Retrieval/
│   │   │   ├── ScoredArticle.cs
│   │   │   └── Retriever.cs                # embed question, top K in-force, threshold
│   │   ├── Answering/
│   │   │   ├── ModelAnswer.cs              # what the LLM must return (structured)
│   │   │   ├── Prompts.cs                  # system prompt (English, asks for Spanish answer)
│   │   │   ├── AnswerGenerator.cs          # IChatClient call with structured output
│   │   │   ├── CitationValidator.cs        # drops citations not in retrieved set
│   │   │   ├── AskResult.cs                # final result returned to callers
│   │   │   └── AskService.cs               # orchestrates retrieval → generation → validation
│   │   ├── Messages/
│   │   │   └── SpanishMessages.cs          # disclaimer, no-basis message
│   │   └── Options/
│   │       ├── OllamaOptions.cs
│   │       ├── RetrievalOptions.cs
│   │       └── CorpusOptions.cs
│   ├── RentalLawAssistant.Ingestion/       # console app
│   │   └── Program.cs
│   └── RentalLawAssistant.Api/             # Minimal API (thin)
│       ├── Program.cs
│       ├── AskEndpoints.cs
│       ├── Contracts/                      # AskRequest, AskResponse, CitationDto
│       ├── RentalLawAssistant.Api.http     # sample requests for VS Code
│       └── appsettings.json
├── tools/
│   └── RentalLawAssistant.Evals/           # console app: runs evals, writes Markdown table
└── tests/
    └── RentalLawAssistant.Tests/
        ├── Fixtures/                       # small sample corpus for tests
        ├── Unit/
        └── Integration/
```

## 6. Corpus

### 6.1 Fuentes
| Norma | Qué aporta | Fuente oficial sugerida |
|---|---|---|
| Ley 820 de 2003 | Régimen de arrendamiento de vivienda urbana: obligaciones, descuento por reparaciones (art. 27), terminación | Gestor Normativo de Función Pública (`https://www.funcionpublica.gov.co/eva/gestornormativo/norma.php?i=8738`) o SUIN-Juriscol |
| Código Civil, Título XXVI (arrendamiento) | Reparaciones necesarias (art. 1985), reembolso de mejoras necesarias (art. 1993), reparaciones locativas (art. 1998) y reglas para casas (arts. 2028 y 2029) | Normograma de Cancillería (`https://cancilleria.gov.co/sites/default/files/Normograma/docs/codigo_civil_pr062.htm`) |
| Decreto 1077 de 2015, art. 2.2.6.1.1.10 | Definición de reparación locativa con ejemplos | Gestor Normativo de Función Pública |

**Importante sobre vigencia:** varios artículos del Código Civil sobre arrendamiento fueron derogados o subrogados (algunos por el art. 43 de la Ley 820). Verificar la nota de vigencia de **cada** artículo en la fuente oficial y marcarlo. El asistente nunca debe citar un artículo derogado como vigente.

**Quién consigue el texto:** yo descargo las normas de las fuentes oficiales y las dejo en `data/raw/`. Claude Code me ayuda a convertirlas al formato de la sección 6.2 y a revisar que no falten artículos. Guardar solo el texto de la norma, sin comentarios editoriales de los portales.

### 6.2 Formato de cada archivo
YAML *front matter* con los datos de la norma, y un encabezado `##` por artículo. Un artículo derogado lleva el comentario `<!-- status: repealed -->` justo debajo del encabezado.

```markdown
---
law_id: cc
law_name: Código Civil
source_url: https://cancilleria.gov.co/sites/default/files/Normograma/docs/codigo_civil_pr062.htm
downloaded_at: 2026-11-19
---

## Artículo 1985
La obligación de mantener la cosa arrendada en buen estado consiste en hacer, durante el arriendo,
todas las reparaciones necesarias, a excepción de las locativas, las cuales corresponden generalmente
al arrendatario. ...

## Artículo 2035
<!-- status: repealed -->
...
```

- **ID de cada artículo:** `{law_id}-{article_number}`, por ejemplo `cc-1985`, `l820-27`, `d1077-2.2.6.1.1.10`.
- **Fragmentación:** un artículo = un fragmento. Solo si un artículo supera un tamaño razonable (por ejemplo ~1.500 caracteres) se parte por incisos, con IDs `cc-1985#1`, `cc-1985#2`, que siguen apuntando al mismo artículo para las citas.
- `data/corpus/SOURCES.md`: tabla con norma, URL, fecha de descarga y fecha de corte del corpus.

## 7. Modelo de datos

```csharp
public sealed record LawArticle(
    string Id,              // "cc-1985"
    string LawId,           // "cc"
    string LawName,         // "Código Civil"
    string ArticleNumber,   // "1985"
    string Content,         // exact legal text
    bool InForce,
    string SourceUrl);

public sealed record IndexedArticle(
    LawArticle Article,
    string ContentHash,     // SHA-256 of Content, to skip re-embedding unchanged articles
    string EmbeddingModel,  // "bge-m3"
    float[] Embedding);

public sealed record ScoredArticle(LawArticle Article, float Similarity);
```

`IArticleStore` (la interfaz que se mantiene en la etapa 2):
```csharp
public interface IArticleStore
{
    Task<IReadOnlyList<ScoredArticle>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding, int topK, bool inForceOnly, CancellationToken ct);
    Task<LawArticle?> GetByIdAsync(string id, CancellationToken ct);
}
```

**Índice en archivo:** la ingesta escribe `data/index/articles.index.json` (claves `snake_case`) y la API lo carga completo en memoria al arrancar. Así no hay que volver a vectorizar el corpus cada vez que se enciende la demo. Si el `embedding_model` del índice no coincide con el configurado, la API no arranca y pide correr la ingesta de nuevo.

## 8. Configuración (`appsettings.json`)

```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "EmbeddingModel": "bge-m3",
    "ChatModel": "qwen2.5:7b"
  },
  "Retrieval": {
    "TopK": 5,
    "MinSimilarity": 0.5
  },
  "Corpus": {
    "Directory": "../../data/corpus",
    "IndexPath": "../../data/index/articles.index.json",
    "CutoffDate": "2026-11-19"
  }
}
```
- `MinSimilarity: 0.5` es un valor inicial **sin calibrar**: se ajusta con las evaluaciones (fase 3), mirando las similitudes de preguntas con y sin respuesta.
- Opciones enlazadas a clases (`IOptions<T>`) y validadas al arrancar (`ValidateOnStart`).

## 9. Detalles técnicos

### 9.1 Ingesta (`RentalLawAssistant.Ingestion`)
1. Leer todos los `.md` de `data/corpus/` (excepto `SOURCES.md`) y parsearlos con `CorpusParser`.
2. Validar: IDs únicos, ningún artículo vacío, `source_url` presente. Si algo falla, detenerse con un mensaje claro.
3. Si existe un índice previo, reutilizar el embedding de los artículos cuyo `content_hash` y `embedding_model` no cambiaron.
4. Generar embeddings de los demás con `IEmbeddingGenerator` (en lotes) usando `bge-m3`.
5. Escribir el índice y mostrar un resumen: artículos totales, vigentes, derogados, re-vectorizados y tiempo.

### 9.2 Recuperación (`Retriever`)
1. Generar el embedding de la pregunta con el **mismo** modelo de la ingesta.
2. `SearchAsync(topK: 5, inForceOnly: true)`.
3. Si la similitud del mejor resultado es menor que `MinSimilarity` → devolver "sin base" **sin llamar al modelo de chat**.

### 9.3 Generación (`AnswerGenerator`)
- Usar salida estructurada de `IChatClient` (por ejemplo `GetResponseAsync<ModelAnswer>`; verificar la API vigente) para obtener:
  ```csharp
  public sealed record ModelAnswer(string Answer, IReadOnlyList<string> CitedArticleIds, bool Grounded);
  ```
- El *system prompt* (en inglés, en `Prompts.cs`) indica:
  - Responder **en español**, en 2 a 4 frases, para alguien sin formación legal.
  - Usar **solo** los artículos entregados; cada artículo va con su ID.
  - Citar por ID cada afirmación.
  - Si los artículos no responden la pregunta, devolver `Grounded = false` y no inventar.
  - No dar asesoría legal personalizada.
- Temperatura baja (por ejemplo 0 a 0.2) para respuestas estables.
- Si el modelo falla o devuelve un JSON inválido → resultado "sin base" y registro del error en log (el usuario nunca ve un error crudo).

### 9.4 Validación (`CitationValidator`) — la regla de oro
- Se descarta toda cita cuyo ID no esté entre los artículos recuperados.
- Si `Grounded` es falso o no queda ninguna cita válida → resultado "sin base".
- El texto de cada cita que se devuelve se toma de `IArticleStore` (el corpus), **nunca** del modelo.
- Registrar en log cuántas citas se descartaron por pregunta (es una métrica útil para el README).

### 9.5 Contrato de la API

`POST /ask`
```json
{ "question": "Se dañó el calentador por viejo, ¿lo paga el inquilino o el propietario?" }
```

`200 OK`
```json
{
  "answer": "…",
  "grounded": true,
  "citations": [
    {
      "id": "cc-1985",
      "law": "Código Civil",
      "article": "1985",
      "text": "La obligación de mantener la cosa arrendada en buen estado…",
      "sourceUrl": "https://…"
    }
  ],
  "disclaimer": "Información general, no es asesoría legal.",
  "corpusCutoff": "2026-11-19"
}
```

Sin base suficiente: `200 OK` con `grounded: false`, `citations: []` y `answer` con el mensaje de `SpanishMessages.NoBasis`.

Validación de entrada: pregunta vacía o de más de ~500 caracteres → `400` con un mensaje claro.

Opcional para depurar y para el video: `GET /ask/debug?question=...` (solo en Development) que devuelve los 5 artículos recuperados con su similitud.

## 10. Evaluaciones (`tools/RentalLawAssistant.Evals`)

### 10.1 Archivo `evals/questions.json`
```json
[
  {
    "id": "q001",
    "split": "tuning",
    "question": "¿Quién arregla una gotera causada por lo viejo del techo?",
    "expected_article_ids": ["cc-1985"],
    "expected_answer": "El propietario, porque es una reparación necesaria y no locativa.",
    "should_answer": true
  },
  {
    "id": "q050",
    "split": "test",
    "question": "¿Cuánto IVA se paga por arrendar un local comercial?",
    "expected_article_ids": [],
    "expected_answer": "",
    "should_answer": false
  }
]
```
- Meta: ~60 preguntas, ~40 de ajuste (`tuning`) y ~20 de prueba (`test`). **Las de prueba no se miran mientras se ajusta.**
- Mezcla: preguntas claras, con palabras cotidianas que no están en la ley ("calentador", "chapa", "humedad"), fuera del corpus (deben rechazarse) y que tocan artículos derogados.
- En la fase 1 bastan las primeras 15, solo con `expected_article_ids`.

### 10.2 Métricas
| Métrica | Qué mide | Cómo |
|---|---|---|
| Recall@5 | El artículo esperado está entre los 5 recuperados | Código |
| MRR | Qué tan arriba aparece | Código |
| Precisión de citas | Las citas finales son las esperadas | Código |
| Rechazo correcto | Rechaza cuando `should_answer` es falso, y solo entonces | Código |
| Fidelidad | La respuesta dice lo mismo que el artículo | LLM como juez (fase 3, opcional) + revisión manual de una muestra |
| Latencia | Segundos por pregunta | Código |

- Modos: `--retrieval-only` (solo recuperación, rápido y sin modelo de chat) y completo.
- Salida: tabla Markdown en consola y en `evals/results/latest.md`, con ajuste y prueba lado a lado, más el detalle de cada pregunta fallida.

## 11. Criterios de aceptación de la etapa 1

- [ ] `AGENTS.md` (instrucciones para agentes, importado por `CLAUDE.md` con `@AGENTS.md`) creado y respetado; convenciones de la sección 1 en todo el código.
- [ ] `RentalLawAssistant.Core` sin dependencias de ASP.NET Core.
- [ ] Corpus de las 3 normas en `data/corpus/` con vigencia marcada y `SOURCES.md`.
- [ ] La ingesta construye el índice, reutiliza embeddings sin cambios y falla con mensaje claro ante un corpus inválido.
- [ ] `POST /ask` responde con citas cuyo texto viene del corpus.
- [ ] Una cita a un artículo no recuperado nunca llega a la respuesta (probado con un `IChatClient` falso que inventa una cita).
- [ ] Preguntas por debajo del umbral responden "sin base" sin llamar al modelo de chat (probado).
- [ ] Ningún artículo derogado aparece en las citas.
- [ ] La API no arranca si el modelo de embeddings del índice no coincide con el configurado.
- [ ] Evaluaciones corren con un comando y producen la tabla de ajuste y prueba.
- [ ] CI en verde: build, formato y pruebas sin llamar a Ollama.
- [ ] README en inglés completo (sección 1.1), GIF y release `v0.1.0`.

## 12. Pruebas

- **Unitarias:**
  - `CorpusParser`: artículos, IDs, front matter, derogados y artículos largos partidos por incisos.
  - `InMemoryArticleStore`: orden por similitud con vectores pequeños hechos a mano y filtro de vigencia.
  - `Retriever`: umbral.
  - `CitationValidator`: citas válidas, inventadas, todas inventadas y `Grounded = false`.
  - `AskService`: flujo completo con dobles de prueba de `IEmbeddingGenerator` e `IChatClient`.
- **Integración:** `WebApplicationFactory` con un índice de prueba de `tests/.../Fixtures/`, para `/ask` (con base, sin base y entrada inválida).
- **Manual:** con Ollama encendido, las 3 preguntas del GIF.

## 13. Orden de trabajo (etapa 1)

**Fase 1 – Recuperación (hoy)**
1. Crear el repositorio público `colombia-rental-law-rag`: `CLAUDE.md`, `dotnet new gitignore` (+ `data/index/`), `LICENSE` MIT, `.editorconfig`, `Directory.Build.props` y README inicial.
2. Crear la solución: `Core`, `Ingestion`, `Api`, `Tests` y `tools/Evals`, con sus referencias.
3. Opciones (`OllamaOptions`, `RetrievalOptions`, `CorpusOptions`) con validación al arrancar.
4. `LawArticle` + `CorpusParser` con pruebas (usando un corpus pequeño de fixtures).
5. Convertir las normas descargadas en `data/raw/` al formato de la sección 6.2.
6. `IndexFile` + `Ingestor` con `IEmbeddingGenerator` (`bge-m3`) y reutilización por hash.
7. `IArticleStore` + `InMemoryArticleStore` + `Retriever`, con pruebas.
8. Primeras 15 preguntas en `evals/questions.json` y modo `--retrieval-only` de las evaluaciones → medir Recall@5.
   **Criterio para pasar a la fase 2:** en la mayoría de las 15, el artículo correcto aparece entre los 5 recuperados.

**Fase 2 – Respuestas**

9. `SpanishMessages`, `Prompts`, `ModelAnswer` y `AnswerGenerator` con salida estructurada.
10. `CitationValidator` + `AskService`, con pruebas de citas inventadas.
11. API: `POST /ask`, contratos, validación de entrada, archivo `.http` y `GET /ask/debug` en Development.
    **Criterio:** ninguna cita inventada llega a la respuesta.

**Fase 3 – Evaluaciones**

12. Completar ~60 preguntas (ajuste y prueba), modo completo de las evaluaciones, calibrar `MinSimilarity` con las de ajuste y comparar 2 modelos de chat (o de embeddings).
13. Opcional: fidelidad con LLM como juez + revisión manual de una muestra.
    **Criterio:** resultados de prueba listos, medidos una sola vez al final.

**Fase 4 – Portafolio**

14. GitHub Actions (`ci.yml`).
15. README completo, diagrama Mermaid, tabla de evaluaciones, GIF y release `v0.1.0`.

## 14. Roadmap (fuera del alcance actual)

- **Etapa 2 – Fase 5: PostgreSQL + pgvector.**
  - `docker-compose.yml` con la imagen `pgvector/pgvector` (único contenedor del proyecto).
  - Tabla `articles` con `content_hash`, `embedding_model` y `embedding vector(1024)`, más un índice HNSW de distancia coseno.
  - Ingesta idempotente.
  - Búsqueda híbrida: vectores + texto completo de PostgreSQL en español.
  - Implementación `PostgresArticleStore` de la **misma** interfaz `IArticleStore`, con Npgsql + `Pgvector` + Dapper y SQL escrito a mano.
  - Se queda solo si las evaluaciones mejoran frente a la `v0.1.0`; entonces se publica la `v0.2.0`.
- **Fase 6 – Integración:** el bot de WhatsApp (incidentes para inmobiliarias) llama a `/ask` para sugerir quién paga cada daño reportado.

---

## Instrucciones para Claude Code

- **Antes de escribir código:** crea `CLAUDE.md` con el contenido de la sección 0 y confirma conmigo el plan de la fase 1 (usa el modo de planificación).
- Trabaja en el orden de la sección 13, un paso a la vez. Al terminar cada paso, corre `dotnet build` y `dotnet test` y dime cómo probarlo.
- Respeta la regla de oro: **el modelo redacta, el código decide qué es válido.** Ninguna cita sin recuperación; el texto citado siempre sale del corpus.
- No implementes nada de la sección 14 ni lo marcado como fuera de alcance (sección 3) sin preguntarme. En particular: nada de PostgreSQL, Docker ni búsqueda híbrida en la etapa 1.
- Pregúntame antes de agregar un paquete NuGet. Verifica nombres de paquetes y APIs de `Microsoft.Extensions.AI` y del conector de Ollama contra la documentación vigente, porque cambian seguido.
- No inventes texto legal: si falta un artículo o no está clara su vigencia, márcalo y pregúntame.
- Antes de cada commit: sin secretos, sin datos personales reales, sin archivos de `data/index/`. Commits pequeños, en inglés e imperativo.
- Explícame las decisiones técnicas en español y brevemente; estoy aprendiendo RAG.
