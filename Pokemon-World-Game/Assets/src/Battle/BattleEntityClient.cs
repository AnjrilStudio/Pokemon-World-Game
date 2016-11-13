using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.Index;
using Anjril.PokemonWorld.Common.Message;
using Anjril.PokemonWorld.Common.State;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class BattleEntityClient : BattleEntity
{
    public GameObject Pokemon { get; set; }

    private float verticalOffset = 0.08f;

    public BattleEntityClient(int id, int pokedexId, int playerId, int level) : base (id, pokedexId, playerId, level)
    {
        var pkmnObj = GameObject.Instantiate(Resources.Load("PokemonPrefab")) as GameObject;

        var spriteRenderer = pkmnObj.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>("pokemonSprites/front/" + pokedexId);

        Pokemon = pkmnObj;
        Pokemon.name = Pokedex.GetPokemonSheetByNationalId(pokedexId).Name;
    }
    
    public void MoveBattleEntity(Position target, BattleArenaClient arena)
    {
        arena.MoveBattleEntity(this, target);
        Pokemon.transform.localPosition = new Vector3(target.X * arena.Tilesize, -target.Y * arena.Tilesize + verticalOffset, -2);
        CurrentPos = new Position(target.X, target.Y);
    }

    public void UpdateBattleEntity(BattleStateEntity entity, BattleArenaClient arena)
    {
        MoveBattleEntity(entity.CurrentPos, arena);
        HP = entity.HP;
        MaxHP = entity.MaxHP;
        AP = entity.AP;
        MaxAP = entity.MaxAP;
        MP = entity.MP;
        MaxMP = entity.MaxMP;
        APMP = entity.APMP;
        ComingBack = entity.ComingBack;
        if (entity.ComingBack)
        {
            Pokemon.SetActive(false);
        }
    }

}
