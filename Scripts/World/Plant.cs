using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Plants;
using static TilemapManager;
using static States;

namespace Plant {

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
            Plants.plant_ids.Add(ID, this);
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
                        if ((i + 1 < CHUNK_WIDTH && plant_map[i + 1, j].ID == ID)
                            || (j + 1 < CHUNK_HEIGHT && plant_map[i, j + 1].ID == ID)
                            || (i - 1 >= 0 && plant_map[i - 1, j].ID == ID)
                            || (j - 1 >= 0 && plant_map[i, j - 1].ID == ID))
                            add_to_map = false;
                    }

                    if (add_to_map)
                        possible_spawns.Add(kvp.Key);
                }
            }

            if (possible_spawns.Count > 0) {
                int rand_index = Random.Range(0, possible_spawns.Count);
                plant_map[i, j] = new PlantTile(ID, possible_spawns[rand_index]);
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
