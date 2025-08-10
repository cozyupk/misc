# Boostable.WhatTalkAbout — Clean skeleton

Three layers:

- **Core** (pure abstractions)
- **Orchestration** (Use-cases and facades)
- **Adapters** (Roslyn, runtime, etc.)

Dependency rule: Core ← Orchestration ← (none), Adapters → Core (never depend on Orchestration).
