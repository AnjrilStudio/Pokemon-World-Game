using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public abstract class ParticlePattern
{
    public float Rate { get; set; }
    public float Scale { get; set; }
    public float LifeTime { get; set; }
    public float Duration { get; set; }
    public float Delay { get; set; }
    public int Repeat { get; set; }
    public float RepeatDelay { get; set; }
    public float Rotation { get; set; }
    public float RotationSpeed { get; set; }
    public Color Color { get; set; }

    public ParticlePatternType PatternType { get; set; }

    private float defaultRate = 50;
    private float defaultScale = 6f;
    private float defaultLifeTime = 0.5f;
    private float defaultDuration = 0.5f;
    private float defaultDelay = 0;
    private int defaultRepeat = 0;
    private float defaultRepeatDelay = 1;
    private float defaultRotationSpeed = 400f;
    private float defaultRotation = 0f;
    private Color defaultColor = Color.clear;

    private List<ParticlePatternModifier> modifiers;

    protected ParticlePattern()
    {
        Rate = defaultRate;
        Scale = defaultScale;
        LifeTime = defaultLifeTime;
        Duration = defaultDuration;
        Delay = defaultDelay;
        Repeat = defaultRepeat;
        RepeatDelay = defaultRepeatDelay;
        Rotation = defaultRotation;
        RotationSpeed = defaultRotationSpeed;
        Color = defaultColor;

        PatternType = ParticlePatternType.Simple;

        modifiers = new List<ParticlePatternModifier>();
    }

    public void AddModifier(ParticlePatternModifier modifier)
    {
        modifiers.Add(modifier);
    }

    public abstract Vector3 ComputeCenter(float time, Vector3 target, float random);


    public virtual float ComputeRotation(float angle)
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
            {
                scale += m.Compute();
            }
        }
        return scale;
    }

    public float ComputeScale(float time)
    {
        var scale = Scale;

        foreach (ParticlePatternModifier m in modifiers)
        {
            if (m.Type() == ParticlePatternModifierType.ScaleOverTime)
            {
                scale += m.Compute(time);
            }
        }
        return scale;
    }
}
