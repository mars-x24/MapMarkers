using AtomicTorch.CBND.GameApi.Extensions;
using AtomicTorch.GameEngine.Common.Primitives;
using System;

namespace CryoFall.MapMarkers.Scripts
{
  public readonly struct CustomMarkData
  {
    public readonly string Key;
    public readonly Vector2D Position;
    public readonly bool IsOwner;

    public CustomMarkData(string key, Vector2D position, bool isOwner)
    {
      this.Key = key;
      this.Position = position;
      this.IsOwner = isOwner;
    }

    public bool CloseTo(string key)
    {
      if (string.IsNullOrEmpty(this.Key))
        return false;

      string[] sm = this.Key.Split(';');
      Vector2Ushort m = new Vector2Ushort(Convert.ToUInt16(sm[0]), Convert.ToUInt16(sm[1]));

      string[] sk = key.Split(';');
      Vector2Ushort k = new Vector2Ushort(Convert.ToUInt16(sk[0]), Convert.ToUInt16(sk[1]));

      double maxTile = 5.0;
      double maxTileTopY = 10.0;

      if (m.TileDistanceTo(k) <= maxTile)
        return true;

      int xDistance = Math.Abs(k.X - m.X);
      int yTopDistance = k.Y - m.Y;
      if (xDistance <= maxTile && yTopDistance > 0 && yTopDistance <= maxTileTopY)
        return true;

      return false;
    }

    public static bool operator ==(CustomMarkData markData, string key)
    {
      if (object.ReferenceEquals(markData, null))
        return false;
      if (string.IsNullOrEmpty(markData.Key))
        return false;

      string[] sm = markData.Key.Split(';');
      Vector2Ushort m = new Vector2Ushort(Convert.ToUInt16(sm[0]), Convert.ToUInt16(sm[1]));

      string[] sk = key.Split(';');
      Vector2Ushort k = new Vector2Ushort(Convert.ToUInt16(sk[0]), Convert.ToUInt16(sk[1]));

      return m.X == k.X && m.Y == k.Y;
    }

    public static bool operator !=(CustomMarkData markData, string key)
    {
      return !(markData == key);
    }

    public static bool operator ==(CustomMarkData markData, CustomMarkData markDataK)
    {
      if (object.ReferenceEquals(markData, null))
        return false;
      if (object.ReferenceEquals(markDataK, null))
        return false;

      string[] sm = markData.Key.Split(';');
      Vector2Ushort m = new Vector2Ushort(Convert.ToUInt16(sm[0]), Convert.ToUInt16(sm[1]));

      string[] sk = markDataK.Key.Split(';');
      Vector2Ushort k = new Vector2Ushort(Convert.ToUInt16(sk[0]), Convert.ToUInt16(sk[1]));

      return m.X == k.X && m.Y == k.Y;
    }

    public static bool operator !=(CustomMarkData markData, CustomMarkData markDataK)
    {
      return !(markData == markDataK);
    }

    public override bool Equals(object obj)
    {
      return base.Equals(obj);
    }
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}
