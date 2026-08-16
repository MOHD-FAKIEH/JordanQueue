import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react'
import type { AuthResponse } from '../types/api'
import { login as apiLogin } from '../api'

interface AuthUser {
  userId: string
  firstName: string
  lastName: string
  email: string
  roles: string[]
}

interface AuthContextValue {
  user: AuthUser | null
  isAuthenticated: boolean
  isOwner: boolean
  login: (emailOrMobile: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function loadUser(): AuthUser | null {
  const raw = localStorage.getItem('user')
  if (!raw) return null
  try {
    return JSON.parse(raw) as AuthUser
  } catch {
    return null
  }
}

function persistAuth(response: AuthResponse) {
  localStorage.setItem('accessToken', response.accessToken)
  localStorage.setItem('refreshToken', response.refreshToken)
  const user: AuthUser = {
    userId: response.userId,
    firstName: response.firstName,
    lastName: response.lastName,
    email: response.email,
    roles: response.roles,
  }
  localStorage.setItem('user', JSON.stringify(user))
  return user
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => loadUser())

  const login = useCallback(async (emailOrMobile: string, password: string) => {
    const result = await apiLogin(emailOrMobile, password)
    if (!result.success) {
      throw new Error(result.message || 'Login failed')
    }
    const roles = result.data.roles
    const allowed = roles.some((r) => r === 'BusinessOwner' || r === 'Staff')
    if (!allowed) {
      throw new Error('Admin access requires BusinessOwner or Staff role.')
    }
    setUser(persistAuth(result.data))
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
    localStorage.removeItem('selectedBusinessId')
    setUser(null)
  }, [])

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: !!user && !!localStorage.getItem('accessToken'),
      isOwner: user?.roles.includes('BusinessOwner') ?? false,
      login,
      logout,
    }),
    [user, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
