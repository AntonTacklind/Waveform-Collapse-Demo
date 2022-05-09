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
        public List<WeightLink> allowedNeighbors;
    }

    public List<List<int>> allowedNeighborCache = new List<List<int>>();

    public List<TileType> tileTypes;

    public static TileTypeManager global;

    // Start is called before the first frame update
    void Start()
    {
        global = this;

        foreach(TileType type in tileTypes)
        {
            List<int> allowed = type.allowedNeighbors.Select(x => x.typeId).ToList();
            allowedNeighborCache.Add(allowed);
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

    public static List<int> GetAllowedTileTypes(List<WaveformTile> neighbors)
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
}
