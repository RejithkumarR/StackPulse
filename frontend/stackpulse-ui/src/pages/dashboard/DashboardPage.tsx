import { useEffect, useState } from 'react';
import { dashboardService, type ActivityItem, type DashboardSummary } from '../../services/dashboardService';
import { getLatestJira, getLatestBitbucket, getLatestGitHub } from '../../services/integrationService';
import { getLatestInventory } from '../../services/systemInventoryService';

type IntegrationItem = Record<string, any>;

export default function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [activity, setActivity] = useState<ActivityItem[]>([]);
  const [jira, setJira] = useState<IntegrationItem[]>([]);
  const [pullRequests, setPullRequests] = useState<IntegrationItem[]>([]);
  const [githubPullRequests, setGithubPullRequests] = useState<IntegrationItem[]>([]);
  const [inventory, setInventory] = useState<IntegrationItem | null>(null);

  useEffect(() => {
    dashboardService.getDashboard().then((data) => { setSummary(data.summary); setActivity(data.activity); }).catch(() => undefined);
    getLatestJira().then(setJira).catch(() => undefined);
    getLatestBitbucket().then(setPullRequests).catch(() => undefined);
    getLatestGitHub().then(setGithubPullRequests).catch(() => undefined);
    getLatestInventory().then(setInventory).catch(() => undefined);
  }, []);

  const drives = inventory?.drives ?? [];
  const storagePercent = drives.length ? Math.round((1 - (drives[0].freeBytes ?? 0) / Math.max(drives[0].totalBytes ?? 1, 1)) * 100) : 0;
  const cards = [
    { label: 'Jira projects', value: summary?.jiraProjectCount ?? '--', detail: summary?.jiraProjects?.join(', ') || 'Waiting for sync', tone: 'blue' },
    { label: 'Bitbucket repos', value: summary?.bitbucketRepositoryCount ?? '--', detail: summary?.bitbucketRepositories?.join(', ') || 'Waiting for sync', tone: 'green' },
    { label: 'Security signals', value: summary?.alerts ?? '--', detail: 'AI triage pending', tone: 'red' },
    { label: 'Storage used', value: inventory ? `${storagePercent}%` : '--', detail: inventory?.hostname ?? 'Awaiting inventory', tone: 'amber' },
  ];

  return (
    <div className="content-panel dashboard-page">
      <section className="page-heading"><div><p className="eyebrow dark">Engineering operations</p><h2>Good morning. Here is what needs you.</h2><p className="muted">A unified view across work, delivery, infrastructure, and risk.</p></div><span className="last-sync">Live workspace <i /></span></section>
      <section className="overview-grid">{cards.map((card) => <div className={`metric-card accent-${card.tone}`} key={card.label}><span>{card.label}</span><strong>{card.value}</strong><small>{card.detail}</small></div>)}</section>
      <section className="dashboard-grid inventory-summary"><div className="panel-card"><div className="section-header"><div><h3>Connected project details</h3><p className="muted">Projects found in the latest Jira snapshot</p></div><span className="status good">{summary?.jiraProjectCount ?? 0} total</span></div><div className="tag-list">{summary?.jiraProjects?.map((project) => <span key={project}>{project}</span>)}{!summary?.jiraProjects?.length && <p className="empty-state">No project data has been synchronized yet.</p>}</div></div><div className="panel-card"><div className="section-header"><div><h3>Repository details</h3><p className="muted">Repositories found in the latest Bitbucket snapshot</p></div><span className="status good">{summary?.bitbucketRepositoryCount ?? 0} total</span></div><div className="tag-list">{summary?.bitbucketRepositories?.map((repo) => <span key={repo}>{repo}</span>)}{!summary?.bitbucketRepositories?.length && <p className="empty-state">No repository data has been synchronized yet.</p>}</div></div></section>
      <section className="dashboard-grid">
        <div className="panel-card chart-panel"><div className="section-header"><div><h3>Operations pulse</h3><p className="muted">Activity volume across the last 7 days</p></div><span className="status good">Stable</span></div><div className="pulse-chart" aria-label="Operations activity trend"><div className="chart-line" /><div className="chart-bars">{[36, 48, 42, 68, 54, 78, 64, 88, 70, 82, 76, 96].map((height, index) => <span key={index} style={{ height: `${height}%` }} />)}</div></div><div className="chart-labels"><span>Mon</span><span>Tue</span><span>Wed</span><span>Thu</span><span>Fri</span><span>Sat</span><span>Sun</span></div></div>
        <div className="panel-card"><div className="section-header"><h3>Service health</h3><span className="muted">Updated now</span></div><div className="health-list">{['StackPulse API', 'Jira sync', 'Bitbucket sync', 'Inventory agent', 'AI analysis'].map((name, index) => <div className="health-row" key={name}><span><i className={index === 3 && !inventory ? 'health-dot warn' : 'health-dot'} />{name}</span><strong>{index === 3 && !inventory ? 'Waiting' : 'Healthy'}</strong></div>)}</div></div>
      </section>
      <section className="dashboard-grid lower-grid">
        <div className="panel-card"><div className="section-header"><h3>Work requiring attention</h3><a href="/settings">Configure</a></div><div className="work-list">{jira.slice(0, 4).map((item, index) => <div className="work-row" key={item.id ?? item.key ?? index}><span className="source-mark jira-mark">J</span><div><strong>{item.key ?? item.summary ?? 'Jira issue'}</strong><p>{item.summary ?? item.status ?? 'Open work item'}</p></div><span className="status-badge inactive">{item.status ?? 'Open'}</span></div>)}{!jira.length && <p className="empty-state">No Jira items have been synchronized yet.</p>}</div></div>
        <div className="panel-card"><div className="section-header"><h3>Risk radar</h3><span className="status warn">Review</span></div><div className="risk-meter"><div className="risk-ring"><strong>{summary?.alerts ?? 0}</strong><small>signals</small></div><div><p><b>Security</b> Credentials and vulnerability findings</p><p><b>Infrastructure</b> Storage, CPU, and service changes</p></div></div><button className="text-button" type="button">Open risk center →</button></div>
      </section>
      <section className="dashboard-grid lower-grid"><div className="panel-card"><div className="section-header"><h3>Pull request queue</h3><span className="muted">{pullRequests.length} detected</span></div>{pullRequests.slice(0, 3).map((item, index) => <div className="work-row" key={item.id ?? index}><span className="source-mark bb-mark">↗</span><div><strong>{item.title ?? 'Pull request'}</strong><p>{item.repo ?? item.author ?? 'Bitbucket'}</p></div><span className="status-badge active">{item.state ?? 'Open'}</span></div>)}{!pullRequests.length && <p className="empty-state">No pull requests have been synchronized yet.</p>}</div><div className="panel-card"><div className="section-header"><h3>GitHub queue</h3><span className="muted">{githubPullRequests.length} detected</span></div>{githubPullRequests.slice(0, 3).map((item, index) => <div className="work-row" key={item.id ?? index}><span className="source-mark gh-mark">GH</span><div><strong>#{item.number ?? ''} {item.title ?? 'Pull request'}</strong><p>{item.repository ?? item.author ?? 'GitHub'}</p></div><span className="status-badge active">{item.state ?? 'open'}</span></div>)}{!githubPullRequests.length && <p className="empty-state">No GitHub pull requests have been synchronized yet.</p>}</div></section>
      <section className="panel-card"><div className="section-header"><h3>Recent activity</h3><span className="muted">Audit stream</span></div><div className="mini-activity">{activity.slice(0, 4).map((item) => <div key={item.id}><i className="activity-dot" /><span><strong>{item.action}</strong><small>{item.details}</small></span><time>{new Date(item.createdAt).toLocaleDateString()}</time></div>)}{!activity.length && <p className="empty-state">No recent activity has been recorded.</p>}</div></section>
    </div>
  );
}
