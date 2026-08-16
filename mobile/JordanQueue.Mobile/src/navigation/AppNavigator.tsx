import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { ActivityIndicator, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';
import { BusinessDetailScreen } from '../screens/BusinessDetailScreen';
import { LoginScreen } from '../screens/LoginScreen';
import { RegisterScreen } from '../screens/RegisterScreen';
import { TicketScreen } from '../screens/TicketScreen';
import { colors } from '../theme/colors';
import { MainTabs } from './MainTabs';
import type { RootStackParamList } from './types';

const Stack = createNativeStackNavigator<RootStackParamList>();

export function AppNavigator() {
  const { loading } = useAuth();
  const { t } = useTranslation();

  if (loading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator color={colors.primary} size="large" />
      </View>
    );
  }

  return (
    <NavigationContainer>
      <Stack.Navigator
        screenOptions={{
          headerStyle: { backgroundColor: colors.primary },
          headerTintColor: '#fff',
          headerTitleStyle: { fontWeight: '700' },
        }}
      >
        <Stack.Screen name="HomeTabs" component={MainTabs} options={{ headerShown: false }} />
        <Stack.Screen
          name="BusinessDetail"
          component={BusinessDetailScreen}
          options={{ title: t('app.title') }}
        />
        <Stack.Screen name="Ticket" component={TicketScreen} options={{ title: t('ticket.yourNumber') }} />
        <Stack.Screen name="Login" component={LoginScreen} options={{ title: t('auth.login') }} />
        <Stack.Screen name="Register" component={RegisterScreen} options={{ title: t('auth.register') }} />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
