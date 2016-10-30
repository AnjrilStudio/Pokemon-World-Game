using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public abstract class ParticlePatternModifier
{
    public abstract ParticlePatternModifierType Type();

    public abstract float Compute(float time);

    public float Compute()
    {
        return Compute(0);
    }
}
