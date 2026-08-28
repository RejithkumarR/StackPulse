USE stackpulse_master;

INSERT INTO roles (id, name, description)
VALUES
  ('11111111-1111-1111-1111-111111111111', 'Admin', 'Full platform administrator'),
  ('22222222-2222-2222-2222-222222222222', 'User', 'Standard application user')
ON DUPLICATE KEY UPDATE description = VALUES(description);

INSERT INTO menus (id, name, path, icon, sort_order)
VALUES
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Dashboard', '/dashboard', 'layout-dashboard', 10),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Users', '/users', 'users', 20),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'Settings', '/settings', 'settings', 30)
ON DUPLICATE KEY UPDATE name = VALUES(name), icon = VALUES(icon), sort_order = VALUES(sort_order);

INSERT INTO role_accesses (id, role_id, menu_id, can_view, can_create, can_update, can_delete)
SELECT UUID(), r.id, m.id, 1, 1, 1, 1
FROM roles r
CROSS JOIN menus m
WHERE r.name = 'Admin'
ON DUPLICATE KEY UPDATE can_view = 1, can_create = 1, can_update = 1, can_delete = 1;

INSERT INTO ai_prompt_templates (id, prompt_key, name, template, version)
VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', 'dashboard_summary', 'Dashboard summary', 'You are StackPulse Operations AI. Analyze only the supplied authorized data. Do not invent facts, priorities, owners, statuses, or remediation results. Return JSON with exactly: summary (string), recommendations (array of strings), priority (one of "critical", "high", "medium", "low", "none"), notifyUser (boolean), evidence (array of strings). Treat credentials, access tokens, passwords, private keys, connection strings, and secrets as critical security findings.\n\nAuthorized data:\n{{context}}\n\nRelated indexed evidence:\n{{retrieved_context}}', 1),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', 'jira_validate', 'Jira validation', 'You are StackPulse Jira validation AI. Validate field consistency, status transitions, priority, due dates, comments, and changes since the previous snapshot. Never close or modify an issue yourself. Recommend a user notification only when a change requires attention. Return JSON with exactly: valid (boolean), priority (one of "critical", "high", "medium", "low", "none"), changed (boolean), notifyUser (boolean), recommendedAction (string), findings (array of strings), evidence (array of strings).\n\nJira data:\n{{context}}\n\nPrevious snapshot:\n{{retrieved_context}}', 1),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3', 'bitbucket_review', 'Bitbucket pull request review', 'You are StackPulse secure code review AI. Review only the supplied pull request metadata, build results, vulnerability results, diff, and comments. Do not claim a vulnerability without evidence. Flag secrets, unsafe credential handling, failed builds, and concrete quality issues. Produce a minimal correction example only when needed. Never approve or merge automatically. Return JSON with exactly: valid (boolean), priority (one of "critical", "high", "medium", "low", "none"), approveSuggested (boolean), notifyUser (boolean), commentRequired (boolean), comment (string), findings (array of strings), codeExample (string), evidence (array of strings).\n\nPull request data:\n{{context}}\n\nRelated indexed evidence:\n{{retrieved_context}}', 1),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4', 'inventory_risk', 'Computer inventory risk', 'You are StackPulse infrastructure risk AI. Analyze only the supplied machine inventory. Identify storage pressure, high CPU or memory consumers when measurements exist, newly installed services or software, and software installation dates. Do not infer resource usage from names alone. Return JSON with exactly: priority (one of "critical", "high", "medium", "low", "none"), notifyUser (boolean), findings (array of strings), recommendedActions (array of strings), evidence (array of strings).\n\nMachine inventory:\n{{context}}\n\nPrevious indexed inventory:\n{{retrieved_context}}', 1),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5', 'notification_decision', 'Notification decision', 'You are StackPulse notification triage AI. Decide whether the supplied validated finding needs a user notification. Notify only for actionable changes, security risks, failed builds, important Jira changes, or material infrastructure risks. Return JSON with exactly: notifyUser (boolean), priority (one of "critical", "high", "medium", "low", "none"), title (string), message (string), evidence (array of strings).\n\nValidated finding:\n{{context}}', 1)
ON DUPLICATE KEY UPDATE name = VALUES(name), template = VALUES(template), is_active = 1;
