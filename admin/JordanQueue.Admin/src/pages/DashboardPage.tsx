import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Grid,
  Typography,
} from '@mui/material'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink } from 'react-router-dom'
import { getBusiness, getDailyStats } from '../api'
import { getApiErrorMessage } from '../api/client'
import { useBusiness } from '../context/BusinessContext'
import type { BusinessDetail, DailyStats } from '../types/api'

function StatCard({ label, value }: { label: string; value: string | number }) {
  return (
    <Card>
      <CardContent>
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="h4" sx={{ fontWeight: 700, color: 'primary.main' }}>
          {value}
        </Typography>
      </CardContent>
    </Card>
  )
}

export function DashboardPage() {
  const { t, i18n } = useTranslation()
  const { selectedBusiness, loading: bizLoading } = useBusiness()
  const [stats, setStats] = useState<DailyStats | null>(null)
  const [detail, setDetail] = useState<BusinessDetail | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!selectedBusiness) return
    setLoading(true)
    setError(null)
    try {
      const [statsRes, detailRes] = await Promise.all([
        getDailyStats(selectedBusiness.id),
        getBusiness(selectedBusiness.id),
      ])
      if (statsRes.success) setStats(statsRes.data)
      if (detailRes.success) setDetail(detailRes.data)
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [selectedBusiness])

  useEffect(() => {
    load()
    const interval = setInterval(load, 15000)
    return () => clearInterval(interval)
  }, [load])

  if (bizLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    )
  }

  if (!selectedBusiness) {
    return <Alert severity="info">{t('common.noBusinesses')}</Alert>
  }

  const businessName =
    i18n.language === 'ar' ? selectedBusiness.nameArabic : selectedBusiness.nameEnglish

  return (
    <Box>
      <Typography variant="h5" gutterBottom sx={{ fontWeight: 700 }}>
        {t('dashboard.title')} — {businessName}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {loading && !stats ? (
        <CircularProgress />
      ) : stats ? (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <StatCard label={t('dashboard.waiting')} value={stats.waitingCount} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <StatCard label={t('dashboard.serving')} value={stats.servingCount} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <StatCard label={t('dashboard.servedToday')} value={stats.servedToday} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <StatCard label={t('dashboard.avgWait')} value={stats.averageWaitMinutes} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <StatCard label={t('dashboard.avgService')} value={stats.averageServiceMinutes} />
          </Grid>
        </Grid>
      ) : null}

      <Card>
        <CardContent>
          {detail?.currentQueue ? (
            <Box>
              <Typography variant="h6" gutterBottom>
                {t('dashboard.nowServing')}: {detail.currentQueue.nowServing ?? '—'}
              </Typography>
              <Typography color="text.secondary">
                {t('dashboard.waiting')}: {detail.currentQueue.waitingCount} ·{' '}
                {t('dashboard.avgWait')}: {detail.currentQueue.estimatedWaitMinutes} min
              </Typography>
              <Button component={RouterLink} to="/queue" variant="contained" sx={{ mt: 2 }}>
                {t('nav.queue')}
              </Button>
            </Box>
          ) : (
            <Box>
              <Typography color="text.secondary" gutterBottom>
                {t('dashboard.noQueue')}
              </Typography>
              <Button component={RouterLink} to="/queue" variant="contained">
                {t('dashboard.openQueue')}
              </Button>
            </Box>
          )}
        </CardContent>
      </Card>
    </Box>
  )
}
