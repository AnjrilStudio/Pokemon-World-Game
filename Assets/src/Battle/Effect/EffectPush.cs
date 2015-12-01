using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class EffectPush : HitEffect
{
    public int Dist { get; private set; }

    public EffectPush(int dist)
    {
        Dist = dist;
    }

    public override void apply(BattleEntity self, BattleEntity target, Direction dir)
    {
        Position newPos = new Position(target.CurrentPos.X + Utils.getDirPosition(dir).X * Dist, target.CurrentPos.Y + Utils.getDirPosition(dir).Y * Dist);
        target.MoveBattleEntity(newPos);
    }
}
