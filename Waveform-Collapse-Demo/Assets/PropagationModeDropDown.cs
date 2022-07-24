using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PropagationModeDropDown : MonoBehaviour
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
        foreach (string name in Enum.GetNames(typeof(WaveformCollapse.PropagationMode)))
        {
            options.Add(new Dropdown.OptionData { text = name });
        }
        drop.options = options;
        drop.value = 2; //Random Neighbor

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
        foreach(WaveformCollapse.PropagationMode mode in Enum.GetValues(typeof(WaveformCollapse.PropagationMode)))
        {
            if (mode.ToString() == drop.options[drop.value].text)
            {
                WaveformCollapse.global.propagationMode = mode;
                break;
            }
        }
    }
}
