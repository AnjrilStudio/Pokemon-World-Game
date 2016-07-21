using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;


static class Utils
{
    public static Position GetDirPosition(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return new Position(0, 1);
            case Direction.Right:
                return new Position(1, 0);
            case Direction.Down:
                return new Position(0, -1);
            case Direction.Left:
                return new Position(-1, 0);
            default:
                return new Position(0, 0);
        }
    }

    public static float GetDirRotation(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return 90f;
            case Direction.Right:
                return 0;
            case Direction.Down:
                return -90f;
            case Direction.Left:
                return 180f;
            default:
                return 0;
        }
    }

    public static Direction GetDirection(Position p1, Position p2)
    {
        if (Position.Distance(p1, p2) != 1)
        {
            return Direction.None;
        } else
        {
            if (p1.X == p2.X)
            {
                if (p1.Y < p2.Y)
                {
                    return Direction.Down;
                } else
                {
                    return Direction.Up;
                }
            } else
            {
                if (p1.X < p2.X)
                {
                    return Direction.Right;
                }
                else
                {
                    return Direction.Left;
                }
            }
        }
    }

    public static Direction GetRandomDir()
    {
        int v = Mathf.FloorToInt(Random.value * 4);
        switch (v)
        {
            case 0:
                return Direction.Up;
            case 1:
                return Direction.Right;
            case 2:
                return Direction.Down;
            case 3:
                return Direction.Left;
            default:
                return Direction.None;
        }

    }
}
