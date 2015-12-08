using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class RandomConeParticlePattern : ParticlePattern
{
    public float AngleSpread { get; protected set; }

    public RandomConeParticlePattern(float angleSpread):base()
    {
        AngleSpread = angleSpread;
    }

    public override float ComputeAngle(float time)
    {
        return (-1 + UnityEngine.Random.value * 2) * AngleSpread / 2;
    }

    public override Vector3 ComputeCenter(float time)
    {
        return new Vector3(0, 0, 0);
    }
}
