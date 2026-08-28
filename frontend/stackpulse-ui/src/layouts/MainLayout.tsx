import { NavLink, Outlet } from 'react-router-dom';
import type { User } from '../types/auth';
import logo from '../assets/logo.png';
import { useToast } from '../components/ToastContext';
import { useState } from 'react';

interface MainLayoutProps {
  user: User | null;
  onLogout: () => void;
  isAuthenticated: boolean;
}

const navigation = [
  { to: '/dashboard', label: 'Overview', group: 'Workspace' },
  { to: '/dashboard#work', label: 'Work', group: 'Workspace' },
  { to: '/dashboard#delivery', label: 'CI / CD', group: 'Workspace' },
  { to: '/dashboard#infrastructure', label: 'Infrastructure', group: 'Workspace' },
  { to: '/dashboard#security', label: 'Security', group: 'Workspace' },
  { to: '/dashboard#incidents', label: 'Incidents', group: 'Workspace' },
  { to: '/dashboard#knowledge', label: 'Knowledge', group: 'Workspace' },
  { to: '/users', label: 'Users', group: 'Administration' },
  { to: '/profile', label: 'Profile', group: 'Administration' },
  { to: '/settings', label: 'Settings', group: 'Administration' },
];

export default function MainLayout({ user, onLogout, isAuthenticated }: MainLayoutProps) {
  const { showToast } = useToast();
  const [isCollapsed, setIsCollapsed] = useState(false);

  const handleLogout = () => {
    onLogout();
    showToast('You have been logged out.', 'success');
  };

  return (
    <div className={`app-shell ${isCollapsed ? 'sidebar-collapsed' : ''}`}>
      <aside className="sidebar">
        <div className="brand-block">
          <img src={logo} alt="StackPulse logo" className="sidebar-logo" />
          <div className="brand-copy"><h2>StackPulse</h2></div>
          <button className="collapse-button" type="button" onClick={() => setIsCollapsed((value) => !value)} aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'} title={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}><span aria-hidden="true">{isCollapsed ? '›' : '‹'}</span></button>
        </div>

        <nav className="sidebar-nav">
          <p className="nav-group-label">Workspace</p>
          {navigation.filter((item) => item.group === 'Workspace').map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`} title={item.label}>
              <span className="nav-icon" aria-hidden="true">{item.label.slice(0, 1)}</span><span className="nav-label">{item.label}</span>
            </NavLink>
          ))}
          <p className="nav-group-label">Administration</p>
          {navigation.filter((item) => item.group === 'Administration').map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`} title={item.label}><span className="nav-icon" aria-hidden="true">{item.label.slice(0, 1)}</span><span className="nav-label">{item.label}</span></NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="user-chip" title={user?.email ?? 'Account'}>
            <span className="avatar">{user?.firstName?.[0] ?? 'A'}</span>
            <div className="user-copy">
              <strong>{user?.firstName ?? 'Admin'} {user?.lastName ?? 'User'}</strong>
              <small>{user?.email ?? 'admin@stackpulse.io'}</small>
            </div>
          </div>

          {isAuthenticated && (
            <button className="logout-button" onClick={handleLogout}><span aria-hidden="true">↪</span><span className="nav-label">Logout</span></button>
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
