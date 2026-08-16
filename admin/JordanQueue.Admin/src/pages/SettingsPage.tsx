import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Navigate } from 'react-router-dom'
import { getBusiness, updateBusiness } from '../api'
import { getApiErrorMessage } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { useBusiness } from '../context/BusinessContext'
import type { BusinessDetail } from '../types/api'

export function SettingsPage() {
  const { t } = useTranslation()
  const { isOwner } = useAuth()
  const { selectedBusiness, refresh } = useBusiness()
  const [form, setForm] = useState<BusinessDetail | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!selectedBusiness) return
    getBusiness(selectedBusiness.id)
      .then((res) => {
        if (res.success) setForm(res.data)
      })
      .catch((err) => setError(getApiErrorMessage(err)))
  }, [selectedBusiness])

  if (!isOwner) {
    return <Navigate to="/" replace />
  }

  if (!selectedBusiness) {
    return <Alert severity="info">{t('common.noBusinesses')}</Alert>
  }

  const handleSave = async () => {
    if (!form) return
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      await updateBusiness(form.id, {
        nameArabic: form.nameArabic,
        nameEnglish: form.nameEnglish,
        descriptionArabic: form.descriptionArabic,
        descriptionEnglish: form.descriptionEnglish,
        phoneNumber: form.phoneNumber,
        addressArabic: form.addressArabic,
        addressEnglish: form.addressEnglish,
        category: form.category,
        isActive: form.isActive,
      })
      setSuccess(t('settings.save'))
      await refresh()
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  if (!form) {
    return null
  }

  return (
    <Box sx={{ maxWidth: 640 }}>
      <Typography variant="h5" gutterBottom sx={{ fontWeight: 700 }}>
        {t('settings.title')}
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {success && <Alert severity="success" sx={{ mb: 2 }}>{success}</Alert>}

      <TextField fullWidth margin="normal" label={t('settings.nameAr')} value={form.nameArabic} onChange={(e) => setForm({ ...form, nameArabic: e.target.value })} />
      <TextField fullWidth margin="normal" label={t('settings.nameEn')} value={form.nameEnglish} onChange={(e) => setForm({ ...form, nameEnglish: e.target.value })} />
      <TextField fullWidth margin="normal" label={t('settings.descAr')} value={form.descriptionArabic} onChange={(e) => setForm({ ...form, descriptionArabic: e.target.value })} multiline rows={2} />
      <TextField fullWidth margin="normal" label={t('settings.descEn')} value={form.descriptionEnglish} onChange={(e) => setForm({ ...form, descriptionEnglish: e.target.value })} multiline rows={2} />
      <TextField fullWidth margin="normal" label={t('settings.phone')} value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} />
      <TextField fullWidth margin="normal" label={t('settings.addressAr')} value={form.addressArabic} onChange={(e) => setForm({ ...form, addressArabic: e.target.value })} />
      <TextField fullWidth margin="normal" label={t('settings.addressEn')} value={form.addressEnglish} onChange={(e) => setForm({ ...form, addressEnglish: e.target.value })} />

      <FormControl fullWidth margin="normal">
        <InputLabel>{t('settings.category')}</InputLabel>
        <Select
          value={form.category}
          label={t('settings.category')}
          onChange={(e) => setForm({ ...form, category: Number(e.target.value) })}
        >
          {[0, 1, 2, 3, 4, 5, 6].map((c) => (
            <MenuItem key={c} value={c}>
              {t(`categories.${c}`)}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <FormControlLabel
        control={<Checkbox checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />}
        label={t('settings.active')}
      />

      <Button variant="contained" size="large" onClick={handleSave} disabled={loading} sx={{ mt: 2 }}>
        {loading ? t('common.loading') : t('settings.save')}
      </Button>
    </Box>
  )
}
