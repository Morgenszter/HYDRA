# ADR-002 — Hybrid LLM routing: Ollama + OpenAI

## Status
PROPOSED

## Context
HYDRA_HOME needs local/offline inference and optional high-capability cloud reasoning. Direct coupling of UI, devices or voice to a vendor endpoint would violate boundaries and complicate security.

## Decision
Create a single `Hydra.AI` boundary behind gRPC.

- Ollama is the local provider through its OpenAI-compatible `/v1/responses` API.
- OpenAI is the cloud provider through the official Responses API.
- Routing is policy-based: `LOCAL_ONLY`, `CLOUD_ONLY`, `LOCAL_FIRST`, `CLOUD_FIRST`, `CLASSIFIED`.
- Device commands are never executed directly from raw model text. Models may only request allowlisted tools; HYDRA validates and authorizes every call.
- Secrets remain in .NET configuration/secret store; never in WPF, Rust logs, prompts or repo.
- Conversation state is maintained by HYDRA. Ollama's Responses compatibility must be treated as non-stateful for portable behavior.

## Topology

```text
WPF / Voice / Automation
          |
          v
Hydra.Bridge.Api -> HydraAiFacade
          |
          v
HybridModelRouter
  |                     |
  v                     v
Ollama localhost        OpenAI Responses API
  |                     |
  +---- tool requests --+
          |
          v
Hydra Tool Policy -> Rust Core gRPC -> devices/runtime
```

## Direct ChatGPT access
ChatGPT cannot directly call `localhost:11434`. To let ChatGPT interact with HYDRA, expose a narrowly scoped **remote MCP server** over trusted HTTPS. That MCP server calls HYDRA services; Ollama remains private on the machine. Require approvals for mutating tools.

## Consequences
- Offline operation remains possible.
- Cloud use is explicit and auditable.
- One stable application contract survives provider changes.
- More implementation work: routing, policy, tool validation, observability and tests.
