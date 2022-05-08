using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveformTile : MonoBehaviour
{
    public int tileType;
    public int x;
    public int y;

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

    public float DistanceTo(int x, int y)
    {
        float x2 = Mathf.Pow(this.x - x, 2);
        float y2 = Mathf.Pow(this.y - y, 2);
        float sq = Mathf.Sqrt(x2 + y2);
        float ab = Mathf.Abs(sq);
        return ab;
    }
}
