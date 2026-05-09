# blink_led

The first Yantra system descriptor.

It intentionally models the smallest useful FPGA composition:

```text
input clock -> counter -> LED
```

The goal is not to generate hardware yet. The goal is to prove the initial descriptor chain:

```text
Block Type -> Instance -> System -> Board + Backend -> Build
```
