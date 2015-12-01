using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


abstract class AreaOfEffect
{
    public int MaxArea { get; protected set; }

    public abstract bool InArea(Position origin, Position target, Direction dir);

    public bool InArea(Position origin, Position target)
    {
        return InArea(origin, target, Direction.None);
    }
}
