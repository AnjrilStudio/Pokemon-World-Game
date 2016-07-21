using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class PushEffect : HitEffect
{
    public int Dist { get; private set; }

    public PushEffect(int dist)
    {
        Dist = dist;
    }

    public override void apply(BattleEntity self, BattleEntity target, Direction dir)
    {
        Position newPos = new Position(target.CurrentPos.X + Utils.GetDirPosition(dir).X * Dist, target.CurrentPos.Y + Utils.GetDirPosition(dir).Y * Dist);
        newPos.NormalizePos(self.Arena.Mapsize);
        target.MoveBattleEntity(newPos);
    }
}
