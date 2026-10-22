---
id: CON-FACTORY-RUNTIME
type: constraint
title: Begin with one modular .NET and Aspire runtime
status: draft
applies-to: [BC-EXECUTION, BC-SCHEDULING, BC-DELIVERY, BC-RUNTIME-OPERATIONS]
---

## Constraint

The initial factory runtime MUST use one modular .NET 10 application composed by Aspire, PostgreSQL for minimum durable control-plane state, an independently supervised Worker, and OpenCode as an external execution engine. It MUST defer brokers, microservices, and generic plugin infrastructure.

## Rationale

The first slice should exercise a complete recoverable loop before the system expands its topology.

## Consequences

The runtime must define narrow module ports and durable concurrency behavior. Exact SDK, Aspire, database, Worker, and OpenCode versions remain implementation decisions after acceptance.
