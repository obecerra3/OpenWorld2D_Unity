using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Storage;
using static Notifications;
using static States;
using static Utils;
using Structs;

[System.Serializable]
public class Player : MonoEvent {
    //camera
    public GameObject main_camera;
    private float camera_z_distance_away = 10f;

    //================
    //   Constants
    //================
    public const int PLAYER_HEIGHT = 5;
    public const float WALK_SPEED = 4f;
    public const float RUN_SPEED = 6f;
    public const float ROLL_SPEED = 12f;
    public const float IN_AIR_SPEED = 3f;
    public const float JUMP_FORCE = -15f;
    public const float DIAG_MULT = 0.7f;
    public const float FACE_CHANGE_CHANCE = 0.2f;
    public const int GROUND_LAYER_MASK = 1 << 9;
    public const float GROUND_RAY_LENGTH = 0.4f;
    public const float GRAVITY_FORCE = 25f;

    //================
    //   Rendering
    //================
    public GameObject body_sprt_obj;
    public Animator body_anim;
    public SpriteRenderer body_sprt_rnd;
    public GameObject face_sprt_obj;
    public Animator face_anim;
    public SpriteRenderer face_sprt_rnd;
    private bool facing_right = true;
    private Vector3 localScale;
    private float og_y;
    private float y_offset;

    //================
    //    Physics
    //================
    public Rigidbody rb;
    public CapsuleCollider cc;
    public float diag_mult;

    //=================
    //     State
    //=================
    public States body_state;
    public States face_state;
    public Vector3 h_input;
    public Vector3 v_input;
    public SerializableVector3 position = new SerializableVector3();
    public bool on_ridge;
    public int ridge_count = 0;
    public float ridge_height = 0;


    //=================
    //     Data
    //=================
    public int health;
    public int mana;
    public int max_health;
    public int max_mana;

    //=================
    //     Debug
    //=================
    public bool debug_jump = false;
    public bool debug_grounded = false;
    public bool debug_print;

    //=================
    //   Initialize
    //=================
    public void Start() {
        main_camera = GameObject.Find("MainCamera");

        //physics
        cc = gameObject.AddComponent<CapsuleCollider>();
        cc.radius = 0.3f;
        cc.height = 0.7f;
        cc.material = Resources.Load<PhysicMaterial>("Player/player_physics_material");
        rb = gameObject.AddComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, 0, GRAVITY_FORCE);
        rb.freezeRotation = true;

        transform.position = Vector3.zero;

        //rendering
        body_sprt_obj = new GameObject("body_sprt_obj");
        body_sprt_obj.transform.parent = transform;
        body_sprt_rnd = body_sprt_obj.AddComponent<SpriteRenderer>();
        body_sprt_rnd.sortingLayerName = "Objects";
        body_anim = body_sprt_obj.AddComponent<Animator>();
        body_anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("PlayerBodyAnimator");

        face_sprt_obj = new GameObject("face_sprt_obj");
        face_sprt_obj.transform.parent = body_sprt_obj.transform;
        face_sprt_rnd = face_sprt_obj.AddComponent<SpriteRenderer>();
        face_sprt_rnd.sortingLayerName = "Objects";
        face_anim = face_sprt_obj.AddComponent<Animator>();
        face_anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("PlayerFaceAnimator");

        transform.position += Vector3.back * 10.0f;

