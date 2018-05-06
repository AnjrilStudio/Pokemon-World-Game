using Anjril.PokemonWorld.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class GroundFx
{
    private static List<FxDescriptor>[] groundFx = new List<FxDescriptor>[50];

    public static List<FxDescriptor> Get(GroundEffectOverTimeId groundEffect)
    {
        if (groundFx[(int)groundEffect] == null)
        {
            init(groundEffect);
        }
        return groundFx[(int)groundEffect];
    }

    private static void init(GroundEffectOverTimeId groundEffect)
    {
        List<FxDescriptor> fx = new List<FxDescriptor>();

        switch (groundEffect)
        {
            case GroundEffectOverTimeId.Gust:
                FxDescriptor fxGust = new FxDescriptor("greydot3");
                var pattern = new VerticalEllipticParticlePattern(0.1f, 0.015f, -0.8f, -0.8f, 22f, -0.15f);
                pattern.Scale = 0.07f;
                pattern.Rate = 200f;
                pattern.Speed = 0.07f;
                pattern.LifeTime = 0.8f;
                pattern.Duration = 1.2f;
                pattern.Repeat = 10;
                pattern.RepeatDelay = 0.20f;
                pattern.RotationSpeed = 0f;
                pattern.AddModifier(new OverTimeScaleModifier(0.07f, 0f));
                fxGust.Pattern = pattern;
                //fxGust.Pattern.AddModifier(new RandomRotationModifier());
                fxGust.Type = FxType.FromTarget;
                fx.Add(fxGust);
                break;
            default:
                break;
        }

        groundFx[(int)groundEffect] = fx;
    }
}

