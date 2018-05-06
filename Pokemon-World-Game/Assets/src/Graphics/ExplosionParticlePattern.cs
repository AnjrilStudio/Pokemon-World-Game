using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class ExplosionParticlePattern : ProjectileParticlePattern
{
    public float SpeedOffset { get; set; }
    public int NbProj { get; set; }

    private float defaultSpeedOffset = 1f;
    private float defaultLifeTime = 0.25f;
    private float defaultSpeed = 1.5f;
    private float defaultScale = 4f;
    private int defaultNbProj = 8;

    public ExplosionParticlePattern():base()
    {
        SpeedOffset = defaultSpeedOffset;
        Duration = 2 * 1f / Rate;
        LifeTime = defaultLifeTime;
        Speed = defaultSpeed;
        Scale = defaultScale;
        NbProj = defaultNbProj;
    }

    public override List<float> ComputeAngles(float time, Vector3 target, float random)
    {
        List < float > angles = new List<float>();
        for(float angle = 0; angle < 360; angle += 360 / NbProj)
        {
            angles.Add(angle);
        }
        return angles;
    }

    public override Vector3 ComputeCenter(float time, Vector3 target, float random)
    {
        return Vector3.zero;
    }

    public override float ComputeSpeed(float time, float random)
    {
        return Speed + ((-1 + UnityEngine.Random.value * 2) * SpeedOffset / 2);
    }
}
