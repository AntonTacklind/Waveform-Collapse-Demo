using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Palette : MonoBehaviour
{
    public enum PaletteMode
    {
        NoRules,
        Cascade
    }

    public Dropdown dropdown;
    public PaletteMode paletteMode;

    private WaveformTile previous = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit raycast;
            Physics.Raycast(ray, out raycast);
            if (raycast.collider != null)
            {
                GameObject hitObject = raycast.collider.gameObject;
                if (hitObject.GetComponent<WaveformTile>())
                {
                    WaveformTile tile = hitObject.GetComponent<WaveformTile>();
                    if (dropdown.value != -1)
                    {
                        string typeName = dropdown.options[dropdown.value].text;
                        int type = TileTypeManager.GetTileType(typeName);

                        if (previous != null)
                        {
                            previous.forced = false;
                        }
                        previous = tile;
                        tile.forced = true;
                        if (type != tile.tileType)
                        {
                            if (paletteMode == PaletteMode.NoRules)
                            {
                                print("NORULES");
                                WaveformCollapse.global.AssertType(tile, type);
                            }
                            else if (paletteMode == PaletteMode.Cascade)
                            {
                                print("CASCADE");
                                WaveformCollapse.global.CleansingCascade(tile, type);
                            }
                        }
                    }
                }
            }
        }
    }
}
