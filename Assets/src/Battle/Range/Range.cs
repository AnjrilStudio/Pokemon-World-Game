using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


abstract class Range
{
    public int MaxRange { get; protected set; }

    public abstract bool InRange(Position origin, Position target, Direction dir);

    public virtual bool InRange(Position origin, Position target)
    {
        return InRange(origin, target, Direction.None);
    }
}
