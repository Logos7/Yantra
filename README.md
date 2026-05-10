# Yantra

Yantra is a block-first FPGA system design environment.

Yantra describes reusable hardware blocks, composes them into systems, maps those systems to physical FPGA boards and toolchain backends, and eventually produces build artifacts.

The current Studio scaffold turns the repository into a small but expandable graphical workspace: documents in the center, dock-style panels around them, a canvas for mouse-driven block composition, a command spine, and a project/domain layer that remains independent from the UI.

## Initial layers

```text
Block Type -> Instance -> System -> Board + Backend -> Build -> Artifact
                    ^
                 Program
                    ^
              Language / DSL
```

## Current repository shape

```text
src/Yantra.Domain       Pure domain concepts
src/Yantra.Schema       File/schema representations and YAML loading
src/Yantra.Composition  System graph/composition logic
src/Yantra.Studio       Avalonia editor shell
blocks/                 Reusable hardware block types
boards/                 Physical FPGA board descriptions and constraints
systems/                Compositions of block instances
programs/               Code/data payloads that may run on systems later
docs/                   Architecture notes
tools/                  Small helper scripts
```

## Run Studio

```powershell
dotnet restore
dotnet run --project src/Yantra.Studio
```

The solution targets `.NET 10.0`. If the machine only has .NET 8 installed, change `TargetFramework` in `Directory.Build.props` to `net8.0`.

## Current Studio skeleton

```text
Project menu + tool bar
Left panels: Project, Toolbox
Center documents: graphical system canvas, architecture note
Right panels: Inspector, Properties
Bottom panels: Console, Problems
Status bar
Mouse canvas: select, drag, pan, zoom, add block
```

## Suggested next milestones

1. Replace the built-in split layout with a real docking backend behind the existing `IStudioPanel`/`IStudioDocument` concepts.
2. Load the real workspace from `blocks/`, `boards/`, `systems/`, and `programs/`.
3. Turn canvas nodes into real `BlockInstance` objects.
4. Add connection drawing by dragging from ports.
5. Add HDL/project generation backends.
