using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class DistanceMPRange : Range
{
    public int Factor { get; private set; }

    public DistanceMPRange(int factor)
    {
        Factor = factor;
        MaxRange = 20; //todo ?
    }

    public override bool InRange(BattleEntity self, Position target, Direction dir)
    {
        var origin = self.CurrentPos;
        var dist = Math.Abs(origin.X - target.X) + Math.Abs(origin.Y - target.Y);
        if (dist <= self.MP * Factor && dist != 0)
        {
            return true;
        }
        return false;
    }
}
