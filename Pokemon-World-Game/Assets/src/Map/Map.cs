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
                    var dirPos = PositionUtils.GetDirPosition(player.CurrentDir);
                    var otherPos = new Position(player.CurrentPos.X + dirPos.X, player.CurrentPos.Y - dirPos.Y);
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
            Vector3 playerPosition = player.Object.transform.position;
            gameObject.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z - 10);
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

        addMapTileOverlay(origin, 60);
    }
    /*
    private void loadMap() {
        Debug.Log("load map");
        
        string map = jsonMap;

        
        map = map.Substring(1);
        map = map.Remove(map.Length - 3); //charactères en trop ?

        var mapArray = map.Split(',');
        int i = 0, j = 0;

        GameObject grdObj;
        GameObject objObj;
        var mapNode = GameObject.FindGameObjectWithTag("Map");

        foreach (string s in mapArray)
        {
            var tmp = s.Split('.');
            int ground = int.Parse(tmp[0]) - 1;

            groundMatrix[i, j] = ground;

            switch (ground)
            {
                case 1:
                case 2:
                    grdObj = GameObject.Instantiate(Resources.Load("sea")) as GameObject;
                    break;
                case 3:
                case 4:
                default:
                    grdObj = GameObject.Instantiate(Resources.Load("ground/ground")) as GameObject;
                    break;
                case 5:
                    grdObj = GameObject.Instantiate(Resources.Load("grass/grass")) as GameObject;
                    break;
                case 6:
                    grdObj = GameObject.Instantiate(Resources.Load("sand/sand")) as GameObject;
                    break;
                case 7:
                case 8:
                    grdObj = GameObject.Instantiate(Resources.Load("road")) as GameObject;
                    break;
            }

            grdObj.transform.parent = mapNode.transform;
            grdObj.transform.position = new Vector3(tilesize * i, -tilesize * j, 0 - j * tileZLayerFactor);
            grdObj.SetActive(false);
            mapMatrix[i, j] = grdObj;


            int mapObj = int.Parse(tmp[1]) - 1;
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
                objObj.SetActive(false);
                mapObjMatrix[i, j] = objObj;
            }

            mapOverlayMatrix[i, j] = new List<GameObject>();
            populationMatrix[i, j] = new List<PopulationEntity>();
            i++;
            if (i == mapsize)
            {
                i = 0;
                j++;
            }
        }
        Debug.Log("map loaded");

        //addMapTileOverlay();

        Position position = null;

        if (ApplicationModel.playerBattleStartPos == null)
        {
            //recevoir la position depuis le serveur à la connexion
            //position = new Position(200, 150);
            position = new Position(200, 175);
        }
        else
        {
            position = ApplicationModel.playerBattleStartPos;
        }

        //temporaire, à faire quand on reçoit la map du serveur
        //spawn player
        //player = spawnPlayerCharacter(playerId, position);
        //gameObject.transform.position = new Vector3(player.Object.transform.position.x, player.Object.transform.position.y, gameObject.transform.position.z);

        //spawn pokemon
        //spawnPokemon(10, new Position(205, 200), 0, 5);
        //spawnPokemon(11, new Position(200, 205), 1, 5);


        //population
        for (int k = 0; k < 15000; k++)
        {
            //addEntity
            var popX = Mathf.FloorToInt(UnityEngine.Random.value * mapsize);
            var popY = Mathf.FloorToInt(UnityEngine.Random.value * mapsize);
            var popId = Mathf.FloorToInt(UnityEngine.Random.value * 3);
            var pop = new PopulationEntity(popId, null, popX, popY);
            populationMatrix[popX, popY].Add(pop);
            pop.Age = Mathf.FloorToInt(UnityEngine.Random.value * 100);
            pop.Level = Mathf.FloorToInt(UnityEngine.Random.value * 5);
        }
    }*/

    #region overlay
    private void addMapTileOverlay(Position origin, int size)
    {
        for (int i = origin.X; i < origin.X + size; i++)
        {
            for (int j = origin.Y; j < origin.Y + size; j++)
            {

                addBorderOverlay(i, j, 3, 1, "grass", 0);
                addBorderOverlay(i, j, 3, 2, "grass", 0);
                addBorderOverlay(i, j, 3, 4, "grass", 0);
                addBorderOverlay(i, j, 3, 5, "grass", 0);

                addBorderOverlay(i, j, 4, 1, "sand", 0);
                addBorderOverlay(i, j, 4, 5, "sand", 0);

                addBorderOverlay(i, j, 2, 4, "ground", 0);
                addBorderOverlay(i, j, 2, 5, "ground", 0);

                addBorderOverlay(i, j, 2, 1, "groundcliff", 1);

                /*addBorderOverlay(i, j, 5, 1, "grass", 0);
                addBorderOverlay(i, j, 5, 2, "grass", 0);
                addBorderOverlay(i, j, 5, 3, "grass", 0);
                addBorderOverlay(i, j, 5, 4, "grass", 0);
                addBorderOverlay(i, j, 5, 6, "grass", 0);
                addBorderOverlay(i, j, 5, 7, "grass", 0);
                addBorderOverlay(i, j, 5, 8, "grass", 0);

                addBorderOverlay(i, j, 6, 1, "sand", 0);
                addBorderOverlay(i, j, 6, 2, "sand", 0);
                addBorderOverlay(i, j, 6, 7, "sand", 0);
                addBorderOverlay(i, j, 6, 8, "sand", 0);

                addBorderOverlay(i, j, 4, 6, "ground", 0);
                addBorderOverlay(i, j, 3, 6, "ground", 0);
                addBorderOverlay(i, j, 4, 7, "ground", 0);
                addBorderOverlay(i, j, 3, 7, "ground", 0);

                addBorderOverlay(i, j, 3, 1, "groundcliff", 1);
                addBorderOverlay(i, j, 3, 2, "groundcliff", 1);
                addBorderOverlay(i, j, 4, 1, "groundcliff", 1);
                addBorderOverlay(i, j, 4, 2, "groundcliff", 1);*/
            }
        }
    }

    private void addBorderOverlay(int i, int j, int fromTileType, int toTileType, string prefabName, int layer)
    {
        var tmppos = new Position(i, j);
        var toppos = new Position(i, j - 1);
        var botpos = new Position(i, j + 1);
        var rightpos = new Position(i + 1, j);
        var leftpos = new Position(i - 1, j);
        var topleftpos = new Position(i - 1, j - 1);
        var botleftpos = new Position(i - 1, j + 1);
        var toprightpos = new Position(i + 1, j - 1);
        var botrightpos = new Position(i + 1, j + 1);


        /*if (i == 0)
        {
            leftpos = null;
            topleftpos = null;
            botleftpos = null;
        }
        if (j == 0)
        {
            toppos = null;
            topleftpos = null;
            toprightpos = null;
        }
        if (i == mapsize - 1)
        {
            rightpos = null;
            botrightpos = null;
            toprightpos = null;
        }
        if (j == mapsize - 1)
        {
            botpos = null;
            botleftpos = null;
            botrightpos = null;
        }*/

        if (groundMatrix[tmppos.X, tmppos.Y] == fromTileType)
        {
            bool right = false;
            bool left = false;
            bool bottom = false;
            bool top = false;

            if (toprightpos != null && groundMatrix[toprightpos.X, toprightpos.Y] == fromTileType)
            {
                if (rightpos != null && groundMatrix[rightpos.X, rightpos.Y] == toTileType)
                {
                    instantiateOverlay(i + 1, j, prefabName + "/" + prefabName + "_topleft", layer * 0.1f);
                    right = true;
                }
                if (toppos != null && groundMatrix[toppos.X, toppos.Y] == toTileType)
                {
                    instantiateOverlay(i, j - 1, prefabName + "/" + prefabName + "_bottomright", layer * 0.1f);
                    top = true;
                }
            }

            if (topleftpos != null && groundMatrix[topleftpos.X, topleftpos.Y] == fromTileType)
            {
                if (leftpos != null && groundMatrix[leftpos.X, leftpos.Y] == toTileType)
                {
                    instantiateOverlay(i - 1, j, prefabName + "/" + prefabName + "_topright", layer * 0.1f);
                    left = true;
                }
                if (toppos != null && groundMatrix[toppos.X, toppos.Y] == toTileType)
                {
                    instantiateOverlay(i, j - 1, prefabName + "/" + prefabName + "_bottomleft", layer * 0.1f);
                    top = true;
                }
            }

            if (botleftpos != null && groundMatrix[botleftpos.X, botleftpos.Y] == fromTileType)
            {
                if (leftpos != null && groundMatrix[leftpos.X, leftpos.Y] == toTileType)
                {
                    instantiateOverlay(i - 1, j, prefabName + "/" + prefabName + "_bottomright", layer * 0.1f);
                    left = true;
                }
                if (botpos != null && groundMatrix[botpos.X, botpos.Y] == toTileType)
                {
                    instantiateOverlay(i, j + 1, prefabName + "/" + prefabName + "_topleft", layer * 0.1f);
                    bottom = true;
                }
            }

            if (botrightpos != null && groundMatrix[botrightpos.X, botrightpos.Y] == fromTileType)
            {
                if (rightpos != null && groundMatrix[rightpos.X, rightpos.Y] == toTileType)
                {
                    instantiateOverlay(i + 1, j, prefabName + "/" + prefabName + "_bottomleft", layer * 0.1f);
                    right = true;
                }
                if (botpos != null && groundMatrix[botpos.X, botpos.Y] == toTileType)
                {
                    instantiateOverlay(i, j + 1, prefabName + "/" + prefabName + "_topright", layer * 0.1f);
                    bottom = true;
                }
            }


            if (rightpos != null && groundMatrix[rightpos.X, rightpos.Y] == toTileType && !right)
            {
                instantiateOverlay(i + 1, j, prefabName + "/" + prefabName + "_left", layer * 0.1f);
                right = true;
            }
            if (toppos != null && groundMatrix[toppos.X, toppos.Y] == toTileType && !top)
            {
                instantiateOverlay(i, j - 1, prefabName + "/" + prefabName + "_bottom", layer * 0.1f);
                top = true;
            }
            if (botpos != null && groundMatrix[botpos.X, botpos.Y] == toTileType && !bottom)
            {
                instantiateOverlay(i, j + 1, prefabName + "/" + prefabName + "_top", layer * 0.1f);
                bottom = true;
            }
            if (leftpos != null && groundMatrix[leftpos.X, leftpos.Y] == toTileType && !left)
            {
                instantiateOverlay(i - 1, j, prefabName + "/" + prefabName + "_right", layer * 0.1f);
                left = true;
            }

            if (top && right)
            {
                instantiateOverlay(i + 1, j - 1, prefabName + "/" + prefabName + "_bottomleftcorner", layer * 0.1f + 0.05f);
            }
            if (top && left)
            {
                instantiateOverlay(i - 1, j - 1, prefabName + "/" + prefabName + "_bottomrightcorner", layer * 0.1f + 0.05f);
            }
            if (bottom && left)
            {
                instantiateOverlay(i - 1, j + 1, prefabName + "/" + prefabName + "_toprightcorner", layer * 0.1f + 0.05f);
            }
            if (bottom && right)
            {
                instantiateOverlay(i + 1, j + 1, prefabName + "/" + prefabName + "_topleftcorner", layer * 0.1f + 0.05f);
            }
        }

        /*
        if (groundMatrix[tmppos.X, tmppos.Y] == toTileType)
        {

            bool topright = false;
            bool topleft = false;
            bool botleft = false;
            bool botright = false;
            bool right = false;
            bool left = false;
            bool bottom = false;
            bool top = false;

            if (toppos != null && rightpos != null && groundMatrix[toppos.X, toppos.Y] == fromTileType && groundMatrix[rightpos.X, rightpos.Y] == fromTileType)
            {
                instantiateOverlay(i, j, prefabName + "/"+ prefabName + "_topright");
                topright = true;
            }
            if (toppos != null && leftpos != null && groundMatrix[toppos.X, toppos.Y] == fromTileType && groundMatrix[leftpos.X, leftpos.Y] == fromTileType)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_topleft");
                topleft = true;
            }
            if (leftpos != null && botpos != null && groundMatrix[leftpos.X, leftpos.Y] == fromTileType && groundMatrix[botpos.X, botpos.Y] == fromTileType)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_bottomleft");
                botleft = true;
            }
            if (botpos != null && rightpos != null && groundMatrix[botpos.X, botpos.Y] == fromTileType && groundMatrix[rightpos.X, rightpos.Y] == fromTileType)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_bottomright");
                botright = true;
            }

            if (rightpos != null && groundMatrix[rightpos.X, rightpos.Y] == fromTileType && !topright && !botright)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_right");
            }
            if (toppos != null && groundMatrix[toppos.X, toppos.Y] == fromTileType && !topright && !topleft)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_top");
            }
            if (botpos != null && groundMatrix[botpos.X, botpos.Y] == fromTileType && !botleft && !botright)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_bottom");
            }
            if (leftpos != null && groundMatrix[leftpos.X, leftpos.Y] == fromTileType && !botleft && !topleft)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_left");
            }


            if (topleftpos != null && groundMatrix[topleftpos.X, topleftpos.Y] == fromTileType && groundMatrix[leftpos.X, leftpos.Y] == toTileType && groundMatrix[toppos.X, toppos.Y] == toTileType && !topleft)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_topleftcorner");
            }
            if (botleftpos != null && groundMatrix[botleftpos.X, botleftpos.Y] == fromTileType && groundMatrix[leftpos.X, leftpos.Y] == toTileType && groundMatrix[botpos.X, botpos.Y] == toTileType && !botleft)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_bottomleftcorner");
            }
            if (toprightpos != null && groundMatrix[toprightpos.X, toprightpos.Y] == fromTileType && groundMatrix[rightpos.X, rightpos.Y] == toTileType && groundMatrix[toppos.X, toppos.Y] == toTileType && !topright)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_toprightcorner");
            }
            if (botrightpos != null && groundMatrix[botrightpos.X, botrightpos.Y] == fromTileType && groundMatrix[rightpos.X, rightpos.Y] == toTileType && groundMatrix[botpos.X, botpos.Y] == toTileType && !botright)
            {
                instantiateOverlay(i, j, prefabName + "/" + prefabName + "_bottomrightcorner");
            }
        }
        */
    }

    private void instantiateOverlay(int i, int j, string prefab, float z)
    {
        var mapNode = GameObject.FindGameObjectWithTag("Map");
        var overlay = GameObject.Instantiate(Resources.Load(prefab)) as GameObject;
        overlay.transform.parent = mapNode.transform;
        overlay.transform.position = new Vector3(tilesize * i, -tilesize * j, -0.5f - z - j * tileZLayerFactor);
        overlay.SetActive(true);
        mapOverlayMatrix[i, j].Add(overlay);
    }
    #endregion

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

                        //test
                        if (anim == null && entity.PokedexId == 60)
                        {
                            if (message.State == EntityState.Swimming)
                            {
                                var oldObj = entity.Object;
                                entity.Object = GameObject.Instantiate(Resources.Load("ptitardSwimming_0")) as GameObject;
                                entity.Object.transform.position = new Vector3(oldObj.transform.position.x, oldObj.transform.position.y, oldObj.transform.position.z);
                                entity.Object.transform.parent = entitiesNode.transform;
                                Destroy(oldObj);
                            }
                            else
                            {
                                var oldObj = entity.Object;
                                entity.Object = GameObject.Instantiate(Resources.Load("Ptitard")) as GameObject;
                                entity.Object.transform.position = new Vector3(oldObj.transform.position.x, oldObj.transform.position.y, oldObj.transform.position.z);
                                entity.Object.transform.parent = entitiesNode.transform;
                                Destroy(oldObj);
                            }
                        }

                        if (anim == null && entity.PokedexId == 16)
                        {
                            if (message.State == EntityState.Flying)
                            {
                                var oldObj = entity.Object;
                                entity.Object = GameObject.Instantiate(Resources.Load("roucoolFlying_0")) as GameObject;
                                entity.Object.transform.position = new Vector3(oldObj.transform.position.x, oldObj.transform.position.y, oldObj.transform.position.z);
                                entity.Object.transform.parent = entitiesNode.transform;
                                Destroy(oldObj);
                            }
                            else
                            {
                                var oldObj = entity.Object;
                                entity.Object = GameObject.Instantiate(Resources.Load("Roucool")) as GameObject;
                                entity.Object.transform.position = new Vector3(oldObj.transform.position.x, oldObj.transform.position.y, oldObj.transform.position.z);
                                entity.Object.transform.parent = entitiesNode.transform;
                                Destroy(oldObj);
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

                if (anim == null && entity.MoveTimer % 0.2f < 0.12f)
                {
                    Vector3 currentPos = entity.Object.transform.position;
                    entity.Object.transform.position = new Vector3(currentPos.x + 0.005f, currentPos.y + 0.015f, currentPos.z);

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
        var prefab = "";
        switch (pkId)
        {
            default:
            case 19:
                prefab = "Rattata";
                break;
            case 16:
                prefab = "Roucool";
                break;
            case 60:
                prefab = "Ptitard";
                break;
        }

        var pkmnObj = GameObject.Instantiate(Resources.Load(prefab)) as GameObject;
        pkmnObj.transform.position = new Vector3(pos.X * tilesize, -pos.Y * tilesize, -2);
        pkmnObj.transform.parent = entitiesNode.transform;
        pkmnObj.SetActive(false);
        var pkmn = new MapEntity(id, pkmnObj, pos.X, pos.Y);
        pkmn.PokedexId = pkId;
        pkmn.Level = level;
        entityMatrix[pos.X, pos.Y] = pkmn;

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
        mapEntities.Add(id, playercharacter);

        return playercharacter;
    }

    private void hidePokemon(MapEntity pkmn)
    {
        entityMatrix[pkmn.CurrentPos.X, pkmn.CurrentPos.Y] = null;
        Destroy(pkmn.Object);
    }

}
