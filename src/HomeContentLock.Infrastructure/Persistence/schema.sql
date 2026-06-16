CREATE TABLE IF NOT EXISTS blocker_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp TEXT NOT NULL,
    action TEXT NOT NULL,
    status TEXT NOT NULL,
    details TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS blocker_config (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

INSERT OR IGNORE INTO blocker_config VALUES ('initialized', '1');
INSERT OR IGNORE INTO blocker_config VALUES ('current_status', 'DISABLED');
INSERT OR IGNORE INTO blocker_config VALUES ('last_password_change', '2026-06-15T00:00:00Z');