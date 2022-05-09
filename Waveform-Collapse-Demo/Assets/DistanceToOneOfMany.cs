using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceToOneOfMany : IComparer
{
    public List<Vector2> many;

    public DistanceToOneOfMany(List<Vector2> many)
    {
        this.many = many;
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
                float distA = float.MaxValue;
                float distB = float.MaxValue;
                foreach(Vector2 vector in many)
                {
                    float contA = tileA.DistanceTo(vector);
                    float contB = tileB.DistanceTo(vector);
                    if (contA < distA)
                    {
                        distA = contA;
                    }
                    if (contB < distB)
                    {
                        distB = contB;
                    }
                }
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