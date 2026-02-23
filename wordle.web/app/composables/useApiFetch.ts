export const useApiFetch = () => {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string
  const { getAuthHeaders } = useAuth()

  const apiFetch = async <T = any>(path: string, options: RequestInit = {}): Promise<T> => {
    const headers = {
      'Content-Type': 'application/json',
      ...getAuthHeaders(),
      ...(options.headers as Record<string, string> || {}),
    }

    const res = await fetch(`${apiBase}${path}`, {
      ...options,
      headers,
    })

    if (!res.ok) {
      throw new Error(`API error: ${res.status}`)
    }

    const text = await res.text()
    return text ? JSON.parse(text) : null
  }

  return { apiFetch, apiBase }
}
