using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;
using static TilemapManager;

public static class Terrain {

    // Constants
    public const float HEIGHT_DIFF = 0.2f;            //0.3f default //height difference used to calc. heightmap values from noise
    public const float H_NOISE_FREQUENCY = 0.25f;     //frequency of height noise

    // Scriptable Tiles
    public static TerrainTile terrain_tile;

    // Physics
    public static Dictionary<int, float[]> collider_types = new Dictionary<int, float[]>();
    public static Mesh ridge_mesh = new Mesh();
    public static List<GameObject> ground_objs = new List<GameObject>();
    public static List<GameObject> ridge_objs = new List<GameObject>();

    //==============
    // Initialize
    //==============
    public static void initialize() {

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
        collider_types[9] = new float[] { 0f, 0f, 0.13f, 1f };        // right outer corner wall

        // create the ridge_mesh
        List<Vector3> verts = new List<Vector3> {
            new Vector3(1, 1, 0),                           // top right, 0
            new Vector3(1, 0, 0),                           // bottom right, 1
            new Vector3(0, 0, 0),                           // bottom left, 2
            new Vector3(0, 1, 0),                           // top left, 3
            new Vector3(0, 1, -LEVEL_HEIGHT),               // upper top left, 4
            new Vector3(1, 1, -LEVEL_HEIGHT),               // upper top right, 5
        };

        List<int> tris = new List<int> {
            // clockwise
            // main ridge
            0, 1, 3, 3, 1, 2,                               // bottom
            4, 2, 3, 5, 0, 1,                               // sides
            5, 4, 3, 3, 0, 5,                               // back
            4, 1, 2, 4, 5, 1,                               // front
        };

        ridge_mesh.vertices = verts.ToArray();
        ridge_mesh.triangles = tris.ToArray();
    }

    //==============
    // Load
    //==============
    public static void load() {
        loadGraphics();
        loadPhysics();
    }

    public static void loadGraphics() {
        fillMap();
        cleanMap();
        fillMaskmap();
    }

    public static void loadPhysics() {
        top_left = new Vector2Int(current_map_center.x - CHUNK_WIDTH / 2, current_map_center.y - CHUNK_HEIGHT / 2);
        int i = 0, j = 0, z = 0;
        GameObject ground_obj;
        GameObject ridge_obj;

        // create ridge colliders
        for (z = 0; z < MAX_LEVEL; ++z) {
            ridge_obj = new GameObject("Ridge_Colliders_" + z);
            ridge_obj.transform.parent = parent_obj.transform;
            ridge_obj.layer = 10;
            ridge_objs.Add(ridge_obj);
        }

        for (int y = top_left.y; y < top_left.y + CHUNK_HEIGHT; ++y) {
            i = 0;
            for (int x = top_left.x; x < top_left.x + CHUNK_WIDTH; ++x) {
                for (z = 0; z < MAX_LEVEL; ++z) {
                    createRidgeCollider(i, j, new Vector2Int(x, y), z);
                }
                i++;
            }
            j++;
        }

        // create merged colliders
        for (z = 0; z < MAX_LEVEL; ++z) {
            ground_obj = new GameObject("Ground_Colliders_" + z);
            ground_obj.transform.parent = parent_obj.transform;
            ground_obj.layer = 9;
            ground_objs.Add(ground_obj);
        }

        j = 0;
        for (int y = top_left.y; y < top_left.y + CHUNK_HEIGHT; ++y) {
            i = 0;
            for (int x = top_left.x; x < top_left.x + CHUNK_WIDTH; ++x) {
                for (z = 0; z < MAX_LEVEL; ++z) {
                    if (!map[i, j].merged[z] && map[i, j].level >= z) {
                        createMergedCollider(x, y, i, j, z);
                    }
                }
                i++;
            }
            j++;
        }
    }

    //==============
    // Reload
    //==============
    public static void reload() {
        // delete old resources
        // TODO replace with deactivating objects/ using an object pool
        foreach(GameObject o in ground_objs) {
            GameObject.Destroy(o);
        }
        foreach(GameObject o in ridge_objs) {
            GameObject.Destroy(o);
        }

        // load new resources
        load();
    }

