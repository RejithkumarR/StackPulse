import { useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../../services/authService';
import logo from '../../assets/logo.png';
import type { User } from '../../types/auth';
import { useToast } from '../../components/ToastContext';

interface LoginPageProps {
  onLogin: (user: User, token: string) => void;
}

type AuthMode = 'login' | 'signup' | 'forgot';

export default function LoginPage({ onLogin }: LoginPageProps) {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [isUnlocked, setIsUnlocked] = useState(false);
  const [mode, setMode] = useState<AuthMode>('login');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [message, setMessage] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setIsSubmitting(true);
    setMessage('');

    try {
      if (mode === 'login') {
        const response = await authService.login({ username, password });
        onLogin(response.user, response.accessToken);
        showToast('Login successful.', 'success');
        navigate('/dashboard');
      } else if (mode === 'signup') {
        const response = await authService.signup({ username, email, password, firstName, lastName });
        onLogin(response.user, response.accessToken);
        showToast('Account created successfully.', 'success');
        navigate('/dashboard');
      } else {
        await authService.forgotPassword(email);
        setMessage('Password reset request recorded.');
        showToast('Password reset request sent.', 'success');
      }
    } catch (error: any) {
      const errorMessage = error?.response?.data?.message ?? 'Unable to complete the request.';
      setMessage(errorMessage);
      showToast(errorMessage, 'danger');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isUnlocked) {
    return (
      <div className="lock-panel">
        <button className="lock-button" type="button" onClick={() => { setIsUnlocked(true); showToast('Login unlocked.', 'info'); }} aria-label="Unlock login">
          <span className="lock-shackle" />
          <span className="lock-body" />
        </button>
        <img src={logo} alt="StackPulse logo" className="login-logo" />
        <p>Unlock to continue</p>
      </div>
    );
  }

  return (
    <div className="login-panel">
      <div className="login-brand">
        <img src={logo} alt="StackPulse logo" className="login-logo small" />
        <button className="unlock-mark" type="button" onClick={() => { setIsUnlocked(false); showToast('Login locked.', 'info'); }} aria-label="Lock login">
          <span className="unlock-shackle" />
          <span className="unlock-body" />
        </button>
        <span>StackPulse</span>
      </div>

      <h2>{mode === 'login' ? 'Welcome back' : mode === 'signup' ? 'Create account' : 'Reset password'}</h2>
      <p>{mode === 'login' ? 'Sign in to your operations workspace.' : mode === 'signup' ? 'Create your StackPulse access.' : 'Enter your email to request reset help.'}</p>

      <div className="auth-tabs">
        <button type="button" className={mode === 'login' ? 'active' : ''} onClick={() => { setMode('login'); showToast('Login form selected.', 'info'); }}>Login</button>
        <button type="button" className={mode === 'signup' ? 'active' : ''} onClick={() => { setMode('signup'); showToast('Signup form selected.', 'info'); }}>Signup</button>
        <button type="button" className={mode === 'forgot' ? 'active' : ''} onClick={() => { setMode('forgot'); showToast('Password reset form selected.', 'info'); }}>Forgot</button>
      </div>

      <form onSubmit={handleSubmit} className="login-form">
        {mode !== 'forgot' && (
          <label>
            Username
            <input required value={username} onChange={(e) => setUsername(e.target.value)} />
          </label>
        )}

        {mode !== 'login' && (
          <label>
            Email
            <input required type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
          </label>
        )}

        {mode === 'signup' && (
          <div className="name-grid">
            <label>
              First name
              <input value={firstName} onChange={(e) => setFirstName(e.target.value)} />
            </label>
            <label>
              Last name
              <input value={lastName} onChange={(e) => setLastName(e.target.value)} />
            </label>
          </div>
        )}

        {mode !== 'forgot' && (
          <label>
            Password
            <input required minLength={6} type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
          </label>
        )}

        {message ? <div className="form-message">{message}</div> : null}

        <button type="submit" className="primary-button" disabled={isSubmitting}>
          {isSubmitting ? 'Please wait' : mode === 'login' ? 'Log in' : mode === 'signup' ? 'Sign up' : 'Send reset request'}
        </button>
      </form>
    </div>
  );
}
