import { Navigate, Outlet } from 'react-router-dom';

interface AuthLayoutProps {
  isAuthenticated: boolean;
}

export default function AuthLayout({ isAuthenticated }: AuthLayoutProps) {
  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="auth-layout">
      <div className="auth-shell">
        <Outlet />
      </div>
    </div>
  );
}
