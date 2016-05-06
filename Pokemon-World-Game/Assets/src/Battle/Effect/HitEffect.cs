﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//effets s'appliquant sur les cibles touchées
abstract class HitEffect
{
    public abstract void apply(BattleEntity self, BattleEntity target, Direction dir);

    public void apply(BattleEntity self, BattleEntity target)
    {
        apply(self, target, Direction.None);
    }
}
