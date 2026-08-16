import { createTheme } from '@mui/material/styles'

export function createAppTheme(direction: 'ltr' | 'rtl') {
  return createTheme({
    direction,
    palette: {
      mode: 'light',
      primary: { main: '#0d7377' },
      secondary: { main: '#32a4a8' },
      background: { default: '#f4f7f8' },
    },
    typography: {
      fontFamily: direction === 'rtl'
        ? '"Noto Sans Arabic", "Segoe UI", sans-serif'
        : '"Segoe UI", "Roboto", sans-serif',
    },
    components: {
      MuiButton: {
        styleOverrides: {
          root: { textTransform: 'none', borderRadius: 8 },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: { borderRadius: 12 },
        },
      },
    },
  })
}
