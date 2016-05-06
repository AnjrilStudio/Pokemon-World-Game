using UnityEngine;
using System;
using Anjril.Common.Network;
using Anjril.Common.Network.UdpImpl;
using System.Net.Sockets;
using System.Collections.Generic;

public class Map : MonoBehaviour {

    private MapEntity player;
    private float tilesize = 0.32f;
    private int mapsize = 400;
    
    public int renderDistance = 15;
    public int zoom = 1;

    private GameObject[,] mapMatrix;
    private GameObject[,] mapObjMatrix;
    private List<GameObject>[,] mapOverlayMatrix;
    private int[,] groundMatrix;
    private MapEntity[,] entityMatrix;
    private List<PopulationEntity>[,] populationMatrix;


    private float moveTime = 0.6f;
    private float moveTimer = 0f;

    private Anjril.Common.Network.Socket socket;
    private UdpClient client;

    private bool upWasUp = false;
    private bool downWasUp = false;
    private bool rightWasUp = false;
    private bool leftWasUp = false;
    private Direction lastInput = Direction.None;

    private int nbMsgReceived = 0;
    private string[] messages;
    private string jsonMap;
    private bool mapLoaded;

    private GameObject entitiesNode;

    //private RemoteConnection connection = new RemoteConnection { IPAddress = "192.168.1.23", Port = 1337 };


    // Use this for initialization
    void Start () {
        /*client = new UdpClient(1337);
        var receiver = new UdpReceiver(client);
        var sender = new UdpSender(client);
        socket = new Anjril.Common.Network.Socket(receiver, sender, MessageReceived);
        socket.StartListening();
        socket.Send("login", connection);
        mapLoaded = false;*/

        jsonMap = Resources.Load("map").ToString();

        entitiesNode = GameObject.FindGameObjectWithTag("Entities");

    }

    private void MessageReceived(RemoteConnection sender, string message)
    {
        Debug.Log("received " + message);
        if (jsonMap == null)
        {
            var currentId = int.Parse(message.Split(':')[0]);
            var totalMessage = int.Parse(message.Split(':')[1].Split('|')[0]);
            var trueMessage = message.Split('|')[1];

            if (messages == null)
            {
                messages = new string[totalMessage];
            }

            nbMsgReceived++;
            messages[currentId] = trueMessage;

            if (nbMsgReceived == totalMessage)
            {
                jsonMap = String.Join("", messages);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if (jsonMap != null)
        {
            if (!mapLoaded)
            {
                loadMap();
            }

            moveTimer += Time.deltaTime;
            if (moveTimer > moveTime)
            {
                var anim = player.Object.GetComponent<Animator>();
                var posX = player.CurrentPos.X;
                var posY = player.CurrentPos.Y;
                //bool move = false;
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
                lastInput = moveDir;

                upWasUp = !up;
                downWasUp = !down;
                leftWasUp = !left;
                rightWasUp = !right;


                if (moveDir != Direction.None)
                {
                    posX += Utils.getDirPosition(moveDir).X;
                    posY -= Utils.getDirPosition(moveDir).Y;
                    player.OldPos = new Position(player.CurrentPos);

                    switch (moveDir)
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

                    //socket.Send(moveDir.ToString(), connection);

                    moveEntity(player, posX, posY);
                    //todo gérer le déplacement serveur

                    moveTimer = 0f;
                } else
                {
                    //Debug.Log(anim.GetCurrentAnimatorStateInfo(0).IsName("player_up"));
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("player_up"))
                    {
                        anim.CrossFade("player_stand_up", 0f);
                    }
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("player_right"))
                    {
                        anim.CrossFade("player_stand_right", 0f);
                    }
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("player_left"))
                    {
                        anim.CrossFade("player_stand_left", 0f);
                    }
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("player_down"))
                    {
                        anim.CrossFade("player_stand_down", 0f);
                    }
                }
            
            } else
            { //moving
                player.Object.transform.position = Vector3.Lerp(new Vector3(player.OldPos.X * tilesize, -player.OldPos.Y * tilesize, player.Object.transform.position.z), new Vector3(player.CurrentPos.X * tilesize, -player.CurrentPos.Y * tilesize, player.Object.transform.position.z), moveTimer / moveTime);
            }

            //deplacement camera
            Vector3 playerPosition = player.Object.transform.position;
            gameObject.transform.position = new Vector3(playerPosition.x, playerPosition.y, gameObject.transform.position.z);


            //maj pokemon sauvages
            updateEntities();
            
