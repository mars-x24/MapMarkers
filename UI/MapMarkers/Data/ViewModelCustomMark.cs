namespace CryoFall.MapMarkers.UI
{
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

  public class ViewModelCustomMark : BaseViewModel
  {
    public ViewModelCustomMark(string name, bool isOwner)
    {
      this.Name = name;
      this.IsOwner = isOwner;
    }

    public BaseCommand CommandCopyName => new ActionCommand(this.ExecuteCommandCopyName);

    public BaseCommand CommandSetMark => new ActionCommand(this.ExecuteCommandSetMark);

    public string Name { get; }

    public bool IsOwner { get; }

    protected override void DisposeViewModel()
    {
      base.DisposeViewModel();
    }

    private void ExecuteCommandCopyName()
    {
      Api.Client.Core.CopyToClipboard(this.Name);
    }

    private void ExecuteCommandSetMark()
    {
      Api.Client.Core.CopyToClipboard(".mark " + this.Name);
    }
  }
}