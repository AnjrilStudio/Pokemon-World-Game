using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class BattleEntity
{
    public GameObject Pokemon { get; private set; }
    public List<Action> Actions { get; private set; }
    public bool AI { get; private set; }
    public Arena Arena { get; private set; }
    public Position CurrentPos { get; set; }

    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Level { get; set; }

    public int Atk { get; set; }
    public int Def { get; set; }
    public int AtkSpe { get; set; }
    public int DefSpe { get; set; }
    public int Vit { get; set; }

    public int AP { get; set; }
    public int MaxAP { get; set; }
    public int MP { get; set; }
    public int MaxMP { get; set; }


    public BattleEntity(Arena arena, GameObject pokemon, bool ai)
    {
        Pokemon = pokemon;
        AI = ai;
        Arena = arena;
        Actions = new List<Action>();
        CurrentPos = new Position(0, 0);

        HP = 20;
        MaxHP = HP;
        Level = 5;
        Atk = 15;
        Def = 15;
        AtkSpe = 15;
        DefSpe = 15;
        Vit = 15;

        MaxAP = 4;
        AP = MaxAP;
        MaxMP = 3;
        MP = MaxMP;
    }
    
    public void MoveBattleEntity(Position target)
    {
        Pokemon.transform.position = new Vector3(target.X * Arena.Tilesize, -target.Y * Arena.Tilesize, 0);
        CurrentPos.X = target.X;
        CurrentPos.Y = target.Y;
    }

}
