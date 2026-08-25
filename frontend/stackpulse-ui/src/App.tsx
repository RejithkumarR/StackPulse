import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import type { ReactNode } from 'react';
import { useMemo, useState } from 'react';
import './App.css';

import LoginPage from './pages/auth/LoginPage';
import DashboardPage from './pages/dashboard/DashboardPage';
import UsersPage from './pages/users/UsersPage';
import ProfilePage from './pages/profile/ProfilePage';
import SettingsPage from './pages/settings/SettingsPage';
import NotFoundPage from './pages/errors/NotFoundPage';
import ForbiddenPage from './pages/errors/ForbiddenPage';
import ErrorPage from './pages/errors/ErrorPage';
import MainLayout from './layouts/MainLayout';
import AuthLayout from './layouts/AuthLayout';
import type { User } from './types/auth';

const defaultUser: User = {
  id: '1',
  username: 'admin',
  email: 'admin@stackpulse.io',
  firstName: 'Avery',
  lastName: 'Stone',
  isActive: true,
};

function App() {
  const [session, setSession] = useState<{ user: User | null; token: string | null }>({
    user: localStorage.getItem('stackpulse_user') ? JSON.parse(localStorage.getItem('stackpulse_user') ?? 'null') : defaultUser,
    token: localStorage.getItem('stackpulse_access_token') ?? 'demo-token',
  });

  const isAuthenticated = !!session.token && !!session.user;

  const handleLogin = (user: User, token: string) => {
    setSession({ user, token });
    localStorage.setItem('stackpulse_user', JSON.stringify(user));
    localStorage.setItem('stackpulse_access_token', token);
  };

  const handleLogout = () => {
    setSession({ user: null, token: null });
    localStorage.removeItem('stackpulse_user');
    localStorage.removeItem('stackpulse_access_token');
    localStorage.removeItem('stackpulse_refresh_token');
  };

  useMemo(
    () => ({ user: session.user, token: session.token, isAuthenticated, login: handleLogin, logout: handleLogout }),
    [session.user, session.token, isAuthenticated],
  );

  const ProtectedRoute = ({ children }: { children: ReactNode }) =>
    isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;

  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AuthLayout isAuthenticated={isAuthenticated} />}>
          <Route path="/login" element={<LoginPage onLogin={handleLogin} />} />
        </Route>

        <Route element={<MainLayout user={session.user} onLogout={handleLogout} isAuthenticated={isAuthenticated} />}>
          <Route path="/" element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />} />
          <Route path="/dashboard" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
          <Route path="/users" element={<ProtectedRoute><UsersPage /></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><ProfilePage user={session.user ?? defaultUser} /></ProtectedRoute>} />
          <Route path="/settings" element={<ProtectedRoute><SettingsPage /></ProtectedRoute>} />
        </Route>

        <Route path="/403" element={<ForbiddenPage />} />
        <Route path="/500" element={<ErrorPage />} />
        <Route path="/404" element={<NotFoundPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
