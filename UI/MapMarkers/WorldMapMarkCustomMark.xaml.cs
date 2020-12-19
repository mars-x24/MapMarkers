namespace CryoFall.MapMarkers.UI
{
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

  public partial class WorldMapMarkCustomMark : BaseUserControl
  {
    private readonly string name;
    private readonly bool isOwner;

    private ViewModelCustomMark viewModel;

    public WorldMapMarkCustomMark(string name, bool isOwner)
    {
      this.name = name;
      this.isOwner = isOwner;
    }

    public WorldMapMarkCustomMark()
    {

    }

    protected override void OnLoaded()
    {
      this.viewModel = new ViewModelCustomMark(this.name, this.isOwner);
      this.DataContext = this.viewModel;

      this.UpdateLayout();

      //var textBlock = this.GetByName<FrameworkElement>("NameGrid");
      //Canvas.SetLeft(textBlock, -textBlock.ActualWidth / 2);
    }

    protected override void OnUnloaded()
    {
      this.DataContext = null;
      this.viewModel?.Dispose();
      this.viewModel = null;
    }

    //test
  }
}