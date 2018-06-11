using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour {

    // Interface
    public InterfaceController interfaceController;

    // TileMap
    public Tilemap tileMap;

    // Seed
    public int seedValue;

    // Dimensiones
    public int length;
    public int height;

    // Seed count
    public int seedCount;
    public int mountainSeedCount;
    public int forestSeedCount;
    public int alliedResourceCount;
    public int neutralResourceCount;
    public int asymmetricResourceCount;

    // Iterations
    public int iterations;
    public int mountainIterations;
    public int forestIterations;

    // Base Chances
    public float baseGrassChance;
    public float baseMountainChance;
    public float baseForestChance;

    // Tile Dictionary
    public static Dictionary<Vector3Int, MapTile> mapTileDictionary = new Dictionary<Vector3Int, MapTile>();

    // Reglas de expansión - Key > Value
    public static Dictionary<SpriteRepository.TileType, SpriteRepository.TileType> expandRules = new Dictionary<SpriteRepository.TileType, SpriteRepository.TileType>();

    // Expansion chances
    public static Dictionary<SpriteRepository.TileType, float> expandChances = new Dictionary<SpriteRepository.TileType, float>();

    // PositionsByType
    public static Dictionary<SpriteRepository.TileType, List<Vector3Int>> tilePosByType = new Dictionary<SpriteRepository.TileType, List<Vector3Int>>();

    // PriorityByType
    public static Dictionary<SpriteRepository.TileType, int> priorityByType = new Dictionary<SpriteRepository.TileType, int>();

    // Vector to type
    public static Dictionary<Vector3Int, SpriteRepository.RoadTileType> vectorToRoadTile = new Dictionary<Vector3Int, SpriteRepository.RoadTileType>();

    // Cost Map
    public static Dictionary<Vector3Int, CostMap> costMapsByCell = new Dictionary<Vector3Int, CostMap>();

    // Method flag
    public bool old = false;
    public bool oldPlayerBasePlacement = false;
    public bool oldResourcePlacemente = false;
    public bool roads = false;
    public bool resources = false;
    public bool expositionMode = false;

    // Timer
    public float expositionTime;
    private float timer;

    // Misc
    public bool autoAdjust = false;
    public GameObject tankFab;

    // Performance
    PerformanceController.MapPerfomanceReport report;

    public struct CostMap
    {
        public MapTile playerBase;
        public Dictionary<int, List<MapTile>> grassTilesByCost;
    }

    public struct DistanceMap
    {
        public MapTile mapTile1;
        public MapTile mapTile2;
        public float distance;
    }

    // Initialization
    private void Awake()
    {
        // Rellenamos la reglas de expansión
        expandRules.Add(SpriteRepository.TileType.TILE_TYPE_WATER, SpriteRepository.TileType.TILE_TYPE_NULL);
        expandRules.Add(SpriteRepository.TileType.TILE_TYPE_GRASS, SpriteRepository.TileType.TILE_TYPE_WATER);
        expandRules.Add(SpriteRepository.TileType.TILE_TYPE_MOUNTAIN, SpriteRepository.TileType.TILE_TYPE_GRASS);
        expandRules.Add(SpriteRepository.TileType.TILE_TYPE_FOREST, SpriteRepository.TileType.TILE_TYPE_GRASS);

        // Rellenamos las chances de expansión
        expandChances.Add(SpriteRepository.TileType.TILE_TYPE_WATER, 0);
        expandChances.Add(SpriteRepository.TileType.TILE_TYPE_GRASS, baseGrassChance);
        expandChances.Add(SpriteRepository.TileType.TILE_TYPE_MOUNTAIN, baseMountainChance);
        expandChances.Add(SpriteRepository.TileType.TILE_TYPE_FOREST, baseForestChance);

        // Inicializamos las posiciones según tipo
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_WATER, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_GRASS, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_MOUNTAIN, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_FOREST, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_ROAD, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_FACTORY, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_HANGAR, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_SHIPYARD, new List<Vector3Int>());
        tilePosByType.Add(SpriteRepository.TileType.TILE_TYPE_BUILDING, new List<Vector3Int>());

        // Generamos el sistema de prioridades
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_GRASS, 1);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_FOREST, 2);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_MOUNTAIN, 6);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_WATER, 8);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE, 0);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_ROAD, 1);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_FACTORY, 1);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_HANGAR, 1);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_SHIPYARD, 1);
        priorityByType.Add(SpriteRepository.TileType.TILE_TYPE_BUILDING, 1);

        // Vector to road Tile
        vectorToRoadTile.Add(new Vector3Int(1, 0, 0), SpriteRepository.RoadTileType.ROAD_TILE_TYPE_HORIZONTAL);
        vectorToRoadTile.Add(new Vector3Int(-1, 0, 0), SpriteRepository.RoadTileType.ROAD_TILE_TYPE_HORIZONTAL);
        vectorToRoadTile.Add(new Vector3Int(0, 1, 0), SpriteRepository.RoadTileType.ROAD_TILE_TYPE_VERTICAL);
        vectorToRoadTile.Add(new Vector3Int(0, -1, 0), SpriteRepository.RoadTileType.ROAD_TILE_TYPE_VERTICAL);
        vectorToRoadTile.Add(new Vector3Int(1, 1, 0), SpriteRepository.RoadTileType.ROAD_TILE_TYPE_UP_RIGHT);
        vectorToRoadTile.Add(new Vector3Int(-1, 1, 0), SpriteRepository.RoadTileType.ROAD_TILE_TYPE_UP_LEFT);
        vectorToRoadTile.Add(new Vector3Int(1, -1, 0), SpriteRepository.RoadTileType.ROAD_TILE_TYPE_DOWN_RIGHT);
        vectorToRoadTile.Add(new Vector3Int(-1, -1, 0), SpriteRepository.RoadTileType.ROAD_TILE_TYPE_DOWN_LEFT);
    }

    // Use this for initialization
    void Start ()
    {
        //generateNewMap();
        timer = expositionTime;
    }

    //Expostion Mode
    void Update()
    {
        if (expositionMode)
        {
            timer -= Time.deltaTime;
            
            if (timer < 0)
            {
                timer = expositionTime;
                seedValue++;

                generateNewMap();
            }
        }
    }

    // Generate new world
    public void generateNewMap()
    {
        resetTileByPosDict();
        mapTileDictionary = new Dictionary<Vector3Int, MapTile>();
        Random.InitState(seedValue);

        initTileMap();

        if (autoAdjust)
        {
            seedCount = Mathf.RoundToInt(height * length / 30f);
            int offset = Mathf.RoundToInt(Random.Range(0f, 1f) * seedCount);
            seedCount += offset;

            neutralResourceCount = Mathf.RoundToInt(seedCount * 0.75f);
            alliedResourceCount = Mathf.RoundToInt(neutralResourceCount * 0.33f);
        }


        // Colocamos bloques de hierba
        System.DateTime date = System.DateTime.Now;
        List<Vector3Int> landPositions = placeSeeds();
        for (int i = 0; i < iterations; i++)
        {
            float chanceFactor = 1f / ((i + 1f) / 2f);
            if (old)
                expandLandOpt(ref landPositions, chanceFactor);
            else
                expandTileOpt(ref landPositions, SpriteRepository.TileType.TILE_TYPE_GRASS, chanceFactor);
        }
        report.landPhase = ((float)(System.DateTime.Now - date).TotalMilliseconds);

        // Limpiamos imperfecciones
        cleanImperfections(iterations);

        // Si método old
        if (old)
        {
            // Ponemos montañas y bosques
            placeMountains(iterations + 1);
            placeForests(iterations + 2);
        }
        // En caso contrario
        else
        {
            if(autoAdjust)
            {
                forestSeedCount = Mathf.RoundToInt(countGrassTiles() * 0.1f);
                mountainSeedCount = Mathf.RoundToInt(countGrassTiles() * 0.075f);
            }

            // Ponemos montañas y bosques
            date = System.DateTime.Now;
            List<Vector3Int> mountainPositions = placeSeedsByType(SpriteRepository.TileType.TILE_TYPE_MOUNTAIN, mountainSeedCount);
            
            for (int i = 0; i < mountainIterations; i++)
            {
                float chanceFactor = 1f / ((i + 1f) / 2f);
                expandTileOpt(ref mountainPositions, SpriteRepository.TileType.TILE_TYPE_MOUNTAIN, chanceFactor);
            }
            report.mountainPhase = ((float)(System.DateTime.Now - date).TotalMilliseconds);

            date = System.DateTime.Now;
            List<Vector3Int> forestPositions = placeSeedsByType(SpriteRepository.TileType.TILE_TYPE_FOREST, forestSeedCount);
            for (int i = 0; i < forestIterations; i++)
            {
                float chanceFactor = 1f / ((i + 1f) / 2f);
                expandTileOpt(ref forestPositions, SpriteRepository.TileType.TILE_TYPE_FOREST, chanceFactor);
            }
            report.forestPhase = ((float)(System.DateTime.Now - date).TotalMilliseconds);
        }

        // Colocamos las bases de los dos jugadores
        date = System.DateTime.Now;
        if (oldPlayerBasePlacement)
            placePlayersBase();
        else
            alternativePlacePlayerBase();
            
        report.playerBase = ((float)(System.DateTime.Now - date).TotalMilliseconds);

        // Colocamos las carreteras
        date = System.DateTime.Now;
        if (roads)
            traceRoads();
        report.roadTracing = ((float)(System.DateTime.Now - date).TotalMilliseconds);

        date = System.DateTime.Now;
        if (resources)
        {
            initCostMaps();

            if (oldResourcePlacemente)
                placeSymmetricResources();
            else
            {
                Debug.Log("Enter");
                placeNeutralResources();
                report.neutral = ((float)(System.DateTime.Now - date).TotalMilliseconds);

                date = System.DateTime.Now;
                placeSymmetricResources();
                report.symmetric = ((float)(System.DateTime.Now - date).TotalMilliseconds);
            }
        }
        updateInterfaceValues();
        interfaceController.mapCamera.orthographicSize = Mathf.Max(length, height) * 1.15f;

        PerformanceController.addMapPerformaceReport(report);
    }

    // Grass tiles count function
    int countGrassTiles()
    {
        int total = 0;

        foreach (var tileKey in mapTileDictionary.Keys)
            if (mapTileDictionary[tileKey].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS)
                total++;

        return total;
    }

    // Inicializa el grid con agua
    void initTileMap()
    {
        tileMap.ClearAllTiles();
        for(int x = -length; x < length; x++)
            for(int y = -height; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                MapTile mapTile = createMapTile(pos, SpriteRepository.TileType.TILE_TYPE_WATER);

                // Añadimos estos al diccionario y al TileMap
                mapTileDictionary.Add(pos, mapTile);
                tilePosByType[SpriteRepository.TileType.TILE_TYPE_WATER].Add(pos);
            }
    }

    void resetTileByPosDict()
    {
        foreach(SpriteRepository.TileType type in System.Enum.GetValues(typeof(SpriteRepository.TileType)))
        {
            tilePosByType[type] = new List<Vector3Int>();
        }
    }

    // Coloca las semillas
    List<Vector3Int> placeSeeds()
    {
        // Elegimos las posiciones dónde se colocaran semillas
        List<Vector3Int> seedPositions = new List<Vector3Int>();

        for(int i = 0; i < seedCount; i++)
        {
            int x = Random.Range(-length, length);
            int y = Random.Range(-height, height);
            Vector3Int seedPos = new Vector3Int(x, y, 0);

            if (!seedPositions.Contains(seedPos))
            {
                seedPositions.Add(seedPos);

                MapTile mapTile = createMapTile(seedPos, SpriteRepository.TileType.TILE_TYPE_GRASS);
                mapTileDictionary[seedPos] = mapTile;
                tilePosByType[SpriteRepository.TileType.TILE_TYPE_GRASS].Add(seedPos);
            }
        }

        return seedPositions;
    }

    // Coloca semillas de un tipo en posiciones válidas
    List<Vector3Int> placeSeedsByType(SpriteRepository.TileType type, int typeSeedCount)
    {
        List<Vector3Int> seedPos = new List<Vector3Int>();
        List<Vector3Int> keys = tilePosByType[expandRules[type]];

        for (int i = 0; i < typeSeedCount; i++)
        {
            int index = Random.Range(0, keys.Count);
            Vector3Int pos = keys[index];

            if(!seedPos.Contains(pos))
            {
                updateMapTileType(pos, mapTileDictionary[pos], type);
                //tilePosByType[type].Add(pos);
                seedPos.Add(pos);
            }
        }

        return seedPos;
    }

    // Versión de Legado, itera por todas las celdas, comprobando el tipo de estas
    IEnumerator expandLand(float chanceFactor = 1f, int timeToWait = 0)
    {
        yield return new WaitForSecondsRealtime(1 * timeToWait);

        Dictionary<Vector3Int, MapTile> auxMapTileDictionary = new Dictionary<Vector3Int, MapTile>(); ;
        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);

        // TODO - Optimizar, iterar solo por los valores seed
        foreach (var tileKey in keys)
        {
            if (mapTileDictionary[tileKey].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS)
            {
                List<Vector3Int> neigbours = new List<Vector3Int>();

                for (int x = -1; x < 2; x++)
                    for (int y = -1; y < 2; y++)
                        if (!(y == 0 && x == 0))
                            neigbours.Add(new Vector3Int(x, y, 0) + tileKey);

                foreach (Vector3Int neiPos in neigbours)
                {
                    float randomValue = Random.Range(0f, 1f);

                    if (randomValue < 0.95f * chanceFactor)
                    {
                        if (auxMapTileDictionary.ContainsKey(neiPos))
                            auxMapTileDictionary[neiPos] = createMapTile(neiPos, SpriteRepository.TileType.TILE_TYPE_GRASS);
                        else
                            auxMapTileDictionary.Add(neiPos, createMapTile(neiPos, SpriteRepository.TileType.TILE_TYPE_GRASS));
                    }
                }
            }
            else if(!auxMapTileDictionary.ContainsKey(tileKey))
                auxMapTileDictionary.Add(tileKey, createMapTile(tileKey, mapTileDictionary[tileKey].tileType));
        }

        mapTileDictionary = auxMapTileDictionary;
    }

    // Expande la tierra, tomando como entrada las posiciones de tierra en esta iteracion
    void expandLandOpt(ref List<Vector3Int> landPositions, float chanceFactor = 1f)
    {
        List<Vector3Int>  newLandPositions = new List<Vector3Int>();

        // Iteramos por cada una de las celdas
        foreach(Vector3Int landPosition in landPositions)
        {
            // Cogemos los vecinos
            List<Vector3Int> neigbours = getNeighbourList(landPosition);

            // Para cada uno de los vecinos de la celda
            foreach (Vector3Int neiPos in neigbours)
            {
                float randomValue = Random.Range(0f, 1f);

                // Si supera el umbral, y no pertenece a las que tenemos en esta iteración, la cambiamos a tipo hierba
                if (randomValue < baseGrassChance * chanceFactor)
                {
                    if (!landPositions.Contains(neiPos))
                    {
                        Debug.Log(mapTileDictionary[neiPos].tileType);
                        if (mapTileDictionary[neiPos].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS)
                            continue;

                        updateMapTileType(neiPos, mapTileDictionary[neiPos], SpriteRepository.TileType.TILE_TYPE_GRASS);
                        //StartCoroutine(updateMapTileType(neiPos, mapTileDictionary[neiPos], SpriteRepository.TileType.TILE_TYPE_GRASS, timeToWait));
                        newLandPositions.Add(neiPos);
                    }
                    // En caso contrarioa aumentamos su contador de transformación
                    else
                    {
                        mapTileDictionary[neiPos].upgradeCount++;
                    }
                }
            }
        }

        // Devolvemos únicamente las posiciones nuevas
        landPositions = newLandPositions;
    }

    // Método general, expande por Tipo de casilla
    void expandTileOpt(ref List<Vector3Int> tilePositions, SpriteRepository.TileType type,  float chanceFactor = 1f)
    {
        List<Vector3Int> newTilePositions = new List<Vector3Int>();

        // Iteramos por cada una de las celdas
        foreach (Vector3Int landPosition in tilePositions)
        {
            // Cogemos los vecinos
            List<Vector3Int> neigbours = getNeighbourListByType(landPosition,type);

            // Para cada uno de los vecinos de la celda
            foreach (Vector3Int neiPos in neigbours)
            {
                float randomValue = Random.Range(0f, 1f);

                // Si supera el umbral, y no pertenece a las que tenemos en esta iteración, la cambiamos a tipo hierba
                if (randomValue < expandChances[type] * chanceFactor)
                {
                    if (!tilePositions.Contains(neiPos))
                    {
                        if (mapTileDictionary[neiPos].tileType == type)
                            continue;

                        updateMapTileType(neiPos, mapTileDictionary[neiPos], type);
                        newTilePositions.Add(neiPos);
                    }
                    // En caso contrarioa aumentamos su contador de transformación
                    else
                    {
                        mapTileDictionary[neiPos].upgradeCount++;
                    }
                }
            }
        }

        // Devolvemos únicamente las posiciones nuevas
        tilePositions = newTilePositions;
    }

    // Limpiamos imperfecciones, celdas de hierba con un único vecino
    void cleanImperfections(int timeToWait = 0)
    {
        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);

        foreach (var tileKey in keys)
        {
            if(mapTileDictionary[tileKey].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS)
            {
                List<Vector3Int> neighbours = getNeighbourList(tileKey, true);

                int neiCount = 0;
                foreach(Vector3Int nei in neighbours)
                {
                    if (mapTileDictionary[nei].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS)
                        neiCount++;
                }

                if (neiCount == 0)
                    updateMapTileType(tileKey, mapTileDictionary[tileKey], SpriteRepository.TileType.TILE_TYPE_WATER);
            }
            else if(mapTileDictionary[tileKey].tileType == SpriteRepository.TileType.TILE_TYPE_WATER)
            {
                List<Vector3Int> neighbours = getNeighbourList(tileKey, true);

                int neiCount = 0;
                foreach (Vector3Int nei in neighbours)
                {
                    if (mapTileDictionary[nei].tileType == SpriteRepository.TileType.TILE_TYPE_WATER)
                        neiCount++;
                }

                if (neiCount == 0)
                    updateMapTileType(tileKey, mapTileDictionary[tileKey], SpriteRepository.TileType.TILE_TYPE_GRASS);
            }
        }
    }

    // Coloca las montañas según su factor de upgrade 
    void placeMountains(int timeToWait = 0)
    {
        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);

        foreach (var tileKey in keys)
        {
            float randomValue = Random.Range(0f, 1f);
            Debug.Log(mapTileDictionary[tileKey].upgradeCount);
            if (mapTileDictionary[tileKey].upgradeCount * baseMountainChance >= randomValue)
                updateMapTileType(tileKey, mapTileDictionary[tileKey], SpriteRepository.TileType.TILE_TYPE_MOUNTAIN);
        }
    }

    // Coloca los bosques
    void placeForests(int timeToWait = 0)
    {
        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);

        foreach (var tileKey in keys)
        {
            if(mapTileDictionary[tileKey].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS)
            {
                
                float randomValue = Random.Range(0f, 1f);
                //Debug.Log(randomValue);
                if (randomValue < baseForestChance)
                    updateMapTileType(tileKey, mapTileDictionary[tileKey], SpriteRepository.TileType.TILE_TYPE_FOREST);
            }
        }
    }

    // Colocamos la base de cada jugador
    void placePlayersBase()
    {
        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);

        // Iteramos por las posiciones hasta encontrar un bosque o llanura 
        Vector3Int foundPoint = new Vector3Int();
        foreach (var tileKey in keys)
        {
            if (mapTileDictionary[tileKey].tileType != SpriteRepository.TileType.TILE_TYPE_GRASS
                && mapTileDictionary[tileKey].tileType != SpriteRepository.TileType.TILE_TYPE_FOREST)
                continue;

            // Punto base que hemos encontrado
            foundPoint = tileKey;
            break;
        }

        // Cogemos todos los vecinos a dos bloques de distancia y elegimos de ellos un punto válido al azar
        List<Vector3Int> validPoints = getNeighbourListOfType(foundPoint, SpriteRepository.TileType.TILE_TYPE_GRASS, false, 2);
        List<Vector3Int> validForestPoints = getNeighbourListOfType(foundPoint, SpriteRepository.TileType.TILE_TYPE_FOREST, false, 2);
        validPoints.AddRange(validForestPoints);
        int randomIndex = Random.Range(0, validPoints.Count);

        // Este será el punto de nuestra primera base
        Vector3Int playerBasePoint = validPoints[randomIndex];
        updateMapTileType(playerBasePoint, mapTileDictionary[playerBasePoint], SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE);

        // Hallamos el de la otra base, a una distancia máxima de la diagonal del mapa
        float startDistance = Mathf.Sqrt(height * height + length * length);
        List<Vector3Int> validEnemyBasePoints = new List<Vector3Int>();

        // Iteramos hasta encontrar uno válido
        while (validEnemyBasePoints.Count == 0)
        {
            foreach (var tileKey in keys)
            {
                if (mapTileDictionary[tileKey].tileType != SpriteRepository.TileType.TILE_TYPE_GRASS
                   && mapTileDictionary[tileKey].tileType != SpriteRepository.TileType.TILE_TYPE_FOREST)
                    continue;

                float distance = (tileKey - playerBasePoint).magnitude;

                if (distance > startDistance)
                    validEnemyBasePoints.Add(tileKey);
            }
            startDistance -= 1f;
        }
        randomIndex = Random.Range(0, validEnemyBasePoints.Count);

        // Enemy PlayerBase point - TODO Elegir uno aleatorio de sus vecinos
        Vector3Int enemyPlayerBasePoint = validEnemyBasePoints[randomIndex];
        updateMapTileType(enemyPlayerBasePoint, mapTileDictionary[enemyPlayerBasePoint], SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE);
    }

    // Método alteranito que maximiza la distancia entre las dos bases
    void alternativePlacePlayerBase()
    {
        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);
        List<DistanceMap> distances = new List<DistanceMap>();

        foreach(Vector3Int tile1 in keys)
        {
            foreach (Vector3Int tile2 in keys)
            {
                if (mapTileDictionary[tile1].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS /*||
                    mapTileDictionary[tile1].tileType == SpriteRepository.TileType.TILE_TYPE_FOREST*/)
                {
                    if (mapTileDictionary[tile2].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS /*||
                    mapTileDictionary[tile2].tileType == SpriteRepository.TileType.TILE_TYPE_FOREST*/)
                    {
                        DistanceMap distanceMap = new DistanceMap();
                        distanceMap.mapTile1 = mapTileDictionary[tile1];
                        distanceMap.mapTile2 = mapTileDictionary[tile2];
                        distanceMap.distance = (tile1 - tile2).magnitude;
                        distances.Add(distanceMap);
                    }
                }
            }
        }

        distances.Sort((x, y) => y.distance.CompareTo(x.distance));
        DistanceMap maxDistance = distances[0];    

        List<MapTile> playerBases = new List<MapTile>();
        playerBases.Add(maxDistance.mapTile1);
        playerBases.Add(maxDistance.mapTile2);
        //Debug.Log("La distancia es " + maxDistance.distance);

        foreach (MapTile playerBase in playerBases)
        {
            List<Vector3Int> neis = getNeighbourList(playerBase.position, false, 3);
            List<MapTile> validNeis = new List<MapTile>();
            neis.Add(playerBase.position);

            foreach(Vector3Int nei in neis)
            {
                //Debug.Log("El tipo es " + mapTileDictionary[nei].tileType);
                if (mapTileDictionary[nei].tileType == SpriteRepository.TileType.TILE_TYPE_GRASS ||
                    mapTileDictionary[nei].tileType == SpriteRepository.TileType.TILE_TYPE_FOREST)
                    validNeis.Add(mapTileDictionary[nei]);
            }

            int randomIndex = Random.Range(0, validNeis.Count);
            //Debug.Log("el index random es " + randomIndex);
            updateMapTileType(validNeis[randomIndex].position, validNeis[randomIndex], SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE);
        }

    }

    // Trace Road between playerBases
    void traceRoads()
    {
        List<Vector3Int> playerBases = tilePosByType[SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE];
        List<MapTile> roadTiles = PathFindingAlg.Astar(mapTileDictionary[playerBases[0]], mapTileDictionary[playerBases[1]], mapTileDictionary);

        foreach(MapTile tile in roadTiles)
            updateMapTileType(tile.position, tile, SpriteRepository.TileType.TILE_TYPE_ROAD);

        foreach (MapTile tile in roadTiles)
            updateRoadTile(tile);
    }

    // Trace roads between tiles
    void traceRoads(MapTile startingPoint, MapTile finishPoint)
    {
        List<MapTile> roadTiles = PathFindingAlg.Astar(startingPoint, finishPoint, mapTileDictionary);

        foreach (MapTile tile in roadTiles)
            updateMapTileType(tile.position, tile, SpriteRepository.TileType.TILE_TYPE_ROAD);

        foreach (MapTile tile in roadTiles)
            updateRoadTile(tile);
    }

    // Place units
    void placeUnits()
    {
        GameObject tank = GameObject.Instantiate(tankFab, new Vector2(tilePosByType[SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE][0].x, tilePosByType[SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE][0].y), Quaternion.identity);
        tank.GetComponent<SpriteRenderer>().flipX = true;
    }

    void initCostMaps()
    {
        List<Vector3Int> playerBases = tilePosByType[SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE];
        costMapsByCell = new Dictionary<Vector3Int, CostMap>();

        foreach (Vector3Int playerBase in playerBases)
        {
            CostMap costMap = new CostMap();
            costMap.playerBase = mapTileDictionary[playerBase];
            costMap.grassTilesByCost = new Dictionary<int, List<MapTile>>();
            PathFindingAlg.Dijkstra(mapTileDictionary[playerBase], mapTileDictionary, ref costMap.grassTilesByCost);
            if (!costMapsByCell.ContainsKey(playerBase))
                costMapsByCell.Add(playerBase, costMap);
        }
    }

    // Coloca recursos simétricos
    void placeSymmetricResources()
    {

        List<Vector3Int> playerBases = tilePosByType[SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE];
        List<List<int>> playerBaseCosts = new List<List<int>>();
        foreach (Vector3Int playerBase in playerBases)
        {
            playerBaseCosts.Add(new List<int>(costMapsByCell[playerBase].grassTilesByCost.Keys));
        }

        // Conseguimos los costes comunes
        List<int> commonCosts = new List<int>();

        foreach(List<int> playerCosts in playerBaseCosts)
        {
            if (commonCosts.Count == 0)
            {
                commonCosts = playerCosts;
                continue;
            }
            commonCosts = commonCosts.Intersect(playerCosts).ToList<int>();
        }
        commonCosts.Sort((x, y) => x.CompareTo(y));

        // Comprobamos si hay costes comunes
        if (commonCosts.Count == 0)
        {
            //Debug.Log("No hay costes comunes!!!");
            return;
        }

        // Ponemos fábricas aliadas
        for (int i = 0; i < alliedResourceCount; i++)
        {
            int randomCost = commonCosts[0];

            //Debug.Log("El coste elegido es " + randomCost);

            foreach (Vector3Int playerBase in playerBases)
            {
                CostMap baseCostMap = costMapsByCell[playerBase];
                List<MapTile> tilesByCost = baseCostMap.grassTilesByCost[randomCost];
                int randomListIndex = Random.Range(0, tilesByCost.Count);

                if(tilesByCost.ElementAtOrDefault(randomListIndex) == null)
                {
                    //Debug.Log("Aquí paso algo");
                    break;
                }

                updateMapTileType(tilesByCost[randomListIndex].position, tilesByCost[randomListIndex], SpriteRepository.TileType.TILE_TYPE_FACTORY);
                tilesByCost.RemoveAt(randomListIndex);

                if (tilesByCost.Count == 0)
                {
                    //Debug.Log("Ahora esta vacío");
                    commonCosts.RemoveAt(0);
                }
            }
        }

        List<Vector3Int> neutralPositions = new List<Vector3Int>(tilePosByType[SpriteRepository.TileType.TILE_TYPE_BUILDING]);
        List<float> neutralCosts = new List<float>();
        List<float> neutralRates = new List<float>();

        foreach (Vector3Int playerBase in playerBases)
        {
            foreach (Vector3Int neutral in neutralPositions)
            {
                neutralCosts.Add(mapTileDictionary[neutral].distanceToPlayerBase[mapTileDictionary[playerBase]]);
                neutralRates.Add(mapTileDictionary[neutral].getSimmetricRate());
            }
        }

        float minCost = neutralCosts.Average();
        float minRate = neutralRates.Min();

        commonCosts.RemoveAll(x => x > minCost);
        commonCosts.RemoveAll(x => x < minCost / 2);

        //Debug.Log("El coste mínimo a una base de los recursos neutrales es " + minCost);

        bool adjustingValue = false;
        int previousCost = -1;
        bool limitReached = false;
        // Ponemos fábricas neutrales
        for (int i = 0; i < neutralResourceCount; i++)
        {
            previousCost = -1;
            foreach (Vector3Int playerBase in playerBases)
            {
                do {

                    List<MapTile> validTiles = new List<MapTile>();
                    foreach (MapTile mapTile in mapTileDictionary.Values)
                    {
                        if (mapTile.tileType == SpriteRepository.TileType.TILE_TYPE_GRASS || mapTile.tileType == SpriteRepository.TileType.TILE_TYPE_FOREST)
                        {
                            if (!mapTile.buildable)
                                continue;
                            if (previousCost == -1)
                            {
                                if (mapTile.distanceToPlayerBase[mapTileDictionary[playerBase]] < minCost * minRate && mapTile.distanceToPlayerBase[mapTileDictionary[playerBase]] > 2)
                                    validTiles.Add(mapTile);
                            }
                            else
                            {
                                if (mapTile.distanceToPlayerBase[mapTileDictionary[playerBase]] == previousCost)
                                    validTiles.Add(mapTile);
                            }
                        }
                    }

                    int randomIndex = Random.Range(0, validTiles.Count);
                    if (validTiles.Count == 0)
                    {
                        previousCost--;
                        // si llegamos al tope recomenzamos por el máximo

                        if (previousCost <= 0)
                        {
                            if (limitReached)
                                return;
                            else
                            {
                                previousCost = Mathf.RoundToInt(minCost) - 1;
                                limitReached = true;
                            }
                        }
                        adjustingValue = true;
                        continue;
                    }

                    MapTile chosenTile = validTiles[randomIndex];

                    List<Vector3Int> neis = getNeighbourList(chosenTile.position);

                    foreach (Vector3Int nei in neis)
                        mapTileDictionary[nei].buildable = false;


                    List<SpriteRepository.TileType> possibleResources = new List<SpriteRepository.TileType>();

                    possibleResources.Add(SpriteRepository.TileType.TILE_TYPE_HANGAR);
                    possibleResources.Add(SpriteRepository.TileType.TILE_TYPE_FACTORY);
                    if (getNeighbourListOfType(chosenTile.position, SpriteRepository.TileType.TILE_TYPE_WATER).Count > 0)
                    {
                        // Double probability
                        possibleResources.Add(SpriteRepository.TileType.TILE_TYPE_SHIPYARD);
                        possibleResources.Add(SpriteRepository.TileType.TILE_TYPE_SHIPYARD);
                    }

                    int randomBuilding = Random.Range(0, possibleResources.Count);

                    updateMapTileType(chosenTile.position, chosenTile, possibleResources[randomBuilding]);

                    previousCost = Mathf.RoundToInt(chosenTile.distanceToPlayerBase[mapTileDictionary[playerBase]]);
                    //Debug.Log("El coste elegido es" + previousCost);
                    adjustingValue = false;

                } while(adjustingValue);
            }
        }
        //Debug.Log("Salí");
    }

    // Coloca recursos equdistantes/neutrales
    void placeNeutralResources()
    {
        // Generamos los mapas de costes
        List<Vector3Int> playerBases = tilePosByType[SpriteRepository.TileType.TILE_TYPE_PLAYER_BASE];

        // Una vez tenemos las distancias calculadas
        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);
        List<MapTile> validTiles = new List<MapTile>();

        // Almacenamos las tiles por rate
        foreach(Vector3Int pos in keys)
        {
            // Si no es un bloque de hierba o bosque, no interesa
            if (mapTileDictionary[pos].tileType != SpriteRepository.TileType.TILE_TYPE_GRASS && mapTileDictionary[pos].tileType != SpriteRepository.TileType.TILE_TYPE_FOREST)
                continue;

            validTiles.Add(mapTileDictionary[pos]);
        }

        // Colocamos recursos simetricos
        validTiles.Sort((x, y) => y.getSimmetricRate().CompareTo(x.getSimmetricRate()));

        int limit = Mathf.Min(neutralResourceCount, validTiles.Count);

        for(int i = 0; i < limit; i++)
        {
            //Debug.Log("El rate " + i + " es " + validTiles[i].getSimmetricRate());
            updateMapTileType(validTiles[i].position, validTiles[i], SpriteRepository.TileType.TILE_TYPE_BUILDING);

            List<Vector3Int> grassNeis = getNeighbourListOfType(validTiles[i].position, SpriteRepository.TileType.TILE_TYPE_GRASS, false, 2);
            List<Vector3Int> forestNeis = getNeighbourListOfType(validTiles[i].position, SpriteRepository.TileType.TILE_TYPE_FOREST, false, 2);
            List<Vector3Int> validNeis = grassNeis.Concat(forestNeis).ToList<Vector3Int>();

            int randomIndex = Random.Range(0, validNeis.Count);
            Vector3Int randomPos = validNeis[randomIndex];

            //Debug.Log("El rate aleatorio" + i + " es " + mapTileDictionary[randomPos].getSimmetricRate());
            //updateMapTileType(mapTileDictionary[randomPos].position, mapTileDictionary[randomPos], SpriteRepository.TileType.TILE_TYPE_HANGAR);

            // Eliminamos en un área alrededor de esa
            foreach(Vector3Int nei in validNeis)
            {
                validTiles.Remove(mapTileDictionary[nei]);
            }
            validTiles.Remove(validTiles[i]);
        }

        /*
        // Colocamos recursos asimetricos
        List<MapTile> asymPlayer1Tiles = new List<MapTile>(validTiles);
        List<MapTile> asymPlayer2Tiles = new List<MapTile>(validTiles);

        asymPlayer1Tiles.RemoveAll(u => u.getSimmetricRate() > 0.75f);
        asymPlayer2Tiles.RemoveAll(u => u.getSimmetricRate() > 0.75f);

        asymPlayer1Tiles.RemoveAll(u => u.distanceToPlayerBase.Values.First() > u.distanceToPlayerBase.Values.Last());
        asymPlayer2Tiles.RemoveAll(u => u.distanceToPlayerBase.Values.First() < u.distanceToPlayerBase.Values.Last());

        asymPlayer1Tiles.Sort((x, y) => y.getSimmetricRate().CompareTo(x.getSimmetricRate()));
        asymPlayer2Tiles.Sort((x, y) => y.getSimmetricRate().CompareTo(x.getSimmetricRate()));

        limit = Mathf.Min(neutralResourceCount/2, validTiles.Count);

        for (int i = 0; i < limit; i++)
        {
            if (i % 2 == 0)
                validTiles = asymPlayer1Tiles;
            else
                validTiles = asymPlayer2Tiles;

            Debug.Log("El rate asym" + i + " es " + validTiles[i].getSimmetricRate());
            Debug.Log("La distancia a base 1 es " + validTiles[i].distanceToPlayerBase[mapTileDictionary[playerBases[0]]]);
            Debug.Log("La distancia a base 2 es " + validTiles[i].distanceToPlayerBase[mapTileDictionary[playerBases[1]]]);
            updateMapTileType(validTiles[i].position, validTiles[i], SpriteRepository.TileType.TILE_TYPE_FACTORY);

            List<Vector3Int> grassNeis = getNeighbourListOfType(validTiles[i].position, SpriteRepository.TileType.TILE_TYPE_GRASS, false, 2);
            List<Vector3Int> forestNeis = getNeighbourListOfType(validTiles[i].position, SpriteRepository.TileType.TILE_TYPE_FOREST, false, 2);
            List<Vector3Int> validNeis = grassNeis.Concat(forestNeis).ToList<Vector3Int>();

            int randomIndex = Random.Range(0, validNeis.Count);
            Vector3Int randomPos = validNeis[randomIndex];

            //Debug.Log("El rate aleatorio asym" + i + " es " + mapTileDictionary[randomPos].getSimmetricRate());
            //updateMapTileType(mapTileDictionary[randomPos].position, mapTileDictionary[randomPos], SpriteRepository.TileType.TILE_TYPE_HANGAR);

            // Eliminamos en un área alrededor de esa
            foreach (Vector3Int nei in validNeis)
            {
                validTiles.Remove(mapTileDictionary[nei]);
            }
            validTiles.Remove(validTiles[i]);
        }
        */
    }

    // Creamos una celda según el tipo indicado
    MapTile createMapTile(Vector3Int pos, SpriteRepository.TileType type)
    {
        Debug.Log("Creando celda tipo " + type);

        // Creamos Tile y le añadimos el sprite
        Tile tile = ScriptableObject.CreateInstance("Tile") as Tile;
        tile.sprite = SpriteRepository.spriteRepo[type];

        // Creamos nuestro objeto Tile
        MapTile mapTile = new MapTile(type, tile, pos);

        tileMap.SetTile(pos, tile);

        return mapTile;
    }

    // Actualizamos el tipo de una celda y su sprite
    void updateMapTileType(Vector3Int pos, MapTile mapTile, SpriteRepository.TileType type)
    {
        tilePosByType[mapTile.tileType].Remove(pos);
        tilePosByType[type].Add(pos);

        mapTile.setTileType(type);
        tileMap.SetTile(pos, mapTile.tile);
    }

    // Corutina - Actualizamos el tipo de una celda y su sprite
    IEnumerator updateMapTileType(Vector3Int pos, MapTile mapTile, SpriteRepository.TileType type, int timeToWait)
    {
        tilePosByType[mapTile.tileType].Remove(pos);
        tilePosByType[type].Add(pos);

        Debug.Log("Waiting for " + 1 * timeToWait);
        yield return new WaitForSecondsRealtime(1 * timeToWait);
        mapTile.setTileType(type);
        tileMap.SetTile(pos, mapTile.tile);
    }

    // Actualizamos el sprite de una carretera en función de sus carreteras vecinas
    void updateRoadTile(MapTile roadTile)
    {
        if (roadTile.tileType != SpriteRepository.TileType.TILE_TYPE_ROAD)
            return;

        // Cogemos las carreteras vecinas en cruz
        List<Vector3Int> roadNeiTiles = getNeighbourListOfType(roadTile.position, SpriteRepository.TileType.TILE_TYPE_ROAD, true);

        // Por ahora nada
        if (roadNeiTiles.Count > 2 || roadNeiTiles.Count <= 0)
            return;

        Vector3Int vector = new Vector3Int(0, 0, 0);

        foreach (Vector3Int roadNei in roadNeiTiles)
        {
            vector += (roadNei - roadTile.position);
        }

        if(roadNeiTiles.Count == 1)
            roadTile.setRoadTileType(vectorToRoadTile[vector]);
        else
        {
            // Si la suma da nula
            if(vector.x == 0 && vector.y == 0)
            {
                vector = roadTile.position - roadNeiTiles[0];
                roadTile.setRoadTileType(vectorToRoadTile[vector]);
            }
            else
                roadTile.setRoadTileType(vectorToRoadTile[vector]);
        }

        tileMap.SetTile(roadTile.position, roadTile.tile);
    }

    public void updateInterfaceValues()
    {
        interfaceController.seedInputField.text = seedValue.ToString();
        interfaceController.widthInputField.text = length.ToString();
        interfaceController.depthInputField.text = height.ToString();
        interfaceController.mapCamera.orthographicSize = Mathf.Max(length, height);

        interfaceController.landSeedCountInputField.text = seedCount.ToString();
        interfaceController.forestSeedCountInputField.text = forestSeedCount.ToString();
        interfaceController.mountainSeedCountInputField.text = mountainSeedCount.ToString();

        interfaceController.alliedResourceCountInputField.text = alliedResourceCount.ToString();
        interfaceController.neutralResourceCountInputField.text = neutralResourceCount.ToString();
        
    }

    // Devuelve la lista de  vecinos
    List<Vector3Int> getNeighbourList(Vector3Int pos, bool inCross = false, int distance = 1)
    {
        // Cogemos los vecinos
        List<Vector3Int> neigbours = new List<Vector3Int>();

        for (int x = -distance; x < distance + 1; x++)
            for (int y = -distance; y < distance + 1; y++)
                // Cogemos en vecinos de matriz 3x3
                if (!(y == 0 && x == 0) && !inCross && mapTileDictionary.ContainsKey(new Vector3Int(x, y, 0) + pos))
                        neigbours.Add(new Vector3Int(x, y, 0) + pos);
                // Cogemos vecinos en cruz
                else if(inCross && mapTileDictionary.ContainsKey(new Vector3Int(x, y, 0) + pos) && Mathf.Abs(x) + Mathf.Abs(y) == 1)
                    neigbours.Add(new Vector3Int(x, y, 0) + pos);

        return neigbours;
    }

    // Devuelve lista de vecinos sobre los que se puede expandir
    List<Vector3Int> getNeighbourListByType(Vector3Int pos, SpriteRepository.TileType type, bool inCross = false)
    {
        // Cogemos los vecinos
        List<Vector3Int> neigbours = new List<Vector3Int>();

        for (int x = -1; x < 2; x++)
            for (int y = -1; y < 2; y++)
                if (mapTileDictionary.ContainsKey(new Vector3Int(x, y, 0) + pos))
                {
                    Vector3Int newPos = new Vector3Int(x, y, 0) + pos;

                    // Solo contamos como vecinos, las celdas sobre las que nos podemos expandir
                    if (mapTileDictionary[newPos].tileType == expandRules[type])
                    {

                        // Cogemos en vecinos de matriz 3x3
                        if (!(y == 0 && x == 0) && !inCross)
                            neigbours.Add(new Vector3Int(x, y, 0) + pos);
                        // Cogemos vecinos en cruz
                        else if (inCross && Mathf.Abs(x) + Mathf.Abs(y) == 1)
                            neigbours.Add(new Vector3Int(x, y, 0) + pos);
                    }
                }

        return neigbours;
    }

    // Devuelve vecinos de la celda dada del tipo dado
    List<Vector3Int> getNeighbourListOfType(Vector3Int pos, SpriteRepository.TileType type, bool inCross = false, int distance = 1)
    {
        // Cogemos los vecinos
        List<Vector3Int> neigbours = new List<Vector3Int>();

        for (int x = -distance; x < distance + 1; x++)
            for (int y = -distance; y < distance + 1; y++)

                if (!(y == 0 && x == 0) && mapTileDictionary.ContainsKey(new Vector3Int(x, y, 0) + pos))
                    if (mapTileDictionary[new Vector3Int(x, y, 0) + pos].tileType == type)
                    {
                        // Cogemos en cruz
                        if (inCross)
                        {
                            if (Mathf.Abs(x) + Mathf.Abs(y) == distance)
                                neigbours.Add(new Vector3Int(x, y, 0) + pos);
                        }
                        // Cogemos en vecinos de matriz 3x3
                        else
                            neigbours.Add(new Vector3Int(x, y, 0) + pos);
                    }

        return neigbours;
    }
}
