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
}
