import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  ActivityIndicator,
  FlatList,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import type { TabScreenProps } from '../navigation/screenProps';
import { searchBusinesses } from '../api';
import { getApiErrorMessage } from '../api/client';
import { ScreenHeader } from '../components/ScreenHeader';
import { colors } from '../theme/colors';
import type { Business } from '../types/api';

type Props = TabScreenProps<'Explore'>;

export function HomeScreen({ navigation }: Props) {
  const { t, i18n } = useTranslation();
  const [search, setSearch] = useState('');
  const [businesses, setBusinesses] = useState<Business[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async (term?: string) => {
    setLoading(true);
    setError(null);
    try {
      const result = await searchBusinesses(term?.trim() || undefined);
      if (result.success) setBusinesses(result.data.items);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const label = (b: Business) =>
    i18n.language === 'ar' ? b.nameArabic : b.nameEnglish;

  const desc = (b: Business) =>
    i18n.language === 'ar' ? b.descriptionArabic : b.descriptionEnglish;

  return (
    <View style={styles.container}>
      <ScreenHeader title={t('app.title')} />
      <View style={styles.searchBox}>
        <TextInput
          style={styles.searchInput}
          placeholder={t('home.search')}
          value={search}
          onChangeText={setSearch}
          onSubmitEditing={() => load(search)}
          returnKeyType="search"
        />
      </View>

      {error && (
        <TouchableOpacity style={styles.errorBox} onPress={() => load(search)}>
          <Text style={styles.errorText}>{error}</Text>
          <Text style={styles.retry}>{t('common.retry')}</Text>
        </TouchableOpacity>
      )}

      {loading ? (
        <ActivityIndicator style={styles.loader} color={colors.primary} />
      ) : (
        <FlatList
          data={businesses}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.list}
          ListEmptyComponent={<Text style={styles.empty}>{t('home.empty')}</Text>}
          renderItem={({ item }) => (
            <TouchableOpacity
              style={styles.card}
              onPress={() => navigation.navigate('BusinessDetail', { businessId: item.id })}
            >
              <Text style={styles.cardTitle}>{label(item)}</Text>
              <Text style={styles.cardCategory}>
                {t(`categories.${item.category}` as 'categories.0')}
              </Text>
              <Text style={styles.cardDesc} numberOfLines={2}>
                {desc(item)}
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
  searchBox: { padding: 16, paddingBottom: 8 },
  searchInput: {
    backgroundColor: colors.surface,
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderWidth: 1,
    borderColor: colors.border,
    fontSize: 16,
  },
  list: { padding: 16, paddingTop: 8, gap: 12 },
  card: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  cardTitle: { fontSize: 18, fontWeight: '700', color: colors.text },
  cardCategory: { fontSize: 13, color: colors.primary, marginTop: 4 },
  cardDesc: { fontSize: 14, color: colors.textSecondary, marginTop: 6 },
  empty: { textAlign: 'center', color: colors.textSecondary, marginTop: 40 },
  loader: { marginTop: 40 },
  errorBox: { margin: 16, padding: 12, backgroundColor: '#fdecea', borderRadius: 8 },
  errorText: { color: colors.error },
  retry: { color: colors.primary, marginTop: 4, fontWeight: '600' },
});
