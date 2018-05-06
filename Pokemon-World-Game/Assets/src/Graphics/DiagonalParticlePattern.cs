using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class DiagonalParticlePattern : ParticlePattern
{
    public float LineLength { get; set; }

    private float defaultLineLength = 0.3f;

    public DiagonalParticlePattern():base()
    {
        LineLength = defaultLineLength;
        LifeTime = 0.25f;
        Duration = 0.4f;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        Vector3 start = target + new Vector3(LineLength/2, LineLength/2, 0);
        Vector3 end = target + new Vector3(-LineLength/2, -LineLength/2, 0);
        return Vector3.Lerp(start, end, time / Duration);
    }
}
