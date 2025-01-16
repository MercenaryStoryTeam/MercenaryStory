using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoSpikeSetter : MonoBehaviour
{
    public List<AutoSpikeTrap> autoSpikeTraps;

    public void Start()
    {
        for (int i = 0; i < autoSpikeTraps.Count; i++)
        {
            autoSpikeTraps[i].trapStartTime = i;
        }
    }
}