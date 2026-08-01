namespace AutoGladiators.Core.Utilities;

public static class BuildingFootprint
{
    public static bool ContainsBlockingTile(int doorX, int doorY, int tileX, int tileY)
    {
        if (tileX < doorX - 1 || tileX > doorX + 1)
            return false;

        if (tileY < doorY - 2 || tileY > doorY)
            return false;

        return tileX != doorX || tileY != doorY;
    }
}
