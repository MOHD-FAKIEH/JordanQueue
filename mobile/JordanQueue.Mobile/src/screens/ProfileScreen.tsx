import { useTranslation } from 'react-i18next';
import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import type { TabScreenProps } from '../navigation/screenProps';
import { useAuth } from '../auth/AuthContext';
import { ScreenHeader } from '../components/ScreenHeader';
import { colors } from '../theme/colors';

type Props = TabScreenProps<'Profile'>;

export function ProfileScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { user, isAuthenticated, logout } = useAuth();

  if (!isAuthenticated) {
    return (
      <View style={styles.container}>
        <ScreenHeader title={t('auth.profile')} />
        <View style={styles.center}>
          <TouchableOpacity style={styles.btn} onPress={() => navigation.navigate('Login')}>
            <Text style={styles.btnText}>{t('auth.login')}</Text>
          </TouchableOpacity>
          <TouchableOpacity style={[styles.btn, styles.btnOutline]} onPress={() => navigation.navigate('Register')}>
            <Text style={[styles.btnText, styles.outlineText]}>{t('auth.register')}</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <ScreenHeader title={t('auth.profile')} />
      <View style={styles.content}>
        <Text style={styles.name}>{user?.firstName} {user?.lastName}</Text>
        <Text style={styles.email}>{user?.email}</Text>
        <TouchableOpacity style={[styles.btn, styles.logout]} onPress={logout}>
          <Text style={styles.btnText}>{t('auth.logout')}</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  center: { flex: 1, justifyContent: 'center', padding: 24, gap: 12 },
  content: { padding: 24 },
  name: { fontSize: 22, fontWeight: '700', color: colors.text },
  email: { fontSize: 15, color: colors.textSecondary, marginTop: 6, marginBottom: 24 },
  btn: { backgroundColor: colors.primary, padding: 16, borderRadius: 10, alignItems: 'center' },
  btnOutline: { backgroundColor: 'transparent', borderWidth: 1, borderColor: colors.primary },
  btnText: { color: '#fff', fontWeight: '700', fontSize: 16 },
  outlineText: { color: colors.primary },
  logout: { marginTop: 8, backgroundColor: colors.error },
});