        localScale = transform.localScale;
    }

    public override void load() {
        PlayerSave player_save = getActiveSave().player_save;
        position = player_save.position;
        health = player_save.health;
        mana = player_save.mana;
        max_health = player_save.max_health;
        max_mana = player_save.max_mana;
    }

    //=================
    //     Update
    //=================
    public void Update() {
        updateCamera();
        updateInput();

        if (transform.position != position) {
            notify(PLAYER_POS_CHANGED, new List<object>() {
                new Vector2Int((int) transform.position.x, (int) transform.position.y),
            });
            position = transform.position;
        }
    }

    public void updateInput() {
        h_input = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        v_input = new Vector3(0f, Input.GetAxis("Vertical"), 0f);
        float input_check = Mathf.Abs(h_input.x) + Mathf.Abs(v_input.y);

        if (body_state != IN_AIR && body_state != LAND && body_state != ROLL) {
            if (input_check > 0) {
                if (Input.GetKey("left shift"))
                {
                    body_state = RUN;
                    body_anim.SetTrigger("isRun");
                } else {
                    body_state = WALK;
                    body_anim.SetTrigger("isWalk");
                }

                diag_mult = (input_check == 2) ? DIAG_MULT : 1f;
            }
            else {
                body_state = IDLE;
                body_anim.SetTrigger("isIdle");
            }

            if (debug_jump || Input.GetKeyDown("space"))
                Jump();

            if (Input.GetMouseButtonDown(1))
                Roll();
        }
    }

    public void updateCamera() {
        main_camera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - camera_z_distance_away);
    }

    public void FixedUpdate() {
        updatePhysics();
    }

    public void updatePhysics() {
        switch (body_state) {
            case (IDLE):
                rb.velocity = Vector3.zero;
                break;
            case (WALK):
                rb.velocity = new Vector3(h_input.x * WALK_SPEED, v_input.y * WALK_SPEED, rb.velocity.z);
                rb.velocity *= diag_mult;
                break;
            case (RUN):
                rb.velocity = new Vector3(h_input.x * RUN_SPEED, v_input.y * RUN_SPEED, rb.velocity.z);
                rb.velocity *= diag_mult;
                break;
            case (IN_AIR):
                float y_offset = (v_input.y * IN_AIR_SPEED) + (rb.velocity.z * -1.0f * 0.5f);

                body_sprt_rnd.sortingLayerName = "Objects2";
                face_sprt_rnd.sortingLayerName = "Objects2";

                if (rb.velocity.z < 0) {
                    float size_offset = 1.0f + (rb.velocity.z * -1.0f * 0.02f);
                    body_sprt_obj.transform.localScale = new Vector3(size_offset, size_offset, 1.0f);
                    face_sprt_obj.transform.localScale = new Vector3(1.0f - (size_offset - 1.0f), 1.0f - (size_offset - 1.0f), 1.0f);
                }

                rb.velocity = new Vector3(h_input.x * IN_AIR_SPEED, y_offset, rb.velocity.z);
                break;
        }

        if (on_ridge && transform.position.z > ridge_height) {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Min(0.0f, rb.velocity.y), rb.velocity.z);
        }
    }

    public void LateUpdate() {
        checkAnimations();
    }

    public void checkAnimations() {
        //facing left or right localScale inversion
        if (body_state != ROLL) {
            if (h_input.x > 0)
                facing_right = true;
            else if (h_input.x < 0)
                facing_right = false;

            if (((facing_right) && (localScale.x < 0)) || ((!facing_right) && (localScale.x > 0))) {
                if (Random.value < FACE_CHANGE_CHANCE)
                    face_anim.SetTrigger("face_right_change");
                localScale.x *= -1;
            }

            transform.localScale = localScale;
        }

        //Falling/ Landing Checks
        if (body_state != IN_AIR && body_state != ROLL && !isGrounded()) {
            body_state = IN_AIR;
            body_anim.SetTrigger("isFall");
        }

        if (body_state == IN_AIR && isGrounded() && rb.velocity.z >= 0) {
            Land();
        }

        //ROLL change to IDLE
        if (body_state == ROLL) {
            string clip_name = body_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (clip_name != "player_roll_left" && clip_name != "player_roll_right" && clip_name != "player_land")
                body_state = IDLE;
        }
    }

    public void onNotify(Notifications _notification, List<object> _data) {

    }

    //==============
    //   HELPERS
    //==============

    public bool isGrounded() {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), GROUND_RAY_LENGTH, GROUND_LAYER_MASK))
            return true;
        return false;
    }

    public void Roll() {
        body_state = ROLL;
        Vector3 roll_dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;
        rb.velocity = new Vector3(roll_dir.x * ROLL_SPEED, roll_dir.y * ROLL_SPEED, rb.velocity.z);

        if (roll_dir.x >= 0) {
            body_anim.SetTrigger("isRollRight");
            if (localScale.x < 0) {
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        } else {
            body_anim.SetTrigger("isRollLeft");
            if (localScale.x > 0) {
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
    }

    public void Jump() {
        body_state = IN_AIR;
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, JUMP_FORCE);
        body_anim.SetTrigger("isJump");
        body_anim.SetTrigger("isFall");
        debug_jump = false;
    }

    public void Land() {
        body_state = LAND;
        body_sprt_rnd.sortingLayerName = "Objects";
        face_sprt_rnd.sortingLayerName = "Objects";
        body_anim.SetTrigger("isLand");
        revertState();
    }

    public void revertState() {
        if (body_state != ROLL) {
            float input_check = Mathf.Abs(h_input.x) + Mathf.Abs(v_input.y);
            if (input_check > 0) {
                if (Input.GetKey("left shift")) {
                    body_state = RUN;
                    body_anim.SetTrigger("isRun");
                } else {
                    body_state = WALK;
                    body_anim.SetTrigger("isWalk");
                }
            } else {
                body_state = IDLE;
                body_anim.SetTrigger("isIdle");
            }
        }
    }

    //===============
    //   RIGIDBODY
    //===============

    public void OnTriggerEnter(Collider collider) {
        switch (collider.gameObject.tag) {
            case ("Plant"):
                collider.gameObject.transform.parent.transform.parent.GetComponent<Plants.PlantObj>().shake();
                break;
        }
    }

    public void OnTriggerExit(Collider collider) {
        switch (collider.gameObject.tag) {
            case ("Plant"):
                collider.gameObject.transform.parent.transform.parent.GetComponent<Plants.PlantObj>().shake();
                break;
        }
    }

    public void OnCollisionEnter(Collision collision) {
        GameObject col_obj = collision.gameObject;
        if (col_obj.layer == 10) {
            int z = (int)char.GetNumericValue(col_obj.name[col_obj.name.Length - 1]);
            ridge_height = z * -TilemapManager.LEVEL_HEIGHT;
            on_ridge = true;
            ridge_count++;
        } else if (col_obj.tag == "Plant") {
            if (col_obj.transform.position.z > transform.position.z) {
                if (col_obj.transform.parent.parent.gameObject.GetComponent<Plants.PlantObj>().bounce()) {
                    rb.AddForce(Vector3.back * 20f, ForceMode.VelocityChange);
                }
            }
        }
    }

    public void OnCollisionExit(Collision collision) {
        if (collision.gameObject.layer == 10 && on_ridge) {
            ridge_count--;
            if (ridge_count == 0) {
                on_ridge = false;
            }
        }
    }

}
