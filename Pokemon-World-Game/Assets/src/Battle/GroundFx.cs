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
                fxGust.Pattern = new VerticalEllipticParticlePattern(0.1f, 0.015f, -0.8f, -0.8f, 22f, -0.15f);
                fxGust.Pattern.Scale = 0.07f;
                fxGust.Pattern.Rate = 200f;
                fxGust.Pattern.Speed = 0.07f;
                fxGust.Pattern.LifeTime = 0.8f;
                fxGust.Pattern.Duration = 1.2f;
                fxGust.Pattern.Repeat = 14;
                fxGust.Pattern.RepeatDelay = 0.16f;
                fxGust.Pattern.RotationSpeed = 0f;
                fxGust.Pattern.AddModifier(new OverTimeScaleModifier(0.07f, 0f));
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

