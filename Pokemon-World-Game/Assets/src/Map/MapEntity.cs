using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class MapEntity
{
    private static float defautMoveTime = 0.6f;

    public GameObject Object { get; private set; }
    public Position CurrentPos { get; set; }
    public Position OldPos { get; set; }
    public Direction CurrentDir { get; set; }
    public float MoveTimer { get; set; }
    public float MoveTime { get; set; }

    public List<Pokemon> Pokemons { get; private set; }
    public bool IA;
    public int Id { get; private set; }

    public MapEntity(int id, GameObject obj, int X, int Y)
    {
        Object = obj;
        CurrentPos = new Position(X, Y);
        OldPos = new Position(X, Y);
        Pokemons = new List<Pokemon>();
        IA = true;
        Id = id;
        MoveTimer = 0;
        MoveTime = defautMoveTime;
        CurrentDir = Direction.Down;
    }

    public MapEntity(int id)
    {
        Id = id;
    }

    public override bool Equals(object entityObj)
    {
        var entity = entityObj as MapEntity;
        return Id == entity.Id;
    }

    public override int GetHashCode()
    {
        return Id;
    }

}
