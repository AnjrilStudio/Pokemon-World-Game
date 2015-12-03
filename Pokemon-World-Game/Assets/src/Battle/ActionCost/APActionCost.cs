using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class APActionCost : ActionCost
{
    public APActionCost(int value)
    {
        Value = value;
    }

    public override void ApplyCost(BattleEntity self, Position target)
    {
        self.AP -= Value;
    }
}
