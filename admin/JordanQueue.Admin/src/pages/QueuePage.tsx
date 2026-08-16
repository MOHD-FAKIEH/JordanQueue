import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  callNext,
  cancelTicket,
  getBusinessQueue,
  getQueueTickets,
  getServices,
  openQueue,
  pauseQueue,
  resumeQueue,
  serveTicket,
  skipTicket,
} from '../api'
import { getApiErrorMessage } from '../api/client'
import { useBusiness } from '../context/BusinessContext'
import type { Queue, Service, Ticket } from '../types/api'
import { QueueStatus, TicketStatus } from '../types/api'

export function QueuePage() {
  const { t, i18n } = useTranslation()
  const { selectedBusiness } = useBusiness()
  const [services, setServices] = useState<Service[]>([])
  const [serviceId, setServiceId] = useState('')
  const [queue, setQueue] = useState<Queue | null>(null)
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [actionLoading, setActionLoading] = useState(false)

  const ticketStatusKey = (status: number) => {
    const map: Record<number, string> = {
      [TicketStatus.Waiting]: 'waiting',
      [TicketStatus.Called]: 'called',
      [TicketStatus.Serving]: 'serving',
      [TicketStatus.Served]: 'served',
      [TicketStatus.Skipped]: 'skipped',
      [TicketStatus.Cancelled]: 'cancelled',
    }
    return t(`queue.ticketStatus.${map[status] ?? 'waiting'}`)
  }

  const queueStatusLabel = (status: number) => {
    const map: Record<number, string> = {
      [QueueStatus.Open]: 'open',
      [QueueStatus.Paused]: 'paused',
      [QueueStatus.Closed]: 'closed',
    }
    return t(`queue.status.${map[status] ?? 'closed'}`)
  }

  useEffect(() => {
    if (!selectedBusiness) return
    getServices(selectedBusiness.id)
      .then((res) => {
        if (res.success) {
          const active = res.data.filter((s) => s.isActive)
          setServices(active)
          if (active.length > 0) setServiceId(active[0].id)
        }
      })
      .catch((err) => setError(getApiErrorMessage(err)))
  }, [selectedBusiness])

  const loadQueue = useCallback(async () => {
    if (!selectedBusiness || !serviceId) return
    setLoading(true)
    setError(null)
    try {
      const res = await getBusinessQueue(selectedBusiness.id, serviceId)
      if (res.success) {
        setQueue(res.data)
        const ticketsRes = await getQueueTickets(res.data.id)
        if (ticketsRes.success) setTickets(ticketsRes.data)
      }
    } catch (err) {
      setQueue(null)
      setTickets([])
      setError(getApiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [selectedBusiness, serviceId])

  useEffect(() => {
    loadQueue()
    const interval = setInterval(loadQueue, 10000)
    return () => clearInterval(interval)
  }, [loadQueue])

  const runAction = async (action: () => Promise<unknown>) => {
    setActionLoading(true)
    setError(null)
    try {
      await action()
      await loadQueue()
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setActionLoading(false)
    }
  }

  if (!selectedBusiness) {
    return <Alert severity="info">{t('common.noBusinesses')}</Alert>
  }

  const serviceLabel = (s: Service) =>
    i18n.language === 'ar' ? s.nameArabic : s.nameEnglish

  return (
    <Box>
      <Typography variant="h5" gutterBottom sx={{ fontWeight: 700 }}>
        {t('queue.title')}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 3 }}>
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>{t('queue.selectService')}</InputLabel>
          <Select
            value={serviceId}
            label={t('queue.selectService')}
            onChange={(e) => setServiceId(e.target.value)}
          >
            {services.map((s) => (
              <MenuItem key={s.id} value={s.id}>
                {serviceLabel(s)}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <Button
          variant="outlined"
          disabled={!serviceId || actionLoading}
          onClick={() => runAction(() => openQueue(selectedBusiness.id, serviceId))}
        >
          {t('queue.openQueue')}
        </Button>
      </Stack>

      {loading && !queue ? (
        <CircularProgress />
      ) : queue ? (
        <>
          <Paper sx={{ p: 2, mb: 3 }}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ alignItems: 'center' }}>
              <Chip label={queueStatusLabel(queue.status)} color={queue.status === QueueStatus.Open ? 'success' : 'default'} />
              <Typography>
                {t('queue.nowServing')}: <strong>{queue.nowServing ?? '—'}</strong>
              </Typography>
              <Typography>
                {t('queue.waiting')}: <strong>{queue.waitingCount}</strong>
              </Typography>
              <Typography>
                {t('queue.estimatedWait')}: <strong>{queue.estimatedWaitMinutes} min</strong>
              </Typography>
              <Box sx={{ flexGrow: 1 }} />
              <Button
                variant="contained"
                disabled={actionLoading || queue.status !== QueueStatus.Open}
                onClick={() => runAction(() => callNext(queue.id))}
              >
                {t('queue.callNext')}
              </Button>
              {queue.status === QueueStatus.Paused ? (
                <Button variant="outlined" disabled={actionLoading} onClick={() => runAction(() => resumeQueue(queue.id))}>
                  {t('queue.resume')}
                </Button>
              ) : (
                <Button variant="outlined" disabled={actionLoading || queue.status !== QueueStatus.Open} onClick={() => runAction(() => pauseQueue(queue.id))}>
                  {t('queue.pause')}
                </Button>
              )}
            </Stack>
          </Paper>

          <Paper>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>#</TableCell>
                  <TableCell>{t('queue.ticketStatus.waiting')}</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {tickets.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={3} align="center">
                      {t('queue.noTickets')}
                    </TableCell>
                  </TableRow>
                ) : (
                  tickets.map((ticket) => (
                    <TableRow key={ticket.id}>
                      <TableCell>
                        <Typography sx={{ fontWeight: 700 }}>{ticket.ticketNumber}</Typography>
                      </TableCell>
                      <TableCell>
                        <Chip size="small" label={ticketStatusKey(ticket.status)} />
                      </TableCell>
                      <TableCell align="right">
                        <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
                          {(ticket.status === TicketStatus.Called || ticket.status === TicketStatus.Serving) && (
                            <Button size="small" variant="contained" disabled={actionLoading} onClick={() => runAction(() => serveTicket(ticket.id))}>
                              {t('queue.serve')}
                            </Button>
                          )}
                          {ticket.status !== TicketStatus.Served && ticket.status !== TicketStatus.Cancelled && ticket.status !== TicketStatus.Skipped && (
                            <Button size="small" variant="outlined" color="warning" disabled={actionLoading} onClick={() => runAction(() => skipTicket(ticket.id))}>
                              {t('queue.skip')}
                            </Button>
                          )}
                          {ticket.status === TicketStatus.Waiting && (
                            <Button size="small" variant="outlined" color="error" disabled={actionLoading} onClick={() => runAction(() => cancelTicket(ticket.id))}>
                              {t('queue.cancel')}
                            </Button>
                          )}
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </Paper>
        </>
      ) : (
        <Alert severity="info">
          {t('dashboard.noQueue')} — {t('queue.openQueue')}
        </Alert>
      )}
    </Box>
  )
}
