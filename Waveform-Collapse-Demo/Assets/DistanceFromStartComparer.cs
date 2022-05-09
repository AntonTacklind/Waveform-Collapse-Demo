using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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