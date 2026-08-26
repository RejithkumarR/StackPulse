import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import {
  getMasterConfiguration,
  saveComputerMaster,
  saveIntegrationAccess,
  type ComputerMaster,
  type IntegrationAccess,
  type MasterConfiguration,
} from '../../services/masterConfigurationService';
import { getLatestInventory } from '../../services/systemInventoryService';
import { useToast } from '../../components/ToastContext';

const emptyComputer: ComputerMaster = { hostname: '', assetTag: '', owner: '', environment: '', isActive: true };
const emptyIntegration: IntegrationAccess = {
  provider: 'Jira',
  displayName: '',
  baseUrl: '',
  projectKey: '',
  username: '',
  secretReference: '',
  isActive: true,
};

export default function SettingsPage() {
  const { showToast } = useToast();
  const [inventory, setInventory] = useState<any | null>(null);
  const [config, setConfig] = useState<MasterConfiguration>({ computers: [], integrations: [] });
  const [computer, setComputer] = useState<ComputerMaster>(emptyComputer);
  const [integration, setIntegration] = useState<IntegrationAccess>(emptyIntegration);
  const [status, setStatus] = useState('');

  useEffect(() => {
    getLatestInventory().then(setInventory).catch(() => setInventory(null));
    getMasterConfiguration().then(setConfig).catch(() => setConfig({ computers: [], integrations: [] }));
  }, []);

  const submitComputer = async (event: FormEvent) => {
    event.preventDefault();
    try {
      const saved = await saveComputerMaster(computer);
      setConfig((current) => ({
        ...current,
        computers: [saved, ...current.computers.filter((item) => item.id !== saved.id)],
      }));
      setComputer(emptyComputer);
      setStatus('Computer master saved');
      showToast('Computer master saved.', 'success');
    } catch (error: any) {
      showToast(error?.response?.data?.message ?? 'Unable to save computer master.', 'danger');
    }
  };

  const submitIntegration = async (event: FormEvent) => {
    event.preventDefault();
    try {
      const saved = await saveIntegrationAccess(integration);
      setConfig((current) => ({
        ...current,
        integrations: [saved, ...current.integrations.filter((item) => item.id !== saved.id)],
      }));
      setIntegration(emptyIntegration);
      setStatus('Integration access saved');
      showToast('Integration access saved.', 'success');
    } catch (error: any) {
      showToast(error?.response?.data?.message ?? 'Unable to save integration access.', 'danger');
    }
  };

  return (
    <div className="content-panel">
      <div className="panel-card">
        <div className="section-header">
          <h3>Master configuration</h3>
          {status ? <span className="status good">{status}</span> : null}
        </div>
        <div className="settings-list">
          <div>
            <span>Authentication database</span>
            <strong>MySQL</strong>
          </div>
          <div>
            <span>Audit and application logs</span>
            <strong>MongoDB</strong>
          </div>
          <div>
            <span>Connection source</span>
            <strong>AWS Secrets Manager</strong>
          </div>
        </div>
      </div>

      <div className="two-column-layout">
        <div className="panel-card">
          <h3>Computer master</h3>
          <form className="config-form" onSubmit={submitComputer}>
            <input required placeholder="Hostname" value={computer.hostname} onChange={(e) => setComputer({ ...computer, hostname: e.target.value })} />
            <input placeholder="Asset tag" value={computer.assetTag} onChange={(e) => setComputer({ ...computer, assetTag: e.target.value })} />
            <input placeholder="Owner" value={computer.owner} onChange={(e) => setComputer({ ...computer, owner: e.target.value })} />
            <input placeholder="Environment" value={computer.environment} onChange={(e) => setComputer({ ...computer, environment: e.target.value })} />
            <button className="primary-button small" type="submit">Save computer</button>
          </form>
          <div className="compact-list">
            {config.computers.map((item) => (
              <button type="button" key={item.id ?? item.hostname} onClick={() => setComputer(item)}>
                <strong>{item.hostname}</strong>
                <span>{item.owner || 'Unassigned'} / {item.environment || 'No environment'}</span>
              </button>
            ))}
          </div>
        </div>

        <div className="panel-card">
          <h3>Integration access</h3>
          <form className="config-form" onSubmit={submitIntegration}>
            <select value={integration.provider} onChange={(e) => setIntegration({ ...integration, provider: e.target.value })}>
              <option>Jira</option>
              <option>Confluence</option>
              <option>Bitbucket</option>
            </select>
            <input required placeholder="Display name" value={integration.displayName} onChange={(e) => setIntegration({ ...integration, displayName: e.target.value })} />
            <input required placeholder="Base URL" value={integration.baseUrl} onChange={(e) => setIntegration({ ...integration, baseUrl: e.target.value })} />
            <input placeholder="Project / space / workspace key" value={integration.projectKey} onChange={(e) => setIntegration({ ...integration, projectKey: e.target.value })} />
            <input placeholder="Username" value={integration.username} onChange={(e) => setIntegration({ ...integration, username: e.target.value })} />
            <input placeholder="AWS secret reference" value={integration.secretReference} onChange={(e) => setIntegration({ ...integration, secretReference: e.target.value })} />
            <button className="primary-button small" type="submit">Save access</button>
          </form>
          <div className="compact-list">
            {config.integrations.map((item) => (
              <button type="button" key={item.id ?? `${item.provider}-${item.displayName}`} onClick={() => setIntegration(item)}>
                <strong>{item.provider}: {item.displayName}</strong>
                <span>{item.projectKey || item.baseUrl}</span>
              </button>
            ))}
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
