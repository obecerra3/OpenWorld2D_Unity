using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;
using static TilemapManager;
using Plant;

public class PlantTile {
    public int ID;
    public int stage;

    public PlantTile() {
        ID = Utils.getID("empty");
        stage = 0;
    }

    public PlantTile(int id, int stg) {
        ID = id;
        stage = stg;
    }
}

public static class Plants {

    public static PlantTile[,] plant_map;
    public static Dictionary<int, PlantObj> plant_ids = new Dictionary<int, PlantObj>();
    public static List<PlantObj> plant_objpl = new List<PlantObj>();
    public static GameObject prefabs_parent = new GameObject("Plant Prefabs Parent");

    //==============
    // Initialize
    //==============
    public static void initialize() {
        plant_map = new PlantTile[CHUNK_WIDTH, CHUNK_HEIGHT];
        for (int j = 0; j < CHUNK_HEIGHT; ++j) {
            for (int i = 0; i < CHUNK_WIDTH; ++i) {
                plant_map[i, j] = new PlantTile();
            }
        }
        initPlantTypes();
    }

    //==============
    // Load
    //==============
    public static void load() {
        fillMap();
    }

    //==============
    // Reload
    //==============
    public static void reload() {
        // load new resources
        load();
    }

    //================
    // Helpers
    //================
    public static void fillMap() {
        // try spawn
        int i = 0;
        int j = 0;
        for (int y = top_left.y; y < top_left.y + CHUNK_HEIGHT; ++y) {
            i = 0;
            for (int x = top_left.x; x < top_left.x + CHUNK_WIDTH; ++x) {
                if (map[i, j].isFlat) {
                    Biome biome = Biomes.biome_ids[Biomes.biome_map[i, j]];
                    foreach(string ps in biome.plants) {
                        PlantObj p;
                        plant_ids.TryGetValue(Utils.getID(ps), out p);
                        if (p) {
                            PlantObj p2;
                            plant_ids.TryGetValue(plant_map[i, j].ID, out p2);
                            if (!p2 || p.spawn_order >= p2.spawn_order) {
                                p.TrySpawn(i, j, x, y);
                            }
                        }
                    }
                }
                i++;
            }
            j++;
        }

        // actually spawn
        // this seperation allows me to replace chosen plants in the plant_map
        // before spawning in try_spawn by just changing the PlantTile after
        // comparing PlantObjs.spawn_order
        i = 0;
        j = 0;
        for (int y = top_left.y; y < top_left.y + CHUNK_HEIGHT; ++y) {
            i = 0;
            for (int x = top_left.x; x < top_left.x + CHUNK_WIDTH; ++x) {
                if (map[i, j].isFlat) {
                    PlantTile pt = plant_map[i, j];
                    if (pt.ID != Utils.getID("empty")) {
                        PlantObj p;
                        plant_ids.TryGetValue(pt.ID, out p);
                        if (p) {
                            p.Spawn(i, j, x, y, pt.stage);
                        }
                    }
                }
                i++;
            }
            j++;
        }
    }

