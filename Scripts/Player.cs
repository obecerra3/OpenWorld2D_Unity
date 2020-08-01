using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Storage;
using static Notifications;
using static States;
using static Utils;
using Structs;

[System.Serializable]
public class Player : MonoEvent
{
    //camera
    public GameObject main_camera;
    private float camera_z_distance_away = 10f;

    //================
    //   Constants
    //================
    public const int PLAYER_HEIGHT = 5;
    public const float WALK_SPEED = 4f;
    public const float RUN_SPEED = 6f;
    public const float ROLL_SPEED = 8f;
    public const float INAIR_SPEED = 3f;
    public const float SLIDE_SPEED = 3f;
    public const float JUMP_FORCE = -10f;
    public const float DIAG_MULT = 0.7f;
    public const float FACE_CHANGE_CHANCE = 0.2f;
    public const int GROUND_LAYER_MASK = 1 << 9;
    public const float GROUND_RAY_LENGTH = 0.4f;

    //================
    //   Rendering
    //================
    public Animator body_anim;
    public SpriteRenderer body_sprite;
    public GameObject face_obj;
    public Animator face_anim;
    public SpriteRenderer face_sprite;
    private bool facing_right = true;
    private Vector3 localScale;

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
    private Vector3 slide_pos;
    private Vector3 slide_dir;

    //=================
    //   Game Status
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
    public void Start()
    {
        main_camera = GameObject.Find("MainCamera");

        //physics
        cc = gameObject.AddComponent<CapsuleCollider>();
        cc.radius = 0.3f;
        cc.height = 0.7f;
        cc.material = Resources.Load<PhysicMaterial>("Player/player_physics_material");
        rb = gameObject.AddComponent<Rigidbody>();
        Physics.gravity = new Vector3(0, 0, 10f);
        rb.freezeRotation = true;
        transform.position = new Vector3(0, 0, -6);

        //rendering
        body_sprite = GetComponent<SpriteRenderer>();
        body_sprite.sortingLayerName = "Player";
        body_anim = GetComponent<Animator>();
        body_anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("PlayerBodyAnimator");

        face_obj = new GameObject("PlayerChild");
        face_obj.transform.parent = transform;
        face_obj.transform.position = new Vector3(0, 0, -6);
        face_sprite = face_obj.AddComponent<SpriteRenderer>();
        face_sprite.sortingLayerName = "PlayerFace";
        face_anim = face_obj.AddComponent<Animator>();
        face_anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("PlayerFaceAnimator");

        localScale = transform.localScale;
    }

    public override void load()
    {
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
    public void Update()
    {
        updateCamera();
        updateInput();

        if (transform.position != position)
        {
            notify(PLAYER_POS_CHANGED, new List<object>()
            {
                new Vector2Int((int) transform.position.x, (int) transform.position.y),
            });
            position = transform.position;
        }
    }

    public void updateInput()
    {
        h_input = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        v_input = new Vector3(0f, Input.GetAxis("Vertical"), 0f);
        float input_check = Mathf.Abs(h_input.x) + Mathf.Abs(v_input.y);

        if (body_state != INAIR && body_state != LAND && body_state != ROLL && body_state != SLIDE)
        {
            if (input_check > 0)
            {
                if (Input.GetKey("left shift"))
                {
                    body_state = RUN;
                    body_anim.SetTrigger("isRun");
                } else
                {
                    body_state = WALK;
                    body_anim.SetTrigger("isWalk");
                }

                diag_mult = (input_check == 2) ? DIAG_MULT : 1f;
            }
            else
            {
                body_state = IDLE;
                body_anim.SetTrigger("isIdle");
            }

            if (debug_jump || Input.GetKeyDown("space"))
                Jump();

            if (Input.GetMouseButtonDown(1))
                Roll();
        }
    }

    public void updateCamera()
    {
        main_camera.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - camera_z_distance_away);
    }

    public void FixedUpdate()
    {
        updatePhysics();
        checkSlide();
    }

