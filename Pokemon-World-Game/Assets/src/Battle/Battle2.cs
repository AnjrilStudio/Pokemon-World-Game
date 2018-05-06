using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.State;
using UnityEngine.SceneManagement;
using Anjril.PokemonWorld.Common.Parameter;
using Anjril.PokemonWorld.Common.Message;

public class Battle2 : MonoBehaviour
{

    private BattleState currentBattleState;

    // Use this for initialization
    void Start()
    {
        Global.Instance.CurrentScene = SceneManager.GetActiveScene().name;
        BattleContext context = new BattleContext(gameObject);
        currentBattleState = new WaitingForActionBattleState(context);

        
        //HighlightAction(playerTurn);
    }
    

    // Update is called once per frame
    void Update()
    {
        updateGroundEffects();

        currentBattleState = currentBattleState.Execute();
    }

    private void updateGroundEffects()
    {
        foreach (GroundEffectClient groundEffect in currentBattleState.Context.GroundEffects.Values)
        {
            groundEffect.Timer += Time.deltaTime;

            if (groundEffect.Timer >= 0)
            {
                foreach (FxDescriptor fx in GroundFx.Get((GroundEffectOverTimeId)groundEffect.EffectId))
                {
                    if (fx.Pattern != null && fx.PrefabName != null)
                    {
                        GameObject fxObj = new GameObject();
                        fxObj.transform.parent = currentBattleState.Context.ScaleNode.transform;
                        fxObj.transform.localScale = new Vector3(1, 1, 1);
                        var partgen = fxObj.AddComponent<ParticleGenerator>();
                        partgen.Pattern = fx.Pattern;
                        partgen.PrefabName = fx.PrefabName;
                        fxObj.transform.localPosition = new Vector3(currentBattleState.Context.Tilesize * groundEffect.Position.X, -currentBattleState.Context.Tilesize * groundEffect.Position.Y, -2);

                        groundEffect.Timer -= fx.Pattern.Duration;
                    }
                }
            }
        }
    }

    

    private void OnApplicationQuit()
    {
        Debug.Log("quit");
        Global.Instance.Client.Disconnect(Global.Instance.PlayerId.ToString());
    }
}