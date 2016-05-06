using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class MapEntity
{
    public GameObject Object { get; private set; }
    public Position CurrentPos { get; set; }
    public Position OldPos { get; set; }
    public List<Pokemon> Pokemons { get; private set; }
    public bool IA;

    public MapEntity(GameObject obj, int X, int Y)
    {
        Object = obj;
        CurrentPos = new Position(X, Y);
        OldPos = new Position(X, Y);
        Pokemons = new List<Pokemon>();
        IA = true;
    }

}
