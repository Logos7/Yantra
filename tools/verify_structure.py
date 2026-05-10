from pathlib import Path

required = [
    "src/Yantra.Domain/Yantra.Domain.csproj",
    "src/Yantra.Schema/Yantra.Schema.csproj",
    "src/Yantra.Composition/Yantra.Composition.csproj",
    "src/Yantra.Studio/Yantra.Studio.csproj",
    "blocks/clock/clock_27mhz/block.yaml",
    "boards/tang_nano_20k/board.yaml",
    "systems/blink_led/system.yaml",
]

root = Path(__file__).resolve().parents[1]
missing = [path for path in required if not (root / path).exists()]
if missing:
    for path in missing:
        print(f"missing: {path}")
    raise SystemExit(1)

print("Yantra structure OK")
