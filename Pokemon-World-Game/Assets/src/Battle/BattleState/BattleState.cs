using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public abstract class BattleState
{
    public BattleContext Context { get; set; }

    public abstract BattleState Execute();
    
    protected void InitCamera()
    {
        //Debug.Log("InitCamera");

        var scaleValue = 10f / Mathf.Max(Context.Arena.Width, Context.Arena.Height);
        Context.ScaleNode.transform.localScale = new Vector3(scaleValue, scaleValue, 1);

        Camera camera = Context.BattleObject.GetComponent<Camera>();
        camera.orthographicSize = 2;
        var cameraOffset = 10; //?
        Context.BattleObject.transform.position = new Vector3(Context.Tilesize * (cameraOffset - 1) / 2, -Context.Tilesize * (cameraOffset - 1) / 2, Context.BattleObject.transform.position.z);
        Context.BattleObject.transform.position = new Vector3(Context.BattleObject.transform.position.x + 1, Context.BattleObject.transform.position.y - 0.35f, Context.BattleObject.transform.position.z);

    }

    protected void AddLog(string log)
    {
        Context.BattleLog.Insert(0, log);
        if (Context.BattleLog.Count > 10)
        {
            Context.BattleLog.RemoveAt(10);
        }
    }

    protected void DisplayGUI()
    {
        GameObject canvas = GameObject.Find("Canvas");

        foreach (Transform child in canvas.transform)
        {
            if (child.name != "TextBox")//FIXME
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        //pokemons
        int index = 0;
        foreach (BattleEntityClient turn in Context.Turns)
        {
            if (!turn.ComingBack)
            {
                var textObject = new GameObject("text");
                textObject.transform.parent = canvas.transform;
                textObject.transform.localPosition = new Vector3(320, 300 - index * 50, 0);
                var textComp = textObject.AddComponent<Text>();
                textComp.fontSize = 20;
                textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                var text = (index == Context.CurrentTurn) ? " -> " : "    ";
                text += turn.Pokemon.name + " ";
                text += "L" + turn.Level + " ";
                text += "HP: " + turn.HP + "/" + turn.MaxHP + " ";
                text += "AP: " + turn.AP + "/" + turn.MaxAP + " ";
                text += "MP: " + turn.MP + "/" + turn.MaxMP + " ";
                textComp.text = text;
                textComp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
            }
            index++;
        }

        //battle log
        int logIndex = 0;
        foreach (string log in Context.BattleLog)
        {
            var textObject = new GameObject("log");
            textObject.transform.parent = canvas.transform;
            textObject.transform.localPosition = new Vector3(320, -100 - logIndex * 30, 0);
            var textComp = textObject.AddComponent<Text>();
            textComp.fontSize = 15;
            textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            textComp.text = log;
            textComp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
            logIndex++;
        }
    }

    
}
