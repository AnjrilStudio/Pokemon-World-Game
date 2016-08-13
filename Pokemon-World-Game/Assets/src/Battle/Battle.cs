using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.State;
using UnityEngine.SceneManagement;

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

    public GameObject hover;
    private List<GameObject> highlightRange;
    private List<GameObject> highlightAOE;

    public float mousex;
    public float mousey;
    public float x0;
    public float y0;
    private Position mouseTilePos;

    private int currentActionInt;
    private Action CurrentAction {
        get
        {
            if (currentActionInt < 0) return null;
            if (isCurrentActionTrainer)
            {
                return trainerActions[currentActionInt];
            } else
            {
                return turns[currentTurn].Actions[currentActionInt];
            }
            
        }
    }
    private Direction currentActionDir;
    
    private float animTimer = 0;



    // Use this for initialization
    void Start()
    {
        //todo recevoir les infos du serveur
        
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

        gameObject.transform.position = new Vector3(tilesize * (mapsize - 1) / 2, -tilesize * (mapsize - 1) / 2, gameObject.transform.position.z);

        highlightRange = new List<GameObject>();
        highlightAOE = new List<GameObject>();

        displayGUI();

        isCurrentActionTrainer = false;
        currentActionInt = -1;
        //HighlightAction(playerTurn);
    }

    // Update is called once per frame
    void Update()
    {
        animTimer += Time.deltaTime;
        if (animTimer >= 0) {
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
                if (battleaction.ActionId == currentActionNumber + 1)
                {
                    battleaction = Global.Instance.BattleActionMessages.Dequeue();
                    if (battleaction.State == null)
                    {
                        Debug.Log("endbattle");
                        SceneManager.LoadScene("scene_map");
                    }

                    if (battleaction.Action != null)
                    {
                        Debug.Log(animTimer);
                        PlayTurn(turns[currentTurn], battleaction.Target, battleaction.Action, battleaction.Dir);
                    }

                    BattleStateMessage battlestate = battleaction.State;
                    foreach (BattleStateEntity entity in battlestate.Entities)
                    {
                        if (entities.ContainsKey(entity.Id))
                        {
                            var battleEntity = entities[entity.Id];
                            battleEntity.UpdateBattleEntity(entity, arena.Tilesize);
                        }
                        else
                        {
                            var pkmn = initPokemon(entity.Id, entity.PokemonId, entity.CurrentPos);
                            pkmn.HP = entity.HP;
                            pkmn.MaxHP = entity.MaxHP;
                            turns.Add(pkmn);
                        }
                    }

                    currentActionNumber++;
                    currentTurn = battlestate.CurrentTurn;
                    displayGUI();
                }
                
            }
            

            if (turns.Count > 0)
            {

                BattleEntity turn = turns[currentTurn];

                bool inRange = false;
                bool inRange2 = false;

                //pointer control
                Camera camera = GetComponent<Camera>();
                Vector3 p = camera.WorldToScreenPoint(gameObject.transform.position);

                mousex = Input.mousePosition.x - p.x;
                mousey = Input.mousePosition.y - p.y;

                Vector3 p2 = camera.ViewportToWorldPoint(new Vector3(mousex, mousey, 10));
                var x0 = p2.x / tilesize / Screen.width;
                var y0 = -p2.y / tilesize / Screen.height;

                var mousetileposx = Mathf.FloorToInt(x0) + arena.ArenaSize / 2;
                var mousetileposy = Mathf.FloorToInt(y0) + arena.ArenaSize / 2;
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
                    if (isCurrentActionTrainer)
                    {
                        Global.Instance.Client.Send("tra/" + Global.Instance.PlayerId + "," + currentTurn + "," + mouseTilePos.ToString() + "," + CurrentAction.Id);
                    } else
                    {
                        Global.Instance.Client.Send("act/" + Global.Instance.PlayerId + "," + currentTurn + "," + mouseTilePos.ToString() + "," + CurrentAction.Id + "," + currentActionDir.ToString());
                    }
                    
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
        if (turns.Count > 0)
        {
            GameObject canvas = GameObject.Find("Canvas");

            foreach (Transform child in canvas.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            //pokemons
            int index = 0;
            foreach(BattleEntityClient turn in turns)
            {
                var textObject = new GameObject("text");
                textObject.transform.parent = canvas.transform;
                textObject.transform.localPosition = new Vector3(160, 80 + index * 30, 0);
                var textComp = textObject.AddComponent<Text>();
                textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                textComp.text = turn.Pokemon.name + " " + turn.HP + "/" + turn.MaxHP;

                index++;
            }

            //attaques
            index = 0;
            foreach (Action action in turns[currentTurn].Actions)
            {

                AddActionButton(canvas, action, index, false);

                index++;
            }

            index = 0;
            foreach (Action action in trainerActions)
            {

                AddActionButton(canvas, action, index, true);

                index++;
            }
        }
    }

    private void AddActionButton(GameObject canvas, Action action, int index, bool isTrainer)
    {
        var buttonObject = new GameObject("button");
        buttonObject.transform.parent = canvas.transform;
        buttonObject.transform.localPosition = new Vector3(-100 + index * 50, isTrainer?-150:-200, 0);
        var buttonComp = buttonObject.AddComponent<Button>();
        int tmpIndex = index;
        buttonComp.onClick.AddListener(delegate {
            currentActionInt = tmpIndex;
            isCurrentActionTrainer = isTrainer;
            HighlightAction(turns[currentTurn]);
        });

        var imgComp = buttonObject.AddComponent<Image>();
        imgComp.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var textObject = new GameObject("text");
        textObject.transform.parent = buttonObject.transform;
        textObject.transform.localPosition = new Vector3(0, 0, 0);

        var textComp = textObject.AddComponent<Text>();
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textComp.text = action.Name;
        textComp.color = Color.black;


        buttonObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 50);
        buttonObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
        textObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 50);
        textObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
    }


    private BattleEntityClient initPokemon(int id, int pokemonId, Position pos)
    {
        var entitiesNode = GameObject.FindGameObjectWithTag("Entities");

        var battleEntity = new BattleEntityClient(id, pokemonId, Global.Instance.PlayerId);

        battleEntity.Pokemon.transform.parent = entitiesNode.transform;

        //todo attaques

        entities.Add(id, battleEntity);

        battleEntity.MoveBattleEntity(pos, arena.Tilesize);

        return battleEntity;
    }

    private void OnApplicationQuit()
    {
        Debug.Log("quit");
        Global.Instance.Client.Disconnect(Global.Instance.PlayerId.ToString());
    }
}
