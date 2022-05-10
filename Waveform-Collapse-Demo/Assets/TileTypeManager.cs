using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileTypeManager : MonoBehaviour
{
    public Material defaultMaterial;

    [System.Serializable]
    public class TileType
    {
        public string name;
        public Material material;
        public int proportionWeight;
        public List<WeightLink> allowedNeighbors;
    }

    public List<List<int>> allowedNeighborCache = new List<List<int>>();

    public List<TileType> tileTypes;

    private List<int> tileTypeProportions;
    private int placed;
    private int proportionSum = 0;

    public static TileTypeManager global;

    // Start is called before the first frame update
    void Start()
    {
        global = this;

        tileTypeProportions = new List<int>();

        foreach (TileType type in tileTypes)
        {
            List<int> allowed = type.allowedNeighbors.Select(x => x.typeId).ToList();
            allowedNeighborCache.Add(allowed);

            tileTypeProportions.Add(0);

            proportionSum += type.proportionWeight;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static List<int> GetFullRandom()
    {
        List<int> ret = new List<int>();
        for(int i=0; i < global.tileTypes.Count; i++)
        {
            ret.Add(i);
        }
        return ret;
    }

    public static List<WeightLink> GetEvenWeightList(List<int> allowed)
    {
        List<WeightLink> ret = new List<WeightLink>();
        for(int i=0; i < allowed.Count;i++)
        {
            ret.Add(new WeightLink { typeId = allowed[i], weight = 1});
        }
        return ret;
    }

    public static int GetTypesAmount()
    {
        return global.tileTypes.Count;
    }

    public static Material GetMaterial(int type)
    {
        if (type == -1)
            return global.defaultMaterial;
        return global.tileTypes[type].material;
    }

    public static List<int> GetAllowedTileTypes(int type)
    {
        //return global.tileTypes[type].allowedNeighbors.Select(x => x.typeId).ToList();
        return global.allowedNeighborCache[type];
    }

    public static int GetClosestPossible(List<int> neighbors)
    {
        int total = 0;
        int counter = 0;
        foreach(int i in neighbors)
        {
            if (i != -1)
            {
                total += i;
                counter++;
            }
        }
        if (counter != 0)
        {
            return total / counter;
        }
        else
        {
            return global.tileTypes.Count / 2;
        }
    }

    public static List<int> GetAllowedTileTypes(WaveformTile tile, bool debug = false)
    {
        return GetAllowedTileTypes(MapCreation.GetNeighbors(tile), debug);
    }

    public static List<int> GetAllowedTileTypes(List<WaveformTile> neighbors, bool debug = false)
    {
        IEnumerable<int> allowed = GetFullRandom();
        foreach (var neigh in neighbors)
        {
            if (neigh.tileType == -1 && (neigh.allowedTypes.Count == global.tileTypes.Count || neigh.allowedTypes.Count == 0))
            {
                //EITHER:
                //All types are allowed by this neighbor, so continue
                //No types are allowed by this neighbor, which means asserting the tile in question will not change anything
                continue;
            }
            else
            {
                /*
                if (debug)
                {
                    print("POSSIBLE NEIGHBORS FROM " + neigh.transform.position + " | " + string.Join(",", neigh.GetPossibleNeighbors()));
                }
                */
                allowed = allowed.Where(x => neigh.GetPossibleNeighbors().Contains(x));
            }
        }

        return allowed.ToList();
    }

    public static List<WeightLink> GetWeightList(WaveformTile tile, List<int> allowed)
    {
        List<WaveformTile> neighbors = MapCreation.GetNeighbors(tile);
        List<WeightLink> ret = new List<WeightLink>();
        foreach(int type in allowed)
        {
            WeightLink possibility = new WeightLink { typeId = type, weight = 0 };
            foreach (WaveformTile neigh in neighbors)
            {
                if (neigh.tileType == -1)
                {
                    continue;
                }
                foreach(WeightLink influence in global.tileTypes[neigh.tileType].allowedNeighbors)
                {
                    if (influence.typeId == type)
                    {
                        possibility.weight += influence.weight;
                    }
                }
            }
            if (possibility.weight > 0)
            {
                ret.Add(possibility);
            }
        }
        if (ret.Count == 0)
        {
            return GetEvenWeightList(allowed);
        }
        return ret;
    }

    public static int GetTileType(string name)
    {
        for(int i=0; i < global.tileTypes.Count; i++)
        {
            if (global.tileTypes[i].name == name)
                return i;
        }
        return -1;
    }

    public static void ResetProportions()
    {
        for(int i=0; i < global.tileTypes.Count; i++)
        {
            global.tileTypeProportions[i] = 0;
        }
        global.placed = 0;
    }

    public static void Placed(int type)
    {
        global.placed++;
        global.tileTypeProportions[type]++;
    }

    public static float GetProportionalFactor(int type)
    {
        //float total = MapCreation.GetTotalCurrentTiles();
        float current = global.tileTypeProportions[type];
        if (current == 0)
        {
            return global.tileTypes[type].proportionWeight; //Used for divide-by-zero reasons, also increases the likelihood of initially placing tiles with higher proportion weight
        }
        float target = global.tileTypes[type].proportionWeight;
        float placedFloat = global.placed;
        float sumFloat = global.proportionSum;

        float placedFraction = current / placedFloat;
        float targetFraction = target / sumFloat;

        return targetFraction / placedFraction;
    }

    public static void PrintFinalProportions()
    {
        print("Final Proportions:");
        for(int i=0; i < global.tileTypes.Count; i++)
        {
            float current = global.tileTypeProportions[i];
            float placedFloat = global.placed;
            float placedFraction = current / placedFloat;
            float target = global.tileTypes[i].proportionWeight;
            float sumFloat = global.proportionSum;
            float targetFraction = target / sumFloat;
            print("" + global.tileTypes[i].name + " : " + placedFraction + " (" + current + "), target fraction : " + targetFraction);
        }
        print("Total placed : " + global.placed);
    }

    public static int GetClosestTileType(List<WaveformTile> neighbors)
    {
        print("GetClosestTileType");
        print("Neighbors:");
        foreach(WaveformTile neigh in neighbors)
        {
            print(global.tileTypes[neigh.tileType].name);
        }
        int lowestDistance = int.MaxValue;
        int type = 0;
        for(int i=0; i < global.tileTypes.Count; i++)
        {
            int totalDistance = 0;
            foreach(var neighbor in neighbors)
            {
                int subDistance = GetTypeDistance(i, neighbor.tileType);
                print("Total distance of " + global.tileTypes[neighbor.tileType].name + " for " + global.tileTypes[i].name + " : " + subDistance);
                if (subDistance == int.MaxValue)
                {
                    totalDistance = int.MaxValue;
                    break;
                }
                else
                {
                    totalDistance += subDistance;
                }
            }
            print("Total distance for " + global.tileTypes[i].name + " : " + totalDistance);
            if (totalDistance < lowestDistance)
            {
                lowestDistance = totalDistance;
                type = i;
            }
        }
        print("Shortest distance : " + global.tileTypes[type].name + " at " + lowestDistance);
        return type;
    }

    public static int GetTypeDistance(int from, int to)
    {
        if (from == to)
        {
            return 0;
        }
        int steps = 1;
        List<int> queue = new List<int>();
        List<int> nextQueue = new List<int>();
        Dictionary<int, bool> visited = new Dictionary<int, bool>();
        queue.Add(from);
        visited[from] = true;
        while(queue.Count > 0)
        {
            int pop = queue[0];
            queue.RemoveAt(0);

            TileType type = global.tileTypes[pop];
            foreach(var link in type.allowedNeighbors)
            {
                if (link.typeId == to)
                {
                    return steps;
                }
                else if (visited.ContainsKey(link.typeId))
                {
                    continue;
                }
                else
                {
                    nextQueue.Add(link.typeId);
                    visited[link.typeId] = true;
                }
            }

            if (queue.Count == 0 && nextQueue.Count > 0)
            {
                queue = nextQueue;
                nextQueue = new List<int>();
                steps++;
            }
        }
        return int.MaxValue;
    }
}
