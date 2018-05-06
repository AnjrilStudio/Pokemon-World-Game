using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.Message;
using Anjril.PokemonWorld.Common.Parameter;
using Anjril.PokemonWorld.Common.State;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForActionBattleState : DialogBattleState
{

    private List<Action> trainerActionList;
    private bool isCurrentActionTrainer;
    private bool isPokemonGoAction;
    private bool isPokemonGoSelection;

    public GameObject hover;
    public GameObject scaleNode;
    private List<GameObject> highlightRange;
    private List<GameObject> highlightAOE;

    public float mousex;
    public float mousey;
    public float x0;
    public float y0;
    private Position mouseTilePos;

    private int currentActionInt;
    private Action CurrentAction
    {
        get
        {
            if (currentActionInt < 0) return null;
            if (isCurrentActionTrainer || isPokemonGoSelection)
            {
                if (!isPokemonGoAction && !isPokemonGoSelection)
                {
                    return trainerActionList[currentActionInt];
                }
                else
                {
                    return TrainerActions.Get(TrainerAction.Pokemon_Go);
                }

            }
            else
            {
                if (Context.Turns.Count > 0)
                {
                    return Context.Turns[Context.CurrentTurn].Moves[currentActionInt];
                }
                else
                {
                    return null;
                }
            }
        }
    }
    private Direction currentActionDir;

    public WaitingForActionBattleState(BattleContext context) : base(context)
    {
        mouseTilePos = new Position(0, 0);

        trainerActionList = new List<Action>();
        trainerActionList.Add(TrainerActions.Get(TrainerAction.End_Battle));
        trainerActionList.Add(TrainerActions.Get(TrainerAction.Pokemon_Go));
        trainerActionList.Add(TrainerActions.Get(TrainerAction.Pokemon_Come_Back));
        trainerActionList.Add(TrainerActions.Get(TrainerAction.Pokeball));

        hover = GameObject.Instantiate(Resources.Load("hover")) as GameObject;
        scaleNode = GameObject.FindGameObjectWithTag("BattleScale");
        hover.transform.parent = scaleNode.transform;
        hover.gameObject.transform.localScale = Vector3.one;
        hover.SetActive(false);

        highlightRange = new List<GameObject>();
        highlightAOE = new List<GameObject>();

        ClearHighlight();
        currentActionInt = -1;
        
}

    public override BattleState Execute()
    {
        //Debug.Log("WaitingForActionBattleState " + Context.CurrentActionNumber);

        if (Global.Instance.BattleActionMessages.Count > 0)
        {
            bool spectator = false;
            BattleActionMessage battleaction = Global.Instance.BattleActionMessages.Peek();
            if (Context.CurrentActionNumber == -1) //spectateur
            {
                spectator = true;
                Context.CurrentActionNumber = battleaction.ActionId - 1;
            }

            if (battleaction.ActionId == Context.CurrentActionNumber + 1)
            {
                Context.CurrentBattleAction = Global.Instance.BattleActionMessages.Dequeue();

                //maj arena (start)
                if (Context.CurrentBattleAction.Arena != null)
                {
                    Context.Arena.update(Context.CurrentBattleAction.Arena);

                    InitCamera();
                }

                //end battle
                if (battleaction.State == null)
                {
                    Context.EndBattle = true;
                }

                //maj entities
                var actualEntities = new List<int>();
                BattleStateMessage battlestate = battleaction.State;
                foreach (BattleStateEntity entityState in battlestate.Entities)
                {
                    actualEntities.Add(entityState.Id);
                    if (Context.Entities.ContainsKey(entityState.Id))
                    {
                        var battleEntity = Context.Entities[entityState.Id];
                        if (!battleEntity.ComingBack && entityState.ComingBack)
                        {
                            Context.PokemonToComeBackList.Add(entityState.Id);
                        }
                    }
                    else
                    {
                        Context.PokemonToGoList.Add(entityState);
                    }
                }

                var movingAction = false;
                if (Context.CurrentBattleAction.Action != null)
                {
                    movingAction = (Move)Context.CurrentBattleAction.Action.Id == Move.Move;
                }

                NextDialog();
                GameObject.Destroy(hover);
                if (movingAction)
                {
                    return new AnimationBattleState(Context);
                } else
                {
                    return new DialogBattleState(Context);
                }
            }

        } else
        {
            if (!dialogInit)
            {
                if (Context.Turns[Context.CurrentTurn].PlayerId == Global.Instance.PlayerId)
                {
                    DialogWhatDo(Context.Turns[Context.CurrentTurn]);

                    UpdateTrainerActions(Context.CurrentBattleAction.ActionsAvailable);

                    DisplayActionButton();
                }
                else if (GetPlayerPokemonCount() == 0)
                {
                    UpdateTrainerActions(Context.CurrentBattleAction.ActionsAvailable);

                    DisplayActionButton();
                } else
                {
                    DialogWaiting(Context.Turns[Context.CurrentTurn]);
                }
                dialogInit = true;
            }

            updateTextBox();

            if (Context.Turns[Context.CurrentTurn].PlayerId == Global.Instance.PlayerId || GetPlayerPokemonCount() == 0)
            {
                ManageCursor();
            }
        }
        
        //en attente d'un message serveur
        return this;

    }

    private void ManageCursor()
    {
        //par defaut
        BattleEntity turn = null;

        if (Context.Turns.Count > 0)
        {
            turn = Context.Turns[Context.CurrentTurn];
        }

        bool inRange = false;
        bool inRange2 = false;

        //pointer control
        Camera camera = Context.BattleObject.GetComponent<Camera>();
        Vector3 p = camera.WorldToScreenPoint(new Vector3(0, 0, -10));
        var scaleX = Context.ScaleNode.transform.localScale.x;
        var scaleY = Context.ScaleNode.transform.localScale.y;

        mousex = Input.mousePosition.x - p.x;
        mousey = Input.mousePosition.y - p.y;

        Vector3 p2 = camera.ViewportToWorldPoint(new Vector3(mousex, mousey, 10));
        var x0 = p2.x / (Context.Tilesize * scaleX) / Screen.width;
        var y0 = -p2.y / (Context.Tilesize * scaleY) / Screen.height;

        var mousetileposx = Mathf.RoundToInt(x0);
        var mousetileposy = Mathf.RoundToInt(y0);
        mouseTilePos = new Position(mousetileposx, mousetileposy);

        //hover
        if (mouseTilePos.X >= 0 && mouseTilePos.X < Context.Arena.Width && mouseTilePos.Y >= 0 && mouseTilePos.Y < Context.Arena.Height)
        {
            //pointer tile
            hover.transform.localPosition = new Vector3(Context.Tilesize * mouseTilePos.X, -Context.Tilesize * mouseTilePos.Y, 0);
            hover.SetActive(true);

            //todo faire que si la case change
            foreach (GameObject obj in highlightAOE)
            {
                GameObject.Destroy(obj);
            }
            highlightAOE.Clear();

            if (CurrentAction != null)
            {
                //aoe
                Position target = mouseTilePos;
                if (CurrentAction.TargetType == TargetType.Position)
                {
                    currentActionDir = Direction.None;
                    if (CurrentAction.Range.InRange(Context.Arena, turn, target))
                    {
                        inRange = true;
                    }
                    if (CurrentAction.Range2 != null && CurrentAction.Range2.InRange(Context.Arena, turn, target))
                    {
                        inRange2 = true;
                    }
                }

                if (CurrentAction.TargetType == TargetType.Directional)
                {
                    foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
                    {
                        if (CurrentAction.Range.InRange(Context.Arena, turn, target, dir))
                        {
                            currentActionDir = dir;
                            inRange = true;
                        }
                        if (CurrentAction.Range2 != null && CurrentAction.Range2.InRange(Context.Arena, turn, target, dir))
                        {
                            currentActionDir = dir;
                            inRange2 = true;
                        }
                    }
                }
                if (inRange || inRange2)
                {
                    HighlightAOE(turn, CurrentAction, target, currentActionDir);
                }
            }
        }
        else
        {
            hover.SetActive(false);
        }


        //action control
        //var highlight = CurrentAction != null;
        var highlight = false;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentActionInt = 0;
            highlight = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentActionInt = 1;
            highlight = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentActionInt = 2;
            highlight = true;
        }
        if (highlight)
        {
            HighlightAction(turn);
        }

        //click control
        if ((inRange || inRange2) && Input.GetMouseButtonDown(0) && hover.activeSelf && CurrentAction != null)
        {
            if (isCurrentActionTrainer || isPokemonGoSelection)
            {
                Global.Instance.SendCommand(new BattleTrainerActionParam(mouseTilePos, CurrentAction, currentActionInt));
            }
            else
            {
                Global.Instance.SendCommand(new BattleActionParam(currentActionDir, mouseTilePos, CurrentAction));
            }

            ClearHighlight();
            currentActionInt = -1;
        }
    }

    private void ClearHighlight()
    {
        //clear highlight range
        foreach (GameObject obj in highlightRange)
        {
            GameObject.Destroy(obj);
        }
        highlightRange.Clear();
        foreach (GameObject obj in highlightAOE)
        {
            GameObject.Destroy(obj);
        }
        highlightAOE.Clear();
    }

    private void HighlightAction(BattleEntity self)
    {
        Action action = CurrentAction;
        foreach (GameObject obj in highlightRange)
        {
            GameObject.Destroy(obj);
        }
        highlightRange.Clear();

        foreach (Position target in action.InRangeTiles(self, Context.Arena))
        {
            var highlight = GameObject.Instantiate(Resources.Load("highlight")) as GameObject;
            highlight.transform.parent = scaleNode.transform;
            highlight.transform.localPosition = new Vector3(Context.Tilesize * target.X, -Context.Tilesize * target.Y, 0);
            highlight.transform.localScale = new Vector3(1, 1, 1);
            highlightRange.Add(highlight);
        }

        foreach (Position target in action.InRange2Tiles(self, Context.Arena))
        {
            var highlight = GameObject.Instantiate(Resources.Load("highlight2")) as GameObject;
            highlight.transform.parent = scaleNode.transform;
            highlight.transform.localPosition = new Vector3(Context.Tilesize * target.X, -Context.Tilesize * target.Y, 0);
            highlight.transform.localScale = new Vector3(1, 1, 1);
            highlightRange.Add(highlight);
        }
    }

    private void HighlightAOE(BattleEntity self, Action action, Position target, Direction dir)
    {
        foreach (Position aoe in action.AoeTiles(self, target, dir, Context.Arena))
        {
            var aoeObj = GameObject.Instantiate(Resources.Load("aoe")) as GameObject;
            aoeObj.transform.parent = scaleNode.transform;
            aoeObj.transform.localPosition = new Vector3(Context.Tilesize * aoe.X, -Context.Tilesize * aoe.Y, -1);
            aoeObj.transform.localScale = new Vector3(1, 1, 1);
            highlightAOE.Add(aoeObj);
        }
    }

    private void UpdateTrainerActions(List<TrainerAction> actions)
    {
        trainerActionList.Clear();
        foreach (TrainerAction action in actions)
        {
            trainerActionList.Add(TrainerActions.Get(action));
        }
    }

    private void DisplayActionButton()
    {
        GameObject canvas = GameObject.Find("Canvas");
        int index = 0;
        if (isPokemonGoAction)
        {
            index = 0;
            foreach (Action action in trainerActionList)
            {

                AddActionButton(canvas, action, index, true, false);

                index++;
            }

            index = 0;
            foreach (Pokemon pokemon in Global.Instance.Team)
            {
                AddActionButton(canvas, TrainerActions.Get(TrainerAction.Pokemon_Go), index, false, true);

                index++;
            }
        }
        else if (GetPlayerPokemonCount() == 0 || Context.Turns[Context.CurrentTurn].ComingBack)
        {
            index = 0;
            foreach (Action action in trainerActionList)
            {

                AddActionButton(canvas, action, index, true, false);

                index++;
            }
        }
        else if (Context.Turns[Context.CurrentTurn].PlayerId == Global.Instance.PlayerId && !Context.Turns[Context.CurrentTurn].ComingBack)
        {
            index = 0;
            foreach (Action action in Context.Turns[Context.CurrentTurn].Moves)
            {

                AddActionButton(canvas, action, index, false, false);

                index++;
            }

            index = 0;
            foreach (Action action in trainerActionList)
            {

                AddActionButton(canvas, action, index, true, false);

                index++;
            }
        }
    }

    private int GetPlayerPokemonCount()
    {
        int result = 0;
        foreach (BattleEntityClient entity in Context.Turns)
        {
            if (entity.PlayerId == Global.Instance.PlayerId && !entity.ComingBack)
            {
                result++;
            }
        }
        return result;
    }

    private void AddActionButton(GameObject canvas, Action action, int index, bool isTrainer, bool isPokemonGo)
    {
        var buttonObject = new GameObject("button");
        buttonObject.transform.parent = canvas.transform;
        buttonObject.transform.localPosition = new Vector3(-450 + index * 120, isTrainer ? -300 : -350, 0);
        var buttonComp = buttonObject.AddComponent<Button>();
        int tmpIndex = index;
        buttonComp.onClick.AddListener(delegate
        {
            if (!(dialogWriting && dialogConfirm))
            {
                if (action.Range != null)
                {
                    currentActionInt = tmpIndex;
                    isCurrentActionTrainer = isTrainer;
                    isPokemonGoSelection = isPokemonGo;
                    if (isTrainer && action.Id == (int)TrainerAction.Pokemon_Go)
                    {
                        isPokemonGoAction = !isPokemonGoAction;
                        DisplayActionButton();
                    }
                    else
                    {
                        isPokemonGoAction = false;
                        if (Context.Turns.Count > 0)
                        {
                            HighlightAction(Context.Turns[Context.CurrentTurn]);
                        }
                        else
                        {
                            //HighlightAction(new BattleEntity(0, 0, Global.Instance.PlayerId,0));
                        }
                    }
                }
                else
                {
                    if (isTrainer)
                    {
                        Global.Instance.SendCommand(new BattleTrainerActionParam(new Position(0, 0), action, 0));
                    }
                }
            }
        });

        var imgComp = buttonObject.AddComponent<Image>();
        imgComp.sprite = Resources.Load<Sprite>("button");

        var textObject = new GameObject("text");
        textObject.transform.parent = buttonObject.transform;
        textObject.transform.localPosition = new Vector3(0, 0, 0);

        var textComp = textObject.AddComponent<Text>();
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        if (isPokemonGo)
        {
            textComp.text = Global.Instance.Team[index].Name;
        }
        else
        {
            textComp.text = action.Name;
        }
        textComp.color = Color.black;


        buttonObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 120);
        buttonObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
        textObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 120);
        textObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
    }

    private void DialogWhatDo(BattleEntityClient pokemon)
    {
        string dialog = string.Format("Que doit faire {0} ?", pokemon.Pokemon.name);
        AddDialog(dialog, false);
    }

    private void DialogWaiting(BattleEntityClient pokemon)
    {
        string dialog = string.Format("{0} va bientôt agir.", pokemon.Pokemon.name);
        AddDialog(dialog, false);
    }
}
