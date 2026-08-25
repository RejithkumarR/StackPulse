import { useEffect, useState } from 'react';
import { dashboardService, type ActivityItem, type DashboardSummary } from '../../services/dashboardService';

export default function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [activity, setActivity] = useState<ActivityItem[]>([]);

  useEffect(() => {
    dashboardService.getDashboard().then((data) => {
      setSummary(data.summary);
      setActivity(data.activity);
    });
  }, []);

  const cards = summary
    ? [
        { label: 'Total users', value: summary.totalUsers, trend: '+12.4%' },
        { label: 'Active sessions', value: summary.activeSessions, trend: '+8.1%' },
        { label: 'Alerts', value: summary.alerts, trend: '-2' },
        { label: 'System health', value: summary.systemHealth, trend: '99.98%' },
      ]
    : [];

  return (
    <div className="content-panel">
      <section className="overview-grid">
        {cards.map((card) => (
          <div key={card.label} className="metric-card">
            <span>{card.label}</span>
            <strong>{card.value}</strong>
            <small>{card.trend}</small>
          </div>
        ))}
      </section>

      <section className="two-column-layout">
        <div className="panel-card">
          <h3>Recent activity</h3>
          <ul className="activity-list">
            {activity.map((item) => (
              <li key={item.id}>
                <div className="activity-dot" />
                <div>
                  <strong>{item.action}</strong>
                  <p>{item.details}</p>
                </div>
                <span>{new Date(item.createdAt).toLocaleDateString()}</span>
              </li>
            ))}
          </ul>
        </div>

        <div className="panel-card">
          <h3>System status</h3>
          <div className="status-block">
            <div className="status-row">
              <span>API</span>
              <span className="status good">Healthy</span>
            </div>
            <div className="status-row">
              <span>Database</span>
              <span className="status good">Connected</span>
            </div>
            <div className="status-row">
              <span>Queue</span>
              <span className="status warn">Monitoring</span>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
