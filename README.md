# Yantra

Yantra is a block-first FPGA system design environment.

Yantra describes reusable hardware blocks, composes them into systems, maps those systems to physical FPGA boards and toolchain backends, and eventually produces build artifacts.

This repository starts intentionally small. The first goal is not to replace vendor tools or build a full IDE. The first goal is to define clean concepts and load a tiny system such as:

```text
clock -> counter -> LED
```

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
src/Yantra.Schema       File/schema representations
src/Yantra.Composition  System graph/composition logic
src/Yantra.Studio       Avalonia editor shell
blocks/                 Reusable hardware block types
boards/                 Physical FPGA board descriptions and constraints
systems/                Compositions of block instances
programs/               Code/data payloads that may run on systems later
```

## First milestone

1. Define block, board, and system descriptors.
2. Load descriptors into C# domain objects.
3. Resolve a minimal system graph.
4. Display the graph in Yantra Studio.
5. Later: generate HDL/project files for a selected board and backend.
