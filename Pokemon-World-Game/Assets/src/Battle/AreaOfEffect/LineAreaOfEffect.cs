using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class LineAreaOfEffect : AreaOfEffect
{
    public int Dist { get; private set; }

    public LineAreaOfEffect(int dist)
    {
        Dist = dist;
        MaxArea = dist;
    }

    public override bool InArea(Position origin, Position target, Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return target.X == origin.X && target.Y >= origin.Y && target.Y - Dist < origin.Y;
            case Direction.Right:
                return target.Y == origin.Y && target.X >= origin.X && target.X - Dist < origin.X;
            case Direction.Down:
                return target.X == origin.X && target.Y <= origin.Y && target.Y + Dist > origin.Y;
            case Direction.Left:
                return target.Y == origin.Y && target.X <= origin.X && target.X + Dist > origin.X;
            default:
                return false;
        }
    }
}
