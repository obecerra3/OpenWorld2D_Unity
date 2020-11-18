using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;
using static TilemapManager;
using static States;

namespace Plants {

    public static class PlantSpawn {
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
                        Biomes.BiomeData biome = Biomes.BiomeSpawn.biome_ids[Biomes.BiomeSpawn.biome_map[i, j]];
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
                            PlantSpawn.plant_ids.TryGetValue(pt.ID, out p);
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
            ps = new PlantStage(1, 2, 2, new int[] { 68, 69 }, new float[] { 0.5f, 0.5f }, false, false, true);
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
            ps = new PlantStage(1, 2, 2, new int[] { 126, 127 }, new float[] { 0.5f, 0.5f }, false, false, true);
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

    public class PlantStage {
        public int width;
        public int height;
        public int types;
        public int[] sprite_indices;
        public float[] sprite_probs;
        public List<GameObject> objs = new List<GameObject>();
        public GameObject physics_obj;
        public bool is_tile;
        public bool is_stack;
        public bool merge_sprite;
        public float shake_delta = 1.0f;
        public float shake_freq = 2.0f;
        public bool rotate_around = true;
        public bool can_shake = false;
        public bool can_bounce = false;
        public float bounce_delta = 0.25f;

        // noise
        public int[] noise_seeds;
        public float[] freqs;
        public float[] min_vals;
        public float[] spawn_probs;

        public PlantStage(int w, int h, int ts, int[] sprt_i, float[] sprt_ps, bool is_tl = false, bool is_stck = false, bool mrge_sprt = false) {
            width = w;
            height = h;
            types = ts;
            sprite_indices = sprt_i;
            sprite_probs = sprt_ps;
            is_tile = is_tl;
            is_stack = is_stck;
            merge_sprite = mrge_sprt;
            initSprts();
        }

        public void initShake(float shk_delta = 1.0f, float shk_freq = 2.0f, bool rte_arnd = true) {
            can_shake = true;
            shake_delta = shk_delta;
            shake_freq = shk_freq;
            rotate_around = rte_arnd;
        }

        public void initBounce(float bnc_delta = 0.25f) {
            can_bounce = true;
            bounce_delta = bnc_delta;
        }

        public void initPhysics() {
            physics_obj = new GameObject("Physics_Obj");
            physics_obj.tag = "Plant";
            objs.Add(physics_obj);
        }

        public void initBoxCollider(bool is_trigger, Vector3 center, Vector3 size) {
            if (!physics_obj) {
                initPhysics();
            }

            BoxCollider box_c = physics_obj.AddComponent<BoxCollider>();
            box_c.isTrigger = is_trigger;
            box_c.center = center;
            box_c.size = size;
            physics_obj.transform.position = Vector3.back * (box_c.size.z * 0.5f);
        }

        public void initCapsuleCollider(bool is_trigger, Vector3 center, int direction, float height, float radius) {
            if (!physics_obj) {
                initPhysics();
            }

            CapsuleCollider cap_c = physics_obj.AddComponent<CapsuleCollider>();
            cap_c.isTrigger = is_trigger;
            cap_c.center = center;
            cap_c.direction = direction;
            cap_c.height = height;
            physics_obj.transform.position = Vector3.back * (cap_c.height * 0.5f);
            cap_c.radius = radius;
        }

        public void initSphereCollider(bool is_trigger, Vector3 center, float radius) {
            if (!physics_obj) {
                initPhysics();
            }

            SphereCollider sphere_c = physics_obj.AddComponent<SphereCollider>();
            sphere_c.isTrigger = is_trigger;
            sphere_c.center = center;
            sphere_c.radius = radius;
            physics_obj.transform.position = Vector3.back * (sphere_c.radius);
        }

        public void initNoise(int[] ns, float[] fs, float[] mvs, float[] sps) {
            noise_seeds = ns;
            freqs = fs;
            min_vals = mvs;
            spawn_probs = sps;
        }

        public bool TrySpawn(int x, int y) {
            for (int k = 0; k < noise_seeds.Length; ++k) {
                if (Utils.seedNoise(noise_seeds[k], x, y, freqs[k]) > min_vals[k]) {
                    if (Random.value <= spawn_probs[k]) {
                        return true;
                    }
                }
            }
            return false;
        }

