# Yantra.Studio

Yantra.Studio is intended to behave like a compact hardware-design IDE.

The core UI rule is simple:

```text
Documents in the center, tools around them.
```

## Document types

```text
System canvas
Block editor
Board editor
Program editor
Build preview
Generated HDL preview
```

## Panel types

```text
Project Explorer
Toolbox
Inspector
Properties
Console
Problems
Build Output
```

## Command spine

Every user action should become a command before it becomes a button.

```text
Project.New
Project.Open
Project.Save
Canvas.AddBlock
Canvas.ZoomToFit
Build.Generate
Build.Run
View.ResetLayout
```

This keeps menu, toolbar, shortcuts, command palette, and scripting coherent.
