using UnityEngine;
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
    private Dictionary<int, GroundEffectClient> groundEffects;
    private List<int> pokemonToGoList;
    private float tilesize = 0.32f;
    private BattleArenaClient arena;
    public int currentTurn = 0;
    private int currentActionNumber = -1;

    private List<Action> trainerActions;
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
                    return turns[currentTurn].Moves[currentActionInt];
                }
                else
                {
                    return null;
                }
            }
        }
    }
    private Direction currentActionDir;
    
    private List<string> battleLog;

    private Queue<DialogMessage> dialogBoxQueue;
    private int textIndex = 0;
    private float textTimer = 0;
    bool dialogWriting = false;
    bool dialogConfirm = false;
    bool nextDialog = false;
    bool moveDialog = false;

    private float animTimer = 0;
    List<GameObject> currentFx;



    // Use this for initialization
    void Start()
    {
        Global.Instance.CurrentScene = SceneManager.GetActiveScene().name;

        mouseTilePos = new Position(0, 0);

        arena = new BattleArenaClient(20, 0.32f);
        turns = new List<BattleEntityClient>();
        pokemonToGoList = new List<int>();
        entities = new Dictionary<int, BattleEntityClient>();
        groundEffects = new Dictionary<int, GroundEffectClient>();
        currentFx = new List<GameObject>();


        trainerActions = new List<Action>();
        trainerActions.Add(TrainerActions.Get(TrainerAction.End_Battle));
        trainerActions.Add(TrainerActions.Get(TrainerAction.Pokemon_Go));
        trainerActions.Add(TrainerActions.Get(TrainerAction.Pokemon_Come_Back));
        trainerActions.Add(TrainerActions.Get(TrainerAction.Pokeball));

        scaleNode = GameObject.FindGameObjectWithTag("BattleScale");
        hover = GameObject.Instantiate(Resources.Load("hover")) as GameObject;
        hover.SetActive(false);
        hover.transform.parent = scaleNode.transform;

        highlightRange = new List<GameObject>();
        highlightAOE = new List<GameObject>();

        battleLog = new List<string>();
        battleLog.Add("Debut du combat");

        dialogBoxQueue = new Queue<DialogMessage>();

        displayGUI();
        //initCamera();

        isCurrentActionTrainer = false;
        isPokemonGoAction = false;
        isPokemonGoSelection = false;
        currentActionInt = -1;
        //HighlightAction(playerTurn);
    }

    private void initCamera()
    {
        var scaleValue = 10f / Mathf.Max(arena.Width, arena.Height);
        scaleNode.transform.localScale = new Vector3(scaleValue, scaleValue, 1);

        Camera camera = GetComponent<Camera>();
        camera.orthographicSize = 2;
        var cameraOffset = 10; //?
        gameObject.transform.position = new Vector3(tilesize * (cameraOffset - 1) / 2, -tilesize * (cameraOffset - 1) / 2, gameObject.transform.position.z);
        gameObject.transform.position = new Vector3(gameObject.transform.position.x + 1, gameObject.transform.position.y - 0.35f, gameObject.transform.position.z);

        
    }

    // Update is called once per frame
    void Update()
    {
        updateEffects();
        updateGroundEffects();
        updateDamageEffects();
        updateTextBox();

        if (Global.Instance.BattleStartMessages.Count > 0)
        {
            BattleStartMessage message = Global.Instance.BattleStartMessages.Dequeue();
            //todo message pour dire qu'un joueur est devenu spectateur
        }

        animTimer += Time.deltaTime;
        if (animTimer >= 0 && currentFx.Count == 0) 
        {
            if (pokemonToGoList.Count > 0)
            {
                turns[pokemonToGoList[0]].Pokemon.SetActive(true); //TODO animation
                pokemonToGoList.RemoveAt(0);
                animTimer = -1.0f;
            }
            else if (Global.Instance.BattleActionMessages.Count > 0)
            {
                bool spectator = false;
                BattleActionMessage battleaction = Global.Instance.BattleActionMessages.Peek();
                if (currentActionNumber == -1) //spectateur
                {
                    spectator = true;
                    currentActionNumber = battleaction.ActionId - 1;
                }
                if (battleaction.ActionId == currentActionNumber + 1)
                {
                    //Debug.Log("action " + battleaction.ActionId + ", null " + (battleaction.Action == null) + ", movedialog " + moveDialog);
                    bool continueAction = true;
                    if (battleaction.Action != null && !moveDialog && (Move)battleaction.Action.Id != Move.Move)
                    {
                        Debug.Log("logMove" + battleaction.ActionId);
                        logMove(turns[currentTurn], (Move)battleaction.Action.Id);
                        moveDialog = true;
                        continueAction = false;
                        
                        //faire defiler le texte si l'action vient de soi-même
                        if (battleaction.Action != null && turns[currentTurn].PlayerId == Global.Instance.PlayerId)
                        {
                            nextDialog = true;
                        }
                    }

                    //Debug.Log("dialogWriting " + dialogWriting + ", dialogConfirm " + dialogConfirm + " : " + battleaction.ActionId);
                    if (!(dialogWriting && dialogConfirm) && continueAction)
                    {
                        Debug.Log("do action " + battleaction.ActionId);
                        battleaction = Global.Instance.BattleActionMessages.Dequeue();
                        //end battle
                        if (battleaction.State == null)
                        {
                            SceneManager.LoadScene("scene_map");
                            return;
                        }
                    
                        //maj arena
                        if (battleaction.Arena != null)
                        {
                            arena.update(battleaction.Arena);

                            initCamera();
                        }

                        //fx
                        if (battleaction.Action != null)
                        {
                            PlayTurn(turns[currentTurn], battleaction.Target, battleaction.Action, battleaction.Dir);
                        }

                        //maj entities
                        List<int> actualEntities = new List<int>();
                        List<int> entitiesToAdd = new List<int>();
                        BattleStateMessage battlestate = battleaction.State;
                        foreach (BattleStateEntity entity in battlestate.Entities)
                        {
                            actualEntities.Add(entity.Id);
                            if (entities.ContainsKey(entity.Id))
                            {
                                var battleEntity = entities[entity.Id];
                                UpdatePokemon(battleEntity, entity);
                            }
                            else
                            {
                                var pkmn = initPokemon(entity.Id, entity.PokemonId, entity.Level, entity.CurrentPos, entity.PlayerId);
                                pkmn.HP = entity.HP;
                                pkmn.MaxHP = entity.MaxHP;
                                turns.Add(pkmn);

                                logPokemonGo(pkmn);
                                //faire defiler le texte si l'action vient de soi-même
                                if (entity.PlayerId == Global.Instance.PlayerId)
                                {
                                    //nextDialog = true;
                                }

                                if (!spectator)
                                {
                                    pkmn.Pokemon.SetActive(false);
                                    entitiesToAdd.Add(entity.Id);
                                }
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

                        turns.Clear();
                        foreach (int id in actualEntities)
                        {
                            if (entitiesToAdd.Contains(id))
                            {
                                pokemonToGoList.Add(turns.Count);
                            }
                            turns.Add(entities[id]);
                        }

                        currentActionNumber++;
                        currentTurn = battlestate.CurrentTurn;

                        //maj ground fx
                        List<int> toRemove = new List<int>();
                        foreach (int groundEffectId in groundEffects.Keys)
                        {
                            if (!battlestate.GroundEffects.Exists(e => e.InstanceId == groundEffectId))
                            {
                                toRemove.Add(groundEffectId);
                            }
                        }
                        toRemove.ForEach(i => groundEffects.Remove(i));
                    
                        foreach (BattleStateGroundEffect groundEffect in battlestate.GroundEffects)
                        {
                            if (!groundEffects.ContainsKey(groundEffect.InstanceId))
                            {
                                groundEffects.Add(groundEffect.InstanceId, new GroundEffectClient(groundEffect));
                            }
                        }

                        UpdateTrainerActions(battleaction.ActionsAvailable);
                        isCurrentActionTrainer = false;
                        isPokemonGoAction = false;
                        isPokemonGoSelection = false;
                        displayGUI();
                        ClearHighlight();
                        currentActionInt = -1;

                        //dialogue
                        if (turns[currentTurn].PlayerId == Global.Instance.PlayerId && !turns[currentTurn].ComingBack && turns[currentTurn].Ready)
                        {
                            dialogWhatDo(turns[currentTurn]);
                            //todo faire en sorte que le dialogue s'affiche pour le premier pokemongo
                        }


                    }

                }

            }

            //par defaut
            BattleEntity turn = null;

            if (turns.Count > 0)
            {
                turn = turns[currentTurn];
            }

            bool inRange = false;
            bool inRange2 = false;

            //pointer control
            Camera camera = GetComponent<Camera>();
            Vector3 p = camera.WorldToScreenPoint(new Vector3(0, 0, -10));
            var scaleX = scaleNode.transform.localScale.x;
            var scaleY = scaleNode.transform.localScale.y;

            mousex = Input.mousePosition.x - p.x;
            mousey = Input.mousePosition.y - p.y;

            Vector3 p2 = camera.ViewportToWorldPoint(new Vector3(mousex, mousey, 10));
            var x0 = p2.x / (tilesize * scaleX) / Screen.width;
            var y0 = -p2.y / (tilesize * scaleY) / Screen.height;

            var mousetileposx = Mathf.RoundToInt(x0);
            var mousetileposy = Mathf.RoundToInt(y0);
            mouseTilePos = new Position(mousetileposx, mousetileposy);

            //hover
            if (mouseTilePos.X >= 0 && mouseTilePos.X < arena.Width && mouseTilePos.Y >= 0 && mouseTilePos.Y < arena.Height)
            {
                //pointer tile
                hover.transform.localPosition = new Vector3(tilesize * mouseTilePos.X, -tilesize * mouseTilePos.Y, 0);
                hover.SetActive(true);

                //todo faire que si la case change
                foreach (GameObject obj in highlightAOE)
                {
                    Destroy(obj);
                }
                highlightAOE.Clear();

                if (CurrentAction != null)
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
    }

    private void PlayTurn(BattleEntityClient entity, Position target, Action action, Direction dir)
    {
        var currentPos = new Vector3(tilesize * entity.CurrentPos.X, -tilesize * entity.CurrentPos.Y, -2);
        var targetPos = new Vector3(tilesize * target.X, -tilesize * target.Y, -2);
        
        foreach (FxDescriptor fx in MoveFx.Get((Move)action.Id))
        {
            if (fx.Pattern != null && fx.PrefabName != null)
            {
                GameObject fxObj = new GameObject();
                fxObj.transform.parent = scaleNode.transform;
                fxObj.transform.localScale = new Vector3(1, 1, 1);
                var partgen = fxObj.AddComponent<ParticleGenerator>();
                partgen.Pattern = fx.Pattern;
                partgen.PrefabName = fx.PrefabName;
                partgen.Active = !(dialogWriting && dialogConfirm);
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
        if ((Move)action.Id == Move.Move){
            animTimer = -0.5f;
        } else
        {
            Debug.Log("movedialog false");
            moveDialog = false;
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
        
        foreach (Position target in action.InRangeTiles(self, arena))
        {
            var highlight = GameObject.Instantiate(Resources.Load("highlight")) as GameObject;
            highlight.transform.parent = scaleNode.transform;
            highlight.transform.localPosition = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);
            highlight.transform.localScale = new Vector3(1, 1, 1);
            highlightRange.Add(highlight);
        }

        foreach (Position target in action.InRange2Tiles(self, arena))
        {
            var highlight = GameObject.Instantiate(Resources.Load("highlight2")) as GameObject;
            highlight.transform.parent = scaleNode.transform;
            highlight.transform.localPosition = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);
            highlight.transform.localScale = new Vector3(1, 1, 1);
            highlightRange.Add(highlight);
        }
    }

    private void HighlightAOE(BattleEntity self, Action action, Position target, Direction dir)
    {
        foreach (Position aoe in action.AoeTiles(self, target, dir, arena))
        {
            var aoeObj = GameObject.Instantiate(Resources.Load("aoe")) as GameObject;
            aoeObj.transform.parent = scaleNode.transform;
            aoeObj.transform.localPosition = new Vector3(tilesize * aoe.X, -tilesize * aoe.Y, 0);
            aoeObj.transform.localScale = new Vector3(1, 1, 1);
            highlightAOE.Add(aoeObj);
        }
    }

    private void displayGUI()
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
        foreach (BattleEntityClient turn in turns)
        {
            if (!turn.ComingBack)
            {
                var textObject = new GameObject("text");
                textObject.transform.parent = canvas.transform;
                textObject.transform.localPosition = new Vector3(320, 300 - index * 50, 0);
                var textComp = textObject.AddComponent<Text>();
                textComp.fontSize = 20;
                textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                var text = (index == currentTurn) ? " -> " : "    ";
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
        foreach (string log in battleLog)
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
        else if (GetPlayerPokemonCount() == 0 || turns[currentTurn].ComingBack)
        {
            index = 0;
            foreach (Action action in trainerActions)
            {

                AddActionButton(canvas, action, index, true, false);

                index++;
            }
        }
        else if (turns[currentTurn].PlayerId == Global.Instance.PlayerId && !turns[currentTurn].ComingBack)
        {
            index = 0;
            foreach (Action action in turns[currentTurn].Moves)
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


    private BattleEntityClient initPokemon(int id, int pokemonId, int level, Position pos, int playerId)
    {
        var entitiesNode = GameObject.FindGameObjectWithTag("Entities");

        var battleEntity = new BattleEntityClient(id, pokemonId, playerId, level);

        battleEntity.Pokemon.transform.parent = entitiesNode.transform;
        battleEntity.Pokemon.transform.localScale = new Vector3(1.25f, 1.25f, 1);

        entities.Add(id, battleEntity);

        battleEntity.MoveBattleEntity(pos, arena);

        return battleEntity;
    }

    private void UpdatePokemon(BattleEntityClient pokemon, BattleStateEntity state)
    {
        var currentHP = pokemon.HP;
        if (!pokemon.ComingBack && state.ComingBack)
        {
            logPokemonComeBack(pokemon);
            //faire defiler le texte si l'action vient de soi-même
            if (pokemon.PlayerId == Global.Instance.PlayerId)
            {
                //nextDialog = true;
            }
        }

        pokemon.UpdateBattleEntity(state, arena);
        int damage = currentHP - pokemon.HP;
        if (damage > 0)
        {
            //animation de prise de dégats
            animTimer -= 0.2f;
            pokemon.DamageAnimationTimer = 0.2f;
            logDamage(pokemon, damage);

            if (pokemon.HP == 0)
            {
                logPokemonKO(pokemon);
            }
        }
    }

    private void RemovePokemon(int id)
    {
        arena.RemoveBattleEntity(entities[id]);
        Destroy(entities[id].Pokemon);
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

    private void updateDamageEffects()
    {
        foreach(BattleEntityClient pokemon in entities.Values)
        {
            if (pokemon.DamageAnimationTimer > 0)
            {
                pokemon.Pokemon.SetActive(!pokemon.Pokemon.activeSelf);
                pokemon.DamageAnimationTimer -= Time.deltaTime;

                // au cas où pile à 0, il ne faut pas rester bloquer à false
                if (pokemon.DamageAnimationTimer == 0)
                {
                    pokemon.Pokemon.SetActive(true);
                }
            } else if (pokemon.DamageAnimationTimer < 0)
            {
                pokemon.DamageAnimationTimer = 0;
                pokemon.Pokemon.SetActive(true);
            }
        }
    }

    private void updateEffects()
    {
        currentFx.RemoveAll(fx => fx == null);
        currentFx.ForEach(fx => fx.SetActive(!(dialogWriting && dialogConfirm)));
    }

    private void updateGroundEffects()
    {
        foreach (GroundEffectClient groundEffect in groundEffects.Values)
        {
            groundEffect.Timer += Time.deltaTime;

            if (groundEffect.Timer >= 0)
            {
                foreach (FxDescriptor fx in GroundFx.Get((GroundEffectOverTimeId)groundEffect.EffectId))
                {
                    if (fx.Pattern != null && fx.PrefabName != null)
                    {
                        GameObject fxObj = new GameObject();
                        fxObj.transform.parent = scaleNode.transform;
                        fxObj.transform.localScale = new Vector3(1, 1, 1);
                        var partgen = fxObj.AddComponent<ParticleGenerator>();
                        partgen.Pattern = fx.Pattern;
                        partgen.PrefabName = fx.PrefabName;
                        fxObj.transform.localPosition = new Vector3(tilesize * groundEffect.Position.X, -tilesize * groundEffect.Position.Y, -2);

                        groundEffect.Timer -= fx.Pattern.Duration;
                    }
                }
            }
        }
    }

    private void updateTextBox()
    {
        var textSpeed = 0.015f;
        GameObject canvas = GameObject.Find("Canvas");

        var skip = false;
        if ((Input.anyKeyDown && dialogConfirm))
        {
            skip = true;
        }

        if (dialogBoxQueue.Count > 0)
        {
            DialogMessage currentDialog = dialogBoxQueue.Peek();
            dialogConfirm = currentDialog.Confirm;

            if (textIndex <= currentDialog.Text.Length)
            {
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

            } else
            {
                if (skip || !dialogConfirm)
                {
                    //pas de nouveau texte à lire
                    dialogWriting = false;
                } 

                if ((dialogBoxQueue.Count > 1 && !dialogWriting) || nextDialog)
                {
                    //prochain texte
                    textIndex = 0;
                    dialogBoxQueue.Dequeue();
                    var textBoxObject = canvas.transform.Find("TextBox");
                    Destroy(textBoxObject.gameObject);
                    dialogWriting = true;

                    if (nextDialog)
                    {
                        nextDialog = false;
                        updateTextBox(); //initialise le prochain dialogue
                    }
                }
            }

        }
    }

    private void logDamage(BattleEntityClient pokemon, int damage)
    {
        string log = string.Format("{0} subit {1} dégats.", pokemon.Pokemon.name, damage);
        addLog(log);
    }

    private void logMove(BattleEntityClient pokemon, Move move)
    {
        string log = string.Format("{0} lance {1}", pokemon.Pokemon.name, move.ToString());
        addLog(log);
        addDialog(log);
    }

    private void logPokemonGo(BattleEntityClient pokemon)
    {
        string log = string.Format("{0} rejoint le combat", pokemon.Pokemon.name);
        addLog(log);
        addDialog(log);
    }

    private void logPokemonComeBack(BattleEntityClient pokemon)
    {
        string log = string.Format("{0} est rappelé", pokemon.Pokemon.name);
        addLog(log);
        addDialog(log);
    }

    private void logPokemonKO(BattleEntityClient pokemon)
    {
        string log = string.Format("{0} est KO.", pokemon.Pokemon.name);
        addLog(log);
        addDialog(log);
    }

    private void addLog(string log)
    {
        battleLog.Insert(0, log);
        if (battleLog.Count > 10)
        {
            battleLog.RemoveAt(10);
        }
    }

    private void dialogWhatDo(BattleEntityClient pokemon)
    {
        string dialog = string.Format("Que doit faire {0} ?", pokemon.Pokemon.name);
        addDialog(dialog, false);
    }

    private void addDialog(string text)
    {
        dialogBoxQueue.Enqueue(new DialogMessage(text, true));
    }

    private void addDialog(string text, bool confirm)
    {
        dialogBoxQueue.Enqueue(new DialogMessage(text, confirm));
    }

    private void OnApplicationQuit()
    {
        Debug.Log("quit");
        Global.Instance.Client.Disconnect(Global.Instance.PlayerId.ToString());
    }
}