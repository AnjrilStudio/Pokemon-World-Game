using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class ProjectileParticlePattern : ParticlePattern
{

    public ProjectileParticlePattern():base()
    {
        Speed = 0;
        LifeTime = 0.25f;
    }

    public override List<float> ComputeAngles(float time, Vector3 target)
    {
        List < float > angles = new List<float>();
        angles.Add(0);
        return angles;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target)
    {
        return Vector3.Lerp(Vector3.zero, target, time / Duration);
    }
}
