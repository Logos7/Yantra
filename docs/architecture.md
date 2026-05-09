# Yantra Block Architecture

Yantra is an independent block-first FPGA design project.

It starts from simple concepts and grows toward a visual system editor.

## Core chain

```text
Block Type -> Instance -> System -> Board + Backend -> Build -> Artifact
                    ^
                 Program
                    ^
               Language / DSL
```

## Concepts

### Block Type

A block type is a reusable hardware component definition: processor, memory, bus, UART, HDMI, PLL, LED, reset synchronizer, and so on.

A processor is also just a block type.

Blocks are logical hardware building blocks. Physical FPGA boards are not blocks.

### Instance

An instance is a concrete occurrence of a block inside a system. One system may instantiate the same processor block multiple times.

Example:

```yaml
instances:
  - id: cpu0
    block: processors/p8_minimal
  - id: cpu1
    block: processors/p8_minimal
  - id: uart0
    block: io/uart
```

### System

A system is a composition of block instances and connections between their interfaces.

A system is not a board. A system is the logical circuit design.

### Board

A board is a physical FPGA board: pins, clocks, connectors, constraints, and available resources.

Examples:

```text
tang_nano_20k
tang_primer_25k
tang_mega_138k
```

### Backend

A backend is the toolchain/vendor flow used to build a system for a board.

Examples:

```text
gowin
xilinx
lattice
yosys_nextpnr
```

A concrete build uses a system, a board, and a backend.

### Interface

An interface is a typed connection contract, such as `clock`, `reset`, `mmio.master`, `mmio.slave`, `pixel.source`, `pixel.sink`, `uart.tx`, or `gpio.out`.

### Subblocks

Blocks may be composite. A processor block can be built from subblocks such as an ALU, decoder, register file, bus adapter, or coprocessor.

Rules:

1. No cycles in block hierarchy.
2. Subblocks are internal unless explicitly exported.
3. External connectivity happens through declared interfaces.
4. Systems compose instances; block definitions compose subblocks.

## First practical board

The first board description is:

```text
boards/tang_nano_20k
```

## First practical system

The first real system should be intentionally tiny:

```text
input clock -> counter -> LED
```

This proves descriptors, composition, validation, board mapping, and the first editor view without dragging in a large CPU too early.
