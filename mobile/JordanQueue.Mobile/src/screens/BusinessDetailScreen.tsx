import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  ActivityIndicator,
  Alert,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import type { RootStackScreenProps } from '../navigation/screenProps';
import { getBusiness, joinQueue } from '../api';
import { getApiErrorMessage } from '../api/client';
import { useAuth } from '../auth/AuthContext';
import { colors } from '../theme/colors';
import type { BusinessDetail, ServiceSummary } from '../types/api';
import { TicketStatus } from '../types/api';

type Props = RootStackScreenProps<'BusinessDetail'>;

export function BusinessDetailScreen({ route, navigation }: Props) {
  const { businessId } = route.params;
  const { t, i18n } = useTranslation();
  const { isAuthenticated } = useAuth();
  const [business, setBusiness] = useState<BusinessDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [joining, setJoining] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getBusiness(businessId);
      if (result.success) setBusiness(result.data);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [businessId]);

  useEffect(() => {
    load();
  }, [load]);

  const name = business
    ? i18n.language === 'ar'
      ? business.nameArabic
      : business.nameEnglish
    : '';

  const serviceLabel = (s: ServiceSummary) =>
    i18n.language === 'ar' ? s.nameArabic : s.nameEnglish;

  const handleJoin = async (serviceId: string) => {
    if (!isAuthenticated) {
      Alert.alert(t('business.loginRequired'), '', [
        { text: t('common.back'), style: 'cancel' },
        { text: t('auth.login'), onPress: () => navigation.navigate('Login') },
      ]);
      return;
    }

    setJoining(serviceId);
    try {
      const result = await joinQueue(businessId, serviceId);
      if (result.success) {
        navigation.replace('Ticket', { ticketId: result.data.id });
      }
    } catch (err) {
      Alert.alert(t('common.error'), getApiErrorMessage(err));
    } finally {
      setJoining(null);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.primary} size="large" />
      </View>
    );
  }

  if (error || !business) {
    return (
      <View style={styles.center}>
        <Text style={styles.error}>{error ?? t('common.error')}</Text>
        <TouchableOpacity style={styles.retryBtn} onPress={load}>
          <Text style={styles.retryText}>{t('common.retry')}</Text>
        </TouchableOpacity>
      </View>
    );
  }

  const queue = business.currentQueue;

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <Text style={styles.title}>{name}</Text>
      <Text style={styles.category}>{t(`categories.${business.category}` as 'categories.0')}</Text>
      <Text style={styles.address}>
        {i18n.language === 'ar' ? business.addressArabic : business.addressEnglish}
      </Text>

      {queue ? (
        <View style={styles.queueBox}>
          <Text style={styles.queueTitle}>{t('business.nowServing')}: {queue.nowServing ?? '—'}</Text>
          <Text style={styles.queueMeta}>
            {t('business.waiting')}: {queue.waitingCount} · {t('business.estimatedWait')}:{' '}
            {queue.estimatedWaitMinutes} {t('business.minutes')}
          </Text>
        </View>
      ) : (
        <Text style={styles.noQueue}>{t('business.queueClosed')}</Text>
      )}

      <Text style={styles.section}>{t('business.services')}</Text>
      {business.services
        .filter((s) => s.isActive)
        .map((service) => (
          <View key={service.id} style={styles.serviceCard}>
            <View style={styles.serviceInfo}>
              <Text style={styles.serviceName}>{serviceLabel(service)}</Text>
              <Text style={styles.serviceDuration}>
                {service.averageServiceMinutes} {t('business.minutes')}
              </Text>
            </View>
            <TouchableOpacity
              style={[styles.joinBtn, joining === service.id && styles.joinBtnDisabled]}
              disabled={!!joining}
              onPress={() => handleJoin(service.id)}
            >
              {joining === service.id ? (
                <ActivityIndicator color="#fff" size="small" />
              ) : (
                <Text style={styles.joinText}>{t('business.join')}</Text>
              )}
            </TouchableOpacity>
          </View>
        ))}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  content: { padding: 16 },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  title: { fontSize: 24, fontWeight: '700', color: colors.text },
  category: { fontSize: 14, color: colors.primary, marginTop: 4 },
  address: { fontSize: 14, color: colors.textSecondary, marginTop: 8 },
  queueBox: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    padding: 16,
    marginTop: 16,
    borderWidth: 1,
    borderColor: colors.border,
  },
  queueTitle: { fontSize: 16, fontWeight: '600', color: colors.text },
  queueMeta: { fontSize: 14, color: colors.textSecondary, marginTop: 6 },
  noQueue: { marginTop: 16, color: colors.textSecondary, fontStyle: 'italic' },
  section: { fontSize: 18, fontWeight: '700', marginTop: 24, marginBottom: 12, color: colors.text },
  serviceCard: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    padding: 14,
    marginBottom: 10,
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: colors.border,
  },
  serviceInfo: { flex: 1 },
  serviceName: { fontSize: 16, fontWeight: '600', color: colors.text },
  serviceDuration: { fontSize: 13, color: colors.textSecondary, marginTop: 2 },
  joinBtn: {
    backgroundColor: colors.primary,
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 8,
    minWidth: 90,
    alignItems: 'center',
  },
  joinBtnDisabled: { opacity: 0.7 },
  joinText: { color: '#fff', fontWeight: '600' },
  error: { color: colors.error, textAlign: 'center', marginBottom: 12 },
  retryBtn: { backgroundColor: colors.primary, paddingHorizontal: 20, paddingVertical: 10, borderRadius: 8 },
  retryText: { color: '#fff', fontWeight: '600' },
});
