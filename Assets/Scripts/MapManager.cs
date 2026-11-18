using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Tilemap floors;
    [SerializeField] private Tilemap walls;
    [SerializeField] private Tilemap dusts;

    public bool IsConfigured => floors != null && walls != null && dusts != null;

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        RequireConfiguration();
        return floors.WorldToCell(worldPosition);
    }

    public Vector3 GetCellCenterWorld(Vector3Int cellPosition)
    {
        RequireConfiguration();
        return floors.GetCellCenterWorld(cellPosition);
    }

    public bool HasFloor(Vector3Int cellPosition)
    {
        RequireConfiguration();
        return floors.HasTile(cellPosition);
    }

    public bool HasWall(Vector3Int cellPosition)
    {
        RequireConfiguration();
        return walls.HasTile(cellPosition);
    }

    public bool CanMoveTo(Vector3Int cellPosition)
    {
        return HasFloor(cellPosition) && !HasWall(cellPosition);
    }

    public bool HasDust(Vector3Int cellPosition)
    {
        RequireConfiguration();
        return dusts.HasTile(cellPosition);
    }

    // Returns false when there is no dust to remove.
    public bool TryRemoveDust(Vector3Int cellPosition)
    {
        if (!HasDust(cellPosition))
            return false;

        dusts.SetTile(cellPosition, null);
        return true;
    }

    private void RequireConfiguration()
    {
        if (!IsConfigured)
            throw new System.InvalidOperationException(
                "MapManager requires Floors, Walls and Dusts tilemap references.");
    }
}
