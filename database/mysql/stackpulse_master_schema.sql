CREATE DATABASE IF NOT EXISTS stackpulse_master
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE stackpulse_master;

CREATE TABLE roles (
  id CHAR(36) NOT NULL PRIMARY KEY,
  name VARCHAR(50) NOT NULL,
  description TEXT NULL,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  updated_at DATETIME(6) NULL,
  UNIQUE KEY ux_roles_name (name)
);

CREATE TABLE users (
  id CHAR(36) NOT NULL PRIMARY KEY,
  username VARCHAR(100) NOT NULL,
  email VARCHAR(200) NOT NULL,
  password_hash LONGTEXT NOT NULL,
  first_name VARCHAR(100) NULL,
  last_name VARCHAR(100) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  updated_at DATETIME(6) NULL,
  last_login_at DATETIME(6) NULL,
  role_id CHAR(36) NOT NULL,
  UNIQUE KEY ux_users_username (username),
  UNIQUE KEY ux_users_email (email),
  KEY ix_users_role_id (role_id),
  CONSTRAINT fk_users_roles FOREIGN KEY (role_id) REFERENCES roles(id)
);

CREATE TABLE refresh_tokens (
  id CHAR(36) NOT NULL PRIMARY KEY,
  token LONGTEXT NOT NULL,
  expires_at DATETIME(6) NOT NULL,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  revoked_at DATETIME(6) NULL,
  user_id CHAR(36) NOT NULL,
  KEY ix_refresh_tokens_user_id (user_id),
  CONSTRAINT fk_refresh_tokens_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE menus (
  id CHAR(36) NOT NULL PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  path VARCHAR(160) NOT NULL,
  icon VARCHAR(80) NULL,
  sort_order INT NOT NULL DEFAULT 0,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UNIQUE KEY ux_menus_path (path)
);

CREATE TABLE role_accesses (
  id CHAR(36) NOT NULL PRIMARY KEY,
  role_id CHAR(36) NOT NULL,
  menu_id CHAR(36) NOT NULL,
  can_view TINYINT(1) NOT NULL DEFAULT 1,
  can_create TINYINT(1) NOT NULL DEFAULT 0,
  can_update TINYINT(1) NOT NULL DEFAULT 0,
  can_delete TINYINT(1) NOT NULL DEFAULT 0,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UNIQUE KEY ux_role_accesses_role_menu (role_id, menu_id),
  KEY ix_role_accesses_menu_id (menu_id),
  CONSTRAINT fk_role_accesses_roles FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
  CONSTRAINT fk_role_accesses_menus FOREIGN KEY (menu_id) REFERENCES menus(id) ON DELETE CASCADE
);

CREATE TABLE computer_masters (
  id CHAR(36) NOT NULL PRIMARY KEY,
  hostname VARCHAR(120) NOT NULL,
  asset_tag VARCHAR(80) NULL,
  owner VARCHAR(120) NULL,
  environment VARCHAR(80) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  updated_at DATETIME(6) NULL,
  UNIQUE KEY ux_computer_masters_hostname (hostname)
);

CREATE TABLE integration_accesses (
  id CHAR(36) NOT NULL PRIMARY KEY,
  provider VARCHAR(40) NOT NULL,
  display_name VARCHAR(160) NOT NULL,
  base_url VARCHAR(300) NOT NULL,
  project_key VARCHAR(160) NULL,
  username VARCHAR(200) NULL,
  secret_reference VARCHAR(500) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  updated_at DATETIME(6) NULL,
  UNIQUE KEY ux_integration_accesses_provider_name (provider, display_name)
);
