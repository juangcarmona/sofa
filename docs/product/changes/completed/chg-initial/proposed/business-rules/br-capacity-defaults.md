---
id: BR-CAPACITY-DEFAULTS
type: business-rule
title: Capacity combines live state and project limits
status: draft
applies-to: [BC-PORTFOLIO-PROJECT-CONFIGURATION, BC-SCHEDULING]
uses-terms: [TERM-PROJECT, TERM-EXECUTION-RUN, TERM-SCHEDULING-DECISION]
---

## Rule

SoFa MUST measure capacity in concurrent work-unit executions and derive live capacity from worker availability and active leases. It MUST apply a project-configurable capacity limit, or a shared default of one active execution per project when no project override exists. A project override MUST NOT exceed live global worker capacity. Scheduling MUST record the capacity inputs and effective limit used for each decision.

The planning default has no project override enabled. A project override is explicit configuration and remains bounded by healthy global worker capacity.

## Rationale

Live runtime state prevents dispatching work that cannot run, while a policy limit prevents one project from consuming all personal capacity.

## Examples

A project with no override allows one active execution and remains bounded by currently available workers. A project with an override may allow more or fewer executions, but never beyond live worker availability or active lease constraints.

## Exceptions

An explicitly accepted scheduling policy may change the limit, but it must record the effective value and its provenance.
