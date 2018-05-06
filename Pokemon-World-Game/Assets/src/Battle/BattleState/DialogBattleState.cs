using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DialogBattleState : BattleState
{
    protected float textSpeed = 0.015f;
    protected bool dialogConfirm = false;
    protected bool dialogWriting = false;

    protected int textIndex;
    protected float textTimer;

    protected bool dialogInit;

    public DialogBattleState(BattleContext context)
    {
        Context = context;

        textIndex = 0;
        textTimer = 0;

        dialogInit = false;
    }

    public override BattleState Execute()
    {
        //Debug.Log("DialogBattleState " + Context.CurrentActionNumber);

        if (!dialogInit)
        {
            if (Context.EndBattle)
            {
                DialogEndBattle();
            }
            else if (Context.PokemonToKOList.Count > 0)
            {
                DialogPokemonKO(Context.PokemonToKOList[0]);
            }
            else if (Context.PokemonToGoList.Count > 0)
            {
                DialogPokemonGo(Context.PokemonToGoList[0].PokemonId);
            }
            else if (Context.PokemonToComeBackList.Count > 0)
            {
                DialogPokemonComeBack(Context.Entities[Context.PokemonToComeBackList[0]]);
            }
            else
            {
                if (Context.CurrentBattleAction.Action != null)
                {
                    DialogMove(Context.Turns[Context.CurrentTurn], (Move)Context.CurrentBattleAction.Action.Id);
                }
            }

            dialogInit = true;
        }

        updateTextBox();

        if (dialogInit && !dialogWriting)
        {
            return new AnimationBattleState(Context);
        } else
        {
            //en cours d'écriture
            return this;
        }

        
    }

    protected void updateTextBox()
    {
        GameObject canvas = GameObject.Find("Canvas");

        var skip = false;
        if ((Input.anyKeyDown && dialogConfirm))
        {
            skip = true;
        }

        if (Context.DialogBoxQueue.Count > 0)
        {
            DialogMessage currentDialog = Context.DialogBoxQueue.Peek();
            dialogConfirm = currentDialog.Confirm;

            if (textIndex <= currentDialog.Text.Length)
            {
                dialogWriting = true;

                if (skip)
                {
                    //fait apparaitre le texte entier directement
                    textIndex = currentDialog.Text.Length;
                }

                if (textIndex == 0)
                {
                    //initialisation d'un nouveau texte

                    var textBoxObject = new GameObject("TextBox");
                    textBoxObject.transform.parent = canvas.transform;
                    textBoxObject.transform.localPosition = new Vector3(-500, -250, 0);

                    //sprite
                    var imgObject = new GameObject("image");
                    imgObject.transform.parent = textBoxObject.transform;
                    imgObject.transform.localPosition = new Vector3(0, 0, 0);

                    var imgComp = imgObject.AddComponent<Image>();
                    imgComp.sprite = Resources.Load<Sprite>("highlight");
                    imgComp.color = Color.gray;
                    var rect1 = imgObject.GetComponent<RectTransform>();
                    rect1.pivot = new Vector2(0, 0);
                    rect1.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 600);
                    rect1.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);

                    //text
                    var textObject = new GameObject("text");
                    textObject.transform.parent = textBoxObject.transform;
                    textObject.transform.localPosition = new Vector3(0, 0, 0);

                    var textComp = textObject.AddComponent<Text>();
                    textComp.fontSize = 15;
                    textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                    textComp.text = "";
                    var rect2 = textObject.GetComponent<RectTransform>();
                    rect2.pivot = new Vector2(0, 0);
                    rect2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 600);
                    rect2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);


                    textIndex++;
                    textTimer = 0;
                }
                else
                {
                    //maj du texte
                    textTimer += Time.deltaTime;
                    while (textTimer > textSpeed && textIndex <= currentDialog.Text.Length)
                    {
                        var textBoxObject = canvas.transform.Find("TextBox");
                        var textComp = textBoxObject.GetComponentInChildren<Text>();

                        textComp.text = currentDialog.Text.Substring(0, textIndex);

                        textIndex++;
                        textTimer -= textSpeed;
                    }
                }

            }
            else
            {
                if (skip)
                {
                    dialogWriting = false;
                    NextDialog();
                }
            }

        }
    }

    protected void NextDialog()
    {
        if (Context.DialogBoxQueue.Count > 0)
        {
            GameObject canvas = GameObject.Find("Canvas");
            var textBoxObject = canvas.transform.Find("TextBox");
            Context.DialogBoxQueue.Dequeue();
            GameObject.Destroy(textBoxObject.gameObject);
        }
    }

    protected void AddDialog(string text)
    {
        AddDialog(text, true);
    }

    protected void AddDialog(string text, bool confirm)
    {
        Context.DialogBoxQueue.Enqueue(new DialogMessage(text, confirm));
    }

    private void DialogMove(BattleEntityClient pokemon, Move move)
    {
        string log = string.Format("{0} lance {1}.", pokemon.Pokemon.name, move.ToString());
        AddDialog(log);
    }

    private void DialogPokemonGo(int pokedexId)
    {
        string log = string.Format("{0} rejoint le combat.", Pokedex.GetPokemonSheetByNationalId(pokedexId).Name);
        AddDialog(log);
    }

    private void DialogPokemonComeBack(BattleEntityClient pokemon)
    {
        string log = string.Format("{0} est rappelé.", pokemon.Pokemon.name);
        AddDialog(log);
    }

    private void DialogPokemonKO(BattleEntityClient pokemon)
    {
        string log = string.Format("{0} est KO.", pokemon.Pokemon.name);
        AddDialog(log);
    }

    private void DialogEndBattle()
    {
        string log = "Vous avez quitté le combat.";
        AddDialog(log);
    }
}
