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
