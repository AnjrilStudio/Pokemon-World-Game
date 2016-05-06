using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Moves
{
    private static Action[] moves = new Action[10];

    public static Action Get (Move move)
    {
        if (moves[(int)move] == null)
        {
            init(move);
        }
        return moves[(int)move];
    }

    private static void init(Move move)
    {
        Action action = null;

        switch (move)
        {
            case Move.Move:
                action = new Action("Move");
                action.TargetType = TargetType.Position;
                action.Range = new DistanceMPRange(1);
                action.Range2 = new DistanceMPAPRange(1);
                action.GroundEffects.Add(new MoveEffect());
                action.ActionCost = new MPAPDistanceActionCost(1);
                action.NextTurn = false;
                break;

            case Move.Tackle:
                action = new Action("Tackle");
                action.TargetType = TargetType.Position;
                action.Range = new DistanceRange(3);
                action.AreaOfEffect = new DistanceAreaOfEffect(1);
                action.HitEffects.Add(new DamageEffect(40));
                FxDescriptor fxTackle1 = new FxDescriptor("flame");
                fxTackle1.Pattern = new ProjectileParticlePattern();
                fxTackle1.Pattern.AddModifier(new RandomRotationModifier());
                fxTackle1.Pattern.Duration = 0.5f;
                fxTackle1.Type = FxType.ToTarget;
                action.Fx.Add(fxTackle1);
                FxDescriptor fxTackle2 = new FxDescriptor("flame"); //todo remplacer le delay par un "onDeath"
                fxTackle2.Pattern = new ExplosionParticlePattern(1f);
                fxTackle2.Pattern.AddModifier(new RandomRotationModifier());
                fxTackle2.Pattern.Delay = 0.5f;
                fxTackle2.Type = FxType.FromTarget;
                action.Fx.Add(fxTackle2);
                break;

            case Move.Gust:
                action = new Action("Gust");
                action.TargetType = TargetType.Directional;
                action.Range = new LineRange(2);
                action.AreaOfEffect = new LineAreaOfEffect(4);
                action.HitEffects.Add(new PushEffect(1));
                action.HitEffects.Add(new DamageEffect(50));
                FxDescriptor fxGust = new FxDescriptor("flame");
                fxGust.Pattern = new RandomLineParticlePattern(0.15f);
                fxGust.Pattern.AddModifier(new RandomRotationModifier());
                fxGust.Type = FxType.FromTarget;
                action.Fx.Add(fxGust);
                break;
            case Move.Bubble:
                action = new Action("Bubble");
                action.TargetType = TargetType.Directional;
                action.Range = new LineRange(2);
                action.AreaOfEffect = new LineAreaOfEffect(4);
                action.HitEffects.Add(new PushEffect(1));
                action.HitEffects.Add(new DamageEffect(50));
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
                action.Fx.Add(fxBubble);
                break;
            case Move.Water_Gun:
                action = new Action("WaterGun");
                action.TargetType = TargetType.Directional;
                action.Range = new LineRange(1);
                action.AreaOfEffect = new LineAreaOfEffect(5);
                action.HitEffects.Add(new DamageEffect(50));
                FxDescriptor fxWaterGun = new FxDescriptor("water2");
                fxWaterGun.Pattern = new RandomLineParticlePattern(0.1f);
                fxWaterGun.Pattern.Scale = 3f;
                fxWaterGun.Pattern.Rate = 200f;
                fxWaterGun.Pattern.Speed = 3f;
                fxWaterGun.Pattern.LifeTime = 0.4f;
                fxWaterGun.Pattern.Duration = 1.2f;
                fxWaterGun.Pattern.RotationSpeed = 0f;
                fxWaterGun.Type = FxType.FromTarget;
                action.Fx.Add(fxWaterGun);
                break;
            default:
                Debug.Log("ne doit pas arriver");
                break;
        }

        moves[(int)move] = action;
    }
}
