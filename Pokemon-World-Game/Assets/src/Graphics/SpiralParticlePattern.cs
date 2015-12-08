using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class SpiralParticlePattern : ParticlePattern
{
    public float AngleRate { get; protected set; }

    public SpiralParticlePattern(float angleRate):base()
    {
        AngleRate = angleRate;
    }

    public override float ComputeAngle(float time)
    {
        return AngleRate * time;
    }

    public override Vector3 ComputeCenter(float time)
    {
        return new Vector3(0, 0, 0);
    }
}
