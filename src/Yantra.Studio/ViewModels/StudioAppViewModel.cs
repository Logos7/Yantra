using System.Collections.ObjectModel;
using Yantra.Composition;
using Yantra.Domain;
using Yantra.Studio.Infrastructure;
using Yantra.Studio.Models;
using Yantra.Studio.Services;

namespace Yantra.Studio.ViewModels;

public sealed class StudioAppViewModel : ObservableObject
{
    private readonly StudioWorkspaceLoader _workspaceLoader = new();
    private StudioWorkspaceSnapshot? _workspace;
    private StudioDocumentViewModel? _activeDocument;
    private CanvasTool _activeTool = CanvasTool.Select;
    private string _statusText = "Ready";

    public StudioAppViewModel()
    {
        NewCanvasCommand = new RelayCommand(NewCanvas);
        AddBlockCommand = new RelayCommand(AddBlock);
        SaveCommand = new RelayCommand(Save);
        BuildCommand = new RelayCommand(Build);
        ZoomToFitCommand = new RelayCommand(ZoomToFit);
        ResetLayoutCommand = new RelayCommand(ResetLayout);
        ShowCommandPaletteCommand = new RelayCommand(ShowCommandPalette);
        Commands = new StudioCommands(this);

        CreatePanels();
        LoadWorkspace();
        NewCanvas();

        if (File.Exists("docs/architecture.md"))
        {
            Documents.Add(new TextDocumentViewModel("doc.arch", "Architecture.md", File.ReadAllText("docs/architecture.md")));
        }
    }

    public ObservableCollection<StudioDocumentViewModel> Documents { get; } = [];
    public ObservableCollection<StudioPanelViewModel> LeftPanels { get; } = [];
    public ObservableCollection<StudioPanelViewModel> RightPanels { get; } = [];
    public ObservableCollection<StudioPanelViewModel> BottomPanels { get; } = [];

    public StudioCommands Commands { get; }
    public RelayCommand NewCanvasCommand { get; }
    public RelayCommand AddBlockCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand BuildCommand { get; }
    public RelayCommand ZoomToFitCommand { get; }
    public RelayCommand ResetLayoutCommand { get; }
    public RelayCommand ShowCommandPaletteCommand { get; }

    public StudioDocumentViewModel? ActiveDocument
    {
        get => _activeDocument;
        set
        {
            if (Set(ref _activeDocument, value))
            {
                UpdateInspector();
            }
        }
    }

