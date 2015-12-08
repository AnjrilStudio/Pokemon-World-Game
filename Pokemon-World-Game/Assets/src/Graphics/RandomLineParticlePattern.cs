using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class RandomLineParticlePattern : ParticlePattern
{
    public float LateralOffset { get; protected set; }

    public RandomLineParticlePattern(float lateralOffset):base()
    {
        LateralOffset = lateralOffset;
    }

    public override float ComputeAngle(float time)
    {
        return 0;
    }

    public override Vector3 ComputeCenter(float time)
    {
        return new Vector3(0, (-1 + UnityEngine.Random.value * 2) * LateralOffset / 2, 0);
    }
}
