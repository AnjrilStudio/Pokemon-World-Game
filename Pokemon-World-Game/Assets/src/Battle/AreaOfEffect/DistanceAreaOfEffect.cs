using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class DistanceAreaOfEffect : AreaOfEffect
{
    public int Dist { get; private set; }

    public DistanceAreaOfEffect(int dist)
    {
        Dist = dist;
        MaxArea = dist;
    }

    public override bool InArea(Position origin, Position target, Direction dir)
    {
        var dist = Math.Abs(origin.X - target.X) + Math.Abs(origin.Y - target.Y);
        if (dist <= Dist)
        {
            return true;
        }
        return false;
    }
}
