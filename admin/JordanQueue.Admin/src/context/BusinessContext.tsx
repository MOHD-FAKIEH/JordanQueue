import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import type { Business } from '../types/api'
import { getMyBusinesses } from '../api'
import { getApiErrorMessage } from '../api/client'

interface BusinessContextValue {
  businesses: Business[]
  selectedBusiness: Business | null
  selectBusiness: (id: string) => void
  loading: boolean
  error: string | null
  refresh: () => Promise<void>
}

const BusinessContext = createContext<BusinessContextValue | null>(null)

export function BusinessProvider({ children }: { children: ReactNode }) {
  const [businesses, setBusinesses] = useState<Business[]>([])
  const [selectedId, setSelectedId] = useState<string | null>(
    () => localStorage.getItem('selectedBusinessId'),
  )
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const refresh = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await getMyBusinesses()
      if (result.success) {
        setBusinesses(result.data)
        const saved = localStorage.getItem('selectedBusinessId')
        const valid = result.data.find((b) => b.id === saved)
        if (valid) {
          setSelectedId(valid.id)
        } else if (result.data.length > 0) {
          setSelectedId(result.data[0].id)
          localStorage.setItem('selectedBusinessId', result.data[0].id)
        } else {
          setSelectedId(null)
          localStorage.removeItem('selectedBusinessId')
        }
      }
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    refresh()
  }, [refresh])

  const selectBusiness = useCallback(
    (id: string) => {
      setSelectedId(id)
      localStorage.setItem('selectedBusinessId', id)
    },
    [],
  )

  const selectedBusiness = useMemo(
    () => businesses.find((b) => b.id === selectedId) ?? null,
    [businesses, selectedId],
  )

  const value = useMemo(
    () => ({ businesses, selectedBusiness, selectBusiness, loading, error, refresh }),
    [businesses, selectedBusiness, selectBusiness, loading, error, refresh],
  )

  return <BusinessContext.Provider value={value}>{children}</BusinessContext.Provider>
}

export function useBusiness() {
  const ctx = useContext(BusinessContext)
  if (!ctx) throw new Error('useBusiness must be used within BusinessProvider')
  return ctx
}
