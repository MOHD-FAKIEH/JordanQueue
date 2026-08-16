import {
  Alert,
  Box,
  Button,
  Chip,
  IconButton,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import DeleteIcon from '@mui/icons-material/Delete'
import PersonAddIcon from '@mui/icons-material/PersonAdd'
import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Navigate } from 'react-router-dom'
import { addStaff, getStaff, removeStaff } from '../api'
import { getApiErrorMessage } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { useBusiness } from '../context/BusinessContext'
import type { StaffMember } from '../types/api'

export function StaffPage() {
  const { t } = useTranslation()
  const { isOwner } = useAuth()
  const { selectedBusiness } = useBusiness()
  const [staff, setStaff] = useState<StaffMember[]>([])
  const [email, setEmail] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const load = useCallback(async () => {
    if (!selectedBusiness) return
    try {
      const res = await getStaff(selectedBusiness.id)
      if (res.success) setStaff(res.data)
    } catch (err) {
      setError(getApiErrorMessage(err))
    }
  }, [selectedBusiness])

  useEffect(() => {
    load()
  }, [load])

  if (!isOwner) {
    return <Navigate to="/" replace />
  }

  if (!selectedBusiness) {
    return <Alert severity="info">{t('common.noBusinesses')}</Alert>
  }

  const handleAdd = async () => {
    if (!email.trim()) return
    setLoading(true)
    setError(null)
    try {
      await addStaff(selectedBusiness.id, email.trim())
      setEmail('')
      await load()
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  const handleRemove = async (staffId: string) => {
    setError(null)
    try {
      await removeStaff(selectedBusiness.id, staffId)
      await load()
    } catch (err) {
      setError(getApiErrorMessage(err))
    }
  }

  return (
    <Box>
      <Typography variant="h5" gutterBottom sx={{ fontWeight: 700 }}>
        {t('staff.title')}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper sx={{ p: 2, mb: 3 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            fullWidth
            label={t('staff.email')}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="staff1@jordanqueue.dev"
          />
          <Button
            variant="contained"
            startIcon={<PersonAddIcon />}
            onClick={handleAdd}
            disabled={loading}
            sx={{ whiteSpace: 'nowrap' }}
          >
            {t('staff.add')}
          </Button>
        </Stack>
      </Paper>

      <Paper>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>{t('services.nameEn')}</TableCell>
              <TableCell>{t('staff.email')}</TableCell>
              <TableCell>{t('services.active')}</TableCell>
              <TableCell align="right">{t('staff.remove')}</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {staff.map((member) => (
              <TableRow key={member.id}>
                <TableCell>{member.firstName} {member.lastName}</TableCell>
                <TableCell>{member.email}</TableCell>
                <TableCell>
                  <Chip
                    size="small"
                    label={member.isActive ? t('staff.active') : t('staff.inactive')}
                    color={member.isActive ? 'success' : 'default'}
                  />
                </TableCell>
                <TableCell align="right">
                  {member.isActive && (
                    <IconButton color="error" onClick={() => handleRemove(member.id)}>
                      <DeleteIcon />
                    </IconButton>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
    </Box>
  )
}
