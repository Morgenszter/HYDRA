# HYDRA AI security checklist

- [ ] Ollama binds to loopback/private interface only.
- [ ] Windows Firewall blocks public access to port 11434.
- [ ] OpenAI key is stored outside repo and never logged.
- [ ] `SECRET` data cannot enter any model prompt.
- [ ] `PRIVATE_LOCAL` data can route only to Ollama.
- [ ] Tool names are allowlisted.
- [ ] Tool JSON schemas are validated.
- [ ] Device IDs are checked against registry.
- [ ] Mutating tools require approval/policy decision.
- [ ] Heater/brightness/color values have bounded validation.
- [ ] Every tool call has correlation ID and audit entry.
- [ ] Fallback cannot replay a mutating call after uncertain completion.
- [ ] MCP server exposes no arbitrary shell or generic gRPC invocation.
- [ ] Remote MCP uses HTTPS and authentication.
- [ ] Prompt/tool outputs are treated as untrusted input.
- [ ] Logs redact prompts, tokens, device keys and local identifiers.
