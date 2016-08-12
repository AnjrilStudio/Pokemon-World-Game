using Anjril.PokemonWorld.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Arena : BattleArena
{
    public float Tilesize { get; private set; }
    public GameObject[,] Tiles;

    public Arena (int size, float tilesize) : base(size)
    {
        Tilesize = tilesize;

        Tiles = new GameObject[ArenaSize, ArenaSize];
        for (int i = 0; i < ArenaSize; i++)
        {
            for (int j = 0; j < ArenaSize; j++)
            {
                var obj = GameObject.Instantiate(Resources.Load("ground/ground")) as GameObject;
                var mapNode = GameObject.FindGameObjectWithTag("Arena");
                obj.transform.parent = mapNode.transform;
                obj.transform.position = new Vector3(tilesize * i, -tilesize * j, 0);
                Tiles[i, j] = obj;
            }
        }
    }
}
