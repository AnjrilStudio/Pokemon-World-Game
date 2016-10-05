using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Anjril.PokemonWorld.Common.State;
using Anjril.PokemonWorld.Common.Utils;

public class population_test : MonoBehaviour
{

    public float timeSpeed = 1;

    private float tilesize = 0.32f;
    private int mapsize = 400;

    private GroundEntity[,] mapMatrix;
    private List<PopulationEntity>[,] populationMatrix;
    private float[,] repChanceMatrix = new float[3, 3] { 
        //red  , purple, yellow
        { 0.00f, 0.00f, 0.18f }, //sea
        { 0.12f, 0.00f, 0.00f }, //ground
        { 0.02f, 0.15f, 0.00f }  //grass
    };

    private string jsonMap;
    private bool mapLoaded;

    private float reproductionTimer = 0.3f;
    private float reproductionTime = 2f;
    //private float repChance = 0.08f; // old 0.10 //old 0.5
    private int repArea = 10;
    private int repAge = 5;

    private float moveTimer = 0.1f;
    private float moveTime = 0.2f;
    private float moveChance = 0.6f;
    private float moveChanceMalusPerLevel = 0.003f;

    private float ageTimer = 0;
    private float ageTime = 1f;
    private int ageValue = 1;
    private int ageLimit = 150;
    private float ageDeathChance = 0.05f;
    private int xpArea = 12;
    private float xplevelUpChance = 0.18f;
    //private float xpLevelFactor = 0.003f;

    private float fightTimer = 0.5f;
    private float fightTime = 1f;
    private int fightArea = 4;
    private float fightChance = 0.30f; //old 0.4 // old 0.4
    private int fightAge = 4;
    private int fightNoRepTime = 1;
    //private int fightLevelValue = 1;
    private int fightLevelDiff = 3;
    private float noRepFightChanceMalusFactor = 0.1f;

    private float debugTimer = 5f;
    private float debugTime = 5f;

    // Use this for initialization
    void Start()
    {
        jsonMap = Resources.Load("map").ToString();
        loadMap();

        for (int i = 0; i < 15000; i++)
        {
            var pop = addEntity(Mathf.FloorToInt(Random.value * mapsize), Mathf.FloorToInt(Random.value * mapsize), 1 + Mathf.FloorToInt(Random.value * 3));
            pop.Age = Mathf.FloorToInt(Random.value * 100);
            pop.Level = Mathf.FloorToInt(Random.value * 5);
        }


    }

    // Update is called once per frame
    void Update()
    {
        reproductionTimer += Time.deltaTime * timeSpeed;
        moveTimer += Time.deltaTime * timeSpeed;
        ageTimer += Time.deltaTime * timeSpeed;
        fightTimer += Time.deltaTime * timeSpeed;
        debugTimer += Time.deltaTime * timeSpeed;

        if (moveTimer > moveTime)
        {
            int total = movePhase();
            moveTimer -= moveTime;
            if (debugTimer > debugTime)
            {
                Debug.Log("Total : " + total);
                debugTimer -= debugTime;
            }
        }


        if (reproductionTimer > reproductionTime)
        {
            int repCount = reproductionPhase();
            reproductionTimer -= reproductionTime;
            Debug.Log("Births : " + repCount);
        }

        if (ageTimer > ageTime)
        {
            int deathCount = agePhase();
            ageTimer -= ageTime;
            Debug.Log("Deaths : " + deathCount);
        }

        if (fightTimer > fightTime)
        {
            int fightCount = fightPhase();
            fightTimer -= fightTime;
            Debug.Log("Fights : " + fightCount);
        }
    }

    private void loadMap()
    {
        Debug.Log("load map");
        mapLoaded = true;

        string map = jsonMap;

        populationMatrix = new List<PopulationEntity>[mapsize, mapsize];
        mapMatrix = new GroundEntity[mapsize, mapsize];
        map = map.Substring(1);
        map = map.Remove(map.Length - 3); //charactères en trop ?

        var mapArray = map.Split(',');
        int i = 0, j = 0;

        GameObject obj;
        GroundEntity ent;

        foreach (string s in mapArray)
        {
            int tile = int.Parse(s);

            switch (tile)
            {
                case 2:
                    obj = GameObject.Instantiate(Resources.Load("sea")) as GameObject;
                    ent = new GroundEntity(0, obj);
                    break;
                case 6:
                default:
                    obj = GameObject.Instantiate(Resources.Load("ground")) as GameObject;
                    ent = new GroundEntity(1, obj);
                    break;
                case 7:
                    obj = GameObject.Instantiate(Resources.Load("grass")) as GameObject;
                    ent = new GroundEntity(2, obj);
                    break;
            }

            var mapNode = GameObject.FindGameObjectWithTag("Map");
            obj.transform.parent = mapNode.transform;
            obj.transform.position = new Vector3(tilesize * i, -tilesize * j, 0);
            mapMatrix[i, j] = ent;
            populationMatrix[i, j] = new List<PopulationEntity>();

            i++;
            if (i == mapsize)
            {
                i = 0;
                j++;
            }
        }
        Debug.Log("map loaded");

    }

