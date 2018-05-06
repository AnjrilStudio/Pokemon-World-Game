using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class RandomConeParticlePattern : ProjectileParticlePattern
{
    public float AngleSpread { get; protected set; }

    public RandomConeParticlePattern(float angleSpread):base()
    {
        AngleSpread = angleSpread;
    }

    public override List<float> ComputeAngles(float time, Vector3 target, float random)
    {
        List<float> angles = new List<float>();
        angles.Add((-1 + UnityEngine.Random.value * 2) * AngleSpread / 2);
        return angles;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        return new Vector3(0, 0, 0);
    }
}
