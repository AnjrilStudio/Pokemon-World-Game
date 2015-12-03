using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


abstract class DirectionalRange : Range
{
    public override bool InRange(BattleEntity self, Position target)
    {
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (InRange(self, target, dir))
            {
                return true;
            }
        }
        return false;
    }
}
