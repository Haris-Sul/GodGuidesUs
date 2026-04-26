import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'

type ChatRole = 'user' | 'model'

type ChatMessage = {
  role: ChatRole
  message: string
}

type ChatHistoryRequestItem = {
  role: ChatRole
  content: string
}

type GuidanceResponseDto = {
  thoughts?: string
  message?: string
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5085'

function App() {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [input, setInput] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const canSend = useMemo(
    () => !isLoading && input.trim().length > 0,
    [isLoading, input],
  )

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const trimmedInput = input.trim()
    if (!trimmedInput || isLoading) {
      return
    }

    const nextMessages: ChatMessage[] = [
      ...messages,
      {
        role: 'user',
        message: trimmedInput,
      },
    ]

    const historyPayload: ChatHistoryRequestItem[] = nextMessages.map((entry) => ({
      role: entry.role,
      content: entry.message,
    }))

    setMessages(nextMessages)
    setInput('')
    setIsLoading(true)
    setErrorMessage(null)

    try {
      const response = await fetch(`${API_BASE_URL}/api/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(historyPayload),
      })

      if (!response.ok) {
        const fallbackMessage = await response.text()
        throw new Error(fallbackMessage || 'request failed')
      }

      const data: GuidanceResponseDto = await response.json()
      const modelReply = (data.message ?? '').trim()

      if (!modelReply) {
        throw new Error('empty response from model')
      }

      setMessages((previousMessages) => [
        ...previousMessages,
        {
          role: 'model',
          message: modelReply,
        },
      ])
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : 'failed to generate guidance. please try again.'

      setErrorMessage(message)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <main className="min-h-screen bg-slate-100 text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-4xl flex-col px-4 py-8">
        <header className="mb-6 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <p className="text-sm uppercase tracking-[0.2em] text-slate-500">god guides us</p>
          <h1 className="mt-1 text-3xl font-semibold">chat with quran + tafsir guidance</h1>
          <p className="mt-2 text-base text-slate-600">
            ask your question and the app retrieves relevant verses before generating a response.
          </p>
        </header>

        <section className="flex-1 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
          <div className="flex h-[60vh] flex-col gap-3 overflow-y-auto pr-1">
            {messages.length === 0 ? (
              <div className="m-auto max-w-md rounded-xl border border-dashed border-slate-300 p-6 text-center text-base text-slate-500">
                start a conversation to test retrieval + gemma generation.
              </div>
            ) : (
              messages.map((message, index) => (
                <article
                  key={`${message.role}-${index}`}
                  className={`max-w-[85%] rounded-2xl px-4 py-3 text-base leading-relaxed ${
                    message.role === 'user'
                      ? 'ml-auto bg-indigo-500 text-white'
                      : 'mr-auto bg-slate-100 text-slate-900'
                  }`}
                >
                  <p className="mb-1 text-[10px] uppercase tracking-[0.18em] opacity-70">
                    {message.role === 'user' ? 'you' : 'model'}
                  </p>
                  <p className="whitespace-pre-wrap">{message.message}</p>
                </article>
              ))
            )}

            {isLoading && (
              <div className="mr-auto inline-flex items-center gap-2 rounded-xl bg-slate-100 px-4 py-2 text-base text-slate-700">
                <span className="h-2 w-2 animate-pulse rounded-full bg-indigo-500" />
                generating response...
              </div>
            )}
          </div>
        </section>

        <form onSubmit={handleSubmit} className="mt-4 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
          <label htmlFor="chat-input" className="mb-2 block text-sm uppercase tracking-[0.18em] text-slate-600">
            your message
          </label>
          <div className="flex gap-2">
            <input
              id="chat-input"
              value={input}
              onChange={(event) => setInput(event.target.value)}
              placeholder="type your question..."
              className="w-full rounded-xl border border-slate-300 bg-slate-50 px-4 py-4 text-lg text-slate-900 outline-none transition focus:border-indigo-500"
            />
            <button
              type="submit"
              disabled={!canSend}
              className="rounded-xl bg-indigo-500 px-6 py-4 text-base font-semibold text-white transition hover:bg-indigo-400 disabled:cursor-not-allowed disabled:opacity-50"
            >
              send
            </button>
          </div>

          {errorMessage && (
            <p className="mt-2 text-base text-rose-600">{errorMessage}</p>
          )}
        </form>
      </div>
    </main>
  )
}

export default App