    public void updatePhysics()
    {
        switch (body_state)
        {
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
            case (INAIR):
                rb.velocity = new Vector3(h_input.x * INAIR_SPEED, v_input.y * INAIR_SPEED, rb.velocity.z);
                break;
        }
    }

    public void checkSlide()
    {
        // SLIDE change to INAIR
        if (body_state == SLIDE)
        {
            if (Utils.AlmostEqual(slide_pos, transform.position, 0.1f))
            {
                rb.isKinematic = false;
                body_state = INAIR;
            } else
            {
                rb.MovePosition(transform.position + slide_dir * Time.fixedDeltaTime * SLIDE_SPEED);
                slide_dir = (slide_pos - transform.position).normalized;
            }
        }
    }

    public void LateUpdate()
    {
        checkAnimations();
    }

    public void checkAnimations()
    {
        //facing left or right localScale inversion
        if (body_state != ROLL)
        {
            if (h_input.x > 0)
                facing_right = true;
            else if (h_input.x < 0)
                facing_right = false;

            if (((facing_right) && (localScale.x < 0)) || ((!facing_right) && (localScale.x > 0)))
            {
                if (Random.value < FACE_CHANGE_CHANCE)
                    face_anim.SetTrigger("face_right_change");
                localScale.x *= -1;
            }

            transform.localScale = localScale;
        }

        //Falling/ Landing Checks
        if (body_state != INAIR && body_state != ROLL && body_state != SLIDE && !isGrounded())
        {
            body_state = INAIR;
            body_anim.SetTrigger("isFall");
        }

        if (body_state == INAIR && isGrounded() && rb.velocity.z >= 0)
        {
            Land();
        }

        //ROLL change to IDLE
        if (body_state == ROLL)
        {
            string clip_name = body_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (clip_name != "player_roll_left" && clip_name != "player_roll_right" && clip_name != "player_land")
                body_state = IDLE;
        }
    }

    public override void onNotify(Notifications _notification, List<object> _data)
    {
    }

    //==============
    //   HELPERS
    //==============

    public bool isGrounded()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), GROUND_RAY_LENGTH, GROUND_LAYER_MASK))
            return true;
        return false;
    }

    public void Roll()
    {
        body_state = ROLL;
        Vector3 roll_dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;
        rb.velocity = new Vector3(roll_dir.x * ROLL_SPEED, roll_dir.y * ROLL_SPEED, rb.velocity.z);

        if (roll_dir.x >= 0)
        {
            body_anim.SetTrigger("isRollRight");
            if (localScale.x < 0)
            {
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
        else
        {
            body_anim.SetTrigger("isRollLeft");
            if (localScale.x > 0)
            {
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
    }

    public void Jump()
    {
        body_state = INAIR;
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, JUMP_FORCE);
        body_anim.SetTrigger("isJump");
        body_anim.SetTrigger("isFall");
        debug_jump = false;
    }

    public void Land()
    {
        body_state = LAND;
        body_anim.SetTrigger("isLand");
        revertStateLand();
    }

    public void revertStateLand()
    {
        if (body_state != ROLL)
        {
            float input_check = Mathf.Abs(h_input.x) + Mathf.Abs(v_input.y);
            if (input_check > 0)
            {
                if (Input.GetKey("left shift"))
                {
                    body_state = RUN;
                    body_anim.SetTrigger("isRun");
                } else
                {
                    body_state = WALK;
                    body_anim.SetTrigger("isWalk");
                }
            }
            else
            {
                body_state = IDLE;
                body_anim.SetTrigger("isIdle");
            }
        }
    }

    //===============
    //   RIGIDBODY
    //===============

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 10 && body_state != SLIDE && body_state != INAIR)
        {
            slide_pos = collider.bounds.center + Vector3.down;
            slide_dir = (slide_pos - transform.position).normalized;
            body_state = SLIDE;
            rb.isKinematic = true;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {

    }

    public void OnCollisionExit(Collision collision)
    {

    }

}
