using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class CircleSpiralParticlePattern : SpiralParticlePattern
{
    public float AngularSpeed { get; private set; }
    public float Radius { get; private set; }

    public CircleSpiralParticlePattern(float angleRate, float angularSpeed, float radius) : base(angleRate)
    {
        AngularSpeed = angularSpeed;
        Radius = radius;
    }


    public override Vector3 ComputeCenter(float time, Vector3 target)
    {
        return new Vector3(Mathf.Cos(time * AngularSpeed) * Radius, Mathf.Sin(time * AngularSpeed) * Radius, 0);
    }
}
