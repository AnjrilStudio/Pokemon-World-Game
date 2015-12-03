using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


abstract class Range
{
    public int MaxRange { get; protected set; }

    public abstract bool InRange(BattleEntity self, Position target, Direction dir);

    public virtual bool InRange(BattleEntity self, Position target)
    {
        return InRange(self, target, Direction.None);
    }

    public List<Position> InRangeTiles(BattleEntity self, Direction dir)
    {
        var result = new List<Position>();

        int startX = Position.NormalizedPos(self.CurrentPos.X - MaxRange, self.Arena.Mapsize);
        int endX = Position.NormalizedPos(self.CurrentPos.X + MaxRange, self.Arena.Mapsize);
        int startY = Position.NormalizedPos(self.CurrentPos.Y - MaxRange, self.Arena.Mapsize);
        int endY = Position.NormalizedPos(self.CurrentPos.Y + MaxRange, self.Arena.Mapsize);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Position target = new Position(x, y);
                if (dir == Direction.None)
                {
                    if (InRange(self, target))
                    {
                        result.Add(target);
                    }
                }
                else
                {
                    if (InRange(self, target, dir))
                    {
                        result.Add(target);
                    }
                }
            }
        }

        return result;
    }
}
