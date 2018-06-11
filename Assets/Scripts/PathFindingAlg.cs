using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingAlg
{
    public PathFindingAlg()
    {
    }

    // Devuelve las casilla que componen el camino entre principio y fin. Excluyendo estas.
    public static List<MapTile> Dijkstra(MapTile firstPoint, MapTile endPoint, Dictionary<Vector3Int, MapTile> mapTileDictionary)
    {
        initGraph(mapTileDictionary);
        
        firstPoint.MinCostToStart = 0;
        List<MapTile> prioQueue = new List<MapTile>();
        prioQueue.Add(firstPoint);

        do
        {
            prioQueue.Sort((x, y) => x.MinCostToStart.CompareTo(y.MinCostToStart));
            MapTile node = prioQueue[0];
            prioQueue.Remove(node);

            List<MapTile.Connection> connections = node.connections;
            connections.Sort((x, y) => x.cost.CompareTo(y.cost));

            foreach (MapTile.Connection cnn in node.connections)
            {
                MapTile childNode = cnn.ConnectedNode;
                if (childNode.visited)
                    continue;

                if (childNode.MinCostToStart == -1 || node.MinCostToStart + cnn.cost < childNode.MinCostToStart)
                {
                    childNode.MinCostToStart = node.MinCostToStart + cnn.cost;
                    childNode.nearestTileToStart = node;
                    if (!prioQueue.Contains(childNode))
                        prioQueue.Add(childNode);
                }
            }
            node.visited = true;

            if (node.position == endPoint.position)
                break;

        } while (prioQueue.Count != 0);

        List<MapTile> roadPath = new List<MapTile>();
        BuildShortestPath(ref roadPath, endPoint);
        roadPath.Remove(firstPoint);
        roadPath.Remove(endPoint);

        return roadPath;
    }

    // Calcula todo el mapa de costes 
    public static void Dijkstra(MapTile firstPoint, Dictionary<Vector3Int, MapTile> mapTileDictionary, ref Dictionary<int, List<MapTile>> grassTilesByCost)
    {
        initGraph(mapTileDictionary);

        firstPoint.MinCostToStart = 0;
        List<MapTile> prioQueue = new List<MapTile>();
        prioQueue.Add(firstPoint);

        do
        {
           
            prioQueue.Sort((x, y) => x.MinCostToStart.CompareTo(y.MinCostToStart));
            MapTile node = prioQueue[0];
            prioQueue.Remove(node);
            List<MapTile.Connection> connections = node.connections;
            connections.Sort((x, y) => x.cost.CompareTo(y.cost));

            foreach (MapTile.Connection cnn in node.connections)
            {
                MapTile childNode = cnn.ConnectedNode;
                if (childNode.visited)
                    continue;
                

                if (childNode.MinCostToStart == -1 || node.MinCostToStart + cnn.cost < childNode.MinCostToStart)
                {
                    childNode.MinCostToStart = node.MinCostToStart + cnn.cost;
                    childNode.nearestTileToStart = node;
                    if (!prioQueue.Contains(childNode))
                        prioQueue.Add(childNode);
                }
            }
            node.visited = true;
            

        } while (prioQueue.Count != 0);

        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);

        foreach (Vector3Int index in keys)
        {
            MapTile node = mapTileDictionary[index];

            if (node.tileType == SpriteRepository.TileType.TILE_TYPE_GRASS || node.tileType == SpriteRepository.TileType.TILE_TYPE_FOREST)
            { 
                List<MapTile> neigh = getNeighbourList(node, mapTileDictionary);

                // Nueva forma
                node.distanceToPlayerBase.Add(firstPoint, 0);

                // Comprobación de accesibilidad
                int validCount = 0;
                foreach(MapTile nei in neigh)
                {
                    if (nei.tileType == SpriteRepository.TileType.TILE_TYPE_MOUNTAIN || nei.tileType == SpriteRepository.TileType.TILE_TYPE_WATER)
                        validCount++;
                }

                // Si no está en una posición válida volvemos
                if (validCount == neigh.Count)
                    continue;

                // Nueva forma
                node.distanceToPlayerBase[firstPoint]= node.MinCostToStart;

                //Debug.Log("Añadiendo node " + node.position + " con coste " + node.MinCostToStart);

                // Si la celda es tipo hierba la añadimos normal, en caso de bosque restamos uno al coste
                List<MapTile> grassTiles = new List<MapTile>();
                if (node.tileType == SpriteRepository.TileType.TILE_TYPE_GRASS)
                {
                    if (!grassTilesByCost.ContainsKey(node.MinCostToStart))
                    {
                        grassTiles.Add(node);
                        grassTilesByCost.Add(node.MinCostToStart, grassTiles);
                    }
                    else
                        grassTilesByCost[node.MinCostToStart].Add(node);
                }
                else
                {
                    if (!grassTilesByCost.ContainsKey(node.MinCostToStart - 1))
                    {
                        grassTiles.Add(node);
                        grassTilesByCost.Add(node.MinCostToStart - 1, grassTiles);
                    }
                    else
                        grassTilesByCost[node.MinCostToStart - 1].Add(node);
                }
            }
        }

        return;
    }

    // Devuelve las casillas que componenen el camnino entre principio a fin, utiliza la distancia recta hasta el final para desempatar
    public static List<MapTile> Astar(MapTile firstPoint, MapTile endPoint, Dictionary<Vector3Int, MapTile> mapTileDictionary)
    {
        initGraph(mapTileDictionary);

        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);

        foreach (var tileKey in keys)
        {
            mapTileDictionary[tileKey].StraightLineDistanceToEnd = (tileKey - endPoint.position).magnitude;
        }

        firstPoint.MinCostToStart = 0;
        List<MapTile> prioQueue = new List<MapTile>();
        prioQueue.Add(firstPoint);

        do
        {
            prioQueue.Sort((x, y) => (x.MinCostToStart + x.StraightLineDistanceToEnd).CompareTo(y.MinCostToStart + y.StraightLineDistanceToEnd));
            MapTile node = prioQueue[0];
            prioQueue.Remove(node);

            List<MapTile.Connection> connections = node.connections;
            connections.Sort((x, y) => x.cost.CompareTo(y.cost));

            foreach (MapTile.Connection cnn in node.connections)
            {
                MapTile childNode = cnn.ConnectedNode;
                if (childNode.visited)
                    continue;

                if (childNode.MinCostToStart == -1 || node.MinCostToStart + cnn.cost < childNode.MinCostToStart)
                {
                    childNode.MinCostToStart = node.MinCostToStart + cnn.cost;
                    childNode.nearestTileToStart = node;
                    if (!prioQueue.Contains(childNode))
                        prioQueue.Add(childNode);
                }
            }
            node.visited = true;

            if (node.position == endPoint.position)
                break;

        } while (prioQueue.Count != 0);

        List<MapTile> roadPath = new List<MapTile>();
        BuildShortestPath(ref roadPath, endPoint);
        roadPath.Remove(firstPoint);
        roadPath.Remove(endPoint);

        return roadPath;
    }

    // Devuelve la lista de  vecinos, en cruz
    public static List<MapTile> getNeighbourList(MapTile tile, Dictionary<Vector3Int, MapTile> mapTileDictionary,int distance = 1)
    {
        // Cogemos los vecinos
        List<MapTile> neigbours = new List<MapTile>();

        for (int x = -distance; x < distance + 1; x++)
            for (int y = -distance; y < distance + 1; y++)
                // Cogemos vecinos en cruz
                if (mapTileDictionary.ContainsKey(new Vector3Int(x, y, 0) + tile.position) && Mathf.Abs(x) + Mathf.Abs(y) == 1)
                {
                    MapTile nei = mapTileDictionary[new Vector3Int(x, y, 0) + tile.position];
                    neigbours.Add(nei);
                }

        return neigbours;
    }

    // Inicializa el grafo con los datos de las conexiones
    static void initGraph(Dictionary<Vector3Int, MapTile> mapTileDictionary)
    {
        List<Vector3Int> keys = new List<Vector3Int>(mapTileDictionary.Keys);

        foreach (var tileKey in keys)
        {
            MapTile tile = mapTileDictionary[tileKey];
            tile.MinCostToStart = -1;
            tile.nearestTileToStart = null;
            tile.StraightLineDistanceToEnd = -1;
            tile.visited = false;

            List<MapTile> neighbours = getNeighbourList(tile, mapTileDictionary);

            foreach(MapTile nei in neighbours)
            {
                MapTile.Connection connection = new MapTile.Connection();
                connection.cost = nei.priority;
                connection.ConnectedNode = nei;
                tile.connections.Add(connection);
            }
        }
    }

    // Función recursiva para recorrer la lista enlazada
    private static void BuildShortestPath(ref List<MapTile> list, MapTile node)
    {
        if (node.nearestTileToStart == null)
            return;
        list.Add(node.nearestTileToStart);
        BuildShortestPath(ref list, node.nearestTileToStart);
    }
}
