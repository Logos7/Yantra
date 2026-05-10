# Yantra.Studio

Yantra.Studio is the graphical workspace for assembling Yantra systems from reusable blocks.

The current scaffold contains:

- a shell with menu, toolbar, document tabs and side panels,
- a graphical canvas with pan, zoom, selection and dragging,
- descriptor-driven block rendering from `blocks/**/block.yaml`,
- system rendering from `systems/**/system.yaml`,
- validation feedback through the Problems panel,
- Inspector and Properties panels bound to the selected node and active document.

## Startup

Run from the repository root:

```powershell
dotnet restore
dotnet run --project src/Yantra.Studio
```

The app searches upward from the current directory and executable directory until it finds a Yantra workspace root containing:

- `Yantra.sln`,
- `blocks/`,
- `systems/`.

It then loads blocks, boards and systems, and opens the first available system canvas. If `blink_led` exists, that system is preferred.

## Current editing model

The canvas is still an MVP editor:

- left click selects and drags block instances,
- mouse wheel zooms around the cursor,
- middle or right drag pans the canvas,
- Build validates the active system descriptor,
- Add Block creates a new unsaved visual node.

Saving node positions back into YAML is intentionally not implemented yet. That should come next through a dedicated layout section, not by mutating the logical system graph accidentally.
