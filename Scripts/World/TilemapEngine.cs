using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using static Utils;

public static class TilemapEngine
{
    //=====================
    // Rendering Variables
    //=====================

    private static GameObject parent_obj;
    private static Vector2Int current_map_center;
    public static Vector2Int top_left;

    //==============
    // Constants
    //==============

    private const int MAX_HEIGHT = 5;
    private const float HEIGHT_DELTA = 3f;            //height_delta to change heights between tilemaps and height of colliders
    private const int CHUNK_WIDTH = 360;              //1440;//720;//360;//180;//90//45;
    private const int CHUNK_HEIGHT = 200;             //800;//400;//200;//100;//50//25;
    private const int MAP_UPDATE_DISTANCE = 1000;
    private const float HEIGHT_DIFF = 0.3f;           //0.3f; default //height difference used to calc. heightmap values from noise //normally 0.3f //0.45f for 5x5
    private const float H_NOISE_FREQUENCY = 0.25f;    //frequency of height noise, usually 0.1f

    //==============
    // Logic Maps
    //==============

    public static int[,,] map;
    private static float[,] height_map;
    public static int[,,] mask_map;
    private static int max_x;
    private static int max_y;

    //==============
    // Tilemaps
    //==============

    private static Tilemap tilemap0;
    private static Tilemap tilemap1;
    private static Tilemap tilemap2;
    private static Tilemap tilemap3;
    private static Tilemap tilemap4;
    private static Tilemap[] tilemaps;

    //==================
    // Scriptable Tiles
    //==================

    private static TerrainTile terrain_tile;

    //===========
    // Sprites
    //============

    public static Sprite[] all_sprites;

    //=============
    // Physics
    //=============

    public static List<BoxCollider> colliders = new List<BoxCollider>();
    public static Dictionary<int, float[]> collider_types = new Dictionary<int, float[]>();
    public static Mesh ridge_mesh = new Mesh();

    //==============
    // Initialize
    //==============

    public static void initialize()
    {
        parent_obj = new GameObject();
        parent_obj.name = "TilemapEngine";

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
            t.gameObject.transform.position = new Vector3(0, 0, -i * HEIGHT_DELTA);
            i++;
        }

        // sprites
        // -------
        all_sprites = Resources.LoadAll<Sprite>("Textures/t");
        if (all_sprites == null)
        {
            Debug.Log("TilemapEngine Resources.LoadAll Error: all_sprites null");
        }

        // Logic Maps
        // ----------
        map = new int[CHUNK_WIDTH, CHUNK_HEIGHT, MAX_HEIGHT];
        height_map = new float[CHUNK_WIDTH, CHUNK_HEIGHT];
        mask_map = new int[CHUNK_WIDTH, CHUNK_HEIGHT, MAX_HEIGHT];

        // custom tiles
        // ------------
        terrain_tile = Resources.Load<TerrainTile>("Tiles/TerrainTile");

        // physics colliders
        // -----------------

        // collider types according to mask in order to account for non uniform box shapes
        // collider_type[index] = new float[] { x_init, y_init, x_size, y_size };

        collider_types[14] = new float[] { 0f, 0f, 1, 0.1f };         // top flat tile
        collider_types[6] = new float[] { 0.87f , 0f, 0.13f, 0.1f };  // top left corner
        collider_types[12] = new float[] { 0f, 0f, 0.13f, 0.1f };     // top right corner
        collider_types[7] = new float[] { 0.87f, 0f, 0.13f, 1f };     // left outer flat wall
        collider_types[3] = new float[] { 0.87f, 0f, 0.13f, 1f };     // left outer corner wall
        collider_types[13] = new float[] { 0f, 0f, 0.13f, 1f };       // right outer wall
        collider_types[9] = new float[] { 0f, 0f, 0.13f, 1f };        //right outer corner wall

        // create the ridge_mesh
        Vector3[] verts = new Vector3[]
        {
            new Vector3(),
        };

        int[] tris = new int[]
        {

        };

