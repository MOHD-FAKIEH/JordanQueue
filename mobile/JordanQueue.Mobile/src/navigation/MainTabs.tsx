import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { useTranslation } from 'react-i18next';
import { HomeScreen } from '../screens/HomeScreen';
import { MyTicketsScreen } from '../screens/MyTicketsScreen';
import { NotificationsScreen } from '../screens/NotificationsScreen';
import { ProfileScreen } from '../screens/ProfileScreen';
import { colors } from '../theme/colors';
import type { TabParamList } from './types';

const Tab = createBottomTabNavigator<TabParamList>();

export function MainTabs() {
  const { t } = useTranslation();

  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textSecondary,
        tabBarStyle: { borderTopColor: colors.border },
      }}
    >
      <Tab.Screen name="Explore" component={HomeScreen} options={{ title: t('tabs.explore') }} />
      <Tab.Screen name="MyTickets" component={MyTicketsScreen} options={{ title: t('tabs.tickets') }} />
      <Tab.Screen name="Notifications" component={NotificationsScreen} options={{ title: t('tabs.notifications') }} />
      <Tab.Screen name="Profile" component={ProfileScreen} options={{ title: t('auth.profile') }} />
    </Tab.Navigator>
  );
}
