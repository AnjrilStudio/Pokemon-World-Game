using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class MoveEffect : GroundEffect
{
    public int Dist { get; private set; }

    public MoveEffect()
    {
    }

    public override void apply(BattleEntity self, Position target, Direction dir)
    {
        self.MoveBattleEntity(target);
    }
}
