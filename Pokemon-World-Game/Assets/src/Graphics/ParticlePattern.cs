using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public abstract class ParticlePattern
{
    public float Rate { get; set; }
    public float Speed { get; set; }
    public float Scale { get; set; }
    public float LifeTime { get; set; }
    public float Duration { get; set; }
    public float Delay { get; set; }

    private float defaultRate = 50;
    private float defaultSpeed = 2f;
    private float defaultScale = 6f;
    private float defaultLifeTime = 0.5f;
    private float defaultDuration = 0.5f;
    private float defaultDelay = 0;

    protected ParticlePattern()
    {
        Rate = defaultRate;
        Speed = defaultSpeed;
        Scale = defaultScale;
        LifeTime = defaultLifeTime;
        Duration = defaultDuration;
        Delay = defaultDelay;
    }

    public abstract List<float> ComputeAngles(float time, Vector3 target);

    public abstract Vector3 ComputeCenter(float time, Vector3 target);


}
