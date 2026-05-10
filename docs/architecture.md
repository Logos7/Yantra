# Yantra architecture

Yantra is organized around a clean split between domain, schema, composition, and user interface.

```text
Yantra.Domain
  Stable concepts: BlockDefinition, SystemDefinition, BoardDefinition, ports, parameters, connections.

Yantra.Schema
  File representations and loaders. This layer knows about YAML and disk layout.

Yantra.Composition
  Resolution layer. It turns a system descriptor and block library into a validated composition graph.

Yantra.Studio
  Avalonia graphical shell. It hosts documents, panels, commands, and canvas tools.
```

## Studio shell model

```text
Shell
  Commands
  Documents
  Panels
  Workspaces
  Canvas tools
```

Documents are central editor tabs. Panels are side/bottom tools. Commands are the single spine behind menu items, toolbar buttons, shortcuts, command palette actions, and later script calls.

## Docking plan

The current scaffold uses a stable split-panel layout so that the app has no extra docking dependency yet. The model is intentionally prepared for real docking:

```text
IStudioDocument
IStudioPanel
PanelDock
WorkspaceLayout
```

A future `Yantra.Studio.Docking` adapter can map these models to Dock.Avalonia, Actipro Docking, or a custom layout engine without touching domain and editor logic.

## Graphical editing plan

The mouse-driven editor should evolve in this order:

1. Select and move blocks.
2. Add blocks from toolbox.
3. Expose typed ports.
4. Drag wires between ports.
5. Validate graph live.
6. Map graph nodes to `SystemDefinition`.
7. Generate build artifacts.