        public void initSprts() {
            // each variation gets a sprite, and each height level is its own
            // sprite, each sprite needs a game object with a sprite renderer
            Texture2D src = TilemapManager.all_sprites[0].texture;
            int curr_sprt_i = 0;
            for (int t = 0; t < types; ++t) {
                // create type X object
                GameObject parent_obj = new GameObject();
                parent_obj.name = "Type" + t;
                objs.Add(parent_obj);

                if (merge_sprite) {
                    // Sprite.create to merge the sprites assuming sprite_indices hold
                    // the bottom left corner of the sprites to merge
                    int index = sprite_indices[t];
                    int i = index % 20;
                    int j = (10 - (index / 20)) - 1;

                    SpriteRenderer sr = parent_obj.AddComponent<SpriteRenderer>();
                    sr.sprite = Sprite.Create(src, new Rect(i * 24f, j * 24f, 24f * width, 24f * height), new Vector2(0.5f, 0.5f), 24.0f);
                    int h = 0;
                    sr.sortingLayerName = (h == 0) ? (is_tile) ? "Tile" : "Objects" : "Objects1";
                } else {
                    // per height create a game object
                    for (int h = 0; h < height; ++h) {
                        GameObject h_obj = new GameObject("Height" + h);
                        h_obj.transform.parent = parent_obj.transform;
                        // per width create a game object with a SpriteRenderer
                        for (int w = 0; w < width; ++w) {
                            GameObject w_obj = new GameObject("Width" + w);
                            w_obj.transform.parent = h_obj.transform;
                            SpriteRenderer sr = w_obj.AddComponent<SpriteRenderer>();
                            sr.sortingLayerName = (h == 0) ? (is_tile) ? "Tile" : "Objects" : "Objects1";
                            sr.sprite = TilemapManager.all_sprites[sprite_indices[curr_sprt_i]];
                            w_obj.transform.position += Vector3.up * (h);
                            w_obj.transform.position += Vector3.right * w;

                            if (width >= 2) w_obj.transform.position += Vector3.left * width * 0.25f;

                            curr_sprt_i++;
                        }
                    }
                }
            }
        }
    }

    public class PlantObj : MonoEvent {
        public int ID;
        public int spawn_order;    // 0 for tiles, 1 for above tile, 2 for trees
        public Dictionary<int, PlantStage> stages = new Dictionary<int, PlantStage>();
        public int stage;
        public bool XY_neighbors = true;
        public States state;
        public float shake_delta;
        public float shake_freq;
        public bool rotate_around;
        public bool can_shake;
        public bool can_bounce;
        public float bounce_delta;

        private GameObject stage_obj;
        private float shake_time;
        private float bounce_time;
        private Vector3 shake_start_pos;

        public void Init(string s) {
            state = IDLE;
            ID = Utils.getID(s);
            PlantSpawn.plant_ids.Add(ID, this);
        }

        public void AddStage(int stg_indx, PlantStage ps) {
            stages.Add(stg_indx, ps);
            GameObject stage_obj = new GameObject("Stage" + stg_indx);
            stage_obj.transform.parent = this.transform;
            foreach(GameObject o in ps.objs) {
                o.transform.parent = stage_obj.transform;
            }
        }

        public void shake() {
            if (can_shake && state != SHAKE && state != BOUNCE) {
                state = SHAKE;
                shake_time = 2 * Mathf.PI;
                shake_start_pos = transform.position;
            }
        }

        public void resetShake() {
            stage_obj.transform.eulerAngles = Vector3.zero;
            stage_obj.transform.position = shake_start_pos;
            state = IDLE;
        }

        public bool bounce() {
            if (can_bounce) {
                if (state == SHAKE) {
                    resetShake();
                }
                bounce_time = 2 * Mathf.PI;
                state = BOUNCE;
                return true;
            }
            return false;
        }

        public void resetBounce() {
            stage_obj.transform.localScale = Vector3.one;
            state = IDLE;
        }

