using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Tilemap))]
public class TilemapManager : MonoBehaviour
{
    Tilemap tilemap;
    Grid grid;
    [SerializeField] TileBase tileBase;


    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        grid = GetComponent<Grid>();
        grid.Init(30, 30);
        grid.Set(1, 1, true);
        //UpdateTileMap();

    }

    // void UpdateTileMap()
    // {
    //     for (int x = 0; x < grid.length; x++)
    //     {
    //         for (int z = 0; z < grid.height; z++)
    //         {
                
    //             TilemapManager.SetTile(new Vector3Int(x, 0, z), tileBase);
    //         }
    //     }
    // }
}
