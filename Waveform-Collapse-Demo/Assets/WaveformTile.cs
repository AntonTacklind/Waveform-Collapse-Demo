using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveformTile : MonoBehaviour
{
    public int tileType;
    public int x;
    public int y;
    public bool forced = false;

    public List<int> allowedTypes = new List<int>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateMaterial()
    {
        GetComponent<Renderer>().material = TileTypeManager.GetMaterial(tileType);
    }

    public List<int> GetPossibleNeighbors()
    {
        if (tileType != -1)
        {
            return TileTypeManager.GetAllowedTileTypes(tileType);
        }
        else
        {
            List<int> possible = new List<int>();
            foreach(int possibility in allowedTypes)
            {
                possible.AddRange(TileTypeManager.GetAllowedTileTypes(possibility));
            }
            possible = possible.Distinct().ToList();
            return possible;
        }
    }

    public void ApplyPossibilityGradient()
    {
        if (tileType == -1)
        {
            //Black = many possibilities
            float allowedTypesFloat = allowedTypes.Count;
            float possible = TileTypeManager.global.tileTypes.Count;
            float gradient = 1 - (allowedTypesFloat / possible);
            var color = GetComponent<Renderer>().material.color;
            color.r = gradient;
            color.g = gradient;
            color.b = gradient;
            GetComponent<Renderer>().material.color = color;
        }
    }

    public float DistanceTo(int x, int y)
    {
        float x2 = Mathf.Pow(this.x - x, 2);
        float y2 = Mathf.Pow(this.y - y, 2);
        float sq = Mathf.Sqrt(x2 + y2);
        float ab = Mathf.Abs(sq);
        return ab;
    }

    public float DistanceTo(Vector2 vector)
    {
        return DistanceTo((int)vector.x, (int)vector.y);
    }

    //Returns true if the Tile was reset (if forced is not true)
    //Otherwise returns false
    public bool Reset()
    {
        if (forced)
        {
            return false;
        }
        else
        {
            tileType = -1;
            UpdateMaterial();
            ApplyPossibilityGradient();
            return true;
        }
    }
}
