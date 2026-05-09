# Yantra Block Architecture

Yantra is an independent block-first FPGA design project.

It is not a refactor of any older project. It starts from simple concepts and grows toward a visual system editor.

## Core chain

```text
Block Type -> Instance -> System -> Target -> Build -> Artifact
                    ^
                 Program
                    ^
               Language / DSL
```

## Concepts

### Block Type

A block type is a reusable hardware component definition: processor, memory, bus, UART, HDMI, PLL, LED, reset synchronizer, and so on.

A processor is also just a block type.

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

### Target

A target is a physical FPGA board plus vendor/backend details: pins, clocks, constraints, resources, and build toolchain.

### Interface

An interface is a typed connection contract, such as `clock`, `reset`, `mmio.master`, `mmio.slave`, `pixel.source`, `pixel.sink`, `uart.tx`, or `gpio.out`.

### Subblocks

Blocks may be composite. A processor block can be built from subblocks such as an ALU, decoder, register file, bus adapter, or coprocessor.

Rules:

1. No cycles in block hierarchy.
2. Subblocks are internal unless explicitly exported.
3. External connectivity happens through declared interfaces.
4. Systems compose instances; block definitions compose subblocks.

## First practical target

The first real system should be intentionally tiny:

```text
input clock -> counter -> LED
```

This proves descriptors, composition, validation, target mapping, and the first editor view without dragging in a large CPU too early.
