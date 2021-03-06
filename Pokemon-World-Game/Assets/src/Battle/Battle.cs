﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.State;
using UnityEngine.SceneManagement;
using Anjril.PokemonWorld.Common.Parameter;
using Anjril.PokemonWorld.Common.Message;

public class Battle : MonoBehaviour
{

    private List<BattleEntityClient> turns;
    private Dictionary<int, BattleEntityClient> entities;
    private float tilesize = 0.32f;
    private Arena arena;
    private int mapsize = 10;
    public int currentTurn = 0;
    private int currentActionNumber = -1;

    private List<Action> trainerActions;
    private bool isCurrentActionTrainer;
    private bool isPokemonGoAction;
    private bool isPokemonGoSelection;

    public GameObject hover;
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
                    return trainerActions[currentActionInt];
                }
                else
                {
                    return TrainerActions.Get(TrainerAction.Pokemon_Go);
                }

            }
            else
            {
                if (turns.Count > 0)
                {
                    return turns[currentTurn].Actions[currentActionInt];
                }
                else
                {
                    return null;
                }
            }
        }
    }
    private Direction currentActionDir;

    private float animTimer = 0;



    // Use this for initialization
    void Start()
    {
        Global.Instance.CurrentScene = SceneManager.GetActiveScene().name;

        mouseTilePos = new Position(0, 0);

        arena = new Arena(mapsize, 0.32f);
        turns = new List<BattleEntityClient>();
        entities = new Dictionary<int, BattleEntityClient>();
        trainerActions = new List<Action>();
        trainerActions.Add(TrainerActions.Get(TrainerAction.End_Battle));
        trainerActions.Add(TrainerActions.Get(TrainerAction.Pokemon_Go));
        trainerActions.Add(TrainerActions.Get(TrainerAction.Pokemon_Come_Back));
        trainerActions.Add(TrainerActions.Get(TrainerAction.Pokeball));

        hover = GameObject.Instantiate(Resources.Load("hover")) as GameObject;
        hover.SetActive(false);

        Camera camera = GetComponent<Camera>();
        camera.orthographicSize = 2;
        gameObject.transform.position = new Vector3(tilesize * (arena.ArenaSize - 1) / 2, -tilesize * (arena.ArenaSize - 1) / 2, gameObject.transform.position.z);
        gameObject.transform.position = new Vector3(gameObject.transform.position.x + 1, gameObject.transform.position.y - 0.35f, gameObject.transform.position.z);

        highlightRange = new List<GameObject>();
        highlightAOE = new List<GameObject>();

        displayGUI();

        isCurrentActionTrainer = false;
        isPokemonGoAction = false;
        isPokemonGoSelection = false;
        currentActionInt = -1;
        //HighlightAction(playerTurn);
    }

    // Update is called once per frame
    void Update()
    {
        animTimer += Time.deltaTime;
        if (animTimer >= 0)
        {
            //todo recevoir les mises à jour du serveur
            //turn play
            /*
            if (turn.HP == 0)
            {
                Application.LoadLevel("scene_map");
            }*/

            if (Global.Instance.BattleActionMessages.Count > 0)
            {
                BattleActionMessage battleaction = Global.Instance.BattleActionMessages.Peek();
                if (currentActionNumber == -1) //spectateur
                {
                    currentActionNumber = battleaction.ActionId - 1;
                }
                if (battleaction.ActionId == currentActionNumber + 1)
                {
                    battleaction = Global.Instance.BattleActionMessages.Dequeue();
                    if (battleaction.State == null)
                    {
                        SceneManager.LoadScene("scene_map");
                    }

                    if (battleaction.Action != null)
                    {
                        PlayTurn(turns[currentTurn], battleaction.Target, battleaction.Action, battleaction.Dir);
                    }


                    List<int> actualEntities = new List<int>();
                    BattleStateMessage battlestate = battleaction.State;
                    foreach (BattleStateEntity entity in battlestate.Entities)
                    {
                        actualEntities.Add(entity.Id);
                        if (entities.ContainsKey(entity.Id))
                        {
                            var battleEntity = entities[entity.Id];
                            battleEntity.UpdateBattleEntity(entity, arena.Tilesize);
                        }
                        else
                        {
                            var pkmn = initPokemon(entity.Id, entity.PokemonId, entity.CurrentPos, entity.PlayerId);
                            pkmn.HP = entity.HP;
                            pkmn.MaxHP = entity.MaxHP;
                            turns.Add(pkmn);
                        }
                    }

                    List<int> entitiesToRemove = new List<int>();
                    foreach (int id in entities.Keys)
                    {
                        if (!actualEntities.Contains(id))
                        {
                            entitiesToRemove.Add(id);
                        }
                    }

                    foreach (int id in entitiesToRemove)
                    {
                        RemovePokemon(id);
                    }

                    currentActionNumber++;
                    currentTurn = battlestate.CurrentTurn;

                    UpdateTrainerActions(battleaction.ActionsAvailable);
                    isCurrentActionTrainer = false;
                    isPokemonGoAction = false;
                    isPokemonGoSelection = false;
                    displayGUI();
                    ClearHighlight();
                    currentActionInt = -1;
                }

            }

            //par defaut
            BattleEntity turn = new BattleEntity(0, 0, Global.Instance.PlayerId);

            if (turns.Count > 0)
            {
                turn = turns[currentTurn];
            }

            bool inRange = false;
            bool inRange2 = false;

            //pointer control
            Camera camera = GetComponent<Camera>();
            Vector3 p = camera.WorldToScreenPoint(new Vector3(0, 0, -10));

            mousex = Input.mousePosition.x - p.x;
            mousey = Input.mousePosition.y - p.y;

            Vector3 p2 = camera.ViewportToWorldPoint(new Vector3(mousex, mousey, 10));
            var x0 = p2.x / tilesize / Screen.width;
            var y0 = -p2.y / tilesize / Screen.height;

            var mousetileposx = Mathf.RoundToInt(x0);
            var mousetileposy = Mathf.RoundToInt(y0);
            mouseTilePos = new Position(mousetileposx, mousetileposy);

            //hover
            if (mouseTilePos.X >= 0 && mouseTilePos.X < mapsize && mouseTilePos.Y >= 0 && mouseTilePos.Y < mapsize)
            {
                //pointer tile
                hover.transform.position = new Vector3(tilesize * mouseTilePos.X, -tilesize * mouseTilePos.Y, 0);
                hover.SetActive(true);

                //todo faire que si la case change
                foreach (GameObject obj in highlightAOE)
                {
                    Destroy(obj);
                }
                highlightAOE.Clear();

                if (CurrentAction != null)
                {

                    if (CurrentAction.TargetType == TargetType.None)
                    {
                        inRange = true;
                    }
                    else
                    {
                        //aoe
                        Position target = mouseTilePos;
                        if (CurrentAction.TargetType == TargetType.Position)
                        {
                            currentActionDir = Direction.None;
                            if (CurrentAction.Range.InRange(arena, turn, target))
                            {
                                inRange = true;
                            }
                            if (CurrentAction.Range2 != null && CurrentAction.Range2.InRange(arena, turn, target))
                            {
                                inRange2 = true;
                            }
                        }

                        if (CurrentAction.TargetType == TargetType.Directional)
                        {
                            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
                            {
                                if (CurrentAction.Range.InRange(arena, turn, target, dir))
                                {
                                    currentActionDir = dir;
                                    inRange = true;
                                }
                                if (CurrentAction.Range2 != null && CurrentAction.Range2.InRange(arena, turn, target, dir))
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

            }
        }
    }

    private void PlayTurn(BattleEntity entity, Position target, Action action, Direction dir)
    {
        var currentPos = new Vector3(tilesize * entity.CurrentPos.X, -tilesize * entity.CurrentPos.Y, 0);
        var targetPos = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);

        bool inRange = action.Range.InRange(arena, entity, target);
        if (action.Range2 != null && action.Range2.InRange(arena, entity, target))
        {
            inRange = true;
        }

        if (inRange)
        {
            foreach (FxDescriptor fx in MoveFx.Get((Move)action.Id))
            {
                if (fx.Pattern != null && fx.PrefabName != null)
                {
                    GameObject fxObj = new GameObject();
                    var partgen = fxObj.AddComponent<ParticleGenerator>();
                    partgen.Pattern = fx.Pattern;
                    partgen.PrefabName = fx.PrefabName;
                    if (fx.Type == FxType.FromTarget)
                    {
                        fxObj.transform.position = targetPos;
                        fxObj.transform.rotation = Quaternion.AngleAxis(ClientUtils.GetDirRotation(dir), Vector3.back);
                    }
                    else if (fx.Type == FxType.ToTarget)
                    {
                        partgen.Target = targetPos - currentPos;
                        fxObj.transform.position = currentPos;
                    }

                    animTimer = Mathf.Min(-(fx.Pattern.Duration + fx.Pattern.Delay), animTimer);
                }
            }
        }

        ClearHighlight();
    }

    private void ClearHighlight()
    {
        //clear highlight range
        foreach (GameObject obj in highlightRange)
        {
            Destroy(obj);
        }
        highlightRange.Clear();
        foreach (GameObject obj in highlightAOE)
        {
            Destroy(obj);
        }
        highlightAOE.Clear();
    }

    private void HighlightAction(BattleEntity self)
    {
        Action action = CurrentAction;
        foreach (GameObject obj in highlightRange)
        {
            Destroy(obj);
        }
        highlightRange.Clear();

        if (action.TargetType != TargetType.None)
        {
            foreach (Position target in action.InRangeTiles(self, arena))
            {
                var highlight = GameObject.Instantiate(Resources.Load("highlight")) as GameObject;
                highlight.transform.position = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);
                highlightRange.Add(highlight);
            }

            foreach (Position target in action.InRange2Tiles(self, arena))
            {
                var highlight = GameObject.Instantiate(Resources.Load("highlight2")) as GameObject;
                highlight.transform.position = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);
                highlightRange.Add(highlight);
            }
        }
    }

    private void HighlightAOE(BattleEntity self, Action action, Position target, Direction dir)
    {
        foreach (Position aoe in action.AoeTiles(self, target, dir, arena))
        {
            var aoeObj = GameObject.Instantiate(Resources.Load("aoe")) as GameObject;
            aoeObj.transform.position = new Vector3(tilesize * aoe.X, -tilesize * aoe.Y, 0);
            highlightAOE.Add(aoeObj);
        }
    }

    private void displayGUI()
    {
        GameObject canvas = GameObject.Find("Canvas");

        foreach (Transform child in canvas.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //pokemons
        int index = 0;
        foreach (BattleEntityClient turn in turns)
        {
            if (!turn.ComingBack)
            {
                var textObject = new GameObject("text");
                textObject.transform.parent = canvas.transform;
                textObject.transform.localPosition = new Vector3(250, 300 - index * 50, 0);
                var textComp = textObject.AddComponent<Text>();
                textComp.fontSize = 25;
                textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                var text = (index == currentTurn) ? " -> " : "    ";
                text += turn.Pokemon.name + " " + turn.HP + "/" + turn.MaxHP;
                textComp.text = text;
                textComp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
                index++;
            }

        }



        //boutons
        if (isPokemonGoAction)
        {
            index = 0;
            foreach (Action action in trainerActions)
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
        else if (GetPlayerPokemonCount() == 0)
        {
            index = 0;
            foreach (Action action in trainerActions)
            {

                AddActionButton(canvas, action, index, true, false);

                index++;
            }
        }
        else if (turns[currentTurn].PlayerId == Global.Instance.PlayerId)
        {
            index = 0;
            foreach (Action action in turns[currentTurn].Actions)
            {

                AddActionButton(canvas, action, index, false, false);

                index++;
            }

            index = 0;
            foreach (Action action in trainerActions)
            {

                AddActionButton(canvas, action, index, true, false);

                index++;
            }
        }
    }

    private int GetPlayerPokemonCount()
    {
        int result = 0;
        foreach (BattleEntityClient entity in turns)
        {
            if (entity.PlayerId == Global.Instance.PlayerId)
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
            if (action.Range != null)
            {
                currentActionInt = tmpIndex;
                isCurrentActionTrainer = isTrainer;
                isPokemonGoSelection = isPokemonGo;
                if (isTrainer && action.Id == (int)TrainerAction.Pokemon_Go)
                {
                    isPokemonGoAction = !isPokemonGoAction;
                    displayGUI();
                }
                else
                {
                    isPokemonGoAction = false;
                    if (turns.Count > 0)
                    {
                        HighlightAction(turns[currentTurn]);
                    }
                    else
                    {
                        HighlightAction(new BattleEntity(0, 0, Global.Instance.PlayerId));
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


    private BattleEntityClient initPokemon(int id, int pokemonId, Position pos, int playerId)
    {
        var entitiesNode = GameObject.FindGameObjectWithTag("Entities");

        var battleEntity = new BattleEntityClient(id, pokemonId, playerId);

        battleEntity.Pokemon.transform.parent = entitiesNode.transform;

        //todo attaques

        entities.Add(id, battleEntity);

        battleEntity.MoveBattleEntity(pos, arena.Tilesize);

        return battleEntity;
    }

    private void RemovePokemon(int id)
    {
        Destroy(entities[id].Pokemon);
        turns.Remove(entities[id]);
        entities.Remove(id);
    }

    private void UpdateTrainerActions(List<TrainerAction> actions)
    {
        trainerActions.Clear();
        foreach (TrainerAction action in actions)
        {
            trainerActions.Add(TrainerActions.Get(action));
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("quit");
        Global.Instance.Client.Disconnect(Global.Instance.PlayerId.ToString());
    }
}