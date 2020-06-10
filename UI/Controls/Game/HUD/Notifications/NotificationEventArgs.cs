namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications
{
  public class NotificationEventArgs : System.EventArgs
  {
    public string Title;
    public string Message;

    public NotificationEventArgs(string title, string message)
    {
      this.Title = title;
      this.Message = message;
    }
  }
}
