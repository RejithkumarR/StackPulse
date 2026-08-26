import { Navigate, Outlet } from 'react-router-dom';
import type { CSSProperties } from 'react';
import backgroundImage from '../assets/Background.png';

interface AuthLayoutProps {
  isAuthenticated: boolean;
}

export default function AuthLayout({ isAuthenticated }: AuthLayoutProps) {
  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="auth-layout" style={{ '--auth-bg-image': `url(${backgroundImage})` } as CSSProperties}>
      <div className="auth-shell">
        <Outlet />
      </div>
    </div>
  );
}
