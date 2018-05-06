using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class RandomLineParticlePattern : ProjectileParticlePattern
{
    public float LateralOffset { get; protected set; }

    public RandomLineParticlePattern() : base()
    {
        LateralOffset = 0f;
    }

    public RandomLineParticlePattern(float lateralOffset):base()
    {
        LateralOffset = lateralOffset;
    }

    public override List<float> ComputeAngles(float time, Vector3 target, float random)
    {
        List<float> angles = new List<float>();
        angles.Add(0);
        return angles;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        return new Vector3(0, (-1 + UnityEngine.Random.value * 2) * LateralOffset / 2, 0);
    }
}
