using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


abstract class ActionCost
{
    public int Value { get; protected set; }

    public abstract void ApplyCost(BattleEntity self, Position target);
}
