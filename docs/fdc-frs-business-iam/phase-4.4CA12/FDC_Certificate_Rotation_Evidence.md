# Certificate rotation

BLOCKED. The run verifies strict revocation for one bound certificate. It does not exercise certificate rollover, persisted successor binding or old-certificate denial after a cutover. Required evidence includes old/new fingerprints, expected binding revision, prior and successor requests, and SQL before/after with audit/outbox.
