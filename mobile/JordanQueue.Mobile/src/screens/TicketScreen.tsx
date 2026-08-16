import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  ActivityIndicator,
  Alert,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import type { RootStackScreenProps } from '../navigation/screenProps';
import { cancelTicket, getTicket } from '../api';
import { getApiErrorMessage } from '../api/client';
import { colors } from '../theme/colors';
import type { Ticket } from '../types/api';
import { TicketStatus } from '../types/api';

type Props = RootStackScreenProps<'Ticket'>;

export function TicketScreen({ route, navigation }: Props) {
  const { ticketId } = route.params;
  const { t } = useTranslation();
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [loading, setLoading] = useState(true);
  const [cancelling, setCancelling] = useState(false);

  const load = useCallback(async () => {
    try {
      const result = await getTicket(ticketId);
      if (result.success) setTicket(result.data);
    } catch {
      // keep last known ticket on poll errors
    } finally {
      setLoading(false);
    }
  }, [ticketId]);

  useEffect(() => {
    load();
    const interval = setInterval(load, 8000);
    return () => clearInterval(interval);
  }, [load]);

  const handleCancel = () => {
    Alert.alert(t('ticket.cancel'), '', [
      { text: t('common.back'), style: 'cancel' },
      {
        text: t('ticket.cancel'),
        style: 'destructive',
        onPress: async () => {
          setCancelling(true);
          try {
            await cancelTicket(ticketId);
            await load();
          } catch (err) {
            Alert.alert(t('common.error'), getApiErrorMessage(err));
          } finally {
            setCancelling(false);
          }
        },
      },
    ]);
  };

  if (loading && !ticket) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.primary} size="large" />
      </View>
    );
  }

  if (!ticket) {
    return (
      <View style={styles.center}>
        <Text style={styles.error}>{t('common.error')}</Text>
      </View>
    );
  }

  const isActive =
    ticket.status === TicketStatus.Waiting ||
    ticket.status === TicketStatus.Called ||
    ticket.status === TicketStatus.Serving;

  const statusLabel = t(`ticket.statuses.${ticket.status}` as 'ticket.statuses.0');

  return (
    <View style={styles.container}>
      <View style={styles.hero}>
        <Text style={styles.heroLabel}>{t('ticket.yourNumber')}</Text>
        <Text style={styles.heroNumber}>{ticket.ticketNumber}</Text>
        <Text style={styles.business}>{ticket.businessNameEnglish}</Text>
        <Text style={styles.service}>{ticket.serviceNameEnglish}</Text>
      </View>

      <View style={styles.stats}>
        <View style={styles.statBox}>
          <Text style={styles.statLabel}>{t('ticket.status')}</Text>
          <Text style={[styles.statValue, isActive && styles.statActive]}>{statusLabel}</Text>
        </View>
        {ticket.status === TicketStatus.Waiting && (
          <View style={styles.statBox}>
            <Text style={styles.statLabel}>{t('ticket.position')}</Text>
            <Text style={styles.statValue}>{ticket.position}</Text>
          </View>
        )}
        <View style={styles.statBox}>
          <Text style={styles.statLabel}>{t('ticket.nowServing')}</Text>
          <Text style={styles.statValue}>{ticket.nowServing ?? '—'}</Text>
        </View>
        <View style={styles.statBox}>
          <Text style={styles.statLabel}>{t('ticket.estimatedWait')}</Text>
          <Text style={styles.statValue}>
            {ticket.estimatedWaitMinutes} {t('business.minutes')}
          </Text>
        </View>
      </View>

      {ticket.status === TicketStatus.Waiting && (
        <TouchableOpacity
          style={styles.cancelBtn}
          onPress={handleCancel}
          disabled={cancelling}
        >
          {cancelling ? (
            <ActivityIndicator color={colors.error} />
          ) : (
            <Text style={styles.cancelText}>{t('ticket.cancel')}</Text>
          )}
        </TouchableOpacity>
      )}

      {!isActive && (
        <TouchableOpacity style={styles.doneBtn} onPress={() => navigation.navigate('HomeTabs')}>
          <Text style={styles.doneText}>{t('common.back')}</Text>
        </TouchableOpacity>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background, padding: 16 },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  hero: {
    backgroundColor: colors.primary,
    borderRadius: 16,
    padding: 24,
    alignItems: 'center',
    marginBottom: 20,
  },
  heroLabel: { color: 'rgba(255,255,255,0.8)', fontSize: 14 },
  heroNumber: { color: '#fff', fontSize: 56, fontWeight: '800', marginVertical: 8 },
  business: { color: '#fff', fontSize: 16, fontWeight: '600' },
  service: { color: 'rgba(255,255,255,0.85)', fontSize: 14, marginTop: 4 },
  stats: { gap: 10 },
  statBox: {
    backgroundColor: colors.surface,
    borderRadius: 10,
    padding: 14,
    flexDirection: 'row',
    justifyContent: 'space-between',
    borderWidth: 1,
    borderColor: colors.border,
  },
  statLabel: { color: colors.textSecondary, fontSize: 15 },
  statValue: { color: colors.text, fontSize: 15, fontWeight: '700' },
  statActive: { color: colors.primary },
  cancelBtn: {
    marginTop: 24,
    borderWidth: 1,
    borderColor: colors.error,
    borderRadius: 10,
    padding: 14,
    alignItems: 'center',
  },
  cancelText: { color: colors.error, fontWeight: '600', fontSize: 16 },
  doneBtn: {
    marginTop: 24,
    backgroundColor: colors.primary,
    borderRadius: 10,
    padding: 14,
    alignItems: 'center',
  },
  doneText: { color: '#fff', fontWeight: '600', fontSize: 16 },
  error: { color: colors.error },
});
