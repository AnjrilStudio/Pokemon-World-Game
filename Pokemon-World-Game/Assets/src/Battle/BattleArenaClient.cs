using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class BattleArenaClient : Arena
{
    public float Tilesize { get; private set; }
    public GameObject[,] Tiles;
    public ChunkMatrix<Int32> TilesInt;
    public ChunkMatrix<List<GameObject>> OverlayMatrix;
    public ChunkMatrix<List<String>> OverlayNameMatrix;

    private float tileZLayerFactor = 0.10f;

    public BattleArenaClient (int size, float tilesize) : base(size)
    {
        Tilesize = tilesize;
        TilesInt = new ChunkMatrix<int>(100);
        OverlayMatrix = new ChunkMatrix<List<GameObject>>(100);
        OverlayNameMatrix = new ChunkMatrix<List<String>>(100);

        init();
    }

    public void init()
    {
        Tiles = new GameObject[Width, Height];
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                var obj = GameObject.Instantiate(Resources.Load("ground/ground")) as GameObject;
                var mapNode = GameObject.FindGameObjectWithTag("Arena");
                obj.transform.parent = mapNode.transform;
                obj.transform.position = new Vector3(Tilesize * i, -Tilesize * j, 0);
                Tiles[i, j] = obj;
                TilesInt[i, j] = 1;
                OverlayMatrix[i, j] = new List<GameObject>();
            }
        }
    }

    public void update(ArenaTile[,] tiles)
    {
        var parentNode = GameObject.FindGameObjectWithTag("Arena");

        Debug.Log("update arena");
        foreach(GameObject obj in Tiles)
        {
            GameObject.Destroy(obj);
        }

        Width = tiles.GetLength(0);
        Height = tiles.GetLength(1);
        Debug.Log("width " + Width);
        Debug.Log("height " + Height);
        Tiles = new GameObject[Width, Height];
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                GameObject obj;
                switch (tiles[i, j])
                {
                    case ArenaTile.Ground:
                        obj = GameObject.Instantiate(Resources.Load("ground/ground")) as GameObject;
                        break;
                    case ArenaTile.Grass:
                        obj = GameObject.Instantiate(Resources.Load("grass/grass")) as GameObject;
                        break;
                    case ArenaTile.Water:
                        obj = GameObject.Instantiate(Resources.Load("sea")) as GameObject;
                        break;
                    case ArenaTile.Sand:
                        obj = GameObject.Instantiate(Resources.Load("sand/sand")) as GameObject;
                        break;
                    default:
                        obj = GameObject.Instantiate(Resources.Load("ground/ground")) as GameObject;
                        break;
                }
                obj.transform.parent = parentNode.transform;
                obj.transform.position = new Vector3(Tilesize * i, -Tilesize * j, 0);
                Tiles[i, j] = obj;
                TilesInt[i, j] = (int)tiles[i,j];
                OverlayMatrix[i, j] = new List<GameObject>();
            }
        }

        var overlayTool = new OverlayTool(TilesInt, OverlayMatrix, OverlayNameMatrix, parentNode, Tilesize, tileZLayerFactor);
        overlayTool.AddArenaTileOverlay(new Position(0,0), Width, Height);
    }
}
