using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    public Tilemap interactableMap;
    public Tilemap seedMap;
    public Tile hiddenInteractableTile;
    public Tile interactedTile;
    public Tile plantedTile;

    private Dictionary<Vector3Int, string> tileStates = new Dictionary<Vector3Int, string>();
    private Dictionary<Vector3Int, bool> wateredTiles = new Dictionary<Vector3Int, bool>();

    void Start()
    {
        foreach (var position in interactableMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = interactableMap.GetTile(position);
            if (tile != null && tile.name == "Interactable_Visible")
            {
                interactableMap.SetTile(position, hiddenInteractableTile);
                seedMap.SetTile(position, hiddenInteractableTile);
            }
        }
    }

    public void SetInteracted(Vector3Int position)
    {
        interactableMap.SetTile(position, interactedTile);
        tileStates[position] = "Plowed";
    }

    public List<Vector3Int> SaveSeededTiles()
    {
        List<Vector3Int> seededTilePositions = new List<Vector3Int>();

        foreach (var tile in tileStates)
        {
            if (tile.Value == "Seeded")
            {
                seededTilePositions.Add(tile.Key);
            }
        }
        return seededTilePositions;
    }

    public string GetTileName(Vector3Int position)
    {
        if (interactableMap != null)
        {
            TileBase tile = interactableMap.GetTile(position);
            if (tile != null)
                return tile.name;
        }
        return "";
    }

    public string GetTileState(Vector3Int position)
    {
        if (tileStates.ContainsKey(position))
            return tileStates[position];

        return "";
    }

    public void SetTileState(Vector3Int position, string state)
    {
        if (tileStates.ContainsKey(position))
        {
            tileStates[position] = state;
        }
        else
        {
            tileStates.Add(position, state);
        }
    }

    public bool GetWateringTile(Vector3Int position)
    {
        if (wateredTiles.ContainsKey(position))
            return wateredTiles[position];

        return false;
    }

    public void SetWateringTile(Vector3Int position, bool iswatered)
    {
        if (wateredTiles.ContainsKey(position))
            wateredTiles[position] = iswatered;
    }

    public bool DoesTileExist(Vector3Int position)
    {
        return tileStates.ContainsKey(position);
    }

    public void RemoveTile(Vector3Int position)
    {
        seedMap.SetTile(position, null);
        seedMap.SetTile(position, hiddenInteractableTile);
        tileStates[position] = "Plowed";
    }

    public void WaterTile(Vector3Int position)
    {
        wateredTiles[position] = true;

        TileBase tile = interactableMap.GetTile(position);
        if (tile != null)
        {
            interactableMap.SetTileFlags(position, TileFlags.None);  // 타일의 기본 속성 플래그를 None으로 설정
            interactableMap.SetColor(position, new Color(222f / 255f, 222f / 255f, 222f / 255f));
        }
    }

    public List<Vector3Int> GetWateredTilesKeys()
    {
        return new List<Vector3Int>(wateredTiles.Keys);
    }

}