    public static void initPlantTypes() {
        // "grasslands":
        // "pebble_tile", "tall_grass", "grass", "grass_tile", "flower",
        // "flower_tile", "mushroom", "mushroom_tile"

        // "pebble_tile"
        GameObject obj = new GameObject();
        PlantObj debri = initPlant("pebble_tile", obj);
        debri.spawn_order = 0;
        // Stage 0
        PlantStage ps = new PlantStage(1, 1, 6, new int[] { 4, 5, 23, 24, 25, 43 }, new float[] { 0.167f, 0.167f, 0.167f, 0.167f, 0.167f, 0.167f }, true);
        // first three are random, fourth follows tall_grass placement
        ps.initNoise(new int[] { 0, 100, 200, 0 }, new float[] { 1.0f, 1.0f, 1.0f, 1.0f }, new float[] { 0.0f, 0.8f, 0.6f, 0.6f }, new float[] { 0.001f, 0.5f, 0.2f, 0.5f });
        debri.AddStage(0, ps);
        // Obj Pool
        ObjPool.Add(obj, 100);

        // "tall_grass"
        obj = new GameObject();
        PlantObj tallgrass = initPlant("tall_grass", obj);
        tallgrass.spawn_order = 1;
        // Stage 0
        ps = new PlantStage(1, 1, 2, new int[] { 46, 47 }, new float[] { 0.5f, 0.5f });
        ps.initNoise(new int[] { 0 }, new float[] { 1.0f }, new float[] { 0.65f }, new float[] { 0.8f });
        ps.initBoxCollider(true, Vector3.zero, Vector3.one);
        ps.initShake();
        tallgrass.AddStage(0, ps);
        // Stage 2
        ps = new PlantStage(1, 2, 2, new int[] { 68, 48, 69, 49 }, new float[] { 0.5f, 0.5f });
        ps.initNoise(new int[] { 25 }, new float[] { 0.3f }, new float[] { 0.78f }, new float[] { 0.9f });
        ps.initBoxCollider(true, Vector3.zero, Vector3.one);
        ps.initShake(0.5f);
        tallgrass.AddStage(1, ps);
        // ObjPool
        ObjPool.Add(obj, 500);

        // "grass"

        // "grass_tile"

        // "flower"
        obj = new GameObject();
        PlantObj flower = initPlant("flower", obj);
        flower.XY_neighbors = false;
        flower.spawn_order = 1;
        // Stage 0
        ps = new PlantStage(1, 1, 1, new int[] { 6 }, new float[] { 1.0f }, true);
        ps.initNoise(new int[] { 560 }, new float[] { 3.0f }, new float[] { 0.85f }, new float[] { 0.05f });
        ps.initBoxCollider(true, new Vector3(-0.02f, -0.02f, 0f), new Vector3(0.4f, 0.45f, 0.1f));
        ps.initShake();
        flower.AddStage(0, ps);
        // Stage 1
        ps = new PlantStage(1, 1, 1, new int[] { 10 }, new float[] { 1.0f });
        ps.initNoise(new int[] { 60 }, new float[] { 1.0f }, new float[] { 0.8f }, new float[] { 0.1f });
        ps.initBoxCollider(true, Vector3.zero, Vector3.one * 0.3f);
        ps.initShake();
        flower.AddStage(1, ps);
        // ObjPool
        ObjPool.Add(obj, 20);

        // "flower_tile"
        obj = new GameObject();
        PlantObj flower_tile = initPlant("flower_tile", obj);
        flower_tile.spawn_order = 0;
        flower_tile.XY_neighbors = false;
        // Stage 0
        ps = new PlantStage(1, 1, 4, new int[] { 61, 62, 81, 82 }, new float[] { 0.25f, 0.25f, 0.25f, 0.25f }, true);
        ps.initNoise(new int[] { 560, 60 }, new float[] { 3.0f, 1.0f }, new float[] { 0.9f, 0.8f }, new float[] { 1.0f, 0.7f });
        flower_tile.AddStage(0, ps);
        // Obj Pool
        ObjPool.Add(obj, 50);

        // "mushroom"
        obj = new GameObject();
        PlantObj mushroom = initPlant("mushroom", obj);
        mushroom.XY_neighbors = false;
        mushroom.spawn_order = 1;
        // Stage 0
        ps = new PlantStage(1, 1, 1, new int[] { 26 }, new float[] { 1.0f }, true);
        ps.initNoise(new int[] { 20 }, new float[] { 2.0f }, new float[] { 0.9f }, new float[] { 0.05f });
        ps.initCapsuleCollider(false, Vector3.zero, 2, 0.5f, 0.1f);
        ps.initCapsuleCollider(true, Vector3.zero, 2, 0.6f, 0.15f);
        ps.initShake();
        mushroom.AddStage(0, ps);
        // Stage 1
        ps = new PlantStage(1, 1, 2, new int[] { 27, 28 }, new float[] { 0.5f, 0.5f });
        ps.initNoise(new int[] { 10, 95 }, new float[] { 2.0f, 1.0f }, new float[] { 0.9f, 0.8f }, new float[] { 0.05f, 0.05f });
        ps.initCapsuleCollider(false, Vector3.zero, 2, 2f, 0.4f);
        ps.initCapsuleCollider(true, Vector3.zero, 2, 2f, 0.45f);
        ps.initShake(0.5f);
        ps.initBounce();
        mushroom.AddStage(1, ps);
        // Stage 2
        ps = new PlantStage(1, 2, 2, new int[] { 126, 106, 127, 107 }, new float[] { 0.5f, 0.5f });
        ps.initNoise(new int[] { 32 }, new float[] { 2.0f }, new float[] { 0.9f }, new float[] { 0.05f });
        ps.initCapsuleCollider(false, Vector3.zero, 2, 2.5f, 0.4f);
        ps.initCapsuleCollider(true, Vector3.zero, 2, 2.5f, 0.45f);
        ps.initShake(0.5f);
        ps.initBounce();
        mushroom.AddStage(2, ps);
        // Stage 3
        ps = new PlantStage(2, 2, 1, new int[] { 128 }, new float[] { 0.5f }, false, false, true);
        // ps = new PlantStage(2, 2, 1, new int[] { 128, 129, 108, 109 }, new float[] { 0.5f });
        ps.initNoise(new int[] { 45 }, new float[] { 2.0f }, new float[] { 0.9f }, new float[] { 0.05f });
        ps.initCapsuleCollider(false, Vector3.zero, 2, 3f, 0.8f);
        ps.initCapsuleCollider(true, Vector3.zero, 2, 3f, 0.85f);
        ps.initShake(0.35f);
        ps.initBounce();
        mushroom.AddStage(3, ps);
        // Obj Pool
        ObjPool.Add(obj, 20);

        // "mushroom_tile"
        obj = new GameObject();
        PlantObj mushroom_tile = initPlant("mushroom_tile", obj);
        mushroom_tile.spawn_order = 0;
        mushroom_tile.XY_neighbors = false;
        // Stage 0
        ps = new PlantStage(1, 1, 2, new int[] { 60, 80 }, new float[] { 0.5f, 0.5f }, true);
        ps.initNoise(new int[] { 20, 10, 95, 32, 45 }, new float[] { 2.0f, 2.0f, 1.0f, 2.0f, 2.0f },
                    new float[] { 0.87f, 0.87f, 0.77f, 0.87f, 0.87f }, new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f });
        mushroom_tile.AddStage(0, ps);
        // Obj Pool
        ObjPool.Add(obj, 50);
    }

    public static PlantObj initPlant(string plant_name, GameObject obj) {
        obj.transform.parent = prefabs_parent.transform;
        obj.name = plant_name;
        obj.tag = "Plant";
        PlantObj p = obj.AddComponent<PlantObj>();
        p.Init(plant_name);
        obj.SetActive(false);
        return p;
    }
}
