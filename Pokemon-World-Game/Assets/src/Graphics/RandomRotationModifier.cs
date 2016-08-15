using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class RandomRotationModifier : RotationModifier
{

    public override float Compute()
    {
        return UnityEngine.Random.value * 360;
    }
}
