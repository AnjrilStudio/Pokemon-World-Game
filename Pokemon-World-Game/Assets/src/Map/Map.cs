using UnityEngine;
using System;
using Anjril.Common.Network;
using Anjril.Common.Network.TcpImpl;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Anjril.PokemonWorld.Common.State;
using Anjril.PokemonWorld.Common.Parameter;
using Anjril.PokemonWorld.Common.Utils;
using Anjril.PokemonWorld.Common.Message;

public class Map : MonoBehaviour
{

    private MapEntity player;
    private float tilesize = 0.32f;
    private float tileZLayerFactor = 0.10f;

    private int chunksize = 20;

    public int renderDistance = 15;
    public int zoom = 1;

    private ChunkMatrix<GameObject> mapMatrix;
    private ChunkMatrix<GameObject> mapObjMatrix;
    private ChunkMatrix<List<GameObject>> mapOverlayMatrix;
    private ChunkMatrix<Int32> groundMatrix;
    private ChunkMatrix<MapEntity> entityMatrix;
    private ChunkMatrix<List<PopulationEntity>> populationMatrix;

    private Dictionary<Int32, MapEntity> mapEntities;

    private float moveInputDelay = 0.2f;

    private bool upWasUp = false;
    private bool downWasUp = false;
    private bool rightWasUp = false;
    private bool leftWasUp = false;
    private Direction lastInput = Direction.None;


    private GameObject entitiesNode;

    // Use this for initialization
    void Start()
    {
        Global.Instance.InitClient();

        Global.Instance.CurrentScene = SceneManager.GetActiveScene().name;
        Global.Instance.MoveMessages.Clear();

        entitiesNode = GameObject.FindGameObjectWithTag("Entities");

        entityMatrix = new ChunkMatrix<MapEntity>(chunksize);
        mapMatrix = new ChunkMatrix<GameObject>(chunksize);
        mapObjMatrix = new ChunkMatrix<GameObject>(chunksize);
        groundMatrix = new ChunkMatrix<Int32>(chunksize);
        mapOverlayMatrix = new ChunkMatrix<List<GameObject>>(chunksize);
        populationMatrix = new ChunkMatrix<List<PopulationEntity>>(chunksize);
        mapEntities = new Dictionary<Int32, MapEntity>();

    }

