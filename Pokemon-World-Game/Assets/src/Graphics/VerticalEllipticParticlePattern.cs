using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class VerticalEllipticParticlePattern : EllipticParticlePattern
{
    public float VerticalSpeed { get; protected set; }

    public VerticalEllipticParticlePattern(float radiusX, float radiusY, float radiusXTimeFactor, float radiusYTimeFactor, float angularSpeed, float verticalSpeed) :base(radiusX, radiusY, radiusXTimeFactor, radiusYTimeFactor, angularSpeed)
    {
        VerticalSpeed = verticalSpeed;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        Vector3 center = base.ComputeCenter(time, target, random);
        center.y = center.y + time * VerticalSpeed - Duration * VerticalSpeed;
        return center;
    }
}
