import AsyncStorage from '@react-native-async-storage/async-storage';
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { login as apiLogin, register as apiRegister } from '../api';
import type { AuthResponse } from '../types/api';

interface AuthUser {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  roles: string[];
}

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  loading: boolean;
  login: (emailOrMobile: string, password: string) => Promise<void>;
  register: (payload: {
    firstName: string;
    lastName: string;
    mobileNumber: string;
    email: string;
    password: string;
  }) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

async function persistAuth(response: AuthResponse): Promise<AuthUser> {
  const user: AuthUser = {
    userId: response.userId,
    firstName: response.firstName,
    lastName: response.lastName,
    email: response.email,
    roles: response.roles,
  };
  await AsyncStorage.multiSet([
    ['accessToken', response.accessToken],
    ['refreshToken', response.refreshToken],
    ['user', JSON.stringify(user)],
  ]);
  return user;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    AsyncStorage.multiGet(['user', 'accessToken']).then(([[, userRaw], [, token]]) => {
      if (userRaw && token) {
        try {
          setUser(JSON.parse(userRaw) as AuthUser);
        } catch {
          setUser(null);
        }
      }
      setLoading(false);
    });
  }, []);

  const login = useCallback(async (emailOrMobile: string, password: string) => {
    const result = await apiLogin(emailOrMobile, password);
    if (!result.success) throw new Error(result.message ?? 'Login failed');
    setUser(await persistAuth(result.data));
  }, []);

  const register = useCallback(
    async (payload: {
      firstName: string;
      lastName: string;
      mobileNumber: string;
      email: string;
      password: string;
    }) => {
      const result = await apiRegister(payload);
      if (!result.success) throw new Error(result.message ?? 'Registration failed');
      setUser(await persistAuth(result.data));
    },
    [],
  );

  const logout = useCallback(async () => {
    await AsyncStorage.multiRemove(['accessToken', 'refreshToken', 'user']);
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: !!user,
      loading,
      login,
      register,
      logout,
    }),
    [user, loading, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
