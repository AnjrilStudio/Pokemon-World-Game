using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.Message;
using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActionBattleState : BattleState
{
    public ActionBattleState(BattleContext context)
    {
        Context = context;
    }

    public override BattleState Execute()
    {
        //Debug.Log("ActionBattleState " + Context.CurrentActionNumber);

        //maj arena
        if (Context.CurrentBattleAction.Arena != null)
        {
            Context.Arena.update(Context.CurrentBattleAction.Arena);
            InitCamera();
        }

        if (Context.EndBattle)
        {
            SceneManager.LoadScene("scene_map");
        }
        else if (Context.PokemonToGoList.Count > 0)
        {

            var entity = Context.PokemonToGoList[0];
            var pkmn = initPokemon(entity.Id, entity.PokemonId, entity.Level, entity.CurrentPos, entity.PlayerId);
            pkmn.HP = entity.HP;
            pkmn.MaxHP = entity.MaxHP;

            //pkmn.Pokemon.SetActive(false);

            Context.PokemonToGoList.RemoveAt(0);
        }
        else if (Context.PokemonToComeBackList.Count > 0)
        {
            RemovePokemon(Context.PokemonToComeBackList[0]);
            Context.PokemonToComeBackList.RemoveAt(0);
        }
        else
        {
            foreach (BattleStateEntity entity in Context.CurrentBattleAction.State.Entities)
            {
                if (Context.Entities.ContainsKey(entity.Id))
                {
                    var battleEntity = Context.Entities[entity.Id];
                    bool alive = UpdatePokemon(battleEntity, entity);
                    if (!alive)
                    {
                        Context.PokemonToKOList.Add(battleEntity);
                    }
                }
            }
        }

        //maj turns (dans le bon ordre)
        Context.Turns.Clear();
        foreach (BattleStateEntity entity in Context.CurrentBattleAction.State.Entities)
        {
            Context.Turns.Add(Context.Entities[entity.Id]);
        }

        //GUI
        DisplayGUI();


        //animation en cours
        var animating = updateDamageEffects();

        if (animating)
        {
            return this;
        } else
        {
            if (Context.PokemonToGoList.Count > 0 || Context.PokemonToComeBackList.Count > 0 || Context.PokemonToKOList.Count > 0)
            {
                return new DialogBattleState(Context);
            } else
            {

                Context.CurrentActionNumber++;
                Context.CurrentTurn = Context.CurrentBattleAction.State.CurrentTurn;

                return new WaitingForActionBattleState(Context);
            }
        }

    }

    private BattleEntityClient initPokemon(int id, int pokemonId, int level, Position pos, int playerId)
    {
        var entitiesNode = GameObject.FindGameObjectWithTag("Entities");

        var battleEntity = new BattleEntityClient(id, pokemonId, playerId, level);

        battleEntity.Pokemon.transform.parent = entitiesNode.transform;
        battleEntity.Pokemon.transform.localScale = new Vector3(1.25f, 1.25f, 1);

        Context.Entities.Add(id, battleEntity);

        battleEntity.MoveBattleEntity(pos, Context.Arena);

        return battleEntity;
    }

    private void RemovePokemon(int id)
    {
        Context.Arena.RemoveBattleEntity(Context.Entities[id]);
        GameObject.Destroy(Context.Entities[id].Pokemon);
        Context.Entities.Remove(id);
    }

    
    //return false si KO, true sinon
    private bool UpdatePokemon(BattleEntityClient pokemon, BattleStateEntity state)
    {
        var currentHP = pokemon.HP;

        pokemon.UpdateBattleEntity(state, Context.Arena);
        int damage = currentHP - pokemon.HP;
        if (damage > 0)
        {
            //animation de prise de dégats
            pokemon.DamageAnimationTimer = 0.2f;
            logDamage(pokemon, damage);

            if (pokemon.HP == 0)
            {
                return false;
            }
        }

        return true;
    }

    private bool updateDamageEffects()
    {
        bool animating = false;
        foreach (BattleEntityClient pokemon in Context.Entities.Values)
        {
            if (pokemon.DamageAnimationTimer > 0)
            {
                animating = true;
                pokemon.Pokemon.SetActive(!pokemon.Pokemon.activeSelf);
                pokemon.DamageAnimationTimer -= Time.deltaTime;

                // au cas où pile à 0, il ne faut pas rester bloquer à false
                if (pokemon.DamageAnimationTimer == 0)
                {
                    pokemon.Pokemon.SetActive(true);
                }
            }
            else if (pokemon.DamageAnimationTimer < 0)
            {
                pokemon.DamageAnimationTimer = 0;
                pokemon.Pokemon.SetActive(true);
            }
        }

        return animating;
    }

    private void logDamage(BattleEntityClient pokemon, int damage)
    {
        string log = string.Format("{0} subit {1} dégats.", pokemon.Pokemon.name, damage);
        AddLog(log);
    }
}