    public CanvasTool ActiveTool
    {
        get => _activeTool;
        set => Set(ref _activeTool, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => Set(ref _statusText, value);
    }

    public void NewCanvas()
    {
        var scene = CreateWorkspaceScene();
        scene.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CanvasSceneViewModel.SelectedNode))
            {
                UpdateInspector();
            }
        };

        var document = new CanvasDocumentViewModel($"canvas.{Documents.Count + 1}", $"{scene.SystemId}.system", scene);
        Documents.Add(document);
        ActiveDocument = document;
        WriteConsole($"Opened graphical system canvas '{scene.SystemId}'.");
    }

    public void AddBlock()
    {
        if (ActiveDocument is not CanvasDocumentViewModel canvas)
        {
            return;
        }

        var block = _workspace?.Blocks.FirstOrDefault(x => x.Kind == BlockKind.Other) ?? _workspace?.Blocks.FirstOrDefault();
        if (block is null)
        {
            var fallback = canvas.Scene.AddNode("custom", "custom.block", "Custom Block", "Other");
            canvas.IsDirty = true;
            StatusText = $"Added {fallback.Title}.";
            WriteConsole($"Added fallback block node '{fallback.Id}'.");
            UpdateInspector();
            return;
        }

        var ports = block.Interfaces.Select(port => new CanvasPortViewModel(port.Id.Value, port.Direction.ToString(), port.Kind, port.Protocol));
        var parameters = block.Parameters
            .Where(x => x.DefaultValue is not null)
            .ToDictionary(x => x.Name, x => x.DefaultValue ?? "");
        var node = canvas.Scene.AddNode(block.Name, block.Id.Value, block.Name, block.Kind.ToString(), ports, parameters);
        canvas.IsDirty = true;
        StatusText = $"Added {node.Title}.";
        WriteConsole($"Added block node '{node.Id}' from descriptor '{block.Id}'.");
        UpdateInspector();
    }

    public void Save()
    {
        if (ActiveDocument is not null)
        {
            ActiveDocument.IsDirty = false;
            StatusText = $"Saved {ActiveDocument.Title}.";
            WriteConsole($"Saved '{ActiveDocument.Title}'.");
        }
    }

    public void Build()
    {
        LoadWorkspace();
        var resolved = GetActiveResolvedSystem();
        if (resolved is null)
        {
            StatusText = "No system selected for build.";
            WriteConsole("Build.Generate: no active system document was selected.");
            return;
        }

        UpdateProblems(resolved);
        StatusText = resolved.IsValid ? "Build plan valid." : $"Build plan has {resolved.Problems.Count} problem(s).";
        WriteConsole($"Build.Generate: {resolved.System.Id} / {resolved.System.Backend}, instances={resolved.System.Instances.Count}, connections={resolved.System.Connections.Count}.");
    }

    public void ZoomToFit()
    {
        if (ActiveDocument is CanvasDocumentViewModel canvas)
        {
            canvas.Scene.ZoomToFit();
            StatusText = "Canvas reset.";
        }
    }

    public void ResetLayout()
    {
        StatusText = "Default layout restored.";
        WriteConsole("View.ResetLayout: using default split-panel layout.");
    }

    public void ShowCommandPalette()
    {
        StatusText = "Command palette placeholder: type-to-run comes next.";
        WriteConsole("Command palette placeholder invoked.");
    }

    private void LoadWorkspace()
    {
        try
        {
            _workspace = _workspaceLoader.LoadNearest();
            UpdateWorkspacePanels();
            UpdateProblems(_workspace.ResolvedSystems.FirstOrDefault());
            WriteConsole($"Workspace loaded: {_workspace.RootDirectory}");
        }
        catch (Exception ex)
        {
            _workspace = null;
            UpdateFallbackPanels(ex);
            WriteConsole($"Workspace load failed: {ex.Message}");
        }
    }

    private CanvasSceneViewModel CreateWorkspaceScene()
    {
        var system = _workspace?.Systems.FirstOrDefault(x => x.Id.Value == "blink_led") ?? _workspace?.Systems.FirstOrDefault();
        if (system is not null && _workspace is not null)
        {
            return CanvasSceneFactory.FromSystem(system, _workspace.BlocksById);
        }

        return CanvasSceneFactory.Sample();
    }

    private void CreatePanels()
    {
        LeftPanels.Add(new StudioPanelViewModel("panel.project", "Project", PanelKind.Project, PanelDock.Left) { Subtitle = "Workspace tree" });
        LeftPanels.Add(new StudioPanelViewModel("panel.toolbox", "Toolbox", PanelKind.Toolbox, PanelDock.Left) { Subtitle = "Block descriptors" });
        RightPanels.Add(new StudioPanelViewModel("panel.inspector", "Inspector", PanelKind.Inspector, PanelDock.Right) { Subtitle = "Selected object" });
        RightPanels.Add(new StudioPanelViewModel("panel.properties", "Properties", PanelKind.Properties, PanelDock.Right) { Subtitle = "Document metadata" });
        BottomPanels.Add(new StudioPanelViewModel("panel.console", "Console", PanelKind.Console, PanelDock.Bottom) { Subtitle = "Command output" });
        BottomPanels.Add(new StudioPanelViewModel("panel.problems", "Problems", PanelKind.Problems, PanelDock.Bottom) { Subtitle = "Validation" });
    }

    private void UpdateWorkspacePanels()
    {
        if (_workspace is null)
        {
            return;
        }

        var project = LeftPanels.FirstOrDefault(x => x.Kind == PanelKind.Project);
        project?.ReplaceLines([
            $"root: {_workspace.RootDirectory}",
            $"blocks: {_workspace.Blocks.Count}",
            $"boards: {_workspace.Boards.Count}",
            $"systems: {_workspace.Systems.Count}",
            .._workspace.Systems.Select(x => $"system: {x.Id} ({x.Instances.Count} blocks, {x.Connections.Count} wires)")
        ]);

        var toolbox = LeftPanels.FirstOrDefault(x => x.Kind == PanelKind.Toolbox);
        toolbox?.ReplaceLines(_workspace.Blocks
            .OrderBy(x => x.Kind)
            .ThenBy(x => x.Id.Value, StringComparer.OrdinalIgnoreCase)
            .Select(x => $"{x.Kind}: {x.Id} — {x.Name}")
            .ToArray());
    }

    private void UpdateFallbackPanels(Exception ex)
    {
        LeftPanels.FirstOrDefault(x => x.Kind == PanelKind.Project)?.ReplaceLines(["Workspace not loaded.", ex.Message]);
        LeftPanels.FirstOrDefault(x => x.Kind == PanelKind.Toolbox)?.ReplaceLines(["No descriptors loaded."]);
        BottomPanels.FirstOrDefault(x => x.Kind == PanelKind.Problems)?.ReplaceLines([$"workspace-load: {ex.Message}"]);
    }

    private void UpdateInspector()
    {
        var inspector = RightPanels.FirstOrDefault(x => x.Kind == PanelKind.Inspector);
        var properties = RightPanels.FirstOrDefault(x => x.Kind == PanelKind.Properties);

        if (ActiveDocument is CanvasDocumentViewModel canvas)
        {
            var node = canvas.Scene.SelectedNode;
            inspector?.ReplaceLines(node is null
                ? ["Nothing selected."]
                : [
                    $"Instance: {node.Id}",
                    $"Block: {node.BlockId}",
                    $"Name: {node.BlockName}",
                    $"Kind: {node.BlockKind}",
                    $"Inputs: {string.Join(", ", node.Inputs.Select(x => x.Id))}",
                    $"Outputs: {string.Join(", ", node.Outputs.Select(x => x.Id))}",
                    ..node.Parameters.Select(x => $"{x.Key}: {x.Value}"),
                    $"X: {node.X:0}",
                    $"Y: {node.Y:0}"
                ]);

            properties?.ReplaceLines([
                $"Document: {canvas.Title}",
                $"System: {canvas.Scene.SystemId}",
                $"Board: {canvas.Scene.BoardId}",
                $"Backend: {canvas.Scene.BackendId}",
                $"Nodes: {canvas.Scene.Nodes.Count}",
                $"Connections: {canvas.Scene.Connections.Count}",
                $"Dirty: {canvas.IsDirty}",
                $"Zoom: {canvas.Scene.Zoom:0.00}"
            ]);
        }
        else if (ActiveDocument is not null)
        {
            inspector?.ReplaceLines([$"Document: {ActiveDocument.Title}"]);
            properties?.ReplaceLines([$"Dirty: {ActiveDocument.IsDirty}"]);
        }
    }

    private ResolvedSystem? GetActiveResolvedSystem()
    {
        if (_workspace is null || ActiveDocument is not CanvasDocumentViewModel canvas)
        {
            return null;
        }

        return _workspace.ResolvedBySystemId.TryGetValue(new SystemId(canvas.Scene.SystemId), out var resolved) ? resolved : null;
    }

    private void UpdateProblems(ResolvedSystem? resolved)
    {
        var problems = BottomPanels.FirstOrDefault(x => x.Kind == PanelKind.Problems);
        if (problems is null)
        {
            return;
        }

        if (resolved is null)
        {
            problems.ReplaceLines(["No system loaded."]);
            return;
        }

        problems.ReplaceLines(resolved.IsValid
            ? [$"{resolved.System.Id}: no problems."]
            : resolved.Problems.Select(x => $"{x.Code}: {x.Message}").ToArray());
    }

    private void WriteConsole(string line)
    {
        var console = BottomPanels.FirstOrDefault(x => x.Kind == PanelKind.Console);
        console?.Lines.Add($"[{DateTime.Now:HH:mm:ss}] {line}");
        StatusText = line;
    }
}
