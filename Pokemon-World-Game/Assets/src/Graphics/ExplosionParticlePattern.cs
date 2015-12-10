using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class ExplosionParticlePattern : ParticlePattern
{

    public ExplosionParticlePattern():base()
    {
        Duration = 2 * 1f / Rate;
        LifeTime = 0.3f;
        Speed = 1f;
    }

    public override List<float> ComputeAngles(float time, Vector3 target)
    {
        List < float > angles = new List<float>();
        for(float angle = 0; angle < 360; angle += 360 / 40)
        {
            angles.Add(angle);
        }
        return angles;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target)
    {
        return Vector3.zero;
    }
}
