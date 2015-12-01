using UnityEngine;
using System;

public class Map : MonoBehaviour {

    private MapEntity player;
    private float tilesize = 0.32f;
    private int mapsize = 400;
    
    public int renderDistance = 15;
    public int zoom = 1;

    private GameObject[,] mapMatrix;
    private MapEntity[,] entityMatrix;

    // Use this for initialization
    void Start () {
        string map = Resources.Load("map").ToString();

        entityMatrix = new MapEntity[mapsize, mapsize];
        mapMatrix = new GameObject[mapsize,mapsize];
        map = map.Substring(1);
        map = map.Remove(map.Length - 3); //charactères en trop ?

        var mapArray = map.Split(',');
        int i = 0, j = 0;

        GameObject obj;

        foreach (string s in mapArray)
        {
            int tile = int.Parse(s);

            switch (tile) {
            case 2:
                obj = GameObject.Instantiate(Resources.Load("sea")) as GameObject;
                break;
            case 6:
            default:
                obj = GameObject.Instantiate(Resources.Load("ground")) as GameObject;
                break;
            case 7:
                obj = GameObject.Instantiate(Resources.Load("grass")) as GameObject;
                break;
            }

            var mapNode = GameObject.FindGameObjectWithTag("Map");
            obj.transform.parent = mapNode.transform;
            obj.transform.position = new Vector3(tilesize * i, - tilesize * j, 0);
            mapMatrix[i, j] = obj;

            i++;
            if (i == mapsize)
            {
                i = 0;
                j++;
            }
        }

        Position position = null;

        if (ApplicationModel.playerBattleStartPos == null)
        {
            position = new Position(200, 200);
        } else {
            position = ApplicationModel.playerBattleStartPos;
        }
        

        var entitiesNode = GameObject.FindGameObjectWithTag("Entities");

        //spawn player
        var playerObj = GameObject.Instantiate(Resources.Load("player")) as GameObject;
        playerObj.transform.position = new Vector3(position.X * tilesize, -position.Y * tilesize, 0);
        playerObj.transform.parent = entitiesNode.transform;
        player = new MapEntity(playerObj, position.X, position.Y);
        player.Pokemons.Add(new Pokemon(1, 5));
        entityMatrix[position.X, position.Y] = player;

        //spawn pokemon
        var pos1 = new Position(205, 200);
        var rattataObj = GameObject.Instantiate(Resources.Load("Rattata")) as GameObject;
        rattataObj.transform.position = new Vector3(pos1.X * tilesize, -pos1.Y * tilesize, 0);
        rattataObj.transform.parent = entitiesNode.transform;
        var rattata = new MapEntity(rattataObj, pos1.X, pos1.Y);
        rattata.Pokemons.Add(new Pokemon(2, 5));
        entityMatrix[pos1.X, pos1.Y] = rattata;

        var pos2 = new Position(200, 205);
        var roucoolObj = GameObject.Instantiate(Resources.Load("Roucool")) as GameObject;
        roucoolObj.transform.position = new Vector3(pos2.X * tilesize, -pos2.Y * tilesize, 0);
        roucoolObj.transform.parent = entitiesNode.transform;
        var roucool = new MapEntity(roucoolObj, pos2.X, pos2.Y);
        roucool.Pokemons.Add(new Pokemon(1, 5));
        entityMatrix[pos2.X, pos2.Y] = roucool;

    }
	
	// Update is called once per frame
	void Update () {

        var posX = player.CurrentPos.X;
        var posY = player.CurrentPos.Y;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            posY--;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            posY++;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            posX++;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            posX--;
        }

        moveEntity(player, posX, posY);

        Vector3 playerPosition = player.Object.transform.position;
        gameObject.transform.position = new Vector3(playerPosition.x, playerPosition.y, gameObject.transform.position.z);

        //todo partir de current - (renderdistance + 1)
        for (int x = 0; x < mapsize; x++)
        {
            for (int y = 0; y < mapsize; y++)
            {
                
                var dist = Math.Max(Math.Abs(player.CurrentPos.X - x), Math.Abs(player.CurrentPos.Y - y));
                if (dist > renderDistance)
                {
                    mapMatrix[x, y].SetActive(false);
                } else
                {
                    mapMatrix[x, y].SetActive(true);
                }
            }
        }
        
    }

    private void moveEntity(MapEntity entity, int x, int y)
    {
        if (entity.CurrentPos.X != x || entity.CurrentPos.Y != y)
        {
            if (entityMatrix[x, y] == null)
            {
                entity.Object.transform.position = new Vector3(x * tilesize, -y * tilesize, 0);
                entityMatrix[entity.CurrentPos.X, entity.CurrentPos.Y] = null;
                entity.CurrentPos.X = x;
                entity.CurrentPos.Y = y;
                entityMatrix[entity.CurrentPos.X, entity.CurrentPos.Y] = entity;
            }
            else
            {
                ApplicationModel.otherBattleStartEntities.Clear();
                ApplicationModel.otherBattleStartEntities.Add(entityMatrix[x, y]);
                ApplicationModel.playerBattleStartEntity = entity;
                ApplicationModel.playerBattleStartPos = new Position(entity.CurrentPos);
                Application.LoadLevel("scene_battle");
            }
        }
    }
}
