# Yantra.Studio scaffold changeset

## Added / rebuilt

- Studio shell layout with menu, toolbar, status bar, central document tabs, left/right/bottom dock-style panels.
- Mouse-driven graphical canvas with grid, select, drag, pan, zoom, sample block graph, and add-block command.
- Command spine via `StudioCommands` and `RelayCommand`.
- Document model: canvas documents and text documents.
- Panel model: Project, Toolbox, Inspector, Properties, Console, Problems.
- Domain model cleanup for blocks, boards, pins, interfaces, systems, instances, and connections.
- YAML schema documents and descriptor loader.
- Workspace composer with basic validation of blocks, instances, and interfaces.
- Sample descriptors for clock, counter, LED, bus, BRAM, Agni, VGA timing, Tang Nano 20K, and blink LED system.
- Docs for architecture and Studio direction.

## Run

```powershell
dotnet restore
dotnet run --project src/Yantra.Studio
```

## Suggested commit

```text
feat(studio): add graphical workspace scaffold
```

## Note

This ZIP is a full scaffold drop. It is meant to be extracted over the repository root or inspected in a clean folder first.
