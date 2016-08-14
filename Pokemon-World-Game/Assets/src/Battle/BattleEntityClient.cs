﻿using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.State;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class BattleEntityClient : BattleEntity
{
    public GameObject Pokemon { get; set; }

    public BattleEntityClient(int id, int pokemonId, int playerId) : base (id, pokemonId, playerId)
    {
        switch (pokemonId)
        {
            case 0:
                Pokemon = GameObject.Instantiate(Resources.Load("Rattata")) as GameObject;
                Pokemon.name = "Rattata";
                break;
            case 1:
                Pokemon = GameObject.Instantiate(Resources.Load("Roucool")) as GameObject;
                Pokemon.name = "Roucool";
                break;
            case 2:
                Pokemon = GameObject.Instantiate(Resources.Load("Ptitard")) as GameObject;
                Pokemon.name = "Ptitard";
                break;
            default:
                break;

        }
    }
    
    public void MoveBattleEntity(Position target, float arenaTilesize)
    {
        Pokemon.transform.position = new Vector3(target.X * arenaTilesize, -target.Y * arenaTilesize, -2);
        CurrentPos = new Position(target.X, target.Y);
    }

    public void UpdateBattleEntity(BattleStateEntity entity, float arenaTilesize)
    {
        MoveBattleEntity(entity.CurrentPos, arenaTilesize);
        HP = entity.HP;
        MaxHP = entity.MaxHP;
        AP = entity.AP;
        MaxAP = entity.MaxAP;
        MP = entity.MP;
        MaxMP = entity.MaxMP;
    }

}