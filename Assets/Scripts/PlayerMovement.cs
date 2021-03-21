﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    //Ground
    float groundSpeed = 8f;
    float runSpeed = 10f;
    float grAccel = 30f;
    float crouchSpeed = 5f;
    float slideSpeed = 10f;

    //Air
    float airSpeed = 4f;
    float airAccel = 4f;

    //Jump
    float jumpUpSpeed = 5.0f;
    float dashSpeed = 5f;

    //Wall
    float wallSpeed = 12f;
    float wallClimbSpeed = 0f;
    float wallAccel = 15f;
    float wallRunTime = 3f;
    float wallStickiness = 66f;
    float wallStickDistance = 1f;
    float wallFloorBarrier = 40f;
    float wallBanTime = 3f;
    Vector3 bannedGroundNormal;

    //Cooldowns
    bool canJump = true;
    bool canDJump = true;
    float wallBan = 0f;
    float wrTimer = 0f;
    float wallStickTimer = 0.0f;
    float dashCooldown = 0.0f;

    public Slider showDashCooldown;

    //States
    bool running;
    bool jump;
    bool crouch;
    bool grounded;
    bool canSlide;
    public bool walk;

    float height;
    float yPosition;

    //Audios
    public AudioSource boostSound;
    public AudioSource jumpSound;
    public AudioSource slideSound;


    Vector3 groundNormal = Vector3.up;

    enum Mode
    {
        Walking,
        Flying,
        Wallruning,

    }
    Mode mode = Mode.Flying;

    CameraController camCon;
    CapsuleCollider pm;
    public Rigidbody rb;
    Vector3 dir = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<CapsuleCollider>();
        camCon = GetComponentInChildren<CameraController>();
        height = pm.height;
    }

    void OnGUI()
    {
        GUILayout.Label("Spid: " + new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);
        GUILayout.Label("SpidUp: " + rb.velocity.y);
    }

    void dashCountdown()
    {
        if (dashCooldown < 0.0f)
        {
            dashCooldown += Time.deltaTime;
        }
        else if (dashCooldown > 0.0f)
        {
            dashCooldown = 0f;
        }
    }


    void Update()
    {
        dir = Direction();

        showDashCooldown.value = dashCooldown;

        crouch = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C));
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetAxisRaw("Mouse ScrollWheel") != 0)
        {
            jump = true;

        }

        if (Input.GetKeyDown(KeyCode.R)){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldown >= 0.0f){
            dashCooldown = -5f;
            boostSound.Play();
            rb.AddForce(dir * 5f, ForceMode.Impulse);
            running = true;
            
        }else if(dashCooldown < 0.0f)
        {
            dashCountdown();
            running =false;
        }

        
        //Crouching
        if(Input.GetKeyDown(KeyCode.LeftControl)||(Input.GetKeyDown(KeyCode.C)))
        {
            yPosition = transform.position.y;
            
            transform.position = new Vector3(transform.position.x, yPosition * 0.9f,transform.position.z);
        }

        if(crouch == true)
        {
            pm.height = height - 0.4f;
           
        }
        else
        {
            pm.height = height;
        }

        //Slide
        if(Input.GetKeyDown(KeyCode.LeftControl)&& Input.GetKey(KeyCode.W))
        {
            Slide();
        }
        else if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            GoUp();
            canSlide = false;
        }


    if(mode != Mode.Walking)
        {      
            walk = false;
        }

    }

    void FixedUpdate()
    {
        if (mode == Mode.Walking && rb.velocity.magnitude < 0.5f)
        {
            pm.material.dynamicFriction = 0.1f;
        }
        else
        {
            pm.material.dynamicFriction = 0f;
        }

        if (wallStickTimer == 0f && wallBan > 0f)
        {
            bannedGroundNormal = groundNormal;
        }
        else
        {
            bannedGroundNormal = Vector3.zero;
        }

        wrTimer = Mathf.Max(wrTimer - Time.deltaTime, 0f);
        wallStickTimer = Mathf.Max(wallStickTimer - Time.deltaTime, 0f);
        wallBan = Mathf.Max(wallBan - Time.deltaTime, 0f);

        switch (mode)
        {
            case Mode.Wallruning:
                camCon.SetTilt(WallrunCameraAngle());
                Wallrun(dir, wallSpeed, wallClimbSpeed, wallAccel);
                canSlide =false;
                break;

            case Mode.Walking:
                camCon.SetTilt(0);
                Walk(dir, running ? runSpeed : groundSpeed,grAccel);
                break;

            case Mode.Flying:
                camCon.SetTilt(0);
                AirMove(dir, airSpeed, airAccel);
                break;
            
            
        }

        jump = false;
    }



    private Vector3 Direction()
    {
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(hAxis, 0, vAxis);
        return rb.transform.TransformDirection(direction);
    }



    #region Collisions
    void OnCollisionStay(Collision collision)
    {
        if (collision.contactCount > 0)
        {
            float angle;

            foreach (ContactPoint contact in collision.contacts)
            {
                angle = Vector3.Angle(contact.normal, Vector3.up);
                if (angle < wallFloorBarrier)
                {
                    EnterWalking();
                    grounded = true;
                    groundNormal = contact.normal;
                    return;
                }
            }

            if (VectorToGround().magnitude > 0.2f)
            {
                grounded = false;
            }

            if (grounded == false)
            {
                foreach (ContactPoint contact in collision.contacts)
                {
                    if (contact.otherCollider.tag != "NoWallrun" && contact.otherCollider.tag != "Player" && mode != Mode.Walking)
                    {
                        angle = Vector3.Angle(contact.normal, Vector3.up);
                        if (angle > wallFloorBarrier && angle < 120f)
                        {
                            grounded = true;
                            groundNormal = contact.normal;
                            EnterWallrun();
                            return;
                        }
                    }
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.contactCount == 0)
        {
            EnterFlying();
        }
    }
    #endregion



    #region Entering States
    void EnterWalking()
    {
        if (mode != Mode.Walking && canJump)
        {
            if (mode == Mode.Flying && crouch)
            {
                rb.AddForce(-rb.velocity.normalized, ForceMode.VelocityChange);
                
            }
            if (rb.velocity.y < -1.2f)
            {
                //camCon.Punch(new Vector2(0, -3f));
            }
            
            mode = Mode.Walking;
        }
    }

    void EnterFlying(bool wishFly = false)
    {
        grounded = false;
        if (mode == Mode.Wallruning && VectorToWall().magnitude < wallStickDistance && !wishFly)
        {
            return;
            canSlide = false;
        }
        else if (mode != Mode.Flying)
        {

            wallBan = wallBanTime;
            canDJump = true;
            mode = Mode.Flying;
        }
    }

    void EnterWallrun()
    {
        canSlide =false;
        if (mode != Mode.Wallruning)
        {
            if (VectorToGround().magnitude > 0.2f && CanRunOnThisWall(bannedGroundNormal) && wallStickTimer == 0f)
            {
                wrTimer = wallRunTime;
                canDJump = true;
                mode = Mode.Wallruning;
                canSlide =false;
            }
            else
            {
                EnterFlying(true);
            }
        }
    }
    #endregion



    #region Movement Types
    void Walk(Vector3 wishDir, float maxSpeed, float Acceleration)
    {
        canSlide=true;
        if (jump && canJump)
        {
            Jump();
        }
        
        else
        {
            wishDir = wishDir.normalized;
            Vector3 spid = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (spid.magnitude > maxSpeed) Acceleration *= spid.magnitude/maxSpeed;
            spid = wishDir * maxSpeed - spid;

            if (spid.magnitude < 0.5f)
            {
                Acceleration *= spid.magnitude / 0.5f;
            }

            spid = spid.normalized * Acceleration;
            float magn = spid.magnitude;
            spid = Vector3.ProjectOnPlane(spid, groundNormal);
            spid = spid.normalized;
            spid *= magn;

            rb.AddForce(spid, ForceMode.Acceleration);
        }
 
        walk = true;
    }

    void AirMove(Vector3 wishDir, float maxSpeed, float Acceleration)
    {
        canSlide =false;

        if (crouch && rb.velocity.y > -10 && Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }

        if (wishDir != Vector3.zero)
        {
            wishDir = wishDir.normalized;
            
            Vector3 projVel = Vector3.Project(rb.velocity, wishDir);

            
            bool isAway = Vector3.Dot(wishDir, projVel) <= 0f;

            
            if (projVel.magnitude < maxSpeed || isAway)
            {
                
                Vector3 vc = wishDir * Acceleration;

                
                rb.AddForce(vc, ForceMode.Acceleration);
            }
        }
    }

    void Wallrun(Vector3 wishDir, float maxSpeed, float climbSpeed, float Acceleration)
    {
        canSlide =false;
        if (jump)
        {
            
            float upForce = Mathf.Clamp(jumpUpSpeed - rb.velocity.y, 0, Mathf.Infinity);
            rb.AddForce(new Vector3(0, upForce, 0), ForceMode.VelocityChange);

            
            Vector3 jumpOffWall = groundNormal.normalized;
            jumpOffWall *= dashSpeed;
            jumpOffWall.y = 0;
            rb.AddForce(jumpOffWall, ForceMode.VelocityChange);
            wrTimer = 0f;
            EnterFlying(true);
            jumpSound.Play();
        }
        else if (wrTimer == 0f || crouch)
        {
            rb.AddForce(groundNormal * 3f, ForceMode.VelocityChange);
            EnterFlying(true);
        }
        else
        {
            
            Vector3 distance = VectorToWall();
            wishDir = RotateToPlane(wishDir, -distance.normalized);
            wishDir = wishDir.normalized * maxSpeed;
            wishDir.y = Mathf.Clamp(wishDir.y, -climbSpeed, climbSpeed);
            Vector3 wallrunForce = wishDir - rb.velocity;
            wallrunForce = wallrunForce.normalized * Acceleration;
            if (new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude > maxSpeed) wallrunForce /= 2f;

            
            if (rb.velocity.y < 0f && wishDir.y > 0f) wallrunForce.y = 2f * Acceleration;

            
            Vector3 antiGravityForce = -Physics.gravity;
            if (wrTimer < 0.33 * wallRunTime)
            {
                antiGravityForce *= wrTimer / wallRunTime;
                wallrunForce += antiGravityForce + Physics.gravity;
            }
            if (distance.magnitude > wallStickDistance) distance = Vector3.zero;

            
            rb.AddForce(antiGravityForce, ForceMode.Acceleration);
            rb.AddForce(distance.normalized * wallStickiness * Mathf.Clamp(distance.magnitude / wallStickDistance, 0, 1), ForceMode.Acceleration);
            rb.AddForce(wallrunForce, ForceMode.Acceleration);
        }
        if (!grounded)
        {
            wallStickTimer = 0.2f;
            EnterFlying();
        }
    }
    void Slide()
    {
        
        if(canSlide&&grounded)
        {
            pm.height = height - 0.2f;
            rb.AddForce(transform.forward * slideSpeed, ForceMode.VelocityChange);
            slideSound.Play();
        }
    }
    void GoUp()
    {
        pm.height = height;
        slideSound.Stop();
    }

    void Jump()
    {
        if (mode == Mode.Walking && canJump)
        {
            float upForce = Mathf.Clamp(jumpUpSpeed - rb.velocity.y, 0, Mathf.Infinity);
            rb.AddForce(new Vector3(0, upForce, 0), ForceMode.VelocityChange);
            StartCoroutine(jumpCooldownCoroutine(0.2f));
            EnterFlying(true);
            jumpSound.Play();
        }
    }

    #endregion



    #region Math
    Vector2 ClampedAdditionVector(Vector2 a, Vector2 b)
    {
        float k, x, y;
        k = Mathf.Sqrt(Mathf.Pow(a.x, 2) + Mathf.Pow(a.y, 2)) / Mathf.Sqrt(Mathf.Pow(a.x + b.x, 2) + Mathf.Pow(a.y + b.y, 2));
        x = k * (a.x + b.x) - a.x;
        y = k * (a.y + b.y) - a.y;
        return new Vector2(x, y);
    }

    Vector3 RotateToPlane(Vector3 vect, Vector3 normal)
    {
        Vector3 rotDir = Vector3.ProjectOnPlane(normal, Vector3.up);
        Quaternion rotation = Quaternion.AngleAxis(-90f, Vector3.up);
        rotDir = rotation * rotDir;
        float angle = -Vector3.Angle(Vector3.up, normal);
        rotation = Quaternion.AngleAxis(angle, rotDir);
        vect = rotation * vect;
        return vect;
    }

    float WallrunCameraAngle()
    {
        Vector3 rotDir = Vector3.ProjectOnPlane(groundNormal, Vector3.up);
        Quaternion rotation = Quaternion.AngleAxis(-90f, Vector3.up);
        rotDir = rotation * rotDir;
        float angle = Vector3.SignedAngle(Vector3.up, groundNormal, Quaternion.AngleAxis(90f, rotDir) * groundNormal);
        angle -= 90;
        angle /= 180;
        Vector3 playerDir = transform.forward;
        Vector3 normal = new Vector3(groundNormal.x, 0, groundNormal.z);

        return Vector3.Cross(playerDir, normal).y * angle;
    }

    bool CanRunOnThisWall(Vector3 normal)
    {
        if (Vector3.Angle(normal, groundNormal) > 10 || wallBan == 0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    Vector3 VectorToWall()
    {
        Vector3 direction;
        Vector3 position = transform.position + Vector3.down * 0.5f;
        RaycastHit hit;
        if (Physics.Raycast(position, -groundNormal, out hit, wallStickDistance) && Vector3.Angle(groundNormal, hit.normal) < 70)
        {
            groundNormal = hit.normal;
            direction = hit.point - position;
            return direction;
        }
        else
        {
            return Vector3.positiveInfinity;
        }
    }

    Vector3 VectorToGround()
    {
        Vector3 position = transform.position;
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, wallStickDistance))
        {
            return hit.point - position;
        }
        else
        {
            return Vector3.positiveInfinity;
        }
    }
    #endregion



    #region Coroutines
    IEnumerator jumpCooldownCoroutine(float time)
    {
        canJump = false;
        yield return new WaitForSeconds(time);
        canJump = true;
    }
    #endregion
}