        ridge_mesh.vertices = verts;
        ridge_mesh.triangles = tris;
    }

    //==============
    // Load
    //==============

    public static void load()
    {
        // Overall function is to create the maps procedurally
        // Seperately save a file containing all of the changes to a region and read from here to
        // update the graphics according to player save.
        loadGraphics();
        loadPhysics();
    }

    public static void loadPhysics()
    {
        int[,,] merge_map = new int[CHUNK_WIDTH, CHUNK_HEIGHT, MAX_HEIGHT];
        top_left = new Vector2Int(current_map_center.x - CHUNK_WIDTH / 2, current_map_center.y - CHUNK_HEIGHT / 2);
        int i = 0, j = 0, _z = 0;
        GameObject height_obj;
        GameObject ridge_obj;

        // create ridge colliders

        for (_z = 0; _z < MAX_HEIGHT; _z++)
        {
            j = 0;
            ridge_obj = new GameObject("RidgeGround" + _z);
            ridge_obj.transform.parent = parent_obj.transform;
            ridge_obj.layer = 10;
            for (int y = top_left.y; y < top_left.y + CHUNK_HEIGHT; y++)
            {
                i = 0;
                for (int x = top_left.x; x < top_left.x + CHUNK_WIDTH; x++)
                {
                    createRidgeCollider(i, j, x, y, _z, merge_map, ridge_obj);
                    i++;
                }
                j++;
            }
        }

        // create merged colliders

        for (_z = 0; _z < MAX_HEIGHT; _z++)
        {
            j = 0;
            height_obj = new GameObject("Ground" + _z);
            height_obj.transform.parent = parent_obj.transform;
            height_obj.layer = 9;
            for (int y = top_left.y; y < top_left.y + CHUNK_HEIGHT; y++)
            {
                i = 0;
                for (int x = top_left.x; x < top_left.x + CHUNK_WIDTH; x++)
                {
                    if (map[i, j, _z] == 1 && merge_map[i, j, _z] == 0)
                    {
                        createMergedCollider(top_left, x, y, i, j, _z, merge_map, height_obj);
                    }
                    i++;
                }
                j++;
            }
        }
    }

    public static void loadGraphics()
    {
        // find height_map -> map -> cleanMap() -> mask_map and map

        int _z = 0;
        float h;
        // assign values to the height_map[x, y] and then to map[x, y, z] using height from noise

        for (int y = 0; y < height_map.GetLength(1); y++)
        {
            for (int x = 0; x < height_map.GetLength(0); x++)
            {
                h = noise(x, y, H_NOISE_FREQUENCY);
                height_map[x, y] = h;
                for (_z = 0; _z < MAX_HEIGHT; _z++)
                {
                    map[x, y, _z] = (h > HEIGHT_DIFF * (_z * 0.5f + 1)) ? 1 : 0;
                }
            }
        }

        // clean values in the map

        List<int> illegal_values = new List<int> { 0, 1, 2, 4, 5, 8, 10 };
        max_x = map.GetLength(0);
        max_y = map.GetLength(1);
        int mask;
        for (int y = 0; y < max_y; y++)
        {
            for (int x = 0; x < max_x; x++)
            {
                _z = 0;
                while (_z < MAX_HEIGHT)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        // remove illegal values

                        mask = y + 1 < max_y && map[x, y + 1, _z] == 1 ? 1 : 0; //top
                        mask += x + 1 < max_x && map[x + 1, y, _z] == 1 ? 2 : 0; //right
                        mask += y - 1 >= 0 && map[x, y - 1, _z] == 1 ? 4 : 0; //bottom
                        mask += x - 1 >= 0 && map[x - 1, y, _z] == 1 ? 8 : 0; //left

                        if (illegal_values.Contains(mask))
                        {
                            map[x, y, _z] = 0;
                        }

                        // check 110 at x = 0, 0110 every where, 011 at x - 3 == max_x horizontal

                        if ((x == 0 && map[x, y, _z] == 1 && map[x + 1, y, _z] == 1
                            && map[x + 2, y, _z] == 0) || (x + 3 == max_x
                            && map[x, y, _z] == 0 && map[x + 1, y, _z] == 1
                            && map[x + 2, y, _z] == 1) || (x + 3 < max_x
                            && map[x, y, _z] == 0 && map[x + 1, y, _z] == 1
                            && map[x + 2, y, _z] == 1 && map[x + 3, y, _z] == 0))
                        {
                            map[x, y, _z] = 0;
                            map[x + 1, y, _z] = 0;
                            map[x + 2, y, _z] = 0;
                        }

                        // remove 110, 0110, 011 vertical paterns

                        if ((y + 3 < max_y && map[x, y, _z] == 0 && map[x, y + 1, _z] == 1
                            && map[x, y + 2, _z] == 1 && map[x, y + 3, _z] == 0))
                        {
                            map[x, y, _z] = 0;
                            map[x, y + 1, _z] = 0;
                            map[x, y + 2, _z] = 0;
                        }
                    }
                    _z++;
                }
            }
        }

        // build mask map

        for (int y = 0; y < max_y; y++)
        {
            for (int x = 0; x < max_x; x++)
            {
                _z = 0;
                while (_z < MAX_HEIGHT)
                {
                    mask = y + 1 < max_y && map[x, y + 1, _z] == 1 ? 1 : 0;   //top
                    mask += x + 1 < max_x && map[x + 1, y, _z] == 1 ? 2 : 0;  //right
                    mask += y - 1 >= 0 && map[x, y - 1, _z] == 1 ? 4 : 0;     //bottom
                    mask += x - 1 >= 0 && map[x - 1, y, _z] == 1 ? 8 : 0;     //left
                    mask_map[x, y, _z] = mask;
                    _z++;
                }
            }
        }
    }

    //==============
    // Render
    //==============

    public static void render()
    {
        // assign values to the TileMap
        top_left = new Vector2Int(current_map_center.x - CHUNK_WIDTH / 2, current_map_center.y - CHUNK_HEIGHT / 2);
        int i = 0;
        int j = 0;

        for (int y = top_left.y; y < top_left.y + CHUNK_HEIGHT; y++)
        {
            i = 0;
            for (int x = top_left.x; x < top_left.x + CHUNK_WIDTH; x++)
            {
                for (int map_h = 0; map_h < MAX_HEIGHT; map_h++)
                {
                    if (map[i, j, map_h] == 1)
                    {
                        tilemaps[map_h].SetTile(new Vector3Int(x, y, 0), terrain_tile);
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

    public static void hide()
    {
        //disable tilemap

    }

    //================
    // Helper Methods
    //================

    public static bool isRidge(int init_i, int init_j, int _z)
    {
        int mask = mask_map[init_i, init_j, _z];
        bool create_ridge = false;

        if (mask == 11)
        {
            create_ridge = true;
        }
        else if (mask == 15)
        {
            // find mask2
            int x = init_i;
            int y = init_j;
            int mask2 = x + 1 < max_x && y + 1 < max_y && map[x + 1, y + 1, _z] == 1 ? 1 : 0;   // top_right
            mask2 += x - 1 >= 0 && y + 1 < max_y && map[x - 1, y + 1, _z] == 1 ? 2 : 0;         // top_left
            mask2 += x + 1 < max_x && y - 1 >= 0 && map[x + 1, y - 1, _z] == 1 ? 4 : 0;         // bottom_right
            mask2 += x - 1 >= 0 && y - 1 >= 0 && map[x - 1, y - 1, _z] == 1 ? 8 : 0;            // bottom_left

            if (mask2 == 11 || mask2 == 7)
            {
                create_ridge = true;
            }
        }

        return create_ridge;
    }

    public static void createRidgeCollider(int init_i, int init_j, float init_x,
                                           float init_y, int _z, int[,,] merge_map,
                                           GameObject ridge_obj)
    {
        if (!isRidge(init_i, init_j, _z))
            return;

        merge_map[init_i, init_j, _z] = 1;
        // MeshCollider new_collider = ridge_obj.AddComponent<MeshCollider>();

        // new_collider.size = new Vector3(1f, 1f, 0.5f);
        // new_collider.center = new Vector3(init_x + 0.5f, init_y + 0.5f, -_z * HEIGHT_DELTA);
    }

    public static void createMergedCollider(Vector2Int top_left, float init_x,
                                                       float init_y, int init_i,
                                                       int init_j, int _z, int[,,] merge_map,
                                                       GameObject height_obj)
    {
        BoxCollider new_collider;
        int i = init_i + 1, j = init_j;
        int mask = mask_map[init_i, init_j, _z];
        float[] c_type;
        if (!collider_types.TryGetValue(mask, out c_type)) c_type = new float[] { 0f, 0f, 1f, 1f };
        float true_init_x = init_x + c_type[0], true_init_y = init_y + c_type[1];
        float x_size = c_type[2], y_size = c_type[3];

        if (mask == 12 || mask == 6)
        {
            new_collider = height_obj.AddComponent<BoxCollider>();
            new_collider.size = new Vector3(x_size, y_size, HEIGHT_DELTA);
            new_collider.center = new Vector3(true_init_x + x_size * 0.5f,
                                              true_init_y + y_size * 0.5f, -_z
                                              * HEIGHT_DELTA + HEIGHT_DELTA * 0.5f);
            colliders.Add(new_collider);
            return;
        }

        // find x_size and x_count
        // -----------------------

        float[] other_c_type;
        Vector2 expected_init = new Vector2(true_init_x + x_size, true_init_y);
        Vector2 current_cell = new Vector2(init_x + 1, init_y);
        int x_count = 1;

        // if the next collider is not already in the merge map we can proceed

        while (i < map.GetLength(0) && map[i, j, _z] == 1 && merge_map[i, j, _z] == 0)
        {
            mask = mask_map[i, j, _z];

            if (!collider_types.TryGetValue(mask, out other_c_type))
                other_c_type = new float[] { 0f, 0f, 1f, 1f };

            // if the next collider starts where the prev collider ends and has the same y size
            // we can merge horizontally

            if (expected_init == current_cell + new Vector2(other_c_type[0], other_c_type[1]) &&
                c_type[3] == other_c_type[3] && merge_map[i, j, _z] == 0)
            {
                expected_init = new Vector2(current_cell.x + other_c_type[0]
                                            + other_c_type[2], expected_init.y);
                current_cell.x += 1;
                x_size += other_c_type[2];
                x_count++;
            }
            else
            {
                // exit the loop we found a value we cannot merge
                i = map.GetLength(0);
            }
            i++;
        }

        // find y_size and y_count
        // -----------------------

        j = init_j + 1;
        int y_count = 1;
        int current_x_count;
        float current_x_size;
        while (j < map.GetLength(1))
        {
            current_x_count = 0;
            current_x_size = 0;
            i = init_i;

            // next vertical collider can merge if x_init for the collider == x_init for first collider
            // and if the next collider is not merged already

            mask = mask_map[i, j, _z];

            if (!collider_types.TryGetValue(mask, out other_c_type))
                other_c_type = new float[] { 0f, 0f, 1f, 1f };

            if (c_type[0] == other_c_type[0] && map[i, j, _z] == 1 && merge_map[i, j, _z] == 0)
            {
                c_type = other_c_type;
                current_cell = new Vector2(init_x, init_y + y_count);
                expected_init = new Vector2(current_cell.x + c_type[0] + c_type[2], current_cell.y + c_type[1]);
                current_x_count = 1;
                current_x_size = c_type[2];
                current_cell.x += 1;
                i++;

                // check horizontal values

                while (i < init_i + x_count && i < map.GetLength(0) && map[i, j, _z] == 1 && merge_map[i, j, _z] == 0)
                {
                    mask = mask_map[i, j, _z];

                    if (!collider_types.TryGetValue(mask, out other_c_type))
                        other_c_type = new float[] { 0f, 0f, 1f, 1f };

                    if (expected_init == current_cell + new Vector2(other_c_type[0], other_c_type[1]) &&
                        c_type[3] == other_c_type[3])
                    {
                        expected_init = new Vector2(current_cell.x + other_c_type[0] + other_c_type[2], expected_init.y);
                        current_cell.x += 1;
                        current_x_count++;
                        current_x_size += other_c_type[2];
                        i++;
                    }
                    else
                    {
                        // exit while loop we found a value we cannot merge
                        i = map.GetLength(0);
                    }
                }
            }
            else
            {
                // initial value does not work complete loop
                j = map.GetLength(1);
            }

            if (current_x_count == x_count && current_x_size == x_size)
            {
                y_count++;
                y_size += c_type[3];
            }
            else
            {
                // complete loop we know y_size and y_count
                j = map.GetLength(1);
            }
            j++;
        }

        // update merged values in merge map
        // ---------------------------------

        for (i = init_i; i < init_i + x_count; i++)
        {
            for (j = init_j; j < init_j + y_count; j++)
            {
                merge_map[i, j, _z] = 1;
            }
        }

        // create collider
        // ---------------

        new_collider = height_obj.AddComponent<BoxCollider>();

        // size = BR - TL
        // center = TL + SIZE * 0.5f

        new_collider.size = new Vector3(x_size, y_size, HEIGHT_DELTA);
        new_collider.center = new Vector3(true_init_x + x_size * 0.5f,
                                          true_init_y + y_size * 0.5f, -_z
                                          * HEIGHT_DELTA + HEIGHT_DELTA * 0.5f);
        colliders.Add(new_collider);
    }


    public static void checkPlayerPosition(Vector2Int _new_position)
    {
        if (L1(current_map_center, _new_position) > MAP_UPDATE_DISTANCE)
        {
            //This does not work at all
            reload(_new_position);
        }
    }

    public static void reload(Vector2Int _current_map_center)
    {
        current_map_center = _current_map_center;
        load();
        render();
    }

}
