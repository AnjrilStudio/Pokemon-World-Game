using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class OverTimeScaleModifier : ScaleModifier
{
    public float StartScale { get; protected set; }
    public float EndScale { get; protected set; }

    public OverTimeScaleModifier(float startScale, float endScale)
    {
        StartScale = startScale;
        EndScale = endScale;
    }

    public override ParticlePatternModifierType Type()
    {
        return ParticlePatternModifierType.ScaleOverTime;
    }

    public override float Compute(float time)
    {
        return StartScale + (EndScale - StartScale) * time;
    }
}
