using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.Message;
using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AnimationBattleState : BattleState
{
    private List<GameObject> currentFx;
    private bool init;
    private bool ko;

    public AnimationBattleState(BattleContext context)
    {
        currentFx = new List<GameObject>();
        init = false;
        ko = false;

        Context = context;
    }

    public override BattleState Execute()
    {
        //Debug.Log("AnimationBattleState " + Context.CurrentActionNumber);

        if (!init)
        {
            if (Context.EndBattle)
            {
                PlayEndBattleFx();
            }
            else if (Context.PokemonToKOList.Count > 0)
            {
                PlayPokemonKOFx();
                ko = true;
            }
            else if (Context.PokemonToGoList.Count > 0)
            {
                PlayPokemonGoFx();
            }
            else if (Context.PokemonToComeBackList.Count > 0)
            {
                PlayComeBackFx();
            }
            else
            {
                if (Context.CurrentBattleAction.Action != null)
                {
                    PlayMoveFx();

                    foreach (BattleStateGroundEffect groundEffect in Context.CurrentBattleAction.State.GroundEffects)
                    {
                        if (!Context.GroundEffects.ContainsKey(groundEffect.InstanceId))
                        {
                            Context.GroundEffects.Add(groundEffect.InstanceId, new GroundEffectClient(groundEffect));
                        }
                    }
                }
            }

            init = true;
        }

        if (init && currentFx.Count == 0)
        {
            if (ko)
            {
                Context.PokemonToKOList.RemoveAt(0);
                if (Context.PokemonToKOList.Count > 0)
                {
                    return new DialogBattleState(Context);
                } else
                {
                    return new WaitingForActionBattleState(Context);
                }
            } else
            {
                return new ActionBattleState(Context);
            }
            
        } else
        {
            //animation en cours
            UpdateEffects();

            return this;
        }
        
    }

    private void PlayEndBattleFx()
    {
        //TODO
    }

    private void PlayPokemonKOFx()
    {
        //TODO
    }

    private void PlayPokemonGoFx()
    {
        //TODO
    }

    private void PlayComeBackFx()
    {
        //TODO
    }

    private void PlayMoveFx()
    {
        if (currentFx.Count == 0) //init
        {
            var target = Context.CurrentBattleAction.Target;
            var entity = Context.Turns[Context.CurrentTurn];
            var action = Context.CurrentBattleAction.Action;
            var dir = Context.CurrentBattleAction.Dir;

            var currentPos = new Vector3(Context.Tilesize * entity.CurrentPos.X, -Context.Tilesize * entity.CurrentPos.Y, -2);
            var targetPos = new Vector3(Context.Tilesize * target.X, -Context.Tilesize * target.Y, -2);

            foreach (FxDescriptor fx in MoveFx.Get((Move)action.Id))
            {
                if (fx.Pattern != null && fx.PrefabName != null)
                {
                    GameObject fxObj = new GameObject();
                    fxObj.transform.parent = Context.ScaleNode.transform;
                    fxObj.transform.localScale = new Vector3(1, 1, 1);
                    var partgen = fxObj.AddComponent<ParticleGenerator>();
                    partgen.Pattern = fx.Pattern;
                    partgen.PrefabName = fx.PrefabName;
                    partgen.Active = true;
                    if (fx.Type == FxType.FromTarget)
                    {
                        fxObj.transform.localPosition = targetPos;
                        fxObj.transform.rotation = Quaternion.AngleAxis(ClientUtils.GetDirRotation(dir), Vector3.back);
                    }
                    else if (fx.Type == FxType.ToTarget)
                    {
                        partgen.Target = targetPos - currentPos;
                        fxObj.transform.localPosition = currentPos;
                    }

                    currentFx.Add(fxObj);
                    //animTimer = Mathf.Min(-(fx.Pattern.Duration + fx.Pattern.Delay), animTimer);
                }
            }

            //todo animation de deplacement
            if ((Move)action.Id == Move.Move)
            {
                GameObject fxObj = new GameObject();
                fxObj.transform.parent = Context.ScaleNode.transform;
                currentFx.Add(fxObj);
                GameObject.Destroy(fxObj, 0.5f); //delay de 0.5 sec
            }

            var spriteRenderer = entity.Pokemon.GetComponent<SpriteRenderer>();
            switch (dir)
            {
                case Direction.Down:
                    spriteRenderer.sprite = Resources.Load<Sprite>("pokemonSprites/front/" + entity.PokedexId);
                    break;
                case Direction.Up:
                    spriteRenderer.sprite = Resources.Load<Sprite>("pokemonSprites/back/" + entity.PokedexId);
                    break;
                case Direction.Right:
                    spriteRenderer.sprite = Resources.Load<Sprite>("pokemonSprites/right/" + entity.PokedexId);
                    break;
                case Direction.Left:
                    spriteRenderer.sprite = Resources.Load<Sprite>("pokemonSprites/left/" + entity.PokedexId);
                    break;
                default:
                    break;
            }

            //ClearHighlight();
        }
    }

    private void UpdateEffects()
    {
        currentFx.RemoveAll(fx => fx == null);
        //currentFx.ForEach(fx => fx.SetActive(!(dialogWriting && dialogConfirm)));
    }
}
