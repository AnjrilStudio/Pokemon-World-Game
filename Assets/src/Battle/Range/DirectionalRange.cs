using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


abstract class DirectionalRange : Range
{
    public override bool InRange(Position origin, Position target)
    {
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (InRange(origin, target, dir))
            {
                return true;
            }
        }
        return false;
    }
}
