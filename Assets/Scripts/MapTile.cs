using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTile
{

    // Tipo de celda
    public SpriteRepository.TileType tileType;

    // Tile asociado
    public Tile tile;

    // Prioridad
    public int priority;

    // Visited Flag
    public bool visited = false;

    // Posicion
    public Vector3Int position;

    // Contador de upgrade
    public int upgradeCount;

    // PathFinding
    public int MinCostToStart;
    public MapTile nearestTileToStart;
    public List<Connection> connections = new List<Connection>();
    public float StraightLineDistanceToEnd = -1;

    // tipo de carretera
    public SpriteRepository.RoadTileType roadTileType;

    // Resource placement
    public Dictionary<MapTile, float> distanceToPlayerBase = new Dictionary<MapTile, float>();

    // flag de construccion
    public bool buildable = true;

    public struct Connection
    {
        public MapTile ConnectedNode;
        public int cost;
    }

    public MapTile(SpriteRepository.TileType type, Tile tile, Vector3Int pos)
    {
        tileType = type;
        this.tile = tile;
        priority = WorldGenerator.priorityByType[type];
        position = pos;
    }

    public void setTileType(SpriteRepository.TileType type)
    {
        tileType = type;
        tile = ScriptableObject.CreateInstance("Tile") as Tile;
        tile.sprite = SpriteRepository.spriteRepo[type];
        priority = WorldGenerator.priorityByType[type];
    }

    public void setRoadTileType(SpriteRepository.RoadTileType roadTile)
    {
        if (tileType != SpriteRepository.TileType.TILE_TYPE_ROAD)
            return;

        roadTileType = roadTile;
        tile = ScriptableObject.CreateInstance("Tile") as Tile;
        tile.sprite = SpriteRepository.roadSpriteRepo[roadTile];
    }

    public float getSimmetricRate()
    {
        float rate = 0;

        List<MapTile> keys = new List<MapTile>(distanceToPlayerBase.Keys);

        if(keys.Count != 2)
        {
            Debug.Log("Error en distanceTOPLayerBase: " + keys.Count);
            return 0f;
        }

        float up = Mathf.Min(distanceToPlayerBase[keys[0]], distanceToPlayerBase[keys[1]]);
        float down = Mathf.Max(distanceToPlayerBase[keys[0]], distanceToPlayerBase[keys[1]]);
        rate = up / down;

        return rate;
    }
}
