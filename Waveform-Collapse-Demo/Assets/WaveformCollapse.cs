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
        StepwiseAutomaticSmooth,
        Stepwise
    }

    public enum PropagationMode
    {
        NeighborsQueue,
        Circular,
        RandomNeighbor,
        CompletelyRandom
    }

    public GenerationMode generationMode;
    public PropagationMode propagationMode;

    public int smoothSpeed = 1;

    private Dictionary<string, WaveformTile> grid = new Dictionary<string, WaveformTile>();
    private int maxWidth;
    private int maxHeight;

    private List<WaveformTile> queue = new List<WaveformTile>();
    private BinaryTree<WaveformTile> binaryTree;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (generationMode == GenerationMode.StepwiseAutomaticSmooth)
        {
            for(int i=0; i < smoothSpeed; i++)
            {
                WaveformStep();
            }
        }
    }

    public void Setup(Dictionary<string, WaveformTile> grid, int maxWidth, int maxHeight)
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
            tile.UpdateMaterial();
        }

        //Select a random tile
        int rX = Random.Range(0, maxWidth);
        int rY = Random.Range(0, maxHeight);

        WaveformTile randomSelect = grid[GetPosKey(rX, rY)];

        //Setup depending on propagation mode
        if (propagationMode == PropagationMode.NeighborsQueue || propagationMode == PropagationMode.RandomNeighbor)
        {
            queue.Clear();
            queue.Add(randomSelect);

            if (generationMode == GenerationMode.BigBang)
            {
                while (queue.Count > 0)
                {
                    WaveformStep();
                }
            }
        }
        else if(propagationMode == PropagationMode.Circular)
        {
            binaryTree = new BinaryTree<WaveformTile>();
            binaryTree.value = randomSelect;
            binaryTree.comparer = new DistanceFromStartComparer(rX, rY);

            if (generationMode == GenerationMode.BigBang)
            {
                while (binaryTree.value != null)
                {
                    WaveformStep();
                }
            }
        }
        else if(propagationMode == PropagationMode.CompletelyRandom)
        {
            //queue.Clear();
            queue = tiles;

            if (generationMode == GenerationMode.BigBang)
            {
                while (queue.Count > 0)
                {
                    WaveformStep();
                }
            }
        }
    }

    public void WaveformStep()
    {
        WaveformTile next = null;

        if (propagationMode == PropagationMode.NeighborsQueue)
        {
            if (queue.Count > 0)
            {
                int index = 0;
                next = queue[index];
                queue.RemoveAt(index);
            }
        }
        else if (propagationMode == PropagationMode.Circular)
        {
            if (binaryTree.value != null)
            {
                next = binaryTree.Extract();
            }
        }
        else if (propagationMode == PropagationMode.RandomNeighbor || propagationMode == PropagationMode.CompletelyRandom)
        {
            if (queue.Count > 0)
            {
                int index = Random.Range(0, queue.Count);
                next = queue[index];
                queue.RemoveAt(index);
            }
        }
        if (next != null && next.tileType == -1)
        {
            List<int> allowed = GetAllowedTypes(next);
            if (allowed.Count > 0)
            {
                AssertType(next, allowed);
            }
            else
            {
                //Needs repair
                //Determine the closest possible tile
                List<WaveformTile> neighbors = GetNeighbors(next);
                int closest = TileTypeManager.GetClosestPossible(neighbors.Select(x => x.tileType).ToList());
                next.tileType = closest;
                next.UpdateMaterial();

                //Reset neighbors and add to evaluation
                foreach(WaveformTile neigh in neighbors)
                {
                    neigh.tileType = -1;
                    neigh.UpdateMaterial();
                    AddToEvaluation(neigh);
                }
            }
            if (propagationMode == PropagationMode.Circular)
            {
                if (binaryTree.value == null && binaryTree.moreLeaf != null)
                {
                    binaryTree.CopyFrom(binaryTree.moreLeaf);
                }
            }
        }
        else if(next != null && next.tileType != -1)
        {
            WaveformStep();
        }
    }

    private void AddToEvaluation(WaveformTile tile)
    {
        if (propagationMode == PropagationMode.NeighborsQueue || propagationMode == PropagationMode.RandomNeighbor || propagationMode == PropagationMode.CompletelyRandom)
        {
            queue.Add(tile);
        }
        else if (propagationMode == PropagationMode.Circular)
        {
            //print("Inserting neighbor with coordinates " + neigh.transform.position);
            binaryTree.Insert(tile);
        }
    }

    private void AssertType(WaveformTile tile, List<int> allowed)
    {
        tile.tileType = SelectType(allowed);
        tile.UpdateMaterial();
        List<WaveformTile> neighbors = GetNeighbors(tile.x, tile.y);
        foreach (WaveformTile neigh in neighbors)
        {
            if (neigh.tileType == -1 && propagationMode != PropagationMode.CompletelyRandom)
            {
                AddToEvaluation(neigh);
            }
        }
    }

    private int SelectType(List<int> allowed)
    {
        return allowed[Random.Range(0, allowed.Count)];
    }

    private List<WaveformTile> GetNeighbors(WaveformTile tile)
    {
        return GetNeighbors(tile.x, tile.y);
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
                allowedTiles.AddRange(TileTypeManager.GetAllowedTileTypes(neighbor.tileType));
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
                    List<int> nAllow = TileTypeManager.GetAllowedTileTypes(tileType);
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


public class DistanceFromStartComparer : IComparer
{
    public int x, y;

    public DistanceFromStartComparer(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int Compare(object a, object b)
    {
        WaveformTile tileA = (WaveformTile)a;
        WaveformTile tileB = (WaveformTile)b;
        if (tileA == null && tileB == null)
        {
            return 0;
        }
        else if (tileA != null)
        {
            if (tileB == null)
            {
                return -1;
            }
            else
            {
                //MonoBehaviour.print("Comparing " + tileA.transform.position + " with " + tileB.transform.position);
                //MonoBehaviour.print("Target coordinates = " + x + "," + y);

                float distA = tileA.DistanceTo(x, y);
                float distB = tileB.DistanceTo(x, y);
                //MonoBehaviour.print("distA = " + distA);
                //MonoBehaviour.print("distB = " + distB);
                if (distA > distB)
                {
                    return -1;
                }
                else if (distA == distB)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }
        else
        {
            return 1;
        }
    }
}