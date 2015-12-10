using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Action
{
    public TargetType TargetType;
    public Range Range { get; set; }
    public Range Range2 { get; set; }
    public AreaOfEffect AreaOfEffect;
    public List<HitEffect> HitEffects { get; private set; }
    public List<GroundEffect> GroundEffects { get; private set; }
    public ActionCost ActionCost { get; set; }
    public List<FxDescriptor> Fx { get; private set; }
    public bool NextTurn { get; set; }

    public Action()
    {
        TargetType = TargetType.None;
        Fx = new List<FxDescriptor>();
        HitEffects = new List<HitEffect>();
        GroundEffects = new List<GroundEffect>();
        NextTurn = true;
    }

    public List<Position> InRangeTiles(BattleEntity self, Direction dir)
    {
        return Range.InRangeTiles(self, dir);
    }

    public List<Position> InRange2Tiles(BattleEntity self, Direction dir)
    {
        if (Range2 == null)
        {
            return new List<Position>();
        }
        return Range2.InRangeTiles(self, dir).Except(Range2.InRangeTiles(self, dir)).ToList();
    }

    public List<Position> InRangeTiles(BattleEntity self)
    {
        return InRangeTiles(self, Direction.None);
    }

    public List<Position> InRange2Tiles(BattleEntity self)
    {
        return InRange2Tiles(self, Direction.None);
    }

    public List<Position> AoeTiles(BattleEntity self, Position target, Direction dir)
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
