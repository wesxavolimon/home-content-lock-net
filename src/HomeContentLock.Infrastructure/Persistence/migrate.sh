#!/bin/bash
# Simple migration script to apply schema.sql to SQLite database

DB_PATH="${1:-blocker.db}"

echo "Applying schema to $DB_PATH..."
sqlite3 "$DB_PATH" < schema.sql
echo "Migration complete."