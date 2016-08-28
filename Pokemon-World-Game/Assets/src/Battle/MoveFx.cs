using Anjril.PokemonWorld.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class MoveFx
{
    private static List<FxDescriptor>[] movefx = new List<FxDescriptor>[20];

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
                FxDescriptor fxTackle1 = new FxDescriptor("flame");
                fxTackle1.Pattern = new ProjectileParticlePattern();
                fxTackle1.Pattern.AddModifier(new RandomRotationModifier());
                fxTackle1.Pattern.Duration = 0.5f;
                fxTackle1.Type = FxType.ToTarget;
                fx.Add(fxTackle1);
                FxDescriptor fxTackle2 = new FxDescriptor("flame"); //todo remplacer le delay par un "onDeath"
                fxTackle2.Pattern = new ExplosionParticlePattern(1f);
                fxTackle2.Pattern.AddModifier(new RandomRotationModifier());
                fxTackle2.Pattern.Delay = 0.5f;
                fxTackle2.Type = FxType.FromTarget;
                fx.Add(fxTackle2);
                break;

            case Move.Gust:
                FxDescriptor fxGust = new FxDescriptor("flame");
                fxGust.Pattern = new RandomLineParticlePattern(0.15f);
                fxGust.Pattern.AddModifier(new RandomRotationModifier());
                fxGust.Type = FxType.FromTarget;
                fx.Add(fxGust);
                break;
            case Move.Bubble:
                FxDescriptor fxBubble = new FxDescriptor("water1");
                fxBubble.Pattern = new RandomLineParticlePattern(0.15f);
                fxBubble.Pattern.AddModifier(new RandomScaleModifier(2f));
                fxBubble.Pattern.AddModifier(new RandomRotationModifier());
                fxBubble.Pattern.Scale = 4f;
                fxBubble.Pattern.Rate = 10f;
                fxBubble.Pattern.Speed = 0.3f;
                fxBubble.Pattern.LifeTime = 1.6f;
                fxBubble.Pattern.Duration = 1.2f;
                fxBubble.Pattern.RotationSpeed = 0f;
                fxBubble.Type = FxType.FromTarget;
                fx.Add(fxBubble);
                break;
            case Move.Water_Gun:
                FxDescriptor fxWaterGun = new FxDescriptor("water2");
                fxWaterGun.Pattern = new RandomLineParticlePattern(0.1f);
                fxWaterGun.Pattern.Scale = 3f;
                fxWaterGun.Pattern.Rate = 200f;
                fxWaterGun.Pattern.Speed = 3f;
                fxWaterGun.Pattern.LifeTime = 0.4f;
                fxWaterGun.Pattern.Duration = 1.2f;
                fxWaterGun.Pattern.RotationSpeed = 0f;
                fxWaterGun.Type = FxType.FromTarget;
                fx.Add(fxWaterGun);
                break;
            case Move.Thunder_Shock:
                FxDescriptor fxThunderShock = new FxDescriptor("spark");
                fxThunderShock.Pattern = new ProjectileArcParticlePattern(4f);
                fxThunderShock.Pattern.Duration = 0.10f;
                fxThunderShock.Pattern.Scale = 1;
                fxThunderShock.Pattern.LifeTime = 0.20f;
                fxThunderShock.Pattern.Rate = 100;
                fxThunderShock.Pattern.RotationSpeed = 0;
                fxThunderShock.Pattern.AddModifier(new RandomRotationModifier());
                fxThunderShock.Type = FxType.ToTarget;
                fx.Add(fxThunderShock);
                //TODO inventer le "repeat"
                FxDescriptor fxThunderShock2 = new FxDescriptor("spark");
                fxThunderShock2.Pattern = new ProjectileArcParticlePattern(-5f);
                fxThunderShock2.Pattern.Duration = 0.10f;
                fxThunderShock2.Pattern.Delay = 0.20f;
                fxThunderShock2.Pattern.Scale = 1;
                fxThunderShock2.Pattern.LifeTime = 0.20f;
                fxThunderShock2.Pattern.Rate = 100;
                fxThunderShock2.Pattern.RotationSpeed = 0;
                fxThunderShock2.Pattern.AddModifier(new RandomRotationModifier());
                fxThunderShock2.Type = FxType.ToTarget;
                fx.Add(fxThunderShock2);

                FxDescriptor fxThunderShock3 = new FxDescriptor("spark");
                fxThunderShock3.Pattern = new ProjectileArcParticlePattern(6f);
                fxThunderShock3.Pattern.Duration = 0.10f;
                fxThunderShock3.Pattern.Delay = 0.40f;
                fxThunderShock3.Pattern.Scale = 1;
                fxThunderShock3.Pattern.LifeTime = 0.20f;
                fxThunderShock3.Pattern.Rate = 100;
                fxThunderShock3.Pattern.RotationSpeed = 0;
                fxThunderShock3.Pattern.AddModifier(new RandomRotationModifier());
                fxThunderShock3.Type = FxType.ToTarget;
                fx.Add(fxThunderShock3);
                break;
            default:
                break;
        }

        movefx[(int)move] = fx;
    }
}

