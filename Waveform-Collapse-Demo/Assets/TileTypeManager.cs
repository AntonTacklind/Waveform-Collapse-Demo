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
}
