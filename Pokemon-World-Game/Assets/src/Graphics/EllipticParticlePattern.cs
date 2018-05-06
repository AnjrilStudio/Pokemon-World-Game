using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class EllipticParticlePattern : ProjectileParticlePattern
{
    public float RadiusX { get; protected set; }
    public float RadiusY { get; protected set; }
    public float RadiusXTimeFactor { get; protected set; }
    public float RadiusYTimeFactor { get; protected set; }
    public float AngularSpeed { get; protected set; }

    public EllipticParticlePattern(float radiusX, float radiusY, float radiusXTimeFactor, float radiusYTimeFactor, float angularSpeed):base()
    {
        RadiusX = radiusX;
        RadiusY = radiusY;
        RadiusXTimeFactor = radiusXTimeFactor;
        RadiusYTimeFactor = radiusYTimeFactor;
        AngularSpeed = angularSpeed;
        Speed = 0;
    }

    public override float ComputeSpeed(float time, float random)
    {
        var factor = new Vector3 ((1 + random * 0.5f + RadiusXTimeFactor * time / Duration), (1 + random * 0.5f + RadiusYTimeFactor * time / Duration));
        return Speed * factor.magnitude;
    }

    public override List<float> ComputeAngles(float time, Vector3 target, float random)
    {
        var baseAngle = random * Mathf.PI * 2;
        var angle = (baseAngle + time * AngularSpeed);
        var radx = RadiusX * (1 + random * 0.5f + RadiusXTimeFactor * time / Duration);
        var rady = RadiusY * (1 + random * 0.5f + RadiusYTimeFactor * time / Duration);
        var angleTan = Mathf.Atan2(Mathf.Cos(angle) / (radx*radx), Mathf.Sin(angle) / (rady * rady));
        List<float> angles = new List<float>();
        angles.Add(90 - (angleTan * 180 / Mathf.PI));
        return angles;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        var baseAngle = random * Mathf.PI * 2;
        return new Vector3(Mathf.Cos(baseAngle + time * AngularSpeed) * RadiusX * (1 + random * 0.5f  + RadiusXTimeFactor * time / Duration ) , Mathf.Sin(baseAngle + time * AngularSpeed) * RadiusY * (1 + random * 0.5f + RadiusYTimeFactor * time / Duration), 0);
    }
}
