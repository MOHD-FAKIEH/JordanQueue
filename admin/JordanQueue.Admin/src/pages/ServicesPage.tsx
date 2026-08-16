import {
  Alert,
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import EditIcon from '@mui/icons-material/Edit'
import AddIcon from '@mui/icons-material/Add'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { createService, getServices, updateService } from '../api'
import { getApiErrorMessage } from '../api/client'
import { useBusiness } from '../context/BusinessContext'
import type { Service } from '../types/api'

type ServiceForm = {
  nameArabic: string
  nameEnglish: string
  descriptionArabic: string
  descriptionEnglish: string
  averageServiceMinutes: number
  isActive: boolean
}

const emptyForm: ServiceForm = {
  nameArabic: '',
  nameEnglish: '',
  descriptionArabic: '',
  descriptionEnglish: '',
  averageServiceMinutes: 15,
  isActive: true,
}

export function ServicesPage() {
  const { t, i18n } = useTranslation()
  const { selectedBusiness } = useBusiness()
  const [services, setServices] = useState<Service[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editing, setEditing] = useState<Service | null>(null)
  const [form, setForm] = useState<ServiceForm>(emptyForm)

  const load = useCallback(async () => {
    if (!selectedBusiness) return
    setLoading(true)
    try {
      const res = await getServices(selectedBusiness.id)
      if (res.success) setServices(res.data)
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [selectedBusiness])

  useEffect(() => {
    load()
  }, [load])

  const openCreate = () => {
    setEditing(null)
    setForm(emptyForm)
    setDialogOpen(true)
  }

  const openEdit = (service: Service) => {
    setEditing(service)
    setForm({
      nameArabic: service.nameArabic,
      nameEnglish: service.nameEnglish,
      descriptionArabic: service.descriptionArabic,
      descriptionEnglish: service.descriptionEnglish,
      averageServiceMinutes: service.averageServiceMinutes,
      isActive: service.isActive,
    })
    setDialogOpen(true)
  }

  const handleSave = async () => {
    if (!selectedBusiness) return
    setError(null)
    try {
      if (editing) {
        await updateService(editing.id, form)
      } else {
        await createService(selectedBusiness.id, form)
      }
      setDialogOpen(false)
      await load()
    } catch (err) {
      setError(getApiErrorMessage(err))
    }
  }

  const label = (s: Service) => (i18n.language === 'ar' ? s.nameArabic : s.nameEnglish)

  if (!selectedBusiness) {
    return <Alert severity="info">{t('common.noBusinesses')}</Alert>
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h5" sx={{ fontWeight: 700 }}>
          {t('services.title')}
        </Typography>
        <Button startIcon={<AddIcon />} variant="contained" onClick={openCreate}>
          {t('services.add')}
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>{t('services.nameEn')}</TableCell>
              <TableCell>{t('services.duration')}</TableCell>
              <TableCell>{t('services.active')}</TableCell>
              <TableCell align="right">{t('services.edit')}</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {services.map((s) => (
              <TableRow key={s.id}>
                <TableCell>{label(s)}</TableCell>
                <TableCell>{s.averageServiceMinutes}</TableCell>
                <TableCell>{s.isActive ? '✓' : '—'}</TableCell>
                <TableCell align="right">
                  <IconButton onClick={() => openEdit(s)}>
                    <EditIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? t('services.edit') : t('services.add')}</DialogTitle>
        <DialogContent>
          <TextField fullWidth margin="normal" label={t('services.nameAr')} value={form.nameArabic} onChange={(e) => setForm({ ...form, nameArabic: e.target.value })} />
          <TextField fullWidth margin="normal" label={t('services.nameEn')} value={form.nameEnglish} onChange={(e) => setForm({ ...form, nameEnglish: e.target.value })} />
          <TextField fullWidth margin="normal" label={t('services.descAr')} value={form.descriptionArabic} onChange={(e) => setForm({ ...form, descriptionArabic: e.target.value })} />
          <TextField fullWidth margin="normal" label={t('services.descEn')} value={form.descriptionEnglish} onChange={(e) => setForm({ ...form, descriptionEnglish: e.target.value })} />
          <TextField fullWidth margin="normal" type="number" label={t('services.duration')} value={form.averageServiceMinutes} onChange={(e) => setForm({ ...form, averageServiceMinutes: Number(e.target.value) })} />
          {editing && (
            <FormControlLabel
              control={<Checkbox checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />}
              label={t('services.active')}
            />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>{t('services.cancel')}</Button>
          <Button variant="contained" onClick={handleSave} disabled={loading}>
            {t('services.save')}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
