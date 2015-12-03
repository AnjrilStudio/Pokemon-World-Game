using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class MPAPDistanceActionCost : ActionCost
{
    public MPAPDistanceActionCost(int value)
    {
        Value = value;
    }

    public override void ApplyCost(BattleEntity self, Position target)
    {
        int cost = Position.Distance(self.CurrentPos, target) * Value;
        self.MP -= cost;
        if (self.MP < 0)
        {
            self.AP += self.MP;
            self.MP = 0;
        }
    }
}
