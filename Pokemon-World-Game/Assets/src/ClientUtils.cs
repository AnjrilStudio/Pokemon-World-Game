using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Anjril.PokemonWorld.Common.State;

static class ClientUtils
{
    public static float GetDirRotation(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return -90f;
            case Direction.Right:
                return 0;
            case Direction.Down:
                return 90f;
            case Direction.Left:
                return 180f;
            default:
                return 0;
        }
    }
    
}
