using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;

public class MapTile {
    public int level;
    public float raw_height;
    public float tile_height;
    public int[] mask;
    public bool isRidge;
    public bool[] merged;
    public bool isFlat;

    public MapTile(int max_level) {
        mask = new int[max_level];
        merged = new bool[max_level];
    }
}

public static class TilemapManager {

    //===========
    // Sprites
    //============
    public static Sprite[] all_sprites;

    //=====================
    // Rendering Variables
    //=====================
    public static GameObject parent_obj;
    public static Vector2Int current_map_center;
    public static Vector2Int top_left;

    //==============
    // Constants
    //==============
    public const int MAX_LEVEL = 5;
    public const float LEVEL_HEIGHT = 3f;             // height between tilemaps/ height of colliders
    public const int CHUNK_WIDTH = 100;
    public const int CHUNK_HEIGHT = 100;
    public const int MAP_UPDATE_DISTANCE = 50;

    //==============
    // Map
    //==============
    public static MapTile[,] map;

    //==============
    // Tilemaps
    //==============
    private static Tilemap tilemap0;
    private static Tilemap tilemap1;
    private static Tilemap tilemap2;
    private static Tilemap tilemap3;
    private static Tilemap tilemap4;
    public static Tilemap[] tilemaps;

    //==============
    // Initialize
    //==============
    public static void initialize() {

        parent_obj = new GameObject();
        parent_obj.name = "TilemapManager";

        // sprites
        // -------
        all_sprites = Resources.LoadAll<Sprite>("Textures/t");
        if (all_sprites == null)
            Debug.Log("TilemapManager Resources.LoadAll Error: all_sprites null");

        // tilemaps
        // --------
        tilemap0 = GameObject.Find("tilemap0").GetComponent<Tilemap>();
        tilemap1 = GameObject.Find("tilemap1").GetComponent<Tilemap>();
        tilemap2 = GameObject.Find("tilemap2").GetComponent<Tilemap>();
        tilemap3 = GameObject.Find("tilemap3").GetComponent<Tilemap>();
        tilemap4 = GameObject.Find("tilemap4").GetComponent<Tilemap>();
        tilemaps = new Tilemap[] { tilemap0, tilemap1, tilemap2, tilemap3, tilemap4 };

        // raise tilemaps z placement
        // --------------------------
        int i = 0;
        foreach (Tilemap t in tilemaps)
        {
            t.gameObject.transform.position = new Vector3(0, 0, -i * LEVEL_HEIGHT);
            i++;
        }

        // Logic Maps
        // ----------
        map = new MapTile[CHUNK_WIDTH, CHUNK_HEIGHT];
        for (int y = 0; y < map.GetLength(1); y++) {
            for (int x = 0; x < map.GetLength(0); x++) {
                map[x, y] = new MapTile(MAX_LEVEL);
            }
        }

        // Initialize WorldEngines
        // -----------------------
        Terrain.initialize();
        Biomes.initialize();
        Plants.initialize();
    }

    //==============
    // Load
    //==============
    public static void load() {
        // call load functions of WorldEngines
        Terrain.load();
        Biomes.load();
        Plants.load();
    }

    //==============
    // Reload
    //==============
    public static void reload(Vector2Int c) {
        // update map center
        current_map_center = c;

        // clear tilemaps' tiles
        foreach(Tilemap t in tilemaps) {
            t.ClearAllTiles();
        }

        // reload other WorldEngines
        Terrain.reload();
        Biomes.reload();
        Plants.reload();

        // render once done
        render();
    }

    //==============
    // Render
    //==============
    public static void render() {
        // assign values to the TileMap
        top_left = new Vector2Int(current_map_center.x - CHUNK_WIDTH / 2, current_map_center.y - CHUNK_HEIGHT / 2);
        int i = 0;
        int j = 0;

        for (int y = top_left.y; y < top_left.y + CHUNK_HEIGHT; y++) {
            i = 0;
            for (int x = top_left.x; x < top_left.x + CHUNK_WIDTH; x++) {
                for (int z = 0; z < MAX_LEVEL; z++) {
                    if (map[i, j].level >= z) {
                        tilemaps[z].SetTile(new Vector3Int(x, y, 0), Terrain.terrain_tile);
                    }
                }
                i++;
            }
            j++;
        }
    }

    //==============
    // Hide
    //==============
    public static void hide() {
        //
    }

    //==============
    // Helpers
    //==============
    public static void checkPlayerPosition(Vector2Int new_position) {
        // if (L1(current_map_center, new_position) >= MAP_UPDATE_DISTANCE) {
        //     Debug.Log("here");
        //     reload(new_position);
        // }
    }
}
