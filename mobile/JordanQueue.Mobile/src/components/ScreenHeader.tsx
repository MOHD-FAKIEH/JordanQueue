import { useTranslation } from 'react-i18next';
import { I18nManager, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { setLanguage } from '../i18n';
import { colors } from '../theme/colors';

export function LanguageToggle() {
  const { i18n, t } = useTranslation();

  const toggle = async () => {
    const next = i18n.language === 'ar' ? 'en' : 'ar';
    await setLanguage(next as 'ar' | 'en');
    // RTL requires app reload in RN — show hint via alert on next launch
    I18nManager.forceRTL(next === 'ar');
  };

  return (
    <TouchableOpacity onPress={toggle} style={styles.button}>
      <Text style={styles.text}>{i18n.language === 'ar' ? 'EN' : 'ع'}</Text>
    </TouchableOpacity>
  );
}

export function ScreenHeader({ title, right }: { title: string; right?: React.ReactNode }) {
  return (
    <View style={styles.header}>
      <Text style={styles.title}>{title}</Text>
      <View style={styles.right}>{right ?? <LanguageToggle />}</View>
    </View>
  );
}

const styles = StyleSheet.create({
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 12,
    backgroundColor: colors.primary,
  },
  title: {
    fontSize: 20,
    fontWeight: '700',
    color: '#fff',
    flex: 1,
  },
  right: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  button: {
    backgroundColor: 'rgba(255,255,255,0.2)',
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 6,
  },
  text: {
    color: '#fff',
    fontWeight: '600',
  },
});