    private PopulationEntity addEntity(int x, int y, int id)
    {
        var entitiesNode = GameObject.FindGameObjectWithTag("Entities");
        var pos = new Position(x, y);

        var prefab = "";
        switch (id)
        {
            case 1:
                prefab = "pop1";
                break;
            case 2:
                prefab = "pop2";
                break;
            case 3:
                prefab = "pop3";
                break;
            default:
                prefab = "pop1";
                break;

        }

        var popObj = GameObject.Instantiate(Resources.Load(prefab)) as GameObject;
        popObj.transform.position = new Vector3(pos.X * tilesize, -pos.Y * tilesize, 0);
        popObj.transform.parent = entitiesNode.transform;
        var pop = new PopulationEntity(id, popObj, pos.X, pos.Y);
        populationMatrix[pos.X, pos.Y].Add(pop);

        var popLevelObj = GameObject.Instantiate(Resources.Load("popLevel")) as GameObject;
        popLevelObj.transform.position = new Vector3(pos.X * tilesize, -pos.Y * tilesize, -1);
        popLevelObj.transform.parent = entitiesNode.transform;
        pop.LevelObject = popLevelObj;

        return pop;
    }

    private void moveEntity(PopulationEntity pop, Direction dir)
    {
        Position p = PositionUtils.GetDirPosition(dir, true);
        Position newPos = new Position(pop.Pos.X + p.X, pop.Pos.Y + p.Y);
        newPos.NormalizePos(mapsize);

        populationMatrix[pop.Pos.X, pop.Pos.Y].Remove(pop);
        pop.Pos = newPos;
        pop.Object.transform.position = new Vector3(newPos.X * tilesize, -newPos.Y * tilesize, 0);
        pop.LevelObject.transform.position = new Vector3(newPos.X * tilesize, -newPos.Y * tilesize, -1);
        populationMatrix[newPos.X, newPos.Y].Add(pop);

    }

    private bool ageEntity(PopulationEntity pop, int value)
    {
        bool death = false;
        pop.Age += value;

        if (pop.NoRepTime > 0)
        {
            pop.NoRepTime -= value;
        }

        /*int minLevel = pop.Level - fightLevelDiff;
        if (minLevel < 0) minLevel = 0;
        int zoneLevel = findZoneLevel(pop.Pos.X, pop.Pos.Y, minLevel);
        zoneLevel -= pop.Level - minLevel;*/

        if (Random.value < pop.Xp * xplevelUpChance)
        {
            pop.Level++;
            pop.Xp = 0;
            var scalePerLevel = 0.01f;
            pop.LevelObject.transform.localScale = new Vector3(0.5f + pop.Level * scalePerLevel, 0.5f + pop.Level * scalePerLevel, 1);
        }


        if (pop.Age > ageLimit)
        {
            if (Random.value < ageDeathChance)
            {
                populationMatrix[pop.Pos.X, pop.Pos.Y].Remove(pop);
                Destroy(pop.Object);
                Destroy(pop.LevelObject);
                death = true;
            }
        }

        return death;
    }

    private int movePhase()
    {
        int totalCount = 0;
        var tmpList = new List<PopulationEntity>();

        for (int i = 0; i < mapsize; i++)
        {
            for (int j = 0; j < mapsize; j++)
            {
                if (populationMatrix[i, j].Count > 0)
                {
                    foreach (PopulationEntity p in populationMatrix[i, j])
                    {
                        totalCount++;
                        if (Random.value < moveChance - p.Level * moveChanceMalusPerLevel)
                        {
                            tmpList.Add(p);

                        }
                    }
                }
            }
        }


        foreach (PopulationEntity p in tmpList)
        {
            moveEntity(p, DirectionUtils.RandomDirection());
        }

        return totalCount;
    }

