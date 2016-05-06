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
    public float Rotation { get; set; }
    public float RotationSpeed { get; set; }

    private float defaultRate = 50;
    private float defaultSpeed = 2f;
    private float defaultScale = 6f;
    private float defaultLifeTime = 0.5f;
    private float defaultDuration = 0.5f;
    private float defaultDelay = 0;
    private float defaultRotationSpeed = 400f;
    private float defaultRotation = 0f;

    private List<ParticlePatternModifier> modifiers;

    protected ParticlePattern()
    {
        Rate = defaultRate;
        Speed = defaultSpeed;
        Scale = defaultScale;
        LifeTime = defaultLifeTime;
        Duration = defaultDuration;
        Delay = defaultDelay;
        Rotation = defaultRotation;
        RotationSpeed = defaultRotationSpeed;

        modifiers = new List<ParticlePatternModifier>();
    }

    public void AddModifier(ParticlePatternModifier modifier)
    {
        modifiers.Add(modifier);
    }

    public abstract List<float> ComputeAngles(float time, Vector3 target);

    public abstract Vector3 ComputeCenter(float time, Vector3 target);

    public virtual float ComputeSpeed()
    {
        return Speed;
    }

    public float ComputeRotation()
    {
        var rotation = Rotation;
        foreach (ParticlePatternModifier m in modifiers)
        {
            if (m.Type() == ParticlePatternModifierType.Rotation)
                rotation += m.Compute();
        }
        return rotation;
    }

    public virtual float ComputeRotationSpeed()
    {
        var rotationSpeed = RotationSpeed;
        if (UnityEngine.Random.value < 0.5f)
        {
            rotationSpeed = -rotationSpeed;
        }
        return rotationSpeed;
    }

    public float ComputeScale()
    {
        var scale = Scale;

        foreach (ParticlePatternModifier m in modifiers)
        {
            if (m.Type() == ParticlePatternModifierType.Scale)
                scale += m.Compute();
        }
        return scale;
    }
}
