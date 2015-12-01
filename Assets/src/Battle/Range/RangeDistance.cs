using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class RangeDistance : Range
{
    public int Dist { get; private set; }

    public RangeDistance(int dist)
    {
        Dist = dist;
        MaxRange = dist;
    }

    override public bool InRange(Position origin, Position target, Direction dir)
    {
        var dist = Math.Abs(origin.X - target.X) + Math.Abs(origin.Y - target.Y);
        if (dist <= Dist && dist != 0)
        {
            return true;
        }
        return false;
    }
}
