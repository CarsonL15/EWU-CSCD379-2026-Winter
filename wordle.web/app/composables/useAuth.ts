const accessToken = ref('')
const refreshToken = ref('')
const email = ref('')
const roles = ref<string[]>([])

export const useAuth = () => {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string

  const isAuthenticated = computed(() => !!accessToken.value)
  const isAdmin = computed(() => roles.value.includes('Admin'))

  const restore = () => {
    if (import.meta.server) return
    accessToken.value = localStorage.getItem('accessToken') || ''
    refreshToken.value = localStorage.getItem('refreshToken') || ''
    email.value = localStorage.getItem('email') || ''
    const savedRoles = localStorage.getItem('roles')
    roles.value = savedRoles ? JSON.parse(savedRoles) : []
  }

  const persist = () => {
    if (import.meta.server) return
    localStorage.setItem('accessToken', accessToken.value)
    localStorage.setItem('refreshToken', refreshToken.value)
    localStorage.setItem('email', email.value)
    localStorage.setItem('roles', JSON.stringify(roles.value))
  }

  const login = async (emailInput: string, password: string) => {
    const res = await fetch(`${apiBase}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: emailInput, password }),
    })
    if (!res.ok) {
      const err = await res.json().catch(() => null)
      throw new Error(err?.detail || 'Login failed')
    }
    const data = await res.json()
    accessToken.value = data.accessToken
    refreshToken.value = data.refreshToken
    email.value = emailInput
    persist()
    await fetchMe()
  }

  const register = async (emailInput: string, password: string) => {
    const res = await fetch(`${apiBase}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: emailInput, password }),
    })
    if (!res.ok) {
      const err = await res.json().catch(() => null)
      const errors = err?.errors
      if (errors) {
        const messages = Object.values(errors).flat() as string[]
        throw new Error(messages.join(', '))
      }
      throw new Error(err?.detail || 'Registration failed')
    }
    await login(emailInput, password)
  }

  const logout = () => {
    accessToken.value = ''
    refreshToken.value = ''
    email.value = ''
    roles.value = []
    if (!import.meta.server) {
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('email')
      localStorage.removeItem('roles')
    }
  }

  const fetchMe = async () => {
    if (!accessToken.value) return
    const res = await fetch(`${apiBase}/api/auth/me`, {
      headers: { Authorization: `Bearer ${accessToken.value}` },
    })
    if (res.ok) {
      const data = await res.json()
      email.value = data.email
      roles.value = data.roles || []
      persist()
    }
  }

  const getAuthHeaders = (): Record<string, string> => {
    if (!accessToken.value) return {}
    return { Authorization: `Bearer ${accessToken.value}` }
  }

  // Restore on first use
  if (!import.meta.server && !accessToken.value) {
    restore()
  }

  return {
    accessToken: readonly(accessToken),
    refreshToken: readonly(refreshToken),
    email: readonly(email),
    roles: readonly(roles),
    isAuthenticated,
    isAdmin,
    login,
    register,
    logout,
    fetchMe,
    getAuthHeaders,
  }
}
