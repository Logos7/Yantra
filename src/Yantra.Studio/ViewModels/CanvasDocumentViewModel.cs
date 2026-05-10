namespace Yantra.Studio.ViewModels;

public sealed class CanvasDocumentViewModel : StudioDocumentViewModel
{
    public CanvasDocumentViewModel(string id, string title, CanvasSceneViewModel scene)
        : base(id, title)
    {
        Scene = scene;
    }

    public CanvasSceneViewModel Scene { get; }
}
