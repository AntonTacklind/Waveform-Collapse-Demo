using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveformCollapse : MonoBehaviour
{
    public enum GenerationMode
    {
        BigBang,
        StepwiseSmooth
    }

    public GenerationMode generationMode;

    private Dictionary<string, WaveformTile> grid = new Dictionary<string, WaveformTile>();
    private int maxWidth;
    private int maxHeight;

    private List<WaveformTile> queue = new List<WaveformTile>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (generationMode == GenerationMode.StepwiseSmooth && queue.Count > 0)
        {
            WaveformStep();
        }
    }

    public void PopulateGrid(Dictionary<string, WaveformTile> grid, int maxWidth, int maxHeight)
    {
        this.grid = grid;
        this.maxWidth = maxWidth;
        this.maxHeight = maxHeight;

        //Create an array with all the tiles in it
        List<WaveformTile> tiles = new List<WaveformTile>();
        for(int i=0; i < maxWidth; i++)
        {
            for(int j=0; j < maxWidth; j++)
            {
                tiles.Add(grid[GetPosKey(i, j)]);
            }
        }

        //Reset all materials?
        foreach(WaveformTile tile in tiles)
        {
            tile.tileType = -1;
        }

        //Select a random tile
        int rX = Random.Range(0, maxWidth);
        int rY = Random.Range(0, maxHeight);

        WaveformTile randomSelect = grid[GetPosKey(rX, rY)];
        queue.Clear();
        queue.Add(randomSelect);

        if (generationMode == GenerationMode.BigBang)
        {
            while(queue.Count > 0)
            {
                WaveformStep();
            }
        }
    }

    public void WaveformStep()
    {
        int index = 0;
        WaveformTile next = queue[index];
        queue.RemoveAt(index);

        if (next.tileType == -1)
        {
            List<int> allowed = GetAllowedTypes(next);
            if (allowed.Count > 0)
            {
                next.tileType = SelectType(allowed);
                next.UpdateMaterial();
                List<WaveformTile> neighbors = GetNeighbors(next.x, next.y);
                foreach (WaveformTile neigh in neighbors)
                {
                    if (neigh.tileType == -1)
                    {
                        queue.Add(neigh);
                    }
                }
            }
        }
    }

    private int SelectType(List<int> allowed)
    {
        return allowed[Random.Range(0, allowed.Count)];
    }

    private List<WaveformTile> GetNeighbors(int x, int y)
    {
        List<WaveformTile> ret = new List<WaveformTile>();
        if (x > 0)
            ret.Add(Get(x - 1, y));
        if (y > 0)
            ret.Add(Get(x, y - 1));
        if (x + 1< maxWidth)
            ret.Add(Get(x + 1, y));
        if (y + 1< maxHeight)
            ret.Add(Get(x, y + 1));
        return ret;
    }

    private List<int> GetAllowedTypes(WaveformTile tile)
    {
        return GetAllowedTypes(tile.x, tile.y);
    }

    //Determines what material a tile should have, based on its neighbors
    private List<int> GetAllowedTypes(int x, int y)
    {
        //Get neighbors
        List<WaveformTile> neighbors = GetNeighbors(x, y);

        //Create a list of tileTypes that are allowed
        List<int> allowedTiles = new List<int>();
        int index = 0;

        while(allowedTiles.Count == 0 && index < neighbors.Count)
        {
            WaveformTile neighbor = neighbors[index];
            if (neighbor.tileType != -1)
            {
                allowedTiles.AddRange(TileTypeManager.GetAllowedMaterials(neighbor.tileType));
            }
            else
            {
                index++;
            }
        }

        if (allowedTiles.Count == 0)
        {
            allowedTiles.AddRange(TileTypeManager.GetFullRandom());
        }

        if (index != neighbors.Count)
        {
            //Iterate through the rest of the neighbors, and ensure that the allowedTiles contain only tiles that are allowed by all neighbors
            for(int i=index; i < neighbors.Count; i++)
            {
                int tileType = neighbors[i].tileType;
                if (tileType != -1)
                {
                    List<int> nAllow = TileTypeManager.GetAllowedMaterials(tileType);
                    allowedTiles = allowedTiles.Where(x => nAllow.Contains(x)).ToList();
                }
            }
        }

        return allowedTiles;
    }

    private WaveformTile Get(int x, int y)
    {
        return grid[GetPosKey(x, y)];
    }

    private string GetPosKey(int x, int y)
    {
        return MapCreation.GetPosKey(x, y);
    }
}
