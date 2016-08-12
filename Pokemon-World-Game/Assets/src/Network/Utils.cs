using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class NetworkUtils
{
    public static Direction ParseDirection(string s)
    {
        switch (s)
        {
            case "U":
                return Direction.Up;
            case "R":
                return Direction.Right;
            case "D":
                return Direction.Down;
            case "L":
                return Direction.Left;
            default:
                return Direction.None;
        }
    }
}
