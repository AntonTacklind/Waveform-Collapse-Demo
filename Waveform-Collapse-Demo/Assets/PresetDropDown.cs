using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PresetDropDown : MonoBehaviour
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
        foreach (string name in Enum.GetNames(typeof(WaveformCollapse.Preset)))
        {
            options.Add(new Dropdown.OptionData { text = name });
        }
        drop.options = options;
        drop.value = 0; //No Preset

        drop.onValueChanged.AddListener( delegate { OnValueChange(drop); });

        OnValueChange(drop);
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

    void OnValueChange(Dropdown drop)
    {
        foreach(WaveformCollapse.Preset mode in Enum.GetValues(typeof(WaveformCollapse.Preset)))
        {
            if (mode.ToString() == drop.options[drop.value].text)
            {
                WaveformCollapse.global.preset = mode;
                break;
            }
        }
    }
}
