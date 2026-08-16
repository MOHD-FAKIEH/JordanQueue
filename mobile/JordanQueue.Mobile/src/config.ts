import { Platform } from 'react-native';

function defaultApiUrl(): string {
  if (Platform.OS === 'android') {
    return 'http://10.0.2.2:5257';
  }
  return 'http://localhost:5257';
}

export const API_URL = process.env.EXPO_PUBLIC_API_URL ?? defaultApiUrl();
