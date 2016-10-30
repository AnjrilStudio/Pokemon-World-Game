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

    public override List<float> ComputeAngles(float time, Vector3 target, float random)
    {
        List<float> angles = new List<float>();
        angles.Add(AngleRate * time);
        return angles;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        return new Vector3(0, 0, 0);
    }
}