    private int reproductionPhase()
    {
        int repCount = 0;
        var tmpList = new List<PopulationEntity>();

        for (int i = 0; i < mapsize; i++)
        {
            for (int j = 0; j < mapsize; j++)
            {
                if (populationMatrix[i, j].Count > 0)
                {

                    foreach (PopulationEntity p in populationMatrix[i, j])
                    {
                        var repChance = repChanceMatrix[mapMatrix[i, j].Id, p.Id - 1];

                        if (p.Sex == "F" && p.Age >= repAge && p.NoRepTime == 0)
                        {
                            //todo renvoyer plus d'infos sur les males
                            float maleFactor = findMaleFactor(p.Id, i, j);

                            if (Random.value < repChance * maleFactor)
                            {
                                tmpList.Add(p);//todo mémoriser un couple M/F
                            }
                        }
                    }
                }
            }
        }

        foreach (PopulationEntity p in tmpList)
        {
            addEntity(p.Pos.X, p.Pos.Y, p.Id);
            repCount++;
        }

        return repCount;
    }

    private int agePhase()
    {
        int deathCount = 0;
        var tmpList = new List<PopulationEntity>();

        for (int i = 0; i < mapsize; i++)
        {
            for (int j = 0; j < mapsize; j++)
            {
                if (populationMatrix[i, j].Count > 0)
                {
                    foreach (PopulationEntity p in populationMatrix[i, j])
                    {
                        tmpList.Add(p);
                    }
                }
            }
        }

        foreach (PopulationEntity p in tmpList)
        {
            var death = ageEntity(p, ageValue);
            if (death) deathCount++;
        }

        return deathCount;
    }

    private int fightPhase()
    {
        int fightCount = 0;

        for (int i = 0; i < mapsize; i++)
        {
            for (int j = 0; j < mapsize; j++)
            {
                if (populationMatrix[i, j].Count > 0)
                {
                    foreach (PopulationEntity p in populationMatrix[i, j])
                    {
                        if (p.Age > fightAge)
                        {
                            var poplist = findPopList(i, j, fightArea);
                            foreach (PopulationEntity p2 in poplist)
                            {
                                if (p2.Age > fightAge)
                                {
                                    if (Random.value < fightChance / (1 + (p.NoRepTime + p2.NoRepTime) * noRepFightChanceMalusFactor))
                                    {
                                        fightCount++;
                                        if (Mathf.Abs(p2.Level - p.Level) > fightLevelDiff)
                                        {
                                            p.Xp += 1;
                                            p2.Xp += 1;
                                            if (p2.Level > p.Level)
                                            {
                                                p.NoRepTime += fightNoRepTime;
                                                //if (p2.NoRepTime < fightNoRepTime) p2.NoRepTime = fightNoRepTime;
                                                //p2.Level += fightLevelValue;
                                            }
                                            else
                                            {
                                                p2.NoRepTime += fightNoRepTime;
                                                //if (p.NoRepTime < fightNoRepTime) p.NoRepTime = fightNoRepTime;
                                                //p.Level += fightLevelValue;
                                            }
                                        }
                                        else
                                        {
                                            if (Random.value > 0.5f)
                                            {
                                                p.NoRepTime += fightNoRepTime;
                                                p2.Xp += 1;
                                                //if (p2.NoRepTime < fightNoRepTime) p2.NoRepTime = fightNoRepTime;
                                                //p2.Level += fightLevelValue;
                                            }
                                            else
                                            {
                                                p2.NoRepTime += fightNoRepTime;
                                                p.Xp += 1;
                                                //if (p.NoRepTime < fightNoRepTime) p.NoRepTime = fightNoRepTime;
                                                //p.Level += fightLevelValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return fightCount;
    }

    private int findZoneLevel(int x, int y, int diff)
    {
        int zoneLevel = 0;

        var poplist = findPopList(x, y, xpArea);

        foreach (PopulationEntity p in poplist)
        {
            var inc = p.Level - diff;
            if (inc < 0) inc = 0;
            zoneLevel += inc;
        }

        return zoneLevel;
    }

    private float findMaleFactor(int id, int x, int y)
    {
        float maleFactor = 0;

        var poplist = findPopList(x, y, repArea);

        foreach (PopulationEntity p in poplist)
        {
            if (p.Sex == "M" && p.Id == id && p.Age >= repAge)
            {
                maleFactor += 1 / (1 + p.NoRepTime);
            }
        }

        return maleFactor;
    }

    private List<PopulationEntity> findPopList(int x, int y, int area)
    {
        var result = new List<PopulationEntity>();

        for (int i = 0; i < area; i++)
        {
            for (int j = 0; j < area; j++)
            {
                var tmpPos = new Position(i + x - area / 2, j + y - area / 2);
                tmpPos.NormalizePos(mapsize);

                if (populationMatrix[tmpPos.X, tmpPos.Y].Count > 0)
                {
                    foreach (PopulationEntity p in populationMatrix[tmpPos.X, tmpPos.Y])
                    {
                        result.Add(p);
                    }
                }
            }
        }

        return result;
    }

}
