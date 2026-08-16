import { useTranslation } from 'react-i18next'
import { IconButton, MenuItem, Select, Tooltip } from '@mui/material'
import LanguageIcon from '@mui/icons-material/Language'
import { useBusiness } from '../context/BusinessContext'

export function LanguageSwitcher() {
  const { i18n } = useTranslation()

  const toggle = () => {
    const next = i18n.language === 'ar' ? 'en' : 'ar'
    i18n.changeLanguage(next)
    localStorage.setItem('language', next)
    window.location.reload()
  }

  return (
    <Tooltip title={i18n.language === 'ar' ? 'English' : 'العربية'}>
      <IconButton color="inherit" onClick={toggle} aria-label="Switch language">
        <LanguageIcon />
      </IconButton>
    </Tooltip>
  )
}

export function BusinessSelector() {
  const { t, i18n } = useTranslation()
  const { businesses, selectedBusiness, selectBusiness } = useBusiness()

  if (businesses.length <= 1) return null

  const label = (b: (typeof businesses)[0]) =>
    i18n.language === 'ar' ? b.nameArabic : b.nameEnglish

  return (
    <Select
      size="small"
      value={selectedBusiness?.id ?? ''}
      onChange={(e) => selectBusiness(e.target.value)}
      sx={{ minWidth: 180, bgcolor: 'background.paper' }}
      displayEmpty
    >
      <MenuItem value="" disabled>
        {t('common.selectBusiness')}
      </MenuItem>
      {businesses.map((b) => (
        <MenuItem key={b.id} value={b.id}>
          {label(b)}
        </MenuItem>
      ))}
    </Select>
  )
}
