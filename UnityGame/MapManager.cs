using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField]
    private int height;
    [SerializeField]
    private int width;
    [SerializeField]
    private Interactable[,] interactableObjects;

    [Header("Tilemaps")]
    [SerializeField]
    private Tilemap groundMap;
    [SerializeField]
    private Tilemap vegetationMap;
    [SerializeField]
    private Tilemap interactablesMap;

    [Header("Ground Settings")]
    [SerializeField]
    private int dirtSpawnPercentage;
    [SerializeField]
    private int transitionSteps;
    [SerializeField]
    private int dirtCellThreshold;
    [SerializeField]
    private int stoneCellThreshold;
    [SerializeField]
    private TileDatas dirtTiles;
    [SerializeField]
    private TileDatas stoneTiles;
    [SerializeField]
    private TileDatas vegetationTiles;
    [SerializeField]
    private int vegetationSpawnRate;


    [SerializeField]
    private TileBase water;
    [SerializeField]
    private int turnRate;

    [Header("Interactable Tiles")]
    [SerializeField]
    private TileBase mountainTiles;

    [Header("Highlight Colors")]
    [SerializeField]
    private Color normalColor;
    [SerializeField]
    private Color highlightColor;

    public void GenerateMap()
    {
        interactableObjects = new Interactable[height, width];

        //Generate TileMap Portion
        GenerateGround();
        //GenerateWater();
        GenerateVegetation();

        //Generate Interactable Objects
        GenerateMountains();
    }

    private void GenerateGround()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                groundMap.SetTile(new Vector3Int(x, y, 0), Random.Range(0, 100) < dirtSpawnPercentage ? dirtTiles.tiles[Random.Range(0, dirtTiles.tiles.Count)]: stoneTiles.tiles[Random.Range(0, stoneTiles.tiles.Count)]);
            }
        }
        SmoothGroundGeneration();
    }

    private void SmoothGroundGeneration()
    {
        for (int t = 0; t < transitionSteps; t++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileBase targetTile = groundMap.GetTile(new Vector3Int(x, y, 0));
                    List<TileBase> targetList = dirtTiles.tiles.Contains(targetTile) ? dirtTiles.tiles : stoneTiles.tiles;
                    int totalSimilarCells = CheckMooreCells(x, y, groundMap, targetList);
                    ReplaceTile(totalSimilarCells, x, y, targetTile, groundMap);
                }
            }
        }
    }

    //TODO: Generate water
    private void GenerateWater() { }

    private (int, int, int[]) DetermineRiverStartLocation(int endSide)
    {
        int endX = 0, endY = 0;
        int[] direction = {0, 0};
        switch (endSide)
        {
            case 0:
                endY = Random.Range(0, 100);
                direction[0] = 1;
                break;
            case 1:
                endX = Random.Range(0, 100);
                direction[1] = 1;
                break;
            case 2:
                endX = 99;
                endY = Random.Range(0, 100);
                direction[0] = -1;
                break;
            case 3:
                endX = Random.Range(0, 100);
                endY = 99;
                direction[1] = -11;
                break;
            default:
                break;
        }
        return (endX, endY, direction);
    }

    private void GenerateMountains()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase targetTile = groundMap.GetTile(new Vector3Int(x, y, 0));
                int totalSimilarCells = stoneTiles.tiles.Contains(targetTile) ? CheckMooreCells(x, y, groundMap, stoneTiles.tiles): -1;
                if (totalSimilarCells == 8)
                {
                    interactablesMap.SetTile(new Vector3Int(x, y, 0), mountainTiles);
                    interactableObjects[x, y] = new Stone(x, y);
                }
            }
        }
    }

    private void GenerateVegetation()
    {
        vegetationMap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase targetTile = groundMap.GetTile(new Vector3Int(x, y, 0));
                if (dirtTiles.tiles.Contains(targetTile) && Random.Range(0, 100) < vegetationSpawnRate) vegetationMap.SetTile(new Vector3Int(x, y, 0), vegetationTiles.tiles[Random.Range(0, vegetationTiles.tiles.Count)]); 
            }
        }
    }

    private int CheckMooreCells(int x, int y, Tilemap tileMap, List<TileBase> targetList)
    {
        int totalSimilarTiles = 0;
        for(int i = -1; i < 2; i++)
        {
            for(int j = -1; j < 2; j++)
            {
                if(i == 0 && j == 0){}
                else
                {
                    TileBase currentTile = tileMap.GetTile(new Vector3Int(x + i, y + j, 0));
                    if (targetList.Contains(currentTile) || currentTile == null) totalSimilarTiles++;
                }
            }
        }
        return totalSimilarTiles;
    }

    private void ReplaceTile(int totalSimilarTiles, int x, int y, TileBase targetTile, Tilemap tileMap)
    {
        if (dirtTiles.tiles.Contains(targetTile))
        {
            if(totalSimilarTiles < dirtCellThreshold)
            {
                tileMap.SetTile(new Vector3Int(x, y, 0), stoneTiles.tiles[Random.Range(0, stoneTiles.tiles.Count)]);
            }
        }
        else if (stoneTiles.tiles.Contains(targetTile))
        {
            if (totalSimilarTiles < stoneCellThreshold)
            {
                tileMap.SetTile(new Vector3Int(x, y, 0), dirtTiles.tiles[Random.Range(0, dirtTiles.tiles.Count)]);
            }
        }
    }

    public bool IsDirtTile(int x, int y)
    {
        return (dirtTiles.tiles.Contains(groundMap.GetTile(new Vector3Int(x, y, 0))));
    }

    public bool InteractableAt(int x, int y)
    {
        return interactableObjects[x, y] != null;
    }

    public Interactable GetInteractable(int x, int y)
    {
        return interactableObjects[x, y];
    }

    public void RemoveInteractable(int x, int y)
    {
        TileBase droppedTile = null;
        Interactable droppedItemInteractable = null;
        if (interactableObjects[x, y].dropsItem)
        {
            interactableObjects[x, y].DropItem();
            droppedTile = interactableObjects[x, y].droppedItemTile;
            droppedItemInteractable = interactableObjects[x, y].droppedItemInteractable;

        }
        interactableObjects[x, y].OnRemoval();
        interactablesMap.SetTile(new Vector3Int(x, y, 0), droppedTile);
        interactableObjects[x, y] = droppedItemInteractable;
    }

    public void HighlightObject(int x, int y, bool active)
    {
        Interactable interactable = interactableObjects[x, y];
        if(interactable != null)
        {
            //Debug.Log("Interactable");
            if (active)
            {
                //Debug.Log("Changing color to " + highlightColor);
                interactablesMap.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
                interactablesMap.SetColor(new Vector3Int(x, y, 0), highlightColor);
                //Debug.Log(interactablesMap.GetColor(new Vector3Int(x, y, 0)));
            }
            else
            {
                //Debug.Log("Changing color to " + normalColor);
                interactablesMap.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
                interactablesMap.SetColor(new Vector3Int(x, y, 0), normalColor);
            }
        }
    }

    public int GetHeight()
    {
        return height;
    }

    public int GetWidth()
    {
        return width;
    }
}
