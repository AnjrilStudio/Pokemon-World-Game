using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class LineRange : DirectionalRange
{
    public int Dist { get; private set; }

    public LineRange(int dist)
    {
        Dist = dist;
        MaxRange = dist;
    }
    

    public override bool InRange(BattleEntity self, Position target, Direction dir)
    {
        var origin = self.CurrentPos;
        switch (dir)
        {
            case Direction.Up:
                return target.X == origin.X && target.Y > origin.Y && target.Y - Dist <= origin.Y;
            case Direction.Right:
                return target.Y == origin.Y && target.X > origin.X && target.X - Dist <= origin.X;
            case Direction.Down:
                return target.X == origin.X && target.Y < origin.Y && target.Y + Dist >= origin.Y;
            case Direction.Left:
                return target.Y == origin.Y && target.X < origin.X && target.X + Dist >= origin.X;
            default:
                return false;
        }
    }
}
