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
                action.Range = new RangeDistance(3);
                action.GroundEffects.Add(new EffectMove());
                break;

            case Move.Tackle:
                action = new Action();
                action.TargetType = TargetType.Position;
                action.Range = new RangeDistance(2);
                action.AreaOfEffect = new AreaOfEffectDistance(1);
                action.HitEffects.Add(new EffectDamage(40));
                break;

            case Move.Gust:
                action = new Action();
                action.TargetType = TargetType.Directional;
                action.Range = new RangeLine(2);
                action.AreaOfEffect = new AreaOfEffectLine(4);
                action.HitEffects.Add(new EffectPush(1));
                action.HitEffects.Add(new EffectDamage(50));
                break;
            default:
                Debug.Log("ne doit pas arriver");
                break;
        }

        moves[(int)move] = action;
    }
}
