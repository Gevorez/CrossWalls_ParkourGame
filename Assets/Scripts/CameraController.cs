using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    float headRotation;
    bool rotateRight;
    float cameraRotationSpeed;
    float velocity;

    public Camera mainCamera;
    public Camera weaponCamera;
    public PlayerMovement playerMove;
    float sensX = 1f;
    float sensY = 1f;
    float baseFov = 90f;
    float maxFov = 140f;
    float wallRunTilt = 15f;

    float wishTilt = 0;
    float curTilt = 0;
    Vector2 currentLook;
    Vector2 sway = Vector3.zero;
    float fov;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        curTilt = transform.localEulerAngles.z;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        headRotation =0f;
        rotateRight = true;
    }

    void Update()
    {
      
        RotateMainCamera();
        if(playerMove.rb.velocity.y != 0)
        {
            RotateOnMove();
          
        }  
          transform.Rotate(new Vector3(headRotation,0,0));
         


    }

    void FixedUpdate()
    { 
        currentLook = Vector2.Lerp(currentLook, currentLook + sway, 0.8f);
        curTilt = Mathf.LerpAngle(curTilt, wishTilt * wallRunTilt, 0.05f);
        sway = Vector2.Lerp(sway, Vector2.zero, 0.2f);
    }

    void RotateMainCamera()
    {
        Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        mouseInput.x *= sensX;
        mouseInput.y *= sensY;

        currentLook.x += mouseInput.x;
        currentLook.y = Mathf.Clamp(currentLook.y += mouseInput.y, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(-currentLook.y, Vector3.right);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, curTilt);
        transform.root.transform.localRotation = Quaternion.Euler(0, currentLook.x, 0);
    }

    void RotateOnMove()
    {
    
       
        
        if(playerMove.rb.velocity.x <0)
        {
            velocity = (-1)*(playerMove.rb.velocity.x) + playerMove.rb.velocity.z;
        }

        if(playerMove.rb.velocity.z<0)
        {
            velocity = playerMove.rb.velocity.x + (-1)*(playerMove.rb.velocity.z);
        } 

        if(playerMove.rb.velocity.x < 0 && playerMove.rb.velocity.z < 0)
        {
             velocity = (-1)*(playerMove.rb.velocity.x) + (-1)*(playerMove.rb.velocity.z);
        }

        if(playerMove.rb.velocity.x > 0 && playerMove.rb.velocity.z > 0)
        {
            velocity = playerMove.rb.velocity.x + playerMove.rb.velocity.z;
        }

       
        if(playerMove.rb.velocity.x>0)
        {
            cameraRotationSpeed = 1 + (velocity /6);
        }
        if(playerMove.rb.velocity.x<0)
        {
            cameraRotationSpeed = 1 + (velocity /6);

        }
        





        
        if(playerMove.walk == true)
        {
            if( rotateRight == true)
            {
                headRotation -= Time.deltaTime * 2 * cameraRotationSpeed;
                if(headRotation < -0.5f )
                {
                    rotateRight = false;
                    headRotation = -0.5f;
                }
            }
        
            if(rotateRight==false )
            {
                headRotation += Time.deltaTime * 2 * cameraRotationSpeed;
                if(headRotation > 0.5f )
                {
                    headRotation = 0.5f;
                    rotateRight = true;
                }
            }
        }
    }

    public void Punch(Vector2 dir)
    {
        sway += dir;
    }

    #region Setters
    public void SetTilt(float newVal)
    {
        wishTilt = newVal;
    }

    public void SetXSens(float newVal)
    {
        sensX = newVal;
    }

    public void SetYSens(float newVal)
    {
        sensY = newVal;
    }

    public void SetFov(float newVal)
    {
        baseFov = newVal;
    }

    #endregion
}
