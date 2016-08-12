using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MoveMessage
{
    public int Id { get; private set; }
    public Position Position { get; private set;}
    public Direction Direction { get; private set; }
    public bool IsPlayer { get; private set; }

    public MoveMessage(int id, Position position, bool isPlayer)
    {
        Id = id;
        Position = position;
        IsPlayer = isPlayer;
    }

    public MoveMessage(string entityStr)
    {
        var id = entityStr.Split('=')[0].Split('-')[1];
        var type = entityStr.Split('=')[0].Split('-')[0];
        var position = entityStr.Split('=')[1];
        var x = position.Split(':')[0];
        var y = position.Split(':')[1];
        var dir = position.Split(':')[2];

        Id = Int32.Parse(id);

        Position = new Position(Int32.Parse(x), Int32.Parse(y));

        IsPlayer = type.Equals("Player");

        Direction = NetworkUtils.ParseDirection(dir);
    }
}
