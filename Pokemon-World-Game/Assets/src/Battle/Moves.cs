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
                action.Range = new DistanceRange(2);
                action.AreaOfEffect = new DistanceAreaOfEffect(1);
                action.HitEffects.Add(new DamageEffect(40));
                break;

            case Move.Gust:
                action = new Action();
                action.TargetType = TargetType.Directional;
                action.Range = new LineRange(2);
                action.AreaOfEffect = new LineAreaOfEffect(4);
                action.HitEffects.Add(new PushEffect(1));
                action.HitEffects.Add(new DamageEffect(50));
                action.FxPattern = new RandomLineParticlePattern(0.15f);
                action.FxPrefabName = "flame";
                break;
            default:
                Debug.Log("ne doit pas arriver");
                break;
        }

        moves[(int)move] = action;
    }
}
