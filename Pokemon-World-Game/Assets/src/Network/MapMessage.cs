using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MapMessage
{
    public Position Origin { get; private set;}
    public string Segments { get; private set; }

    public MapMessage(Position origin, string segments)
    {
        Origin = origin;
        Segments = segments;
    }
}
