using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class SimpleParticlePattern : ParticlePattern
{

    public SimpleParticlePattern():base()
    {
        LifeTime = 0.25f;
        Rate = 0.01f; //100 sec -> un seul sprite
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        return Vector3.zero;
    }
}