    //================
    // Helpers
    //================
    public static void fillMap() {
        // fill map with initial height values from perlin noise
        for (int y = 0; y < map.GetLength(1); y++) {
            for (int x = 0; x < map.GetLength(0); x++) {
                float h = noise(x + current_map_center.x, y + current_map_center.y, H_NOISE_FREQUENCY);
                map[x, y].raw_height = h;
                map[x, y].level = Mathf.FloorToInt(h / HEIGHT_DIFF);
                map[x, y].tile_height = -map[x, y].level * LEVEL_HEIGHT;
            }
        }
    }

    public static void cleanMap() {
        // clean values in the map
        // List<int> illegal_values = new List<int> { 0, 1, 2, 4, 5, 8, 10 };
        // CHUNK_WIDTH = map.GetLength(0);
        // CHUNK_HEIGHT = map.GetLength(1);
        // Debug.Log("CHUNK_WIDTH == CHUNK_WIDTH" + CHUNK_WIDTH == CHUNK_WIDTH);
        // int z = 0;
        // int mask;
        // for (int y = 0; y < CHUNK_HEIGHT; y++) {
        //     for (int x = 0; x < CHUNK_WIDTH; x++) {
        //         z = 0;
        //         while (z < MAX_LEVEL) {
        //             for (int i = 0; i < 3; i++) {
        //                 // remove illegal values
        //                 mask = y + 1 < CHUNK_HEIGHT && map[x, y + 1, z] == 1 ? 1 : 0; //top
        //                 mask += x + 1 < CHUNK_WIDTH && map[x + 1, y, z] == 1 ? 2 : 0; //right
        //                 mask += y - 1 >= 0 && map[x, y - 1, z] == 1 ? 4 : 0; //bottom
        //                 mask += x - 1 >= 0 && map[x - 1, y, z] == 1 ? 8 : 0; //left
        //
        //                 if (illegal_values.Contains(mask)) {
        //                     map[x, y, z] = 0;
        //                 }
        //
        //                 // check 110 at x = 0, 0110 every where, 011 at x - 3 == CHUNK_WIDTH horizontal
        //
        //                 if ((x == 0 && map[x, y, z] == 1 && map[x + 1, y, z] == 1
        //                     && map[x + 2, y, z] == 0) || (x + 3 == CHUNK_WIDTH
        //                     && map[x, y, z] == 0 && map[x + 1, y, z] == 1
        //                     && map[x + 2, y, z] == 1) || (x + 3 < CHUNK_WIDTH
        //                     && map[x, y, z] == 0 && map[x + 1, y, z] == 1
        //                     && map[x + 2, y, z] == 1 && map[x + 3, y, z] == 0)) {
        //                     map[x, y, z] = 0;
        //                     map[x + 1, y, z] = 0;
        //                     map[x + 2, y, z] = 0;
        //                 }
        //
        //                 // remove 110, 0110, 011 vertical paterns
        //
        //                 if ((y + 3 < CHUNK_HEIGHT && map[x, y, z] == 0 && map[x, y + 1, z] == 1
        //                     && map[x, y + 2, z] == 1 && map[x, y + 3, z] == 0)) {
        //                     map[x, y, z] = 0;
        //                     map[x, y + 1, z] = 0;
        //                     map[x, y + 2, z] = 0;
        //                 }
        //             }
        //             z++;
        //         }
        //     }
        // }
    }

    public static void fillMaskmap() {
        // mask values hold a bit mask indicating the neighbors of each MapTile
        for (int y = 0; y < CHUNK_HEIGHT; ++y) {
            for (int x = 0; x < CHUNK_WIDTH; ++x) {
                for (int z = 0; z < MAX_LEVEL; ++z) {
                    int mask = y + 1 < CHUNK_HEIGHT && map[x, y + 1].level >= z ? 1 : 0;        //top
                    mask += x + 1 < CHUNK_WIDTH && map[x + 1, y].level >= z ? 2 : 0;            //right
                    mask += y - 1 >= 0 && map[x, y - 1].level >= z ? 4 : 0;                     //bottom
                    mask += x - 1 >= 0 && map[x - 1, y].level >= z ? 8 : 0;                     //left
                    map[x, y].mask[z] = mask;
                }
            }
        }
    }

