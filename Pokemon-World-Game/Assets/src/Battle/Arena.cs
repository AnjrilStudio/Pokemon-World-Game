﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Arena
{
    public float Tilesize { get; private set; }
    public int Mapsize { get; private set; }
    public GameObject[,] Tiles;

    public Arena (float tilesize, int mapsize)
    {
        Tilesize = tilesize;
        Mapsize = mapsize;

        Tiles = new GameObject[mapsize, mapsize];
        for (int i = 0; i < mapsize; i++)
        {
            for (int j = 0; j < mapsize; j++)
            {
                var obj = GameObject.Instantiate(Resources.Load("ground")) as GameObject;
                var mapNode = GameObject.FindGameObjectWithTag("Arena");
                obj.transform.parent = mapNode.transform;
                obj.transform.position = new Vector3(tilesize * i, -tilesize * j, 0);
                Tiles[i, j] = obj;
            }
        }
    }
}