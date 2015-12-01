using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


abstract class GroundEffect
{
    public abstract void apply(BattleEntity self, Position target, Direction dir);

    public void apply(BattleEntity self, Position target)
    {
        apply(self, target, Direction.None);
    }
}
