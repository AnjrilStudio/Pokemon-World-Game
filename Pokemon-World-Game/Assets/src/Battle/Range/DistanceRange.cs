using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class DistanceRange : Range
{
    public int Dist { get; private set; }

    public DistanceRange(int dist)
    {
        Dist = dist;
        MaxRange = dist;
    }

    public override bool InRange(BattleEntity self, Position target, Direction dir)
    {
        var origin = self.CurrentPos;
        var dist = Math.Abs(origin.X - target.X) + Math.Abs(origin.Y - target.Y);
        if (dist <= Dist && dist != 0)
        {
            return true;
        }
        return false;
    }
}
