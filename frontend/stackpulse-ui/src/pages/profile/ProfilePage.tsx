import type { User } from '../../types/auth';

export default function ProfilePage({ user }: { user: User }) {
  return (
    <div className="content-panel">
      <div className="panel-card profile-card">
        <div className="profile-header">
          <div className="avatar large">{user.firstName?.[0] ?? 'A'}</div>
          <div>
            <h3>{user.firstName} {user.lastName}</h3>
            <p>{user.email}</p>
          </div>
        </div>

        <div className="profile-grid">
          <div><label>Username</label><span>{user.username}</span></div>
          <div><label>Role</label><span>Administrator</span></div>
          <div><label>Region</label><span>US-East</span></div>
          <div><label>Status</label><span className="status good">Active</span></div>
        </div>
      </div>
    </div>
  );
}
