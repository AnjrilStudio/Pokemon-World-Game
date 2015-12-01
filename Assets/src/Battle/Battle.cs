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
        mouseTilePos = new Position(0, 0);

        arena = new Arena(0.32f, mapsize);
        turns = new List<BattleEntity>();

        if (ApplicationModel.playerBattleStartEntity != null)
        {
            Pokemon playerPkmn = ApplicationModel.playerBattleStartEntity.Pokemons[0];
            turns.Add(initPokemon(playerPkmn, false));
            foreach (MapEntity other in ApplicationModel.otherBattleStartEntities)
            {
                turns.Add(initPokemon(other.Pokemons[0], true));
            }
        } else
        {
            //tmp
            turns.Add(initPokemon(new Pokemon(1, 5), false));
            turns.Add(initPokemon(new Pokemon(2, 5), true));
        }

        hover = GameObject.Instantiate(Resources.Load("hover")) as GameObject;
        hover.SetActive(false);

        gameObject.transform.position = new Vector3(tilesize * (mapsize - 1) / 2, -tilesize * (mapsize - 1) / 2, gameObject.transform.position.z);

        highlightRange = new List<GameObject>();
        highlightAOE = new List<GameObject>();

        displayGUI();
    }

    // Update is called once per frame
    void Update()
    {
        //turn play
        BattleEntity turn = turns[currentTurn];
        if (turn.HP == 0)
        {
            Application.LoadLevel("scene_map");
        }



        bool inRange = false;

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
                Position origin = turn.CurrentPos;
                Position target = mouseTilePos;
                if (currentAction.TargetType == TargetType.Position)
                {
                    currentActionDir = Direction.None;
                    if (currentAction.Range.InRange(origin, target))
                    {
                        inRange = true;
                    }
                }

                if (currentAction.TargetType == TargetType.Directional)
                {
                    foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                    {
                        if (currentAction.Range.InRange(origin, target, dir))
                        {
                            currentActionDir = dir;
                            inRange = true;
                            break;
                        }
                    }
                }
                if (inRange)
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
            AITurnTime += Time.deltaTime;
            if (AITurnTime > 1f)
            {
                Action actionAI = turn.Actions[UnityEngine.Random.Range(0, turn.Actions.Count)];
                Position targetPos = null;
                var dir = Direction.None;

                if (actionAI.TargetType == TargetType.Position)
                {
                    List<Position> targets = actionAI.getInRangeTiles(turn);
                    targetPos = targets[UnityEngine.Random.Range(0, targets.Count)];
                }

                if (actionAI.TargetType == TargetType.Directional)
                {
                    dir = (Direction)UnityEngine.Random.Range(1, 5);
                    List<Position> targets = actionAI.getInRangeTiles(turn, dir);
                    targetPos = targets[UnityEngine.Random.Range(0, targets.Count)];
                }

                PlayTurn(turn, targetPos, actionAI, dir);
                AITurnTime = 0;
                currentTurn = (currentTurn + 1) % turns.Count;
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
            if (inRange && Input.GetMouseButtonDown(0) && hover.activeSelf && currentAction != null)
            {
                PlayTurn(turns[currentTurn], mouseTilePos, currentAction, currentActionDir);
                currentTurn = (currentTurn + 1) % turns.Count;
                displayGUI();
            }
        }
    }

    private void PlayTurn(BattleEntity turn, Position target, Action action, Direction dir)
    {
        if (action.Range.InRange(turn.CurrentPos, target)){
            foreach(GroundEffect effect in action.GroundEffects)
            {
                effect.apply(turn, target, dir);
            }
            
            foreach(Position aoe in action.getAoeTiles(turn, target, dir))
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

        foreach (Position target in action.getInRangeTiles(self))
        {
            var highlight = GameObject.Instantiate(Resources.Load("highlight")) as GameObject;
            highlight.transform.position = new Vector3(tilesize * target.X, -tilesize * target.Y, 0);
            highlightRange.Add(highlight);
        }
    }

    private void HighlightAOE(BattleEntity self, Action action, Position target, Direction dir)
    {
        foreach (Position aoe in action.getAoeTiles(self, target, dir))
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
