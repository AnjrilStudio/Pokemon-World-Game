using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Anjril.PokemonWorld.Common.State;
using System;

public class OverlayTool
{
    private ChunkMatrix<Int32> _groundMatrix;
    private ChunkMatrix<List<GameObject>> _mapOverlayMatrix;
    private GameObject _parentNode;

    private float _tilesize;
    private float _tileZLayerFactor;

    public OverlayTool(ChunkMatrix<Int32> groundMatrix, ChunkMatrix<List<GameObject>> mapOverlayMatrix, GameObject parentNode, float tilesize, float tileZLayerFactor)
    {
        _groundMatrix = groundMatrix;
        _mapOverlayMatrix = mapOverlayMatrix;
        _parentNode = parentNode;

        _tilesize = tilesize;
        _tileZLayerFactor = tileZLayerFactor;
    }


    public void AddMapTileOverlay(Position origin, int sizeX, int sizeY)
    {
        for (int i = origin.X; i < origin.X + sizeX; i++)
        {
            for (int j = origin.Y; j < origin.Y + sizeY; j++)
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
            }
        }
    }

    public void AddArenaTileOverlay(Position origin, int sizeX, int sizeY)
    {
        for (int i = origin.X; i < origin.X + sizeX; i++)
        {
            for (int j = origin.Y; j < origin.Y + sizeY; j++)
            {
                addBorderOverlay(i, j, 1, 2, "grass", 0);
                addBorderOverlay(i, j, 1, 3, "grass", 0);
                addBorderOverlay(i, j, 1, 4, "grass", 0);

                addBorderOverlay(i, j, 4, 3, "sand", 0);

                addBorderOverlay(i, j, 2, 4, "ground", 0);

                addBorderOverlay(i, j, 2, 3, "groundcliff", 1);
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

        if (_groundMatrix[tmppos.X, tmppos.Y] == fromTileType)
        {
            bool right = false;
            bool left = false;
            bool bottom = false;
            bool top = false;

            if (toprightpos != null && _groundMatrix[toprightpos.X, toprightpos.Y] == fromTileType)
            {
                if (rightpos != null && _groundMatrix[rightpos.X, rightpos.Y] == toTileType)
                {
                    instantiateOverlay(i + 1, j, prefabName + "/" + prefabName + "_topleft", layer * 0.1f);
                    right = true;
                }
                if (toppos != null && _groundMatrix[toppos.X, toppos.Y] == toTileType)
                {
                    instantiateOverlay(i, j - 1, prefabName + "/" + prefabName + "_bottomright", layer * 0.1f);
                    top = true;
                }
            }

            if (topleftpos != null && _groundMatrix[topleftpos.X, topleftpos.Y] == fromTileType)
            {
                if (leftpos != null && _groundMatrix[leftpos.X, leftpos.Y] == toTileType)
                {
                    instantiateOverlay(i - 1, j, prefabName + "/" + prefabName + "_topright", layer * 0.1f);
                    left = true;
                }
                if (toppos != null && _groundMatrix[toppos.X, toppos.Y] == toTileType)
                {
                    instantiateOverlay(i, j - 1, prefabName + "/" + prefabName + "_bottomleft", layer * 0.1f);
                    top = true;
                }
            }

            if (botleftpos != null && _groundMatrix[botleftpos.X, botleftpos.Y] == fromTileType)
            {
                if (leftpos != null && _groundMatrix[leftpos.X, leftpos.Y] == toTileType)
                {
                    instantiateOverlay(i - 1, j, prefabName + "/" + prefabName + "_bottomright", layer * 0.1f);
                    left = true;
                }
                if (botpos != null && _groundMatrix[botpos.X, botpos.Y] == toTileType)
                {
                    instantiateOverlay(i, j + 1, prefabName + "/" + prefabName + "_topleft", layer * 0.1f);
                    bottom = true;
                }
            }

            if (botrightpos != null && _groundMatrix[botrightpos.X, botrightpos.Y] == fromTileType)
            {
                if (rightpos != null && _groundMatrix[rightpos.X, rightpos.Y] == toTileType)
                {
                    instantiateOverlay(i + 1, j, prefabName + "/" + prefabName + "_bottomleft", layer * 0.1f);
                    right = true;
                }
                if (botpos != null && _groundMatrix[botpos.X, botpos.Y] == toTileType)
                {
                    instantiateOverlay(i, j + 1, prefabName + "/" + prefabName + "_topright", layer * 0.1f);
                    bottom = true;
                }
            }


            if (rightpos != null && _groundMatrix[rightpos.X, rightpos.Y] == toTileType && !right)
            {
                instantiateOverlay(i + 1, j, prefabName + "/" + prefabName + "_left", layer * 0.1f);
                right = true;
            }
            if (toppos != null && _groundMatrix[toppos.X, toppos.Y] == toTileType && !top)
            {
                instantiateOverlay(i, j - 1, prefabName + "/" + prefabName + "_bottom", layer * 0.1f);
                top = true;
            }
            if (botpos != null && _groundMatrix[botpos.X, botpos.Y] == toTileType && !bottom)
            {
                instantiateOverlay(i, j + 1, prefabName + "/" + prefabName + "_top", layer * 0.1f);
                bottom = true;
            }
            if (leftpos != null && _groundMatrix[leftpos.X, leftpos.Y] == toTileType && !left)
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
        
    }

    private void instantiateOverlay(int i, int j, string prefab, float z)
    {
        var overlay = GameObject.Instantiate(Resources.Load(prefab)) as GameObject;
        overlay.transform.parent = _parentNode.transform;
        overlay.transform.position = new Vector3(_tilesize * i, -_tilesize * j, -0.5f - z - j * _tileZLayerFactor);
        overlay.SetActive(true);
        _mapOverlayMatrix[i, j].Add(overlay);
    }
}
