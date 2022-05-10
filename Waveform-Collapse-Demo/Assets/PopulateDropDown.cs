using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateDropDown : MonoBehaviour
{
    private bool started = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void PostStart()
    {
        Dropdown drop = GetComponent<Dropdown>();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach (TileTypeManager.TileType type in TileTypeManager.global.tileTypes)
        {
            options.Add(new Dropdown.OptionData { text = type.name });
        }
        drop.options = options;
    }

    // Update is called once per frame
    void Update()
    {
        if (!started)
        {
            started = true;
            PostStart();
        }
    }
}