        public void FixedUpdate() {
            switch (state) {
                case (SHAKE):
                    if (shake_time > 0) {
                        if (rotate_around) {
                            stage_obj.transform.RotateAround(
                                            transform.position + Vector3.down,
                                            Vector3.forward,
                                            shake_delta * Mathf.Cos(shake_time * shake_freq));
                        } else {
                            stage_obj.transform.Rotate(0, 0, shake_delta * Mathf.Cos(shake_time * shake_freq));
                        }
                        shake_time -= 0.1f;
                    } else {
                        resetShake();
                    }
                    break;
                case (BOUNCE):
                    if (bounce_time > 0) {
                        stage_obj.transform.localScale = Vector3.one + Vector3.up * (Mathf.Cos(bounce_time) * 0.5f * bounce_delta);
                        bounce_time -= 0.1f;
                    } else {
                        resetBounce();
                    }
                    break;
            }

        }

        // =====================================
        // SPAWN METHODS
        // =====================================

        public void TrySpawn(int i, int j, int x, int y) {
            List<int> possible_spawns = new List<int>();
            foreach(KeyValuePair<int, PlantStage> kvp in stages) {
                if (kvp.Value.TrySpawn(i, j)) {

                    bool add_to_map = true;

                    if (!XY_neighbors) {
                        if ((i + 1 < CHUNK_WIDTH && PlantSpawn.plant_map[i + 1, j].ID == ID)
                            || (j + 1 < CHUNK_HEIGHT && PlantSpawn.plant_map[i, j + 1].ID == ID)
                            || (i - 1 >= 0 && PlantSpawn.plant_map[i - 1, j].ID == ID)
                            || (j - 1 >= 0 && PlantSpawn.plant_map[i, j - 1].ID == ID))
                            add_to_map = false;
                    }

                    if (add_to_map)
                        possible_spawns.Add(kvp.Key);
                }
            }

            if (possible_spawns.Count > 0) {
                int rand_index = Random.Range(0, possible_spawns.Count);
                PlantSpawn.plant_map[i, j] = new PlantTile(ID, possible_spawns[rand_index]);
            }
        }

        public void Spawn(int i, int j, int x, int y, int stage_index) {
            // get PlantObj Clone from ObjPool
            GameObject obj = ObjPool.Get(gameObject.name);

            // if a Clone is found, activate correct Stage and Type and set the correct Transform
            if (obj) {
                obj.transform.position = new Vector3(x + 0.5f, y + 0.5f, TilemapManager.map[i, j].tile_height);
                obj.SetActive(true);
                PlantObj plant_obj = obj.GetComponent<PlantObj>();

                // set shake/bounce data
                PlantStage plant_stage = stages[stage_index];
                plant_obj.can_shake = plant_stage.can_shake;
                plant_obj.rotate_around = plant_stage.rotate_around;
                plant_obj.shake_delta = plant_stage.shake_delta;
                plant_obj.shake_freq = plant_stage.shake_freq;
                plant_obj.can_bounce = plant_stage.can_bounce;
                plant_obj.bounce_delta = plant_stage.bounce_delta;

                // deactivate inactive stages for newly spawned plant_obj
                int k = 0;
                foreach (Transform t in plant_obj.transform) {
                    if (k != stage_index) {
                        t.gameObject.SetActive(false);
                    }
                    k++;
                }

                // deactivate all variations in chosen plant stage
                if (!stages[stage_index].is_stack) {
                    foreach (Transform t in plant_obj.transform.GetChild(stage_index)) {
                        // do not deactivate the physics_obj
                        if (t.gameObject.name != "Physics_Obj")
                            t.gameObject.SetActive(false);
                    }

                    // choose the variation to activate
                    List<float> w_range = new List<float>();
                    for (int t = 0; t < stages[stage_index].types; ++t) {
                        w_range.Add(t);
                        w_range.Add(t);
                        w_range.Add(stages[stage_index].sprite_probs[t] * 100);
                    }
                    int type_index = (int) Utils.weightedRange(w_range.ToArray());
                    // set stage_obj
                    plant_obj.stage_obj = plant_obj.transform.GetChild(stage_index).gameObject;
                    // type_obj =
                    plant_obj.stage_obj.transform.GetChild(type_index).gameObject.SetActive(true);
                } else {
                    plant_obj.stage_obj = plant_obj.transform.GetChild(stage_index).gameObject;
                }
            }
        }
    }
}
