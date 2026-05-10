using System.Collections.ObjectModel;
using Yantra.Studio.Infrastructure;
using Yantra.Studio.Models;

namespace Yantra.Studio.ViewModels;

public sealed class StudioAppViewModel : ObservableObject
{
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
        NewCanvas();
        Documents.Add(new TextDocumentViewModel("doc.arch", "Architecture.md", File.Exists("docs/architecture.md") ? File.ReadAllText("docs/architecture.md") : "# Yantra"));
        WriteConsole("Studio shell initialized.");
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
        var scene = CreateSampleScene();
        scene.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CanvasSceneViewModel.SelectedNode))
            {
                UpdateInspector();
            }
        };

        var document = new CanvasDocumentViewModel($"canvas.{Documents.Count + 1}", "blink_led.system", scene);
        Documents.Add(document);
        ActiveDocument = document;
        WriteConsole("Opened graphical system canvas.");
    }

    public void AddBlock()
    {
        if (ActiveDocument is CanvasDocumentViewModel canvas)
        {
            var node = canvas.Scene.AddNode("custom_block", "Other");
            canvas.IsDirty = true;
            StatusText = $"Added {node.Title}.";
            WriteConsole($"Added block node '{node.Id}'.");
            UpdateInspector();
        }
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
        StatusText = "Build plan generated.";
        WriteConsole("Build.Generate: descriptor validation and backend generation will be attached here.");
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

    private void CreatePanels()
    {
        var project = new StudioPanelViewModel("panel.project", "Project", PanelKind.Project, PanelDock.Left);
        project.Subtitle = "Workspace tree";
        project.ReplaceLines([
            "blocks/",
            "boards/tang_nano_20k/",
            "systems/blink_led/",
            "programs/",
            "src/Yantra.Studio/"
        ]);

        var toolbox = new StudioPanelViewModel("panel.toolbox", "Toolbox", PanelKind.Toolbox, PanelDock.Left);
        toolbox.Subtitle = "Canvas tools";
        toolbox.ReplaceLines([
            "Select / Move",
            "Add Clock",
            "Add Counter",
            "Add LED",
            "Wire ports"
        ]);

        var inspector = new StudioPanelViewModel("panel.inspector", "Inspector", PanelKind.Inspector, PanelDock.Right);
        inspector.Subtitle = "Selected object";

        var properties = new StudioPanelViewModel("panel.properties", "Properties", PanelKind.Properties, PanelDock.Right);
        properties.Subtitle = "Document metadata";
        properties.ReplaceLines(["No document selected."]);

        var console = new StudioPanelViewModel("panel.console", "Console", PanelKind.Console, PanelDock.Bottom);
        console.Subtitle = "Command output";

        var problems = new StudioPanelViewModel("panel.problems", "Problems", PanelKind.Problems, PanelDock.Bottom);
        problems.Subtitle = "Validation";
        problems.ReplaceLines(["No problems yet."]);

        LeftPanels.Add(project);
        LeftPanels.Add(toolbox);
        RightPanels.Add(inspector);
        RightPanels.Add(properties);
        BottomPanels.Add(console);
        BottomPanels.Add(problems);
    }

    private CanvasSceneViewModel CreateSampleScene()
    {
        var scene = new CanvasSceneViewModel();
        scene.Nodes.Add(new CanvasNodeViewModel("clock", "clock_27mhz", "Clock", 60, 80));
        scene.Nodes.Add(new CanvasNodeViewModel("counter", "counter", "Arithmetic", 280, 80));
        scene.Nodes.Add(new CanvasNodeViewModel("led", "led0", "Io", 500, 80));
        scene.Connections.Add(new CanvasConnectionViewModel("clock", "counter", "tick"));
        scene.Connections.Add(new CanvasConnectionViewModel("counter", "led", "bit[24]"));
        scene.SelectedNode = scene.Nodes.FirstOrDefault();
        return scene;
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
                : [$"Id: {node.Id}", $"Title: {node.Title}", $"Kind: {node.BlockKind}", $"X: {node.X:0}", $"Y: {node.Y:0}"]);

            properties?.ReplaceLines([
                $"Document: {canvas.Title}",
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

    private void WriteConsole(string line)
    {
        var console = BottomPanels.FirstOrDefault(x => x.Kind == PanelKind.Console);
        console?.Lines.Add($"[{DateTime.Now:HH:mm:ss}] {line}");
        StatusText = line;
    }
}
