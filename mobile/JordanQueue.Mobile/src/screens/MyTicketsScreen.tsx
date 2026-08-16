import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  ActivityIndicator,
  FlatList,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import type { TabScreenProps } from '../navigation/screenProps';
import { getMyTickets } from '../api';
import { getApiErrorMessage } from '../api/client';
import { useAuth } from '../auth/AuthContext';
import { ScreenHeader } from '../components/ScreenHeader';
import { colors } from '../theme/colors';
import type { Ticket } from '../types/api';
import { TicketStatus } from '../types/api';

type Props = TabScreenProps<'MyTickets'>;

export function MyTicketsScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!isAuthenticated) {
      setTickets([]);
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await getMyTickets();
      if (result.success) setTickets(result.data);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    const unsubscribe = navigation.addListener('focus', load);
    return unsubscribe;
  }, [navigation, load]);

  const isActive = (status: number) =>
    status === TicketStatus.Waiting ||
    status === TicketStatus.Called ||
    status === TicketStatus.Serving;

  if (!isAuthenticated) {
    return (
      <View style={styles.container}>
        <ScreenHeader title={t('tickets.title')} />
        <View style={styles.center}>
          <Text style={styles.hint}>{t('business.loginRequired')}</Text>
          <TouchableOpacity style={styles.loginBtn} onPress={() => navigation.navigate('Login')}>
            <Text style={styles.loginText}>{t('auth.login')}</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <ScreenHeader title={t('tickets.title')} />
      {loading ? (
        <ActivityIndicator style={styles.loader} color={colors.primary} />
      ) : error ? (
        <TouchableOpacity style={styles.errorBox} onPress={load}>
          <Text style={styles.error}>{error}</Text>
        </TouchableOpacity>
      ) : (
        <FlatList
          data={tickets}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.list}
          ListEmptyComponent={<Text style={styles.empty}>{t('tickets.empty')}</Text>}
          renderItem={({ item }) => (
            <TouchableOpacity
              style={styles.card}
              onPress={() => navigation.navigate('Ticket', { ticketId: item.id })}
            >
              <View style={styles.cardHeader}>
                <Text style={styles.ticketNum}>{item.ticketNumber}</Text>
                {isActive(item.status) && (
                  <View style={styles.badge}>
                    <Text style={styles.badgeText}>{t('tickets.active')}</Text>
                  </View>
                )}
              </View>
              <Text style={styles.business}>{item.businessNameEnglish}</Text>
              <Text style={styles.status}>
                {t(`ticket.statuses.${item.status}` as 'ticket.statuses.0')}
              </Text>
            </TouchableOpacity>
          )}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  hint: { color: colors.textSecondary, marginBottom: 16, textAlign: 'center' },
  loginBtn: { backgroundColor: colors.primary, paddingHorizontal: 24, paddingVertical: 12, borderRadius: 8 },
  loginText: { color: '#fff', fontWeight: '600' },
  list: { padding: 16 },
  card: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  cardHeader: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  ticketNum: { fontSize: 22, fontWeight: '800', color: colors.primary },
  badge: { backgroundColor: colors.success, paddingHorizontal: 8, paddingVertical: 2, borderRadius: 4 },
  badgeText: { color: '#fff', fontSize: 11, fontWeight: '600' },
  business: { fontSize: 15, color: colors.text, marginTop: 6 },
  status: { fontSize: 13, color: colors.textSecondary, marginTop: 4 },
  empty: { textAlign: 'center', color: colors.textSecondary, marginTop: 40 },
  loader: { marginTop: 40 },
  errorBox: { margin: 16, padding: 12 },
  error: { color: colors.error },
});
