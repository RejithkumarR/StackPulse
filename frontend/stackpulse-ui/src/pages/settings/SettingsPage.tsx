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

const providers = ['Jira', 'Bitbucket', 'GitHub', 'Confluence', 'Azure DevOps', 'Jenkins', 'GitLab', 'AWS', 'Docker', 'Kubernetes', 'MySQL', 'MongoDB', 'Email', 'Microsoft Teams', 'Webex', 'ServiceNow', 'PagerDuty'];
const providerGroups: Record<string, string[]> = {
  'Work & knowledge': ['Jira', 'Bitbucket', 'GitHub', 'Confluence', 'Azure DevOps', 'GitLab'],
  'Delivery & infrastructure': ['Jenkins', 'AWS', 'Docker', 'Kubernetes', 'MySQL', 'MongoDB'],
  'Communication & response': ['Email', 'Microsoft Teams', 'Webex', 'ServiceNow', 'PagerDuty'],
};

export default function SettingsPage() {
  const { showToast } = useToast();
  const [inventory, setInventory] = useState<any | null>(null);
  const [config, setConfig] = useState<MasterConfiguration>({ computers: [], integrations: [] });
  const [computer, setComputer] = useState<ComputerMaster>(emptyComputer);
  const [integration, setIntegration] = useState<IntegrationAccess>(emptyIntegration);
  const [status, setStatus] = useState('');
  const [activeTab, setActiveTab] = useState('Work & knowledge');
  const [showIntegrationModal, setShowIntegrationModal] = useState(false);
  const [pollingMinutes, setPollingMinutes] = useState('15');

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
      setShowIntegrationModal(false);
      setStatus('Integration access saved');
      showToast('Integration access saved.', 'success');
    } catch (error: any) {
      showToast(error?.response?.data?.message ?? 'Unable to save integration access.', 'danger');
    }
  };

  return (
    <div className="content-panel">
      <div className="panel-card settings-hero">
        <div className="section-header">
          <div><p className="eyebrow dark">Control plane</p><h2>Configure your operations workspace</h2><p className="muted">Connect services once. StackPulse keeps the signals, prompts, and secrets organized.</p></div>
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
          <div><span>Automation cadence</span><select className="inline-select" value={pollingMinutes} onChange={(e) => setPollingMinutes(e.target.value)}><option value="5">Every 5 minutes</option><option value="15">Every 15 minutes</option><option value="30">Every 30 minutes</option><option value="60">Every hour</option></select></div>
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
          <div className="section-header"><div><h3>Integration access</h3><p className="muted">Credentials stay in the secret store. Only references are saved here.</p></div><button className="primary-button small" type="button" onClick={() => { setIntegration({ ...emptyIntegration, provider: providerGroups[activeTab][0] }); setShowIntegrationModal(true); }}>+ Add service</button></div>
          <div className="settings-tabs">{Object.keys(providerGroups).map((group) => <button type="button" className={activeTab === group ? 'active' : ''} key={group} onClick={() => setActiveTab(group)}>{group}</button>)}</div>
          <div className="compact-list">
            {config.integrations.filter((item) => providerGroups[activeTab].includes(item.provider)).map((item) => (
              <button type="button" key={item.id ?? `${item.provider}-${item.displayName}`} onClick={() => { setIntegration(item); setShowIntegrationModal(true); }}>
                <strong>{item.provider}: {item.displayName}</strong>
                <span>{item.projectKey || item.baseUrl}</span>
              </button>
            ))}
            {!config.integrations.some((item) => providerGroups[activeTab].includes(item.provider)) && <p className="empty-state">No services configured in this group.</p>}
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

      {showIntegrationModal && <div className="modal-backdrop" role="presentation" onMouseDown={() => setShowIntegrationModal(false)}><div className="modal-card" role="dialog" aria-modal="true" aria-labelledby="integration-title" onMouseDown={(event) => event.stopPropagation()}><div className="section-header"><div><p className="eyebrow dark">Service connection</p><h2 id="integration-title">Add integration</h2></div><button className="close-button" type="button" onClick={() => setShowIntegrationModal(false)} aria-label="Close">×</button></div><form className="config-form modal-form" onSubmit={submitIntegration}><label>Provider<select value={integration.provider} onChange={(e) => setIntegration({ ...integration, provider: e.target.value })}>{providers.map((provider) => <option key={provider}>{provider}</option>)}</select></label><label>Display name<input required placeholder="Production Jira" value={integration.displayName} onChange={(e) => setIntegration({ ...integration, displayName: e.target.value })} /></label><label>Base URL<input required placeholder="https://..." value={integration.baseUrl} onChange={(e) => setIntegration({ ...integration, baseUrl: e.target.value })} /></label><label>Project / space / workspace<input placeholder="Optional scope" value={integration.projectKey} onChange={(e) => setIntegration({ ...integration, projectKey: e.target.value })} /></label><label>Username<input placeholder="Optional username" value={integration.username} onChange={(e) => setIntegration({ ...integration, username: e.target.value })} /></label><label>AWS secret reference<input placeholder="secrets/stackpulse/jira" value={integration.secretReference} onChange={(e) => setIntegration({ ...integration, secretReference: e.target.value })} /></label><div className="modal-actions"><button className="secondary-button" type="button" onClick={() => setShowIntegrationModal(false)}>Cancel</button><button className="primary-button small" type="submit">Save service</button></div></form></div></div>}
    </div>
  );
}
