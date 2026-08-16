import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { API_URL } from '../config';
import type { ApiResponse, AuthResponse } from '../types/api';

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

apiClient.interceptors.request.use(async (config) => {
  const token = await AsyncStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    if (error.response?.status === 401 && original && !original._retry) {
      original._retry = true;
      const refreshToken = await AsyncStorage.getItem('refreshToken');
      if (refreshToken) {
        try {
          const { data } = await axios.post<ApiResponse<AuthResponse>>(
            `${API_URL}/api/auth/refresh`,
            { refreshToken },
          );
          if (data.success) {
            await AsyncStorage.setItem('accessToken', data.data.accessToken);
            await AsyncStorage.setItem('refreshToken', data.data.refreshToken);
            original.headers.Authorization = `Bearer ${data.data.accessToken}`;
            return apiClient(original);
          }
        } catch {
          await AsyncStorage.multiRemove(['accessToken', 'refreshToken', 'user']);
        }
      }
    }
    return Promise.reject(error);
  },
);

export function getApiErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiResponse<unknown> | undefined;
    if (data?.message) return data.message;
    if (data?.errors?.length) return data.errors[0].message;
    if (error.message) return error.message;
  }
  return 'An unexpected error occurred.';
}
