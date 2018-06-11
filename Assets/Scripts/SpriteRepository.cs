using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRepository : MonoBehaviour {

    // Tipo de celdas
    public enum TileType
    {
        TILE_TYPE_WATER = 1,
        TILE_TYPE_GRASS = 2,
        TILE_TYPE_MOUNTAIN = 4,
        TILE_TYPE_FOREST = 8,
        TILE_TYPE_PLAYER_BASE = 16,
        TILE_TYPE_ROAD = 32,
        TILE_TYPE_FACTORY = 64,
        TILE_TYPE_HANGAR = 128,
        TILE_TYPE_SHIPYARD = 256,
        TILE_TYPE_BUILDING = 512,
        TILE_TYPE_NULL
    }

    public enum RoadTileType
    {
        ROAD_TILE_TYPE_HORIZONTAL,
        ROAD_TILE_TYPE_VERTICAL,
        ROAD_TILE_TYPE_DOWN_LEFT,
        ROAD_TILE_TYPE_DOWN_RIGHT,
        ROAD_TILE_TYPE_UP_LEFT,
        ROAD_TILE_TYPE_UP_RIGHT,
        ROAD_TILE_TYPE_NONE
    }

    // Repositorio de Sprites
    public static Dictionary<TileType, Sprite> spriteRepo = new Dictionary<TileType, Sprite>();

    public static Dictionary<RoadTileType, Sprite> roadSpriteRepo = new Dictionary<RoadTileType, Sprite>();

    // Use this for initialization
    void Awake ()
    {
        // Rellenamos los sprites
        spriteRepo.Add(TileType.TILE_TYPE_WATER, null);
        spriteRepo.Add(TileType.TILE_TYPE_GRASS, null);
        spriteRepo.Add(TileType.TILE_TYPE_MOUNTAIN, null);
        spriteRepo.Add(TileType.TILE_TYPE_FOREST, null);
        spriteRepo.Add(TileType.TILE_TYPE_PLAYER_BASE, null);
        spriteRepo.Add(TileType.TILE_TYPE_ROAD, null);
        spriteRepo.Add(TileType.TILE_TYPE_FACTORY, null);
        spriteRepo.Add(TileType.TILE_TYPE_HANGAR, null);
        spriteRepo.Add(TileType.TILE_TYPE_SHIPYARD, null);
        spriteRepo.Add(TileType.TILE_TYPE_BUILDING, null);

        spriteRepo[TileType.TILE_TYPE_WATER] = Resources.Load<Sprite>("Images/SimpleWater");
        spriteRepo[TileType.TILE_TYPE_GRASS] = Resources.Load<Sprite>("Images/SimpleGrass");
        spriteRepo[TileType.TILE_TYPE_MOUNTAIN] = Resources.Load<Sprite>("Images/Mountain");
        spriteRepo[TileType.TILE_TYPE_FOREST] = Resources.Load<Sprite>("Images/Grass");
        spriteRepo[TileType.TILE_TYPE_PLAYER_BASE] = Resources.Load<Sprite>("Images/WierdCouncil");
        spriteRepo[TileType.TILE_TYPE_ROAD] = Resources.Load<Sprite>("Images/Road");
        spriteRepo[TileType.TILE_TYPE_FACTORY] = Resources.Load<Sprite>("Images/Factory");
        spriteRepo[TileType.TILE_TYPE_HANGAR] = Resources.Load<Sprite>("Images/Hangar");
        spriteRepo[TileType.TILE_TYPE_SHIPYARD] = Resources.Load<Sprite>("Images/Shipyard");
        spriteRepo[TileType.TILE_TYPE_BUILDING] = Resources.Load<Sprite>("Images/Building");

        // ROADS
        roadSpriteRepo[RoadTileType.ROAD_TILE_TYPE_HORIZONTAL] = Resources.Load<Sprite>("Images/Road");
        roadSpriteRepo[RoadTileType.ROAD_TILE_TYPE_VERTICAL] = Resources.Load<Sprite>("Images/verticalRoad");
        roadSpriteRepo[RoadTileType.ROAD_TILE_TYPE_DOWN_LEFT] = Resources.Load<Sprite>("Images/RoadLeft-Down");
        roadSpriteRepo[RoadTileType.ROAD_TILE_TYPE_DOWN_RIGHT] = Resources.Load<Sprite>("Images/RoadDown-Right");
        roadSpriteRepo[RoadTileType.ROAD_TILE_TYPE_UP_LEFT] = Resources.Load<Sprite>("Images/RoadLeft-Up");
        roadSpriteRepo[RoadTileType.ROAD_TILE_TYPE_UP_RIGHT] = Resources.Load<Sprite>("Images/RoadUp-Right");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
