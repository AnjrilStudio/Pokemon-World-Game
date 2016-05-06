using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class PopulationEntity
{
    public int Id { get; private set; }
    public GameObject Object { get; private set; }
    public GameObject LevelObject { get; set; }
    public Position Pos { get; set; }
    public int Age { get; set; }
    public string Sex { get; private set; }
    public int Level { get; set; }
    public int NoRepTime { get; set; }
    public int Xp { get; set; }

    public PopulationEntity(int id, GameObject obj, int X, int Y)
    {
        Id = id;
        Object = obj;
        Pos = new Position(X, Y);
        Level = 1;
        Age = 0;
        Sex = UnityEngine.Random.value > 0.5f ? "M" : "F";
        NoRepTime = 0;
        Xp = 0;
    }
}