    public static bool isRidge(int init_i, int init_j, int z) {
        int mask = map[init_i, init_j].mask[z];
        bool create_ridge = false;

        if (mask == 11) {
            create_ridge = true;
        } else if (mask == 15) {
            // find mask2 since mask == 15 means we have neighbors right, left, top, bottom
            int x = init_i;
            int y = init_j;
            int mask2 = x + 1 < CHUNK_WIDTH && y + 1 < CHUNK_HEIGHT && map[x + 1, y + 1].level >= z ? 1 : 0;    // top_right
            mask2 += x - 1 >= 0 && y + 1 < CHUNK_HEIGHT && map[x - 1, y + 1].level >= z ? 2 : 0;                // top_left
            mask2 += x + 1 < CHUNK_WIDTH && y - 1 >= 0 && map[x + 1, y - 1].level >= z ? 4 : 0;                 // bottom_right
            mask2 += x - 1 >= 0 && y - 1 >= 0 && map[x - 1, y - 1].level >= z ? 8 : 0;                          // bottom_left

            // if bottom_right or bottom_left is empty this is a ridge
            if (mask2 == 11 || mask2 == 7) {
                create_ridge = true;
            // if top_left or top_right is not empty we don't have a corner, thus this is a flat tile
            } else if (mask2 != 13 && mask2 != 14) {
                if (z == map[init_i, init_j].level) {
                    map[init_i, init_j].isFlat = true;
                }
            }
        }

        if (create_ridge) {
            map[init_i, init_j].isRidge = true;
        }

        return create_ridge;
    }

    public static void createRidgeCollider(int init_i, int init_j, Vector2Int init_pos, int z) {
        if (!isRidge(init_i, init_j, z))
            return;

        // means this index is already accounted for by collider physics
        map[init_i, init_j].merged[z] = true;

        GameObject ridge_obj = ridge_objs[z];
        MeshCollider ridge_collider = ridge_obj.AddComponent<MeshCollider>();
        ridge_collider.convex = true;
        Mesh world_ridge_mesh = new Mesh();

        world_ridge_mesh.vertices = modelToWorld(ridge_mesh.vertices, init_pos, z);
        world_ridge_mesh.triangles = ridge_mesh.triangles;
        ridge_collider.sharedMesh = world_ridge_mesh;
    }

    public static Vector3[] modelToWorld(Vector3[] verts, Vector2Int init_pos, int z) {
        Vector3 new_pos = new Vector3(init_pos.x, init_pos.y, (z * -LEVEL_HEIGHT) + LEVEL_HEIGHT);
        for (int i = 0; i < verts.Length; ++i) {
            verts[i] += new_pos;
        }
        return verts;
    }