            //todo recevoir la map du serveur
            for (int x = player.CurrentPos.X - renderDistance * 2; x < player.CurrentPos.X + renderDistance * 2; x++)
            {
                for (int y = player.CurrentPos.Y - renderDistance * 2; y < player.CurrentPos.Y + renderDistance * 2; y++)
                {
                    if (x >= 0 && y >= 0 && x < mapsize && y < mapsize)
                    {
                        var dist = Math.Max(Math.Abs(player.CurrentPos.X - x), Math.Abs(player.CurrentPos.Y - y));
                        if (dist > renderDistance && dist < renderDistance * 2)
                        {
                            mapMatrix[x, y].SetActive(false);
                            if (mapObjMatrix[x, y] != null)
                            {
                                mapObjMatrix[x, y].SetActive(false);
                            }
                            if (entityMatrix[x, y] != null)
                            {
                                entityMatrix[x, y].Object.SetActive(false);
                            }
                            foreach (GameObject overlay in mapOverlayMatrix[x, y])
                            {
                                overlay.SetActive(false);
                            }
                        }
                        else if (dist <= renderDistance)
                        {
                            mapMatrix[x, y].SetActive(true);
                            if (mapObjMatrix[x, y] != null)
                            {
                                var mapObj = mapObjMatrix[x, y];
                                mapObj.SetActive(true);
                                mapObj.transform.position = new Vector3(mapObj.transform.position.x, mapObj.transform.position.y, -1f + (float)(player.CurrentPos.Y - y) / (float)(renderDistance + 1));
                            }
                            if (entityMatrix[x, y] != null)
                            {
                                entityMatrix[x, y].Object.SetActive(true);
                            }
                            foreach (GameObject overlay in mapOverlayMatrix[x, y])
                            {
                                overlay.SetActive(true);
                            }
                        }
                    }
                }
            }
            
        }

    }

    private void moveEntity(MapEntity entity, int x, int y)
    {
        if (Position.isInMap(x, y, mapsize) && (entity.CurrentPos.X != x || entity.CurrentPos.Y != y))
        {
            if (entityMatrix[x, y] == null)
            {
                if (entity.IA == true)
                {
                    //todo gérer l'animation
                    entity.Object.transform.position = new Vector3(x * tilesize, -y * tilesize, entity.Object.transform.position.z);
                }

                entityMatrix[entity.CurrentPos.X, entity.CurrentPos.Y] = null;
                entity.CurrentPos.X = x;
                entity.CurrentPos.Y = y;
                entityMatrix[entity.CurrentPos.X, entity.CurrentPos.Y] = entity;
            }
            else
            {
                if (entity.IA == false && entityMatrix[x, y].IA == true)
                {
                    //à recevoir du serveur
                    ApplicationModel.otherBattleStartEntities.Clear();
                    ApplicationModel.otherBattleStartEntities.Add(entityMatrix[x, y]);
                    ApplicationModel.playerBattleStartEntity = entity;
                    ApplicationModel.playerBattleStartPos = new Position(entity.CurrentPos);
                    Application.LoadLevel("scene_battle");
                }
            }
        }
    }

    private void OnApplicationQuit() {
        //client.Close();
    }

    private void loadMap() {
        Debug.Log("load map");
        mapLoaded = true;
        
        string map = jsonMap;

        entityMatrix = new MapEntity[mapsize, mapsize];
        mapMatrix = new GameObject[mapsize, mapsize];
        mapObjMatrix = new GameObject[mapsize, mapsize];
        groundMatrix = new int[mapsize, mapsize];
        mapOverlayMatrix = new List<GameObject>[mapsize, mapsize];
        populationMatrix = new List < PopulationEntity >[mapsize, mapsize];
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
            grdObj.transform.position = new Vector3(tilesize * i, -tilesize * j, 0);
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
                objObj.transform.position = new Vector3(tilesize * i, -tilesize * j, -2);
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

        addMapTileOverlay();

        Position position = null;

        if (ApplicationModel.playerBattleStartPos == null)
        {
            //recevoir la position depuis le serveur à la connexion
            position = new Position(200, 150);
        }
        else
        {
            position = ApplicationModel.playerBattleStartPos;
        }

        //temporaire, à faire quand on reçoit la map du serveur
        //spawn player
        var playerObj = GameObject.Instantiate(Resources.Load("player")) as GameObject;
        playerObj.transform.position = new Vector3(position.X * tilesize, -position.Y * tilesize, -2 - (1/ (float)(renderDistance * 2)));
        playerObj.transform.parent = entitiesNode.transform;
        player = new MapEntity(playerObj, position.X, position.Y);
        player.Pokemons.Add(new Pokemon(1, 5));
        player.IA = false;
        entityMatrix[position.X, position.Y] = player;

        //spawn pokemon
        spawnPokemon(new Position(205, 200), 0, 5);
        spawnPokemon(new Position(200, 205), 1, 5);


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
    }

    private void addMapTileOverlay()
    {
        for (int i = 0; i < mapsize; i++)
        {
            for (int j = 0; j < mapsize; j++)
            {
                addBorderOverlay(i, j, 5, 1, "grass", 0);
                addBorderOverlay(i, j, 5, 2, "grass", 0);
                addBorderOverlay(i, j, 5, 3, "grass", 0);
                addBorderOverlay(i, j, 5, 4, "grass", 0);
                addBorderOverlay(i, j, 5, 6, "grass", 0);
                addBorderOverlay(i, j, 5, 7, "grass", 0);
                addBorderOverlay(i, j, 5, 8, "grass", 0);

                addBorderOverlay(i, j, 6, 1, "sand", 0);
                addBorderOverlay(i, j, 6, 2, "sand", 0);

                addBorderOverlay(i, j, 4, 6, "ground", 0);
                addBorderOverlay(i, j, 3, 6, "ground", 0);
                addBorderOverlay(i, j, 4, 7, "ground", 0);
                addBorderOverlay(i, j, 3, 7, "ground", 0);

                addBorderOverlay(i, j, 3, 1, "groundcliff", 1);
                addBorderOverlay(i, j, 3, 2, "groundcliff", 1);
                addBorderOverlay(i, j, 4, 1, "groundcliff", 1);
                addBorderOverlay(i, j, 4, 2, "groundcliff", 1);
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


        if (i == 0)
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
        }

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

    private void instantiateOverlay(int i , int j, string prefab, float z)
    {
        var mapNode = GameObject.FindGameObjectWithTag("Map");
        var overlay = GameObject.Instantiate(Resources.Load(prefab)) as GameObject;
        overlay.transform.parent = mapNode.transform;
        overlay.transform.position = new Vector3(tilesize * i, -tilesize * j, -0.5f-z);
        overlay.SetActive(false);
        mapOverlayMatrix[i, j].Add(overlay);
    }

    private void updateEntities()
    {
        var spawnRate = 0.0002f;
        var hideRate = 0.001f;
        var moveRate = 0.01f;
        List<PopulationEntity> spawnList = new List<PopulationEntity>();
        List<MapEntity> hideList = new List<MapEntity>();
        List<MapEntity> moveList = new List<MapEntity>();
        for (int i = 0; i < mapsize; i++)
        {
            for (int j = 0; j < mapsize; j++)
            {
                if (populationMatrix[i, j].Count > 0 && entityMatrix[i, j] == null)
                {
                    foreach (PopulationEntity p in populationMatrix[i, j])
                    {
                        if (UnityEngine.Random.value < spawnRate)
                        {
                            spawnList.Add(p);
                        }
                    }
                }

                if (entityMatrix[i,j] != null && entityMatrix[i, j].IA == true)
                {
                    if (UnityEngine.Random.value < moveRate)
                    {
                        moveList.Add(entityMatrix[i, j]);
                    } else if (UnityEngine.Random.value < hideRate)
                    {
                        hideList.Add(entityMatrix[i, j]);
                    }
                }
            }
        }

        foreach (PopulationEntity p in spawnList)
        {
            spawnPokemon(p.Pos, p.Id, p.Level);
        }

        foreach (MapEntity p in hideList)
        {
            hidePokemon(p);
        }

        foreach (MapEntity p in moveList)
        {
            var dir = Utils.getDirPosition(Utils.getRandomDir());
            moveEntity(p, p.CurrentPos.X + dir.X, p.CurrentPos.Y + dir.Y);
        }


    }

    private void spawnPokemon(Position pos, int id, int level)
    {
        var prefab = "";
        switch (id)
        {
            case 0:
                prefab = "Rattata";
                break;
            case 1:
                prefab = "Roucool";
                break;
            case 2:
                prefab = "Ptitard";
                break;
            default:
                prefab = "Rattata";
                break;
        }

        var pkmnObj = GameObject.Instantiate(Resources.Load(prefab)) as GameObject;
        pkmnObj.transform.position = new Vector3(pos.X * tilesize, -pos.Y * tilesize, -2);
        pkmnObj.transform.parent = entitiesNode.transform;
        pkmnObj.SetActive(false);
        var pkmn = new MapEntity(pkmnObj, pos.X, pos.Y);
        pkmn.Pokemons.Add(new Pokemon(id, level));
        entityMatrix[pos.X, pos.Y] = pkmn;
    }

    private void hidePokemon(MapEntity pkmn)
    {
        entityMatrix[pkmn.CurrentPos.X, pkmn.CurrentPos.Y] = null;
        Destroy(pkmn.Object);
    }


}
