using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class Battle : MonoBehaviour
{

    private List<BattleEntity> turns;
    private float tilesize = 0.32f;
    private Arena arena;
    private int mapsize = 10;
    public int currentTurn = 0;

    public GameObject hover;
    private List<GameObject> highlightRange;
    private List<GameObject> highlightAOE;

    public float mousex;
    public float mousey;
    public float x0;
    public float y0;
    private Position mouseTilePos;

    private Action currentAction;
    private Direction currentActionDir;

    private float AITurnTime = 0;

    // Use this for initialization
    void Start()
    {
        //todo recevoir les infos du serveur

        mouseTilePos = new Position(0, 0);

        arena = new Arena(0.32f, mapsize);
        turns = new List<BattleEntity>();
        BattleEntity playerTurn = null;

        if (ApplicationModel.playerBattleStartEntity != null)
        {
            Pokemon playerPkmn = ApplicationModel.playerBattleStartEntity.Pokemons[0];
            playerTurn = initPokemon(playerPkmn, false);
            turns.Add(playerTurn);

            foreach (MapEntity other in ApplicationModel.otherBattleStartEntities)
            {
                turns.Add(initPokemon(other.Pokemons[0], true));
            }
        } else
        {
            //tmp
            playerTurn = initPokemon(new Pokemon(1, 5), false);
            turns.Add(playerTurn);
            turns.Add(initPokemon(new Pokemon(2, 5), true));
        }

        hover = GameObject.Instantiate(Resources.Load("hover")) as GameObject;
        hover.SetActive(false);

        gameObject.transform.position = new Vector3(tilesize * (mapsize - 1) / 2, -tilesize * (mapsize - 1) / 2, gameObject.transform.position.z);

        highlightRange = new List<GameObject>();
        highlightAOE = new List<GameObject>();

        displayGUI();
        
        currentAction = playerTurn.Actions[0];
        HighlightAction(playerTurn, currentAction, playerTurn.CurrentPos);
    }

    // Update is called once per frame
    void Update()
    {
        //todo recevoir les mises à jour du serveur
        //turn play
        BattleEntity turn = turns[currentTurn];
        if (turn.HP == 0)
        {
            Application.LoadLevel("scene_map");
        }



        bool inRange = false;
        bool inRange2 = false;

        //pointer control
        Camera camera = GetComponent<Camera>();
        Vector3 p = camera.WorldToScreenPoint(gameObject.transform.position);

        mousex = Input.mousePosition.x - p.x;
        mousey = Input.mousePosition.y - p.y;

        var factor = 250 * tilesize / camera.orthographicSize; // comprendre 250 ?
        x0 = mousex + factor * mapsize / 2;
        y0 = mousey + factor * mapsize / 2;

        mouseTilePos.X = Mathf.FloorToInt(x0 / (80 / camera.orthographicSize)); //comprendre 80 ?
        mouseTilePos.Y = mapsize - (Mathf.FloorToInt(y0 / (80 / camera.orthographicSize)) + 1);

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

            if (currentAction != null)
            {
                //aoe
                Position target = mouseTilePos;
                if (currentAction.TargetType == TargetType.Position)
                {
                    currentActionDir = Direction.None;
                    if (currentAction.Range.InRange(turn, target))
                    {
                        inRange = true;
                    }
                    if (currentAction.Range2 != null && currentAction.Range2.InRange(turn, target))
                    {
                        inRange2 = true;
                    }
                }

                if (currentAction.TargetType == TargetType.Directional)
                {
                    foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                    {
                        if (currentAction.Range.InRange(turn, target, dir))
                        {
                            currentActionDir = dir;
                            inRange = true;
                        }
                        if (currentAction.Range2 != null && currentAction.Range2.InRange(turn, target, dir))
                        {
                            currentActionDir = dir;
                            inRange2 = true;
                        }
                    }
                }
                if (inRange || inRange2)
                {
                    HighlightAOE(turn, currentAction, target, currentActionDir);
                }
            }
        }
        else
        {
            hover.SetActive(false);
        }

        if (turn.AI)
        {
            //côté serveur
            AITurnTime += Time.deltaTime;
            if (AITurnTime > 1f)
            {
                Action actionAI = turn.Actions[UnityEngine.Random.Range(0, turn.Actions.Count)];
                Position targetPos = null;
                var dir = Direction.None;
                while (targetPos == null){ // attention boucle infinie potentielle, mais ne devrait jamais arriver
                    if (actionAI.TargetType == TargetType.Position)
                    {
                        List<Position> targets = actionAI.InRangeTiles(turn);
                        if (targets.Count != 0)
                        {
                            targetPos = targets[UnityEngine.Random.Range(0, targets.Count)];
                        }
                    
                    }

                    if (actionAI.TargetType == TargetType.Directional)
                    {
                        dir = (Direction)UnityEngine.Random.Range(1, 5);
                        List<Position> targets = actionAI.InRangeTiles(turn, dir);
                        if (targets.Count != 0)
                        {
                            targetPos = targets[UnityEngine.Random.Range(0, targets.Count)];
                        }
                    }
                }
                PlayTurn(turn, targetPos, actionAI, dir);
                AITurnTime = 0;
                currentTurn = (currentTurn + 1) % turns.Count;
                turns[currentTurn].MP = turns[currentTurn].MaxMP;
                turns[currentTurn].MP = turns[currentTurn].MaxAP;
                displayGUI();
            }
        }
        else
        {
            //action control
            var highlight = currentAction != null;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentAction = turn.Actions[0];
                highlight = true;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentAction = turn.Actions[1];
                highlight = true;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentAction = turn.Actions[2];
                highlight = true;
            }
            if (highlight)
            {
                HighlightAction(turn, currentAction, turn.CurrentPos);
            }
            
            //click control
            if ((inRange || inRange2) && Input.GetMouseButtonDown(0) && hover.activeSelf && currentAction != null)
            {
                PlayTurn(turns[currentTurn], mouseTilePos, currentAction, currentActionDir);
                if (currentAction.NextTurn)
                {
                    currentTurn = (currentTurn + 1) % turns.Count; //todo methode nextturn
                    turns[currentTurn].MP = turns[currentTurn].MaxMP;
                    turns[currentTurn].MP = turns[currentTurn].MaxAP;
                    displayGUI();
                }
            }
        }
    }

    private void PlayTurn(BattleEntity turn, Position target, Action action, Direction dir)
    {
        var currentPos = new Vector3(tilesize * turn.CurrentPos.X, -tilesize * turn.CurrentPos.Y, 0);
        var targetPos = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);
        //côté serveur
        //todo envoyer les paramètres au serveur
        bool inRange = action.Range.InRange(turn, target);
        if (action.Range2 != null && action.Range2.InRange(turn, target))
        {
            inRange = true;
        }
        
        if (inRange)
        {
            foreach (FxDescriptor fx in action.Fx)
            {
                if (fx.Pattern != null && fx.PrefabName != null)
                {
                    GameObject fxObj = new GameObject();
                    var partgen = fxObj.AddComponent<ParticleGenerator>();
                    partgen.Pattern = fx.Pattern;
                    if (fx.Type == FxType.FromTarget)
                    {
                        fxObj.transform.position = targetPos;
                        fxObj.transform.rotation = Quaternion.AngleAxis(Utils.getDirRotation(dir), Vector3.back);
                    }
                    else if (fx.Type == FxType.ToTarget)
                    {
                        partgen.Target = targetPos - currentPos;
                        fxObj.transform.position = currentPos;
                    }

                }
            }
            

            if (action.ActionCost != null)
            {
                action.ActionCost.ApplyCost(turn, target);
            }

            foreach (GroundEffect effect in action.GroundEffects)
            {
                effect.apply(turn, target, dir);
            }
            
            foreach(Position aoe in action.AoeTiles(turn, target, dir))
            {
                foreach (BattleEntity pokemon in turns)
                {
                    //todo ne pas toucher soi-même
                    if (aoe.Equals(pokemon.CurrentPos))
                    {
                        foreach (HitEffect effect in action.HitEffects)
                        {
                            effect.apply(turn, pokemon, dir);
                        }
                    }
                }
            }
        }
        //clear highlight range
        foreach (GameObject obj in highlightRange)
        {
            Destroy(obj);
        }
        highlightRange.Clear();
    }

    private void HighlightAction(BattleEntity self, Action action, Position tilePos)
    {

        foreach (GameObject obj in highlightRange)
        {
            Destroy(obj);
        }
        highlightRange.Clear();

        foreach (Position target in action.InRangeTiles(self))
        {
            var highlight = GameObject.Instantiate(Resources.Load("highlight")) as GameObject;
            highlight.transform.position = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);
            highlightRange.Add(highlight);
        }

        foreach (Position target in action.InRange2Tiles(self))
        {
            var highlight = GameObject.Instantiate(Resources.Load("highlight2")) as GameObject;
            highlight.transform.position = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);
            highlightRange.Add(highlight);
        }
    }

    private void HighlightAOE(BattleEntity self, Action action, Position target, Direction dir)
    {
        foreach (Position aoe in action.AoeTiles(self, target, dir))
        {
            var aoeObj = GameObject.Instantiate(Resources.Load("aoe")) as GameObject;
            aoeObj.transform.position = new Vector3(tilesize * aoe.X, -tilesize * aoe.Y, 0);
            highlightAOE.Add(aoeObj);
        }
    }

    private void displayGUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        foreach(Text text in canvas.GetComponentsInChildren<Text>())
        {
            GameObject.Destroy(text);
        }

        int index = 0;
        foreach(BattleEntity turn in turns)
        {
            var textObject = new GameObject();
            textObject.transform.parent = canvas.transform;
            textObject.transform.localPosition = new Vector3(160, 80 + index * 30, 0);
            var textComp = textObject.AddComponent<Text>();
            textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            textComp.text = turn.Pokemon.name + " " + turn.HP + "/" + turn.MaxHP;

            index++;
        }
        

    }

    private BattleEntity initPokemon(Pokemon pokemon, bool isAI)
    {
        var entitiesNode = GameObject.FindGameObjectWithTag("Entities");
        GameObject pkmnObj = null;

        //todo methode
        switch (pokemon.Id)
        {
            case 1:
                pkmnObj = GameObject.Instantiate(Resources.Load("Roucool")) as GameObject;
                pkmnObj.name = "Roucool";
                pkmnObj.transform.parent = entitiesNode.transform;
                break;
            case 2:
                pkmnObj = GameObject.Instantiate(Resources.Load("Rattata")) as GameObject;
                pkmnObj.name = "Rattata";
                pkmnObj.transform.parent = entitiesNode.transform;
                break;
            default:
                break;

        }

        var battleEntity = new BattleEntity(arena, pkmnObj, isAI);

        //todo attaques
        battleEntity.Actions.Add(Moves.Get(Move.Move));
        battleEntity.Actions.Add(Moves.Get(Move.Tackle));
        battleEntity.Actions.Add(Moves.Get(Move.Gust));

        battleEntity.MoveBattleEntity(Position.Random(arena.Mapsize, arena.Mapsize));

        return battleEntity;
    }
}
