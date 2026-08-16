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
import { getNotifications, markNotificationRead } from '../api';
import { getApiErrorMessage } from '../api/client';
import { useAuth } from '../auth/AuthContext';
import { ScreenHeader } from '../components/ScreenHeader';
import { colors } from '../theme/colors';
import type { NotificationItem } from '../types/api';

type Props = TabScreenProps<'Notifications'>;

export function NotificationsScreen({ navigation }: Props) {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const [items, setItems] = useState<NotificationItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!isAuthenticated) {
      setItems([]);
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await getNotifications();
      if (result.success) setItems(result.data);
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

  const handleRead = async (item: NotificationItem) => {
    if (item.isRead) {
      if (item.ticketId) navigation.navigate('Ticket', { ticketId: item.ticketId });
      return;
    }
    try {
      await markNotificationRead(item.id);
      await load();
      if (item.ticketId) navigation.navigate('Ticket', { ticketId: item.ticketId });
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  };

  if (!isAuthenticated) {
    return (
      <View style={styles.container}>
        <ScreenHeader title={t('notifications.title')} />
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
      <ScreenHeader title={t('notifications.title')} />
      {loading ? (
        <ActivityIndicator style={styles.loader} color={colors.primary} />
      ) : error ? (
        <TouchableOpacity style={styles.errorBox} onPress={load}>
          <Text style={styles.error}>{error}</Text>
        </TouchableOpacity>
      ) : (
        <FlatList
          data={items}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.list}
          ListEmptyComponent={<Text style={styles.empty}>{t('notifications.empty')}</Text>}
          renderItem={({ item }) => (
            <TouchableOpacity
              style={[styles.card, !item.isRead && styles.unread]}
              onPress={() => handleRead(item)}
            >
              <Text style={styles.title}>{item.title}</Text>
              <Text style={styles.message}>{item.message}</Text>
              {!item.isRead && <Text style={styles.markRead}>{t('notifications.markRead')}</Text>}
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
  unread: { borderColor: colors.primary, borderWidth: 2 },
  title: { fontSize: 16, fontWeight: '700', color: colors.text },
  message: { fontSize: 14, color: colors.textSecondary, marginTop: 6 },
  markRead: { fontSize: 12, color: colors.primary, marginTop: 8, fontWeight: '600' },
  empty: { textAlign: 'center', color: colors.textSecondary, marginTop: 40 },
  loader: { marginTop: 40 },
  errorBox: { margin: 16, padding: 12 },
  error: { color: colors.error },
});
