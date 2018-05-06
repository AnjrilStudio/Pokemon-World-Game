using Anjril.PokemonWorld.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class MoveFx
{
    private static List<FxDescriptor>[] movefx = new List<FxDescriptor>[50];

    public static List<FxDescriptor> Get(Move move)
    {
        if (movefx[(int)move] == null)
        {
            init(move);
        }
        return movefx[(int)move];
    }

    public static List<FxDescriptor> Get(Action action)
    {
        return Get((Move)action.Id);
    }

    private static void init(Move move)
    {
        List<FxDescriptor> fx = new List<FxDescriptor>();

        switch (move)
        {
            case Move.Move:
                break;

            case Move.Ember:
                {
                    FxDescriptor fx1 = new FxDescriptor("flame");
                    var pattern1 = new ProjectileParticlePattern();
                    pattern1.AddModifier(new RandomRotationModifier());
                    pattern1.Duration = 0.5f;
                    pattern1.Speed = 0;
                    fx1.Pattern = pattern1;
                    fx1.Type = FxType.ToTarget;
                    fx.Add(fx1);
                    FxDescriptor fx2 = new FxDescriptor("flame"); //todo remplacer le delay par un "onDeath"
                    var pattern2 = new ExplosionParticlePattern();
                    pattern2.AddModifier(new RandomRotationModifier());
                    pattern2.Delay = 0.5f;
                    pattern2.NbProj = 18;
                    fx2.Pattern = pattern2;
                    fx2.Type = FxType.FromTarget;
                    fx.Add(fx2);
                    break;
                }
            case Move.Gust:
                {
                    FxDescriptor fx1 = new FxDescriptor("greydot3");
                    var pattern1 = new VerticalEllipticParticlePattern(0.1f, 0.015f, -0.8f, -0.8f, 22f, -0.15f);
                    pattern1.Scale = 0.07f;
                    pattern1.Rate = 350f;
                    pattern1.Speed = 0.07f;
                    pattern1.LifeTime = 0.8f;
                    pattern1.Duration = 1.2f;
                    pattern1.Repeat = 28;
                    pattern1.RepeatDelay = 0.08f;
                    pattern1.RotationSpeed = 0f;
                    pattern1.AddModifier(new OverTimeScaleModifier(0.07f, 0f));
                    //fxGust.Pattern.AddModifier(new RandomRotationModifier());
                    fx1.Pattern = pattern1;
                    fx1.Type = FxType.FromTarget;
                    fx.Add(fx1);
                    break;
                }
            case Move.Bubble:
                {
                    FxDescriptor fx1 = new FxDescriptor("water1");
                    var pattern1 = new RandomLineParticlePattern(0.15f);
                    pattern1.AddModifier(new RandomScaleModifier(2f));
                    pattern1.AddModifier(new RandomRotationModifier());
                    pattern1.Scale = 4f;
                    pattern1.Rate = 10f;
                    pattern1.Speed = 0.3f;
                    pattern1.LifeTime = 1.6f;
                    pattern1.Duration = 1.2f;
                    pattern1.RotationSpeed = 0f;
                    fx1.Pattern = pattern1;
                    fx1.Type = FxType.FromTarget;
                    fx.Add(fx1);
                    break;
                }
            case Move.Water_Gun:
                {
                    FxDescriptor fx1 = new FxDescriptor("water2");
                    var pattern1 = new RandomLineParticlePattern(0.1f);
                    pattern1.Scale = 3f;
                    pattern1.Rate = 200f;
                    pattern1.Speed = 3f;
                    pattern1.LifeTime = 0.4f;
                    pattern1.Duration = 1.2f;
                    pattern1.RotationSpeed = 0f;
                    fx1.Pattern = pattern1;
                    fx1.Type = FxType.FromTarget;
                    fx.Add(fx1);
                    break;
                }
            case Move.Thunder_Shock:
                {
                    //TODO tout refaire
                    FxDescriptor fx1 = new FxDescriptor("spark");
                    fx1.Pattern = new ProjectileArcParticlePattern(4f);
                    fx1.Pattern.Duration = 0.10f;
                    fx1.Pattern.Scale = 1;
                    fx1.Pattern.LifeTime = 0.20f;
                    fx1.Pattern.Rate = 100;
                    fx1.Pattern.RotationSpeed = 0;
                    fx1.Pattern.AddModifier(new RandomRotationModifier());
                    fx1.Type = FxType.ToTarget;
                    fx.Add(fx1);
                    //TODO inventer le "repeat"
                    FxDescriptor fx2 = new FxDescriptor("spark");
                    fx2.Pattern = new ProjectileArcParticlePattern(-5f);
                    fx2.Pattern.Duration = 0.10f;
                    fx2.Pattern.Delay = 0.20f;
                    fx2.Pattern.Scale = 1;
                    fx2.Pattern.LifeTime = 0.20f;
                    fx2.Pattern.Rate = 100;
                    fx2.Pattern.RotationSpeed = 0;
                    fx2.Pattern.AddModifier(new RandomRotationModifier());
                    fx2.Type = FxType.ToTarget;
                    fx.Add(fx2);

                    FxDescriptor fx3 = new FxDescriptor("spark");
                    fx3.Pattern = new ProjectileArcParticlePattern(6f);
                    fx3.Pattern.Duration = 0.10f;
                    fx3.Pattern.Delay = 0.40f;
                    fx3.Pattern.Scale = 1;
                    fx3.Pattern.LifeTime = 0.20f;
                    fx3.Pattern.Rate = 100;
                    fx3.Pattern.RotationSpeed = 0;
                    fx3.Pattern.AddModifier(new RandomRotationModifier());
                    fx3.Type = FxType.ToTarget;
                    fx.Add(fx3);
                }
                break;
            case Move.Pound:
                {
                    FxDescriptor fx1 = new FxDescriptor("whitestar");
                    fx1.Pattern = new ExplosionParticlePattern();
                    fx1.Pattern.Scale = 0.5f;
                    fx1.Pattern.LifeTime = 0.15f;
                    fx1.Pattern.AddModifier(new RandomRotationModifier());
                    fx1.Type = FxType.FromTarget;
                    fx.Add(fx1);

                    FxDescriptor fx2 = new FxDescriptor("whitecircle");
                    fx2.Pattern = new SimpleParticlePattern();
                    fx2.Pattern.LifeTime = 0.15f;
                    fx2.Pattern.Scale = 0.1f;
                    fx2.Pattern.AddModifier(new OverTimeScaleModifier(0, 0.7f));
                    fx2.Type = FxType.FromTarget;
                    fx.Add(fx2);
                    break;
                }
            case Move.Scratch:
                {
                    FxDescriptor fx1 = new FxDescriptor("whitedisc");
                    var pattern = new DiagonalParticlePattern();
                    pattern.Scale = 0.5f;
                    pattern.Color = new Color(1, 0.8f, 0.4f);
                    pattern.AddModifier(new OverTimeScaleModifier(0, -0.5f));
                    fx1.Pattern = pattern;
                    fx1.Type = FxType.FromTarget;
                    fx.Add(fx1);
                    break;
                }
            case Move.Tackle:
                {
                    FxDescriptor fx1 = new FxDescriptor("whiteline");
                    var pattern = new ExplosionParticlePattern();
                    pattern.Scale = 0.2f;
                    pattern.LifeTime = 0.15f;
                    pattern.RotationSpeed = 0;
                    pattern.NbProj = 16;
                    fx1.Type = FxType.FromTarget;
                    fx1.Pattern = pattern;
                    fx.Add(fx1);
                    break;
                }
            default:
                break;
        }

        movefx[(int)move] = fx;
    }
}

