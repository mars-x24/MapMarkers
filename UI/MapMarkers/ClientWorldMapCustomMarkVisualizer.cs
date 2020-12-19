namespace CryoFall.MapMarkers.UI
{
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
  using AtomicTorch.GameEngine.Common.Primitives;
  using CryoFall.MapMarkers.Scripts;
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;

  public class ClientWorldMapCustomMarkVisualizer : IWorldMapVisualizer
  {
    private readonly CustomMarkManager customMarkManager;

    private readonly WorldMapController worldMapController;

    private bool isEnabled;

    private readonly Dictionary<string, FrameworkElement> markersControl
      = new Dictionary<string, FrameworkElement>();

    public ClientWorldMapCustomMarkVisualizer(WorldMapController worldMapController)
    {
      this.worldMapController = worldMapController;

      this.customMarkManager = CustomMarkManager.GetInstance();
      this.customMarkManager.MarkListChanged += CustomMarkManager_MarkListChanged;
      this.customMarkManager.MarkAdded += CustomMarkManager_MarkAdded;
      this.customMarkManager.MarkRemoved += CustomMarkManager_MarkRemoved;
    }

    private void CustomMarkManager_MarkRemoved(CustomMarkManager sender, CustomMarkDataEventArgs e)
    {
      this.RemoveMarker(e.MarkData[0].Key);
    }

    private void CustomMarkManager_MarkAdded(CustomMarkManager sender, CustomMarkDataEventArgs e)
    {
      this.AddMarker(e.MarkData[0]);
    }

    private void CustomMarkManager_MarkListChanged(CustomMarkManager sender, CustomMarkDataEventArgs e)
    {
      this.Load(e.MarkData);
    }

    public bool IsEnabled
    {
      get => this.isEnabled;
      set
      {
        if (this.isEnabled == value)
        {
          return;
        }

        this.isEnabled = value;

        if(this.isEnabled)
          this.Load();
      }
    }

    private void Load()
    {
      this.Load(this.customMarkManager.GetMarks());
    }

    private void Load(List<CustomMarkData> listMarkData)
    {
      this.RemoveAllMarkers();
      foreach (CustomMarkData markData in listMarkData)
      {
        this.AddMarker(markData);
      }
    }

    public void Dispose()
    {
      this.RemoveAllMarkers();
      this.customMarkManager.MarkListChanged -= CustomMarkManager_MarkListChanged;
      this.customMarkManager.MarkAdded -= CustomMarkManager_MarkAdded;
      this.customMarkManager.MarkRemoved -= CustomMarkManager_MarkRemoved;
    }

    private void RemoveAllMarkers()
    {
      if (this.markersControl.Count != 0)
      {
        foreach (string key in this.markersControl.Keys.ToList())
        {
          this.RemoveMarker(key);
        }
      }
    }

    private void RemoveMarker(string key)
    {
      if (!this.markersControl.TryGetValue(key, out var control))
        return;

      this.markersControl.Remove(key);
      this.worldMapController.RemoveControl(control);
    }

    private void AddMarker(CustomMarkData markData)
    {
      string key = markData.Key;
      var mapControl = new WorldMapMarkCustomMark(key, markData.IsOwner);
      mapControl.IsHitTestVisible = false;

      this.UpdatePosition(mapControl, markData.Position.ToVector2Ushort());
      Panel.SetZIndex(mapControl, 9);

      this.worldMapController.AddControl(mapControl);
      this.markersControl[key] = mapControl;
    }

    private void UpdatePosition(FrameworkElement control, in Vector2Ushort position)
    {
      control.Visibility = Visibility.Visible;
      var canvasPosition = this.worldMapController.WorldToCanvasPosition(position.ToVector2D());
      Canvas.SetLeft(control, canvasPosition.X);
      Canvas.SetTop(control, canvasPosition.Y);
    }

    public static void UserRemoveAllMarks()
    {
      CustomMarkManager.GetInstance().Clear();
    }

    public static void UserAddMarker(Vector2D position)
    {
      CustomMarkManager.GetInstance().AddMarker(position, true, true, true, true);
    }


  }
}