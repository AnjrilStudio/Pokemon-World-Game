using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Action
{
    public TargetType TargetType;
    public Range Range { get; set; }
    public AreaOfEffect AreaOfEffect;
    public List<HitEffect> HitEffects { get; private set; }
    public List<GroundEffect> GroundEffects { get; private set; }

    public Action()
    {
        TargetType = TargetType.None;
        HitEffects = new List<HitEffect>();
        GroundEffects = new List<GroundEffect>();
    }

    public List<Position> getInRangeTiles(BattleEntity self, Direction dir)
    {
        var result = new List<Position>();

        int startX = Position.NormalizedPos(self.CurrentPos.X - Range.MaxRange, self.Arena.Mapsize);
        int endX = Position.NormalizedPos(self.CurrentPos.X + Range.MaxRange, self.Arena.Mapsize);
        int startY = Position.NormalizedPos(self.CurrentPos.Y - Range.MaxRange, self.Arena.Mapsize);
        int endY = Position.NormalizedPos(self.CurrentPos.Y + Range.MaxRange, self.Arena.Mapsize);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Position origin = self.CurrentPos;
                Position target = new Position(x, y);
                if (dir == Direction.None)
                {
                    if (Range.InRange(origin, target))
                    {
                        result.Add(target);
                    }
                } else
                {
                    if (Range.InRange(origin, target, dir))
                    {
                        result.Add(target);
                    }
                }
            }
        }

        return result;
    }

    public List<Position> getInRangeTiles(BattleEntity self)
    {
        return getInRangeTiles(self, Direction.None);
    }

    public List<Position> getAoeTiles(BattleEntity self, Position target, Direction dir)
    {
        var result = new List<Position>();
        if (AreaOfEffect == null)
        {
            result.Add(target);
            return result;
        }

        int startX = Position.NormalizedPos(target.X - AreaOfEffect.MaxArea, self.Arena.Mapsize);
        int endX = Position.NormalizedPos(target.X + AreaOfEffect.MaxArea, self.Arena.Mapsize);
        int startY = Position.NormalizedPos(target.Y - AreaOfEffect.MaxArea, self.Arena.Mapsize);
        int endY = Position.NormalizedPos(target.Y + AreaOfEffect.MaxArea, self.Arena.Mapsize);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Position origin = target;
                Position aoe = new Position(x, y);
                bool inArea = false;

                if (TargetType == TargetType.Position)
                {

                    if (AreaOfEffect.InArea(origin, aoe))
                    {
                        inArea = true;
                    }
                }

                if (TargetType == TargetType.Directional)
                {
                    if (AreaOfEffect.InArea(origin, aoe, dir))
                    {
                        inArea = true;
                    }
                }

                if (inArea)
                {
                    result.Add(aoe);
                }
            }
        }
        return result;
    }
}
