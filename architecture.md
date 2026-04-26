# God Guides Us - Architecture & Rules
## The Vision
Navigate modern problems with practical ancient wisdom. God Guides Us runs Gemma 4 26B for private, tailored insights rooted in authentic Islamic texts and classical commentaries (note this is quran and tafsir). Built for everyone.
(note we will use Gemma 4 via Gemini API for simplicity though)

## Architecture
- Backend: C# ASP.NET Core Web API (.NET 9).
- Frontend: React (Vite) + Tailwind CSS (v4).
- Database: MongoDB Atlas with Vector Search (Index: 'vector_index', Field: 'Vector', Dim: 768).
- AI: Google Generative AI REST API (via HttpClient). 
  - Embeddings Model: 'models/text-embedding-004' (outputDimensionality: 768).
  - Generation Model: 'models/gemma-4-26b-a4b-it' (Use thinkingConfig: { thinkingLevel: 'high' }).

## Chat Flow (Multi-turn + RAG)
- The frontend sends the user's latest message AND the chat history.
- The backend embeds the *latest* message, searches MongoDB, and injects the retrieved Verses/Tafsirs as context into the prompt before sending the full history to Gemma.

## Style Rules
- follow repository pattern and dependency injection.
- do not number code comments, or capitalize the start of sentences for comments.
- make logic easy to follow but not too concise.