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
        StepwiseAutomaticWeighted,
        Stepwise
    }

    public enum PropagationMode
    {
        NeighborsQueue,
        Circular,
        RandomNeighbor,
        CompletelyRandom,
        LeastPossibleOutcomesFirst
    }

    public enum CalculationMode
    {
        OnEvaluation,
        FullPropagation
    }

    public GenerationMode generationMode;
    public PropagationMode propagationMode;
    public CalculationMode calculationMode;

    public int automaticSpeed = 1;

    private Dictionary<string, WaveformTile> grid = new Dictionary<string, WaveformTile>();
    private int maxWidth;
    private int maxHeight;

    private List<WaveformTile> queue = new List<WaveformTile>();
    private BinaryTree<WaveformTile> binaryTree;
    private Dictionary<int, List<WaveformTile>> prioQueue = new Dictionary<int, List<WaveformTile>>();

    private FullPropagation fullPropagation;

    // Start is called before the first frame update
    void Start()
    {
        fullPropagation = GetComponent<FullPropagation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (generationMode == GenerationMode.StepwiseAutomaticSmooth)
        {
            for(int i=0; i < automaticSpeed; i++)
            {
                WaveformStep();
            }
        }
        else if(generationMode == GenerationMode.StepwiseAutomaticWeighted)
        {
            int total = automaticSpeed;
            while(total > 0)
            {
                total -= WaveformStep();
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
            tile.allowedTypes.Clear();
        }

        if (calculationMode == CalculationMode.FullPropagation)
        {
            foreach(WaveformTile tile in tiles)
            {
                tile.allowedTypes = TileTypeManager.GetFullRandom();
                tile.ApplyPossibilityGradient();
            }
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
        else if(propagationMode == PropagationMode.LeastPossibleOutcomesFirst)
        {
            prioQueue.Clear();

            for(int i=0; i < TileTypeManager.global.tileTypes.Count; i++)
            {
                prioQueue[i + 1] = new List<WaveformTile>();
            }

            prioQueue[5].Add(randomSelect);

            if (generationMode == GenerationMode.BigBang)
            {
                while (PrioQueueCount() > 0)
                {
                    WaveformStep();
                }
            }
        }
    }

    private bool PrioQueueHasElements()
    {
        foreach(var key in prioQueue.Keys)
        {
            if (prioQueue[key].Count > 0)
            {
                return true;
            }
        }
        return false;
    }
    
    private int PrioQueueCount()
    {
        int total = 0;
        foreach(var key in prioQueue.Keys)
        {
            total += prioQueue[key].Count;
        }
        return total;
    }

    private WaveformTile ExtractFromPrioQueue()
    {
        if (propagationMode == PropagationMode.LeastPossibleOutcomesFirst)
        {
            int max = TileTypeManager.global.tileTypes.Count;
            int min = 1;

            for(int i=min; i <= max; i++)
            {
                List<WaveformTile> local = prioQueue[i];
                if (local.Count > 0)
                {
                    WaveformTile ret = null;
                    while (local.Count > 0)
                    {
                        if (local[0].tileType == -1)
                        {
                            ret = local[0];
                            local.RemoveAt(0);
                            return ret;
                        }
                        local.RemoveAt(0);
                    }
                }
            }
        }
        return null;
    }

    public void WaveformStepButton()
    {
        WaveformStep();
    }

    //Returns how many different possibilities a tile had in selection, used in StepwiseAutomaticWeighted to speed up in cases where the waveform is collapsing and slow down when there are many choices
    public int WaveformStep()
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
        else if (propagationMode == PropagationMode.LeastPossibleOutcomesFirst)
        {
            next = ExtractFromPrioQueue();
        }
        if (next != null && next.tileType == -1)
        {
            int selectWeight = 0;

            List<int> allowed;
            if (calculationMode == CalculationMode.FullPropagation)
            {
                allowed = next.allowedTypes;
            }
            else
            {
                allowed = GetAllowedTypes(next);
            }
            selectWeight += allowed.Count * allowed.Count;
            if (allowed.Count > 0)
            {
                AssertType(next, allowed);
            }
            else
            {
                //Needs repair
                //Determine the closest possible tile
                List<WaveformTile> neighbors = MapCreation.GetNeighbors(next);
                int closest = TileTypeManager.GetClosestPossible(neighbors.Select(x => x.tileType).ToList());
                next.tileType = closest;
                next.UpdateMaterial();

                //Reset neighbors and add to evaluation
                foreach(WaveformTile neigh in neighbors)
                {
                    neigh.tileType = -1;
                    neigh.UpdateMaterial();
                    AddToEvaluation(neigh, next);

                    if (calculationMode == CalculationMode.FullPropagation)
                    {
                        fullPropagation.Add(neigh);
                        neigh.allowedTypes = TileTypeManager.GetFullRandom();
                    }
                }

                if (calculationMode == CalculationMode.FullPropagation)
                {
                    fullPropagation.Calculate();
                }

                selectWeight = 1;
            }
            if (propagationMode == PropagationMode.Circular)
            {
                if (binaryTree.value == null && binaryTree.moreLeaf != null)
                {
                    binaryTree.CopyFrom(binaryTree.moreLeaf);
                }
            }
            return selectWeight;
        }
        else if(next != null && next.tileType != -1)
        {
            return WaveformStep();
        }
        return 1;
    }

    private void AddToEvaluation(WaveformTile tile, WaveformTile origin)
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
        else if (propagationMode == PropagationMode.LeastPossibleOutcomesFirst && calculationMode == CalculationMode.OnEvaluation)
        {
            prioQueue[TileTypeManager.GetAllowedTileTypes(origin.tileType).Count].Add(tile);
        }
    }

    public void AddToPrioQueue(WaveformTile tile)
    {
        if (tile.tileType != -1)
        {
            return;
        }
        prioQueue[tile.allowedTypes.Count].Add(tile);
    }

    public void AssertType(WaveformTile tile, List<int> allowed)
    {
        tile.tileType = SelectType(allowed);
        tile.UpdateMaterial();
        List<WaveformTile> neighbors = MapCreation.GetNeighbors(tile.x, tile.y);
        foreach (WaveformTile neigh in neighbors)
        {
            if (neigh.tileType == -1)
            {
                if (calculationMode == CalculationMode.FullPropagation)
                {
                    fullPropagation.Add(neigh);
                }
                if (propagationMode != PropagationMode.CompletelyRandom)
                {
                    AddToEvaluation(neigh, tile);
                }
            }
        }

        if (calculationMode == CalculationMode.FullPropagation)
        {
            fullPropagation.Calculate();
        }
    }

    private int SelectType(List<int> allowed)
    {
        return allowed[Random.Range(0, allowed.Count)];
    }

    private List<int> GetAllowedTypes(WaveformTile tile)
    {
        return GetAllowedTypes(tile.x, tile.y);
    }

    //Determines what material a tile should have, based on its neighbors
    private List<int> GetAllowedTypes(int x, int y)
    {
        //Get neighbors
        List<WaveformTile> neighbors = MapCreation.GetNeighbors(x, y);

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
                float distA = tileA.DistanceTo(x, y);
                float distB = tileB.DistanceTo(x, y);
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