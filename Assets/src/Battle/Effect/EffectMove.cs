using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class EffectMove : GroundEffect
{
    public int Dist { get; private set; }

    public EffectMove()
    {
    }

    public override void apply(BattleEntity self, Position target, Direction dir)
    {
        self.MoveBattleEntity(target);
    }
}
