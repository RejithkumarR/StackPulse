import { NavLink, Outlet } from 'react-router-dom';
import type { User } from '../types/auth';
import logo from '../assets/logo.png';

interface MainLayoutProps {
  user: User | null;
  onLogout: () => void;
  isAuthenticated: boolean;
}

const navigation = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/users', label: 'Users' },
  { to: '/profile', label: 'Profile' },
  { to: '/settings', label: 'Settings' },
];

export default function MainLayout({ user, onLogout, isAuthenticated }: MainLayoutProps) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-block">
          <div className="brand-mark">S</div>
          <div>
            <h2>StackPulse</h2>
          </div>
        </div>

        <nav className="sidebar-nav">
          {navigation.map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="user-chip">
            <span className="avatar">{user?.firstName?.[0] ?? 'A'}</span>
            <div>
              <strong>{user?.firstName ?? 'Admin'} {user?.lastName ?? 'User'}</strong>
              <small>{user?.email ?? 'admin@stackpulse.io'}</small>
            </div>
          </div>

          {isAuthenticated && (
            <button className="logout-button" onClick={onLogout}>Logout</button>
          )}
        </div>
      </aside>

      <main className="main-panel">
        <header className="topbar">
          <div className="topbar-brand">
            <img src={logo} alt="StackPulse logo" className="app-logo header-logo" />
            <div>
            <p className="eyebrow">Operations Center</p>
            <h1>StackPulse Enterprise</h1>
            </div>
          </div>
          <div className="topbar-actions">
            <div className="search-box">Search</div>
            <div className="profile-pill">
              <span className="avatar small">{user?.firstName?.[0] ?? 'A'}</span>
              <span>{user?.username ?? 'admin'}</span>
            </div>
          </div>
        </header>

        <Outlet />

        <footer className="app-footer">
          <span className="footer-brand">
            <img src={logo} alt="StackPulse logo" className="app-logo footer-logo" />
            StackPulse
          </span>
          <span>Enterprise operations platform</span>
        </footer>
      </main>
    </div>
  );
}
