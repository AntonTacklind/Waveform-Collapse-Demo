using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullPropagation : MonoBehaviour
{
    public List<WaveformTile> queue = new List<WaveformTile>();

    private WaveformCollapse waveformCollapse;

    // Start is called before the first frame update
    void Start()
    {
        waveformCollapse = GetComponent<WaveformCollapse>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Add(WaveformTile tile)
    {
        queue.Add(tile);
    }

    public void Calculate()
    {
        //Continue until queue is empty
        while(queue.Count > 0)
        {
            //Extract a tile
            WaveformTile tile = queue[0];
            queue.RemoveAt(0);

            //Evaluate possible tiles given its neighbors
            List<WaveformTile> neighbors = MapCreation.GetNeighbors(tile);

            List<int> allowedTypes = TileTypeManager.GetAllowedTileTypes(neighbors);

            if (tile.allowedTypes.Count > allowedTypes.Count)
            {
                //The amount of allowedTypes has been restricted, so this nodes neighbors must be updated, if they dont already have a tileType
                foreach(var neigh in neighbors)
                {
                    if (neigh.tileType == -1)
                    {
                        Add(neigh);
                    }
                }

                if (allowedTypes.Count == 1)
                {
                    //Assert type, since only 1 is possible
                    //This will add neighbors to this queue as well, and handle possible additions to the propagation modes order container
                    waveformCollapse.AssertType(tile, allowedTypes);
                }
                else
                {
                    tile.allowedTypes = allowedTypes;
                    tile.ApplyPossibilityGradient();
                }

                if (waveformCollapse.propagationMode == WaveformCollapse.PropagationMode.LeastPossibleOutcomesFirst)
                {
                    waveformCollapse.AddToPrioQueue(tile);
                }
            }
        }
    }
}
