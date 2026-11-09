CREATE TABLE IF NOT EXISTS pilot_schema(version INTEGER PRIMARY KEY, migration TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS pilot_downloads(
  id TEXT PRIMARY KEY,station TEXT NOT NULL,ied TEXT NOT NULL,remote_cfg TEXT NOT NULL,
  state TEXT NOT NULL CHECK(state IN('Discovered','Downloading','Complete','Failed')),
  attempts INTEGER NOT NULL DEFAULT 0,max_attempts INTEGER NOT NULL CHECK(max_attempts BETWEEN 1 AND 10),
  lease TEXT,lease_until TEXT,next_attempt TEXT NOT NULL,error_code TEXT,record_id TEXT REFERENCES pilot_records(id) DEFERRABLE INITIALLY DEFERRED);
CREATE TABLE IF NOT EXISTS pilot_records(
  id TEXT PRIMARY KEY,manifest TEXT NOT NULL,manifest_sha TEXT NOT NULL,directory TEXT NOT NULL,files_digest TEXT NOT NULL,
  state TEXT NOT NULL CHECK(state IN('Ready','Uploading','Acknowledged','Failed')),
  attempts INTEGER NOT NULL DEFAULT 0,max_attempts INTEGER NOT NULL CHECK(max_attempts BETWEEN 1 AND 10),
  lease TEXT,lease_until TEXT,next_attempt TEXT NOT NULL,error_code TEXT,ack TEXT);
CREATE TABLE IF NOT EXISTS pilot_attempts(
  id INTEGER PRIMARY KEY,kind TEXT NOT NULL,job_id TEXT NOT NULL,attempt INTEGER NOT NULL,
  lease TEXT NOT NULL,started TEXT NOT NULL,result TEXT NOT NULL,UNIQUE(kind,job_id,attempt));
CREATE INDEX IF NOT EXISTS pilot_downloads_due ON pilot_downloads(state,next_attempt);
CREATE INDEX IF NOT EXISTS pilot_records_due ON pilot_records(state,next_attempt);
CREATE TABLE IF NOT EXISTS pilot_capabilities(id TEXT PRIMARY KEY,station TEXT NOT NULL,ied TEXT NOT NULL,
  observation TEXT NOT NULL,source_kind TEXT NOT NULL,observed TEXT NOT NULL);
CREATE INDEX IF NOT EXISTS pilot_capabilities_ied ON pilot_capabilities(station,ied,observed);
INSERT OR IGNORE INTO pilot_schema VALUES(1,'001_PilotTransferQueue');
