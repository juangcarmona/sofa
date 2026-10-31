---
id: ACT-IDENTITY-PROVIDER
type: actor
title: External factory identity provider
status: draft
actor-kind: external-system
---

## Purpose

Authenticate the person accessing the factory through the configured external identity authority.

## Goals

Provide a verifiable identity without SoFa operating its own password directory.

## Responsibilities

Supply identity and authentication evidence within the configured trust boundary, and make authentication failures explicit.

## Boundaries

Provider identity does not establish GitHub repository access, permission to request a stage, a human approval or domain authority in another application. SoFa remains responsible for admitting only its configured owner and protecting its own sessions.
