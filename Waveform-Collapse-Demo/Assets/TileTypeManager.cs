using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTypeManager : MonoBehaviour
{
    [System.Serializable]
    public class TileType
    {
        public string name;
        public Material material;
        public List<int> allowedNeighbors;
    }

    public List<TileType> tileTypes;

    public static TileTypeManager global;

    // Start is called before the first frame update
    void Start()
    {
        global = this;
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

    public static int GetTypesAmount()
    {
        return global.tileTypes.Count;
    }

    public static Material GetMaterial(int type)
    {
        return global.tileTypes[type].material;
    }

    public static List<int> GetAllowedMaterials(int type)
    {
        return global.tileTypes[type].allowedNeighbors;
    }
}