    public static void createMergedCollider(float init_x, float init_y, int init_i, int init_j, int z) {
        BoxCollider new_collider;
        int i = init_i, j = init_j;
        int mask = map[init_i, init_j].mask[z];
        float[] c_type;
        if (!collider_types.TryGetValue(mask, out c_type)) c_type = new float[] { 0f, 0f, 1f, 1f };
        float true_init_x = init_x + c_type[0], true_init_y = init_y + c_type[1];
        float x_size = c_type[2], y_size = c_type[3];

        GameObject ground_obj = ground_objs[z];

        if (mask == 12 || mask == 6) {
            map[i, j].merged[z] = true;
            new_collider = ground_obj.AddComponent<BoxCollider>();
            new_collider.size = new Vector3(x_size, y_size, LEVEL_HEIGHT);
            new_collider.center = new Vector3(true_init_x + x_size * 0.5f,
                                              true_init_y + y_size * 0.5f,
                                              -z * LEVEL_HEIGHT + LEVEL_HEIGHT * 0.5f);
            return;
        }

        // find x_size and x_count
        // -----------------------

        float[] other_c_type;
        Vector2 expected_init = new Vector2(true_init_x + x_size, true_init_y);
        Vector2 current_cell = new Vector2(init_x + 1, init_y);
        int x_count = 1;
        i++;

        // if the next collider is not already in the merge map we can proceed

        while (i < map.GetLength(0) && map[i, j].level >= z && !map[i, j].merged[z]) {
            mask = map[i, j].mask[z];

            if (!collider_types.TryGetValue(mask, out other_c_type))
                other_c_type = new float[] { 0f, 0f, 1f, 1f };

            // if the next collider starts where the prev collider ends and has the same y size
            // we can merge horizontally

            if (expected_init == current_cell + new Vector2(other_c_type[0], other_c_type[1]) &&
                c_type[3] == other_c_type[3] && !map[i, j].merged[z]) {
                expected_init = new Vector2(current_cell.x + other_c_type[0]
                                            + other_c_type[2], expected_init.y);
                current_cell.x += 1;
                x_size += other_c_type[2];
                x_count++;
            } else {
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
        while (j < map.GetLength(1)) {
            current_x_count = 0;
            current_x_size = 0;
            i = init_i;

            // next vertical collider can merge if x_init for the collider == x_init for first collider
            // and if the next collider is not merged already

            mask = map[i, j].mask[z];

            if (!collider_types.TryGetValue(mask, out other_c_type))
                other_c_type = new float[] { 0f, 0f, 1f, 1f };

            if (c_type[0] == other_c_type[0] && map[i, j].level >= z && !map[i, j].merged[z]) {
                c_type = other_c_type;
                current_cell = new Vector2(init_x, init_y + y_count);
                expected_init = new Vector2(current_cell.x + c_type[0] + c_type[2], current_cell.y + c_type[1]);
                current_x_count = 1;
                current_x_size = c_type[2];
                current_cell.x += 1;
                i++;

                // check horizontal values

                while (i < init_i + x_count && i < map.GetLength(0) && map[i, j].level >= z && !map[i, j].merged[z]) {
                    mask = map[i, j].mask[z];

                    if (!collider_types.TryGetValue(mask, out other_c_type))
                        other_c_type = new float[] { 0f, 0f, 1f, 1f };

                    if (expected_init == current_cell + new Vector2(other_c_type[0], other_c_type[1]) &&
                        c_type[3] == other_c_type[3]) {
                        expected_init = new Vector2(current_cell.x + other_c_type[0] + other_c_type[2], expected_init.y);
                        current_cell.x += 1;
                        current_x_count++;
                        current_x_size += other_c_type[2];
                        i++;
                    } else {
                        // exit while loop we found a value we cannot merge
                        i = map.GetLength(0);
                    }
                }
            } else {
                // initial value does not work complete loop
                j = map.GetLength(1);
            }

            if (current_x_count == x_count && current_x_size == x_size) {
                y_count++;
                y_size += c_type[3];
            } else {
                // complete loop we know y_size and y_count
                j = map.GetLength(1);
            }
            j++;
        }

        // update merged values in merge map
        // ---------------------------------

        for (i = init_i; i < init_i + x_count; i++) {
            for (j = init_j; j < init_j + y_count; j++) {
                map[i, j].merged[z] = true;
            }
        }

        // create collider
        // ---------------
        new_collider = ground_obj.AddComponent<BoxCollider>();

        // size = BR - TL
        // center = TL + SIZE * 0.5f

        new_collider.size = new Vector3(x_size, y_size, LEVEL_HEIGHT);
        new_collider.center = new Vector3(true_init_x + x_size * 0.5f,
                                          true_init_y + y_size * 0.5f,
                                          -z * LEVEL_HEIGHT + LEVEL_HEIGHT * 0.5f);
    }

}
