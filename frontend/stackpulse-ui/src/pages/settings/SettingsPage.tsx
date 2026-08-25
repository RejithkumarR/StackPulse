export default function SettingsPage() {
  return (
    <div className="content-panel">
      <div className="panel-card">
        <h3>Workspace settings</h3>
        <div className="settings-list">
          <div>
            <span>Theme</span>
            <strong>Dark Interface</strong>
          </div>
          <div>
            <span>Notifications</span>
            <strong>Enabled</strong>
          </div>
          <div>
            <span>Security</span>
            <strong>MFA enforced</strong>
          </div>
        </div>
      </div>
    </div>
  );
}
