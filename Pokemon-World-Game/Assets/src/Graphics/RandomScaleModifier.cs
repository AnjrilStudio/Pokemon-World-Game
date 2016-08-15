using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class RandomScaleModifier : ScaleModifier
{
    private float ScaleOffset;

    public RandomScaleModifier(float scaleOffset)
    {
        ScaleOffset = scaleOffset;
    }

    public override float Compute()
    {
        return ((-1 + UnityEngine.Random.value * 2) * ScaleOffset / 2);
    }
}
