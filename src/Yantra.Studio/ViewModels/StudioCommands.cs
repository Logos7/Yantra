using System.Windows.Input;

namespace Yantra.Studio.ViewModels;

public sealed class StudioCommands
{
    public StudioCommands(StudioAppViewModel app)
    {
        NewCanvas = app.NewCanvasCommand;
        AddBlock = app.AddBlockCommand;
        Save = app.SaveCommand;
        Build = app.BuildCommand;
        ZoomToFit = app.ZoomToFitCommand;
        ResetLayout = app.ResetLayoutCommand;
        ShowCommandPalette = app.ShowCommandPaletteCommand;
    }

    public ICommand NewCanvas { get; }
    public ICommand AddBlock { get; }
    public ICommand Save { get; }
    public ICommand Build { get; }
    public ICommand ZoomToFit { get; }
    public ICommand ResetLayout { get; }
    public ICommand ShowCommandPalette { get; }
}
