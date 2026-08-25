import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { User } from '../../types/auth';

interface LoginPageProps {
  onLogin: (user: User, token: string) => void;
}

export default function LoginPage({ onLogin }: LoginPageProps) {
  const navigate = useNavigate();
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('password123');

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    const demoUser: User = {
      id: 'demo-user-1',
      username,
      email: `${username}@stackpulse.io`,
      firstName: 'Demo',
      lastName: 'Operator',
      isActive: true,
    };

    const demoToken = 'demo-access-token';
    onLogin(demoUser, demoToken);
    navigate('/dashboard');
  };

  return (
    <div className="login-panel">
      <div className="login-brand">
        <div className="brand-mark large">S</div>
        <span>StackPulse</span>
      </div>

      <h2>Welcome back</h2>
      <p>Sign in to your operations workspace.</p>

      <form onSubmit={handleSubmit} className="login-form">
        <label>
          Username
          <input value={username} onChange={(e) => setUsername(e.target.value)} />
        </label>

        <label>
          Password
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </label>

        <button type="submit" className="primary-button">Log in</button>
      </form>
    </div>
  );
}
