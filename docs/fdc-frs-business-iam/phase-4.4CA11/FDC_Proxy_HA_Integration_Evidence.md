# Proxy and HA

BLOCKED for multi-node acceptance. Build two isolated Center processes, shared disposable IAM SQL and a protected test key ring. Proxy must strip client-supplied identity headers and accept forwarded identity only from an explicit authenticated peer. Assert direct backend bypass and forged client-cert forwarding rejected. Establish a session on node 1, fail to node 2, revoke and assert both deny without restart. Add fenced SQL commit and audit/outbox correlations. No shared environment changes are authorized.
