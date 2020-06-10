using AtomicTorch.CBND.CoreMod.Systems.Chat;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.ServicesClient;
using AtomicTorch.GameEngine.Common.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace CryoFall.MapMarkers.Scripts
{
  public class CustomMarkManager
  {
    public event MarkDataChangedHandler MarkListChanged;
    public event MarkDataChangedHandler MarkAdded;
    public event MarkDataChangedHandler MarkRemoved;

    public delegate void MarkDataChangedHandler(CustomMarkManager sender, CustomMarkDataEventArgs e);

    private static CustomMarkManager Instance;

    private string markStorageLocalFilePath;
    protected IClientStorage clientStorage;

    private readonly List<CustomMarkData> markers;

    protected CustomMarkManager()
    {
      markers = new List<CustomMarkData>();

      HUDNotificationsPanelControl.NewNotification += HUDNotificationsPanelControl_NewNotification;

      ChatSystem.ClientChatRoomMessageReceived += ChatSystem_ClientChatRoomMessageReceived;
    }

    public static CustomMarkManager GetInstance()
    {
      if (Instance == null)
        Instance = new CustomMarkManager();
      return Instance;
    }

    public void Dispose()
    {
      markers.Clear();
      HUDNotificationsPanelControl.NewNotification -= HUDNotificationsPanelControl_NewNotification;
      ChatSystem.ClientChatRoomMessageReceived -= ChatSystem_ClientChatRoomMessageReceived;
    }

    public void Load()
    {
      this.markers.Clear();

      this.markStorageLocalFilePath = "Mods/MapMarkers/" + Api.Client.CurrentGame.ServerInfo.ServerAddress.PublicGuid.ToString();

      clientStorage = Api.Client.Storage.GetStorage(markStorageLocalFilePath);
      clientStorage.RegisterType(typeof(CustomMarkData));
      clientStorage.RegisterType(typeof(Vector2D));

      if (!clientStorage.TryLoad<List<CustomMarkData>>(out var snapshot))
        return;

      foreach (var mark in snapshot)
      {
        this.AddMarker((CustomMarkData)mark, false, false, false);
      }

      this.OnMarkListChanged(true);
    }

    public List<CustomMarkData> GetMarks()
    {
      return new List<CustomMarkData>(this.markers);
    }

    private void HUDNotificationsPanelControl_NewNotification(NotificationEventArgs e)
    {
      if (string.IsNullOrEmpty(e.Message))
        return;

      this.AddMarkerFromMessage(e.Message, true);
    }

    private void ChatSystem_ClientChatRoomMessageReceived(BaseChatRoom chatRoom, in ChatEntry chatEntry)
    {
      this.AddMarkerFromMessage(chatEntry.Message, false);
    }

    private void AddMarkerFromMessage(string message, bool isOwner)
    {
      (double x, double y) = GetMark(message);
      if (double.IsNaN(x) || double.IsNaN(y))
        return;

      var worldBoundsOffset = Api.Client.World.WorldBounds.Offset;

      this.AddMarker(new Vector2D(x + worldBoundsOffset.X, y + worldBoundsOffset.Y), isOwner, false, false, true);
    }

    public void AddMarker(Vector2D position, bool isOwner, bool removeMark, bool removeCloseMark, bool save)
    {
      var worldBoundsOffset = Api.Client.World.WorldBounds.Offset;

      Vector2Ushort mapPosition = new Vector2Ushort((ushort)(position.X - worldBoundsOffset.X), (ushort)(position.Y - worldBoundsOffset.Y));

      string key = mapPosition.X + ";" + mapPosition.Y;

      this.AddMarker(new CustomMarkData(key, position, isOwner), removeMark, removeCloseMark, save);
    }

    public void AddMarker(CustomMarkData markData, bool removeMark, bool removeCloseMark, bool save)
    {
      CustomMarkData? markDataEquals = this.GetMarker(markData.Key);
      if(markDataEquals.HasValue)
      {
        if(removeMark)
          this.RemoveMarker(markData.Key, false, true);
        return;
      }

      if (removeCloseMark)
      {
        if (this.RemoveMarker(markData.Key, true, true))
          return;
      }

      this.markers.Add(markData);
      this.OnMarkAddedChanged(markData, true);

      this.ClearNotOwned();

      if (save)
      {
        this.SaveMarkersToStorage(false);
        
      }
    }

    private void ClearNotOwned()
    {
      int maxNotOwned = 5;
      List<CustomMarkData> notOwned = this.markers.Where(m => m.IsOwner == false).ToList();
      for (int i = 0; i < notOwned.Count - maxNotOwned; i++)
      {
        this.markers.Remove(notOwned[i]);
        this.OnMarkRemovedChanged(notOwned[i], true);
      }
    }

    private bool RemoveMarker(string key, bool removeCloseMark, bool save)
    {
      List<CustomMarkData> markDataRemove = new List<CustomMarkData>();

      CustomMarkData? markData = this.GetMarker(key);
      if (markData.HasValue)
        markDataRemove.Add(markData.Value);

      if (removeCloseMark)
        markDataRemove.AddRange(this.GetMarkerCloseTo(key));

      if (markDataRemove.Count == 0)
        return false;

      foreach (CustomMarkData m in markDataRemove)
      {
        this.markers.Remove(m);
        this.OnMarkRemovedChanged(m, true);
      }

      if (save)
      {
        this.SaveMarkersToStorage(false);
      }

      return true;
    }

    public List<CustomMarkData> GetMarkerCloseTo(string key)
    {
      List<CustomMarkData> list = new List<CustomMarkData>();

      foreach (CustomMarkData markData in this.markers)
      {
        if (markData.CloseTo(key))
          list.Add(markData);
      }

      return list;
    }

    public CustomMarkData? GetMarker(string key)
    {
      List<CustomMarkData> list = new List<CustomMarkData>();

      foreach (CustomMarkData markData in this.markers)
      {
        if (markData == key)
          return markData;
      }

      return null;
    }

    private (double x, double y) GetMark(string message)
    {
      double x = double.NaN;
      double y = double.NaN;

      int index = message.ToLower().IndexOf("mark(");
      if (index >= 0)
      {
        index += 5;

        int index2 = message.IndexOf(")", index);
        if (index2 >= index)
        {
          string[] mark = message.Substring(index, index2 - index).Split(';');
          if (mark.Length == 2)
          {
            if (double.TryParse(mark[0], out double xOk))
              if (double.TryParse(mark[1], out double yOk))
              {
                x = xOk;
                y = yOk;
              }
          }
        }
      }

      return (x, y);
    }

    private void SaveMarkersToStorage(bool doEvent)
    {
      clientStorage.Save(markers);
      OnMarkListChanged(doEvent);
    }

    public void Clear()
    {
      this.markers.Clear();
      this.SaveMarkersToStorage(true);
    }

    public void OnMarkListChanged(bool doEvent)
    {
      if (doEvent && MarkListChanged != null)
        MarkListChanged(this, new CustomMarkDataEventArgs(this, this.GetMarks()));
    }
    public void OnMarkRemovedChanged(CustomMarkData markData, bool doEvent)
    {
      if (doEvent && MarkRemoved != null)
        MarkRemoved(this, new CustomMarkDataEventArgs(this, new List<CustomMarkData> { markData }));
    }
    public void OnMarkAddedChanged(CustomMarkData markData, bool doEvent)
    {
      if (doEvent && MarkAdded != null)
        MarkAdded(this, new CustomMarkDataEventArgs(this, new List<CustomMarkData> { markData }));
    }

  }
}
