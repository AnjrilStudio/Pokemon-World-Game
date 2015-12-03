using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class MPActionCost : ActionCost
{
    public MPActionCost(int value)
    {
        Value = value;
    }

    public override void ApplyCost(BattleEntity self, Position target)
    {
        self.MP -= Value;
    }
}
