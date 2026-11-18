using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Tilemap floors;
    [SerializeField] private Tilemap walls;
    [SerializeField] private Tilemap dusts;

    private Dictionary<Vector3Int, TileBase> initialDust;
    public int RemainingDust { get; private set; }

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

    public void RestoreDust()
    {
        RequireConfiguration();
        if (initialDust == null)
        {
            initialDust = new Dictionary<Vector3Int, TileBase>();
            foreach (Vector3Int cell in dusts.cellBounds.allPositionsWithin)
            {
                TileBase tile = dusts.GetTile(cell);
                if (tile != null)
                    initialDust.Add(cell, tile);
            }
        }
        else
        {
            dusts.ClearAllTiles();
            foreach (var entry in initialDust)
                dusts.SetTile(entry.Key, entry.Value);
        }

        RemainingDust = initialDust.Count;
    }

    // Returns false when there is no dust to remove.
    public bool TryRemoveDust(Vector3Int cellPosition)
    {
        if (!HasDust(cellPosition))
            return false;

        dusts.SetTile(cellPosition, null);
        if (initialDust != null)
            RemainingDust--;
        return true;
    }

    private void RequireConfiguration()
    {
        if (!IsConfigured)
            throw new System.InvalidOperationException(
                "MapManager requires Floors, Walls and Dusts tilemap references.");
    }
}
