import { useEffect, useState } from 'react';
import { getLatestInventory } from '../../services/systemInventoryService';

export default function SettingsPage() {
  const [inventory, setInventory] = useState<any | null>(null);

  useEffect(() => {
    getLatestInventory().then(setInventory).catch(() => setInventory(null));
  }, []);

  return (
    <div className="content-panel">
      <div className="panel-card">
        <h3>Workspace settings</h3>
        <div className="settings-list">
          <div>
            <span>Theme</span>
            <strong>Black / Sky Blue</strong>
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

      <div className="panel-card">
        <h3>System Inventory (latest)</h3>
        {inventory ? (
          <div>
            <p><strong>Host:</strong> {inventory.hostname}</p>
            <p><strong>OS:</strong> {inventory.osVersion}</p>
            <p><strong>Collected:</strong> {new Date(inventory.collectedAt).toLocaleString()}</p>

            <h4>Drives</h4>
            <ul>
              {inventory.drives?.map((d: any) => (
                <li key={d.id}>{d.name} — {d.driveType} — {(d.freeBytes ?? 0).toLocaleString()} free / {(d.totalBytes ?? 0).toLocaleString()} total</li>
              ))}
            </ul>

            <h4>Installed Software (sample)</h4>
            <ul>
              {inventory.installedSoftwares?.slice(0, 10).map((s: any) => (
                <li key={s.id}>{s.name} {s.version ? `— ${s.version}` : ''}</li>
              ))}
            </ul>

            <h4>Windows Services (sample)</h4>
            <ul>
              {inventory.windowsServices?.slice(0, 10).map((s: any) => (
                <li key={s.id}>{s.displayName ?? s.serviceName} — {s.state}</li>
              ))}
            </ul>
          </div>
        ) : (
          <p>No inventory available yet.</p>
        )}
      </div>
    </div>
  );
}
