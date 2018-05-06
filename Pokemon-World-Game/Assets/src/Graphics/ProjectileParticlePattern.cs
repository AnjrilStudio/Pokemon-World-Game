using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class ProjectileParticlePattern : ParticlePattern
{

    public float Speed { get; set; }
    private float defaultSpeed = 2f;

    public ProjectileParticlePattern():base()
    {
        Speed = defaultSpeed;
        LifeTime = 0.25f;

        PatternType = ParticlePatternType.Projectile;
    }

    public virtual float ComputeSpeed(float time, float random)
    {
        return Speed;
    }

    public virtual List<float> ComputeAngles(float time, Vector3 target, float random)
    {
        List < float > angles = new List<float>();
        angles.Add(0);
        return angles;
    }

    public override float ComputeRotation(float angle)
    {
        return base.ComputeRotation(angle) + angle;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        return Vector3.Lerp(Vector3.zero, target, time / Duration); //TODO modfier ?
    }
}
