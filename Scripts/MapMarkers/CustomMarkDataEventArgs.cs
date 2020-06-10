using System.Collections.Generic;

namespace CryoFall.MapMarkers.Scripts
{
  public class CustomMarkDataEventArgs : System.EventArgs
  {
    public List<CustomMarkData> MarkData;
    public CustomMarkManager Manager;

    public CustomMarkDataEventArgs(CustomMarkManager manager, List<CustomMarkData> markdata)
    {
      this.MarkData = markdata;
      this.Manager = manager;
    }
  }
}
