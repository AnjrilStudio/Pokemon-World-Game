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
                action = new Action();
                action.TargetType = TargetType.Position;
                action.Range = new DistanceMPRange(1);
                action.Range2 = new DistanceMPAPRange(1);
                action.GroundEffects.Add(new MoveEffect());
                action.ActionCost = new MPAPDistanceActionCost(1);
                action.NextTurn = false;
                break;

            case Move.Tackle:
                action = new Action();
                action.TargetType = TargetType.Position;
                action.Range = new DistanceRange(3);
                action.AreaOfEffect = new DistanceAreaOfEffect(1);
                action.HitEffects.Add(new DamageEffect(40));
                FxDescriptor fxTackle1 = new FxDescriptor("flame");
                fxTackle1.Pattern = new ProjectileParticlePattern();
                fxTackle1.Pattern.Duration = 0.5f;
                fxTackle1.Type = FxType.ToTarget;
                action.Fx.Add(fxTackle1);
                FxDescriptor fxTackle2 = new FxDescriptor("flame");
                fxTackle2.Pattern = new ExplosionParticlePattern();
                fxTackle2.Pattern.Delay = 0.5f;
                fxTackle2.Type = FxType.FromTarget;
                action.Fx.Add(fxTackle2);
                break;

            case Move.Gust:
                action = new Action();
                action.TargetType = TargetType.Directional;
                action.Range = new LineRange(2);
                action.AreaOfEffect = new LineAreaOfEffect(4);
                action.HitEffects.Add(new PushEffect(1));
                action.HitEffects.Add(new DamageEffect(50));
                FxDescriptor fxGust = new FxDescriptor("flame");
                fxGust.Pattern = new RandomLineParticlePattern(0.15f);
                fxGust.Type = FxType.FromTarget;
                action.Fx.Add(fxGust);
                break;
            default:
                Debug.Log("ne doit pas arriver");
                break;
        }

        moves[(int)move] = action;
    }
}