    // Update is called once per frame
    void Update()
    {

        if (Global.Instance.BattleStartMessages.Count > 0)
        {
            BattleStartMessage message = Global.Instance.BattleStartMessages.Dequeue();
            //todo animation exclamation sur les entités
            //todo animation cool de lancement de fight
            SceneManager.LoadScene("scene_battle");
        }

        updateEntities();

        if (Global.Instance.MapMessages.Count > 0)
        {
            MapMessage message = Global.Instance.MapMessages.Dequeue();
            loadMap(message.Origin, message.Segments);
        }

        if (player != null)
        {
            if (player.MoveTimer > player.MoveTime - moveInputDelay)
            {
                Direction moveDir = Direction.None;

                bool up = Input.GetKey(KeyCode.UpArrow);
                bool down = Input.GetKey(KeyCode.DownArrow);
                bool right = Input.GetKey(KeyCode.RightArrow);
                bool left = Input.GetKey(KeyCode.LeftArrow);

                if (up && upWasUp)
                {
                    moveDir = Direction.Up;
                }
                else if (down && downWasUp)
                {
                    moveDir = Direction.Down;
                }
                else if (left && this.leftWasUp)
                {
                    moveDir = Direction.Left;
                }
                else if (right && this.rightWasUp)
                {
                    moveDir = Direction.Right;
                }
                else if (up && lastInput == Direction.Up)
                {
                    moveDir = Direction.Up;
                }
                else if (down && lastInput == Direction.Down)
                {
                    moveDir = Direction.Down;
                }
                else if (left && lastInput == Direction.Left)
                {
                    moveDir = Direction.Left;
                }
                else if (right && lastInput == Direction.Right)
                {
                    moveDir = Direction.Right;
                }
                else if (up)
                {
                    moveDir = Direction.Up;
                }
                else if (down)
                {
                    moveDir = Direction.Down;
                }
                else if (left)
                {
                    moveDir = Direction.Left;
                }
                else if (right)
                {
                    moveDir = Direction.Right;
                }
                else
                {
                    moveDir = Direction.None;
                }


                if (moveDir != Direction.None)
                {
                    if (lastInput != moveDir) //turn
                    {
                        Global.Instance.SendCommand(new TurnParam(moveDir));
                        //Debug.Log("turn sent : " + moveDir.ToString());
                    }
                    else //move
                    {
                        Global.Instance.SendCommand(new MoveParam(moveDir));
                        //Debug.Log("move sent : " + moveDir.ToString());

                        //moveInput = false; //empêche l'envoie de plus d'une direction
                    }

                }


                lastInput = moveDir;

                upWasUp = !up;
                downWasUp = !down;
                leftWasUp = !left;
                rightWasUp = !right;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    var dirPos = PositionUtils.GetDirPosition(player.CurrentDir, true);
                    var otherPos = new Position(player.CurrentPos.X + dirPos.X, player.CurrentPos.Y + dirPos.Y);
                    Debug.Log(player.CurrentPos);
                    Debug.Log(otherPos);
                    if (entityMatrix[otherPos.X, otherPos.Y] != null)
                    {
                        Debug.Log("battle");
                        Global.Instance.Client.Send("btl/");
                        Global.Instance.SendCommand(new BattleStartParam());
                    }

                }
            }

            //deplacement camera
            if (player.Object != null)
            {
                Vector3 playerPosition = player.Object.transform.position;
                gameObject.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z - 10);
            }
        }
    }

    private void moveEntity(MapEntity entity, int x, int y)
    {
        if (entity.CurrentPos.X != x || entity.CurrentPos.Y != y)
        {
            if (entityMatrix[x, y] == null)
            {
                entityMatrix[entity.CurrentPos.X, entity.CurrentPos.Y] = null;
                entity.OldPos = new Position(entity.CurrentPos);
                entity.CurrentPos = new Position(x, y);
                entityMatrix[entity.CurrentPos.X, entity.CurrentPos.Y] = entity;
            }
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("quit");
        Global.Instance.Client.Disconnect(Global.Instance.PlayerId.ToString());
    }

    private void loadMap(Position origin, string segments)
    {
        var mapArray = segments.Split(',');
        int i = origin.X, j = origin.Y;

        GameObject grdObj;
        GameObject objObj;
        var mapNode = GameObject.FindGameObjectWithTag("Map");

        foreach (string s in mapArray)
        {
            if (groundMatrix[i, j] == 0)
            {
                var tmp = s.Split('.');
                int ground = int.Parse(tmp[0]);

                groundMatrix[i, j] = ground;

                switch (ground)
                {
                    case 1:
                        grdObj = GameObject.Instantiate(Resources.Load("sea")) as GameObject;
                        break;
                    case 2:
                        grdObj = GameObject.Instantiate(Resources.Load("ground/ground")) as GameObject;
                        break;
                    case 3:
                        grdObj = GameObject.Instantiate(Resources.Load("grass/grass")) as GameObject;
                        break;
                    case 4:
                        grdObj = GameObject.Instantiate(Resources.Load("sand/sand")) as GameObject;
                        break;
                    case 5:
                        grdObj = GameObject.Instantiate(Resources.Load("road")) as GameObject;
                        break;
                    default:
                        grdObj = null;
                        break;

                }

                if (grdObj != null)
                {
                    grdObj.transform.parent = mapNode.transform;
                    grdObj.transform.position = new Vector3(tilesize * i, -tilesize * j, 0 - j * tileZLayerFactor);
                    grdObj.SetActive(true);
                    mapMatrix[i, j] = grdObj;
                }

                int mapObj = int.Parse(tmp[1]);
                switch (mapObj)
                {
                    case 1:
                        objObj = GameObject.Instantiate(Resources.Load("tree1")) as GameObject;
                        break;
                    case 2:
                        objObj = GameObject.Instantiate(Resources.Load("rock1")) as GameObject;
                        break;
                    case 3:
                        objObj = GameObject.Instantiate(Resources.Load("highgrass")) as GameObject;
                        break;
                    case 4:
                        objObj = GameObject.Instantiate(Resources.Load("bush")) as GameObject;
                        break;
                    default:
                        objObj = null;
                        break;
                }
                if (objObj != null)
                {
                    objObj.transform.parent = mapNode.transform;
                    objObj.transform.position = new Vector3(tilesize * i, -tilesize * j, -2 - j * tileZLayerFactor + tileZLayerFactor / 2);
                    objObj.SetActive(true);
                    mapObjMatrix[i, j] = objObj;
                }

                mapOverlayMatrix[i, j] = new List<GameObject>();
                populationMatrix[i, j] = new List<PopulationEntity>();
            }

            i++;
            if (i == origin.X + 60 - 1)
            {
                i = origin.X;
                j++;
            }
        }

        Debug.Log("map loaded");
        
        var overlayTool = new OverlayTool(groundMatrix, mapOverlayMatrix, mapNode, tilesize, tileZLayerFactor);
        overlayTool.AddMapTileOverlay(origin, 60, 60);
    }

    private void updateEntities()
    {
        List<MapEntity> entitiesToRemove = new List<MapEntity>();
        foreach (MapEntity entity in mapEntities.Values)
        {
            entity.MoveTimer += Time.deltaTime;
            entity.IsAliveTimer += Time.deltaTime;


            if (entity.IsAliveTimer > 1)
            {
                entitiesToRemove.Add(entity);
            }
        }

        foreach (MapEntity entityToRemove in entitiesToRemove)
        {
            mapEntities.Remove(entityToRemove.Id);
            entityMatrix[entityToRemove.CurrentPos.X, entityToRemove.CurrentPos.Y] = null;
            Destroy(entityToRemove.Object);
        }


        List<PositionEntity> transferList = new List<PositionEntity>();
        while (Global.Instance.MoveMessages.Count > 0)
        {
            PositionEntity message = Global.Instance.MoveMessages.Dequeue();
            MapEntity entity = null;

            if (mapEntities.ContainsKey(message.Id))
            {
                entity = mapEntities[message.Id];
                entity.Object.SetActive(true);
                entity.IsAliveTimer = 0;

                if (message.Orientation != Direction.None)
                {
                    entity.CurrentDir = message.Orientation;
                }

                if (!entity.CurrentPos.Equals(message.Position)) //todo gérer si la distance est trop grande (>1)
                {
                    if (entity.MoveTimer > entity.MoveTime)
                    {
                        //var currentDir = Utils.GetDirection(entity.CurrentPos, message.Position);
                        moveEntity(entity, message.Position.X, message.Position.Y);
                        if (entity.MoveTimer - Time.deltaTime > entity.MoveTime)
                        {
                            entity.MoveTimer = 0;
                        }
                        else
                        {
                            //Debug.Log("continue");
                            entity.MoveTimer -= entity.MoveTime;
                        }

                        //animation
                        var anim = entity.Object.GetComponent<Animator>();
                        //entity.CurrentDir = currentDir;

                        if (entity.PokedexId != -1)
                        {
                            var spriteRenderer = entity.Object.GetComponent<SpriteRenderer>();

                            switch (entity.CurrentDir)
                            {
                                case Direction.Down:
                                    spriteRenderer.sprite = Resources.Load<Sprite>("pokemonSprites/front/"+ entity.PokedexId);
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
                            }
                            
                            entity.State = message.State;

                            var overlaySpriteRenderer = entity.OverlayObject.GetComponent<SpriteRenderer>();
                            if (entity.State == EntityState.Swimming)
                            {
                                overlaySpriteRenderer.sprite = Resources.Load<Sprite>("swimming");
                                entity.OverlayObject.SetActive(true);
                            } else if (entity.State == EntityState.Flying)
                            {
                                overlaySpriteRenderer.sprite = Resources.Load<Sprite>("shadow");
                                entity.OverlayObject.SetActive(true);
                            } else
                            {
                                overlaySpriteRenderer.sprite = null;
                                entity.OverlayObject.SetActive(false);
                            }
                                
                        }

                        if (entity.CurrentDir != Direction.None && anim != null)
                        {
                            //Debug.Log("walk");
                            switch (entity.CurrentDir)
                            {
                                case Direction.Down:
                                    anim.CrossFade("player_down", 0f);
                                    break;
                                case Direction.Up:
                                    anim.CrossFade("player_up", 0f);
                                    break;
                                case Direction.Right:
                                    anim.CrossFade("player_right", 0f);
                                    break;
                                case Direction.Left:
                                    anim.CrossFade("player_left", 0f);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        transferList.Add(message);//on remet le message dans la file car on attend la fin de l'animation
                    }
                }

            }
            else
            {
                //Debug.Log("new entity");
                if (message.Type == EntityType.Player)
                {
                    entity = spawnPlayerCharacter(message.Id, message.Position);
                    if (entity.Id == Global.Instance.PlayerId)
                    {
                        player = entity;
                    }
                }
                else
                {
                    entity = spawnPokemon(message.Id, message.Position, message.PokedexId, 5);
                }

                entity.Object.SetActive(true);
                mapEntities.Add(message.Id, entity);
            }
        }



        //Debug.Log(transferList.Count);
        foreach (PositionEntity message in transferList)
        {
            Global.Instance.MoveMessages.Enqueue(message);
        }

        //Debug.Log("update");
        foreach (MapEntity entity in mapEntities.Values)
        {
            var anim = entity.Object.GetComponent<Animator>();

            if (entity.MoveTimer <= entity.MoveTime)
            {
                //moving
                entity.Object.transform.position = Vector3.Lerp(new Vector3(entity.OldPos.X * tilesize, -entity.OldPos.Y * tilesize, -2 - entity.OldPos.Y * tileZLayerFactor), new Vector3(entity.CurrentPos.X * tilesize, -entity.CurrentPos.Y * tilesize, -2 - entity.OldPos.Y * tileZLayerFactor), entity.MoveTimer / entity.MoveTime);

                if (entity.PokedexId != -1 && entity.MoveTimer % 0.2f < 0.12f && entity.State == EntityState.Walking)
                {
                    Vector3 currentPos = entity.Object.transform.position;
                    entity.Object.transform.position = new Vector3(currentPos.x + 0.005f, currentPos.y + 0.015f, currentPos.z);
                }


                //decalage vertical des pokemons
                if (entity.PokedexId != -1)
                {
                    entity.Object.transform.Translate(new Vector3(0, tilesize / 4, 0));// 4 ?
                    var overlayPos = entity.OverlayObject.transform.localPosition;
                    //decalage pour le vol
                    if (entity.State == EntityState.Flying)
                    {
                        entity.Object.transform.Translate(new Vector3(0, tilesize / 4, 0));
                        entity.OverlayObject.transform.localPosition = new Vector3(overlayPos.x, - tilesize / 4, overlayPos.z);
                    } else
                    {
                        entity.OverlayObject.transform.localPosition = new Vector3(overlayPos.x, 0, overlayPos.z);
                    }
                }
            }
            else
            {
                //if (entity.Id == playerId)  Debug.Log("stop");
                entity.OldPos = entity.CurrentPos;
                //animation
                if (anim != null)
                {
                    switch (entity.CurrentDir)
                    {
                        case Direction.Down:
                            anim.CrossFade("player_stand_down", 0f);
                            break;
                        case Direction.Right:
                            anim.CrossFade("player_stand_right", 0f);
                            break;
                        case Direction.Up:
                            anim.CrossFade("player_stand_up", 0f);
                            break;
                        case Direction.Left:
                            anim.CrossFade("player_stand_left", 0f);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    private MapEntity spawnPokemon(int id, Position pos, int pkId, int level)
    {
        var pkmnObj = GameObject.Instantiate(Resources.Load("PokemonPrefab")) as GameObject;
        pkmnObj.transform.localScale = new Vector3(1.25f, 1.25f, 1);

        var spriteRenderer = pkmnObj.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>("pokemonSprites/front/" + pkId);

        pkmnObj.transform.position = new Vector3(pos.X * tilesize , -pos.Y * tilesize, -2);
        pkmnObj.transform.parent = entitiesNode.transform;
        pkmnObj.SetActive(false);


        var overlayObj = GameObject.Instantiate(Resources.Load("PokemonPrefab")) as GameObject;
        overlayObj.transform.position = new Vector3(pos.X * tilesize, -pos.Y * tilesize, -2.1f);
        overlayObj.transform.parent = pkmnObj.transform;
        overlayObj.transform.localScale = new Vector3(1, 1, 1);
        overlayObj.SetActive(false);

        var pkmn = new MapEntity(id, pkmnObj, pos.X, pos.Y);
        pkmn.PokedexId = pkId;
        pkmn.Level = level;
        entityMatrix[pos.X, pos.Y] = pkmn;
        pkmn.OverlayObject = overlayObj;

        return pkmn;
    }

    private MapEntity spawnPlayerCharacter(int id, Position position)
    {
        var playerObj = GameObject.Instantiate(Resources.Load("player")) as GameObject;
        playerObj.transform.position = new Vector3(position.X * tilesize, -position.Y * tilesize, -2 - position.Y * tileZLayerFactor);
        playerObj.transform.parent = entitiesNode.transform;
        var playercharacter = new MapEntity(id, playerObj, position.X, position.Y);
        playercharacter.IA = false;
        entityMatrix[position.X, position.Y] = playercharacter;

        return playercharacter;
    }

    private void hidePokemon(MapEntity pkmn)
    {
        entityMatrix[pkmn.CurrentPos.X, pkmn.CurrentPos.Y] = null;
        Destroy(pkmn.Object);
    }

}
