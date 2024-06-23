#!/bin/bash

gzip -k -d /tmp/dump.sql.gz

sudo -i -u postgres << EOF

EOF

dropdb "backup_db"
createdb backup_db
psql -d backup_db < /tmp/dump.sql

psql -d backup_db -c "SELECT ssn FROM criminal_records WHERE status='alive'" -o /tmp/ssn.txt