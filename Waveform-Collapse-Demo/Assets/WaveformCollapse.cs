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

    public enum Preset
    {
        NoPreset,
        FourMountains,
        CentralLake
    }

    public GenerationMode generationMode;
    public PropagationMode propagationMode;
    public CalculationMode calculationMode;
    public Preset preset;

    public float automaticSpeed = 1;
    private float budget = 0;

    private Dictionary<string, WaveformTile> grid = new Dictionary<string, WaveformTile>();
    private int maxWidth;
    private int maxHeight;

    private List<WaveformTile> queue = new List<WaveformTile>();
    private BinaryTree<WaveformTile> binaryTree;
    private Dictionary<int, List<WaveformTile>> prioQueue = new Dictionary<int, List<WaveformTile>>();

    private FullPropagation fullPropagation;

    public static WaveformCollapse global;

    // Start is called before the first frame update
    void Start()
    {
        global = this;
        fullPropagation = GetComponent<FullPropagation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (generationMode == GenerationMode.StepwiseAutomaticSmooth)
        {
            budget += automaticSpeed;
            while(budget > 0)
            {
                WaveformStep();
                budget -= 1;
            }
        }
        else if(generationMode == GenerationMode.StepwiseAutomaticWeighted)
        {
            budget += automaticSpeed;
            while(budget > 0)
            {
                budget -= WaveformStep();
            }
        }
    }

    public void Setup(Dictionary<string, WaveformTile> grid, int maxWidth, int maxHeight)
    {
        budget = 0;

        this.grid = grid;
        this.maxWidth = maxWidth;
        this.maxHeight = maxHeight;

        TileTypeManager.ResetProportions();

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
            tile.forced = false;
        }

        if (calculationMode == CalculationMode.FullPropagation)
        {
            foreach(WaveformTile tile in tiles)
            {
                tile.allowedTypes = TileTypeManager.GetFullRandom();
                tile.ApplyPossibilityGradient();
            }
        }
        queue.Clear();

        prioQueue.Clear();
        for (int i = 0; i < TileTypeManager.global.tileTypes.Count; i++)
        {
            prioQueue[i + 1] = new List<WaveformTile>();
        }

        binaryTree = new BinaryTree<WaveformTile>();
        BinaryTree<WaveformTile>.comparer = new DistanceFromStartComparer(0, 0);

        //Select a random tile
        int rX = Random.Range(0, maxWidth);
        int rY = Random.Range(0, maxHeight);
        WaveformTile randomSelect = null;
        if (preset == Preset.NoPreset)
        {
            randomSelect = grid[GetPosKey(rX, rY)];
            BinaryTree<WaveformTile>.comparer = new DistanceFromStartComparer(rX, rY);
        }
        else if(preset == Preset.FourMountains)
        {
            if (maxWidth < 4 || maxHeight < 4)
            {
                throw new System.Exception("Four Mountains requires width and height to be at least 4");
            }

            //Place a mountain at the center of each quadrant
            int quadX = maxWidth / 4;
            int quadY = maxHeight / 4;
            List<WeightLink> mountainList = new List<WeightLink>();
            mountainList.Add(new WeightLink { typeId = TileTypeManager.GetTileType("Mountain"), weight = 1 });

            List<Vector2> mountainPositions = new List<Vector2>();
            mountainPositions.Add(new Vector2(quadX, quadY));
            mountainPositions.Add(new Vector2(maxWidth - quadX, quadY));
            mountainPositions.Add(new Vector2(quadX, maxHeight - quadY));
            mountainPositions.Add(new Vector2(maxWidth - quadX, maxHeight - quadY));

            BinaryTree<WaveformTile>.comparer = new DistanceToOneOfMany(mountainPositions);

            AssertType(grid[GetPosKey(quadX, quadY)], mountainList);
            AssertType(grid[GetPosKey(maxWidth - quadX, quadY)], mountainList);
            AssertType(grid[GetPosKey(quadX, maxHeight - quadY)], mountainList);
            AssertType(grid[GetPosKey(maxWidth - quadX, maxHeight - quadY)], mountainList);
        }
        else if(preset == Preset.CentralLake)
        {
            if (maxWidth < 10 || maxHeight < 10)
            {
                throw new System.Exception("Central Lake requires width and height to be at least 10");
            }

            //Place a lake at the center of the map, that is 10% of the maps size
            int tenthX = maxWidth / 10;
            int tenthY = maxHeight / 10;
            int centerX = maxWidth / 2;
            int centerY = maxHeight / 2;

            BinaryTree<WaveformTile>.comparer = new DistanceFromStartComparer(centerX, centerY);

            List<WeightLink> deepWaterList = new List<WeightLink>();
            deepWaterList.Add(new WeightLink { typeId = TileTypeManager.GetTileType("Deep Water"), weight = 1 });
            for(int i=0; i < tenthX; i++)
            {
                for(int j=0; j < tenthY; j++)
                {
                    AssertType(grid[GetPosKey(centerX + i - (tenthX / 2), centerY + j - (tenthY / 2))], deepWaterList);
                }
            }
        }

        //Setup depending on propagation mode
        if (propagationMode == PropagationMode.NeighborsQueue || propagationMode == PropagationMode.RandomNeighbor)
        {
            if (randomSelect != null)
            {
                queue.Add(randomSelect);
            }
        }
        else if(propagationMode == PropagationMode.Circular)
        {
            if (randomSelect != null)
            {
                binaryTree.value = randomSelect;
            }
        }
        else if(propagationMode == PropagationMode.CompletelyRandom)
        {
            queue = tiles;
        }
        else if(propagationMode == PropagationMode.LeastPossibleOutcomesFirst)
        {
            if (randomSelect != null)
            {
                prioQueue[5].Add(randomSelect);
            }
        }

        if (generationMode == GenerationMode.BigBang)
        {
            BigBang();

            TileTypeManager.PrintFinalProportions();
        }
    }

    private void BigBang()
    {
        while (PrioQueueCount() > 0 || queue.Count > 0 || binaryTree.value != null)
        {
            WaveformStep();
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
                allowed = TileTypeManager.GetAllowedTileTypes(next);
            }
            selectWeight += allowed.Count * allowed.Count;
            if (allowed.Count > 0)
            {
                List<WeightLink> weights = TileTypeManager.GetWeightList(next, allowed);
                AssertType(next, weights);
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
                    if (neigh.Reset())
                    {
                        AddToEvaluation(neigh, next);

                        if (calculationMode == CalculationMode.FullPropagation)
                        {
                            fullPropagation.Add(neigh);
                            neigh.allowedTypes = TileTypeManager.GetFullRandom();
                        }
                    }
                }

                if (calculationMode == CalculationMode.FullPropagation)
                {
                    fullPropagation.Calculate();
                }

                selectWeight = 1;
            }
            /*
            if (propagationMode == PropagationMode.Circular)
            {
                if (binaryTree.value == null && binaryTree.moreLeaf != null)
                {
                    binaryTree.CopyFrom(binaryTree.moreLeaf);
                }
            }
            */
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

    public void AssertType(WaveformTile tile, List<WeightLink> weights)
    {
        AssertType(tile, SelectType(weights));
    }

    public void AssertType(WaveformTile tile, List<int> allowed)
    {
        AssertType(tile, SelectType(allowed));
    }

    public void AssertType(WaveformTile tile, int type)
    {
        tile.tileType = type;
        tile.UpdateMaterial();
        TileTypeManager.Placed(type);
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

    private List<WaveformTile> cascadeQueue;
    private WaveformTile cascadeTile;

    public void CleansingCascade(WaveformTile tile, int type)
    {
        //Assert type
        tile.tileType = type;
        tile.UpdateMaterial();

        cascadeQueue = MapCreation.GetNeighbors(tile);
        cascadeTile = tile;
        if (!(propagationMode == PropagationMode.CompletelyRandom || propagationMode == PropagationMode.LeastPossibleOutcomesFirst))
        {
            if (propagationMode == PropagationMode.Circular)
            {
                BinaryTree<WaveformTile>.comparer = new DistanceFromStartComparer(tile.x, tile.y);
            }
            foreach(var touched in cascadeQueue)
            {
                AddToEvaluation(touched, tile);
            }
        }
        int loopBreak = 0;
        while (cascadeQueue.Count > 0 && loopBreak < 200)
        {
            CascadeStep();
            //loopBreak++;
        }

        if (calculationMode == CalculationMode.FullPropagation)
        {
            fullPropagation.Calculate();
        }

        if (generationMode == GenerationMode.BigBang)
        {
            BigBang();
        }
    }

    public void CascadeStep()
    {
        if (cascadeQueue.Count == 0)
        {
            //print("Cascade over");
            return;
        }
        WaveformTile touched = cascadeQueue[0];
        cascadeQueue.RemoveAt(0);
        //print("CASCADE : Starting with " + touched.transform.position);

        int previousAllowedCount = touched.allowedTypes.Count;
        List<int> allowed = TileTypeManager.GetAllowedTileTypes(touched, debug: true);
        if (allowed.Count == 0 || !allowed.Contains(touched.tileType))
        {
            //It cannot be anything, or it cannot be what it is, reset it if possible
            if (touched.Reset())
            {
                if (propagationMode == PropagationMode.CompletelyRandom || propagationMode == PropagationMode.LeastPossibleOutcomesFirst)
                {
                    AddToEvaluation(touched, cascadeTile);
                }
                //Can it be anything?
                if (allowed.Count > 0)
                {
                    //Ok, then its neighbors can be what they are without issue, for the moment being
                    //Update its allowedTypes
                    touched.allowedTypes = allowed;
                    touched.ApplyPossibilityGradient();
                }
                if (allowed.Count == 0 || (allowed.Count != previousAllowedCount && previousAllowedCount != 0))
                {
                    //It's impossible for it to be what it is, or the amount of allowed types was not the same as it previously was (the neighbors have been updated, so this should be updated as well
                    //Reset its neighbors, update its allowed types, and add the neighbors to the cascade if they were not reset
                    List<WaveformTile> neighbors = MapCreation.GetNeighbors(touched);
                    foreach (var neigh in neighbors)
                    {
                        if (neigh.tileType != -1 || 1 == 1)
                        {
                            int previousTileType = neigh.tileType;
                            if (previousTileType != -1)
                            {
                                neigh.allowedTypes.Clear();
                            }
                            if (neigh.Reset())
                            {
                                cascadeQueue.Add(neigh);
                            }
                        }
                    }

                    if (allowed.Count == 0)
                    {
                        //print("CASCADE : Refreshing touched allowed types");
                        touched.allowedTypes = TileTypeManager.GetAllowedTileTypes(touched, debug: true);
                        touched.ApplyPossibilityGradient();
                    }
                }
            }
        }
        //print("CASCADE : Finished with " + touched.transform.position);
        //print("allowedTypes.Count = " + touched.allowedTypes.Count);
    }

    private int SelectType(List<int> allowed)
    {
        return allowed[Random.Range(0, allowed.Count)];
    }

    private int SelectType(List<WeightLink> weights)
    {
        float sum = 0;
        foreach(WeightLink link in weights)
        {
            float proportionFactor = TileTypeManager.GetProportionalFactor(link.typeId);
            float weightFloat = link.weight;
            float weightProduct = weightFloat * proportionFactor;
            sum += weightProduct;
        }
        float randomNumber = Random.Range(0, sum);
        foreach(WeightLink link in weights)
        {
            float proportionFactor = TileTypeManager.GetProportionalFactor(link.typeId);
            float weightFloat = link.weight;
            float weightProduct = weightFloat * proportionFactor;
            if (randomNumber < weightProduct)
            {
                return link.typeId;
            }
            else
            {
                randomNumber -= weightProduct;
            }
        }
        foreach(WeightLink link in weights)
        {
            if (link.weight > 0)
            {
                return link.typeId;
            }
        }
        return -1;
    }

    private string GetPosKey(int x, int y)
    {
        return MapCreation.GetPosKey(x, y);
    }
}