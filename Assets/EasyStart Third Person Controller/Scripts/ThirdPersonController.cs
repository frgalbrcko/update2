
using UnityEditor.VersionControl;
using UnityEngine;


public class ThirdPersonController : MonoBehaviour
{

    public float velocity = 5f;  
    public float sprintAdittion = 3.5f;
    public float jumpForce = 18f;
    public float jumpTime = 0.85f;
    public float gravity = 9.8f;

    float jumpElapsedTime = 0;

    // hráč
    bool isJumping = false;
    bool isSprinting = false;
    bool isCrouching = false;

    // vstupy
    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    Animator animator;
    CharacterController cc;


    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // animace jeste neanimacujou
        if (animator == null)
            Debug.LogWarning("neanimacuje");
    }


    //update
    void Update()
    {

        // vtupsní checket
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

        // kontrola funkce "crouch"
        if (inputCrouch)
            isCrouching = !isCrouching;

        // animace na skrčení která neamimacuje jeste
        if (cc.isGrounded && animator != null)
        {

            // Crouch
            animator.SetBool("crouch", isCrouching);

            // Run
            float minimumSpeed = 0.9f;
            animator.SetBool("run", cc.velocity.magnitude > minimumSpeed);

            // Sprint
            isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
            animator.SetBool("sprint", isSprinting);

        }

        // skok animace co neanimacuje
        if (animator != null)
            animator.SetBool("air", cc.isGrounded == false);

        // jumpík pipík
        if (inputJump && cc.isGrounded)
        {
            isJumping = true;
            //kdyz skocim tak se nektrcim zeo logicky
        }

        HeadHittingDetect();

    }


    //chtra vecicka 
    private void FixedUpdate()
    {

        // velocity boos .D
        float velocityAdittion = 0;
        if (isSprinting)
            velocityAdittion = sprintAdittion;
        if (isCrouching)
            velocityAdittion = -(velocity * 0.50f); // -50% sranda mínus

        // movement
        float directionX = inputHorizontal * (velocity + velocityAdittion) * Time.deltaTime;
        float directionZ = inputVertical * (velocity + velocityAdittion) * Time.deltaTime;
        float directionY = 0;

        // Jumpíkování
        if (isJumping)
        {

           
            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;

            // odleva od skoku
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }

        //PROC NEWTON VYTVORIL GRAVITACI
        directionY = directionY - gravity * Time.deltaTime;


        // ROTACE CHARACTERU  

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        forward = forward * directionZ;
        right = right * directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        //KONEC ROTACE


        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 moviment = verticalDirection + horizontalDirection;
        cc.Move(moviment);

    }


    //Model registruje kdyz hitne strop pri skoku
    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

       

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc))
        {
            jumpElapsedTime = 0;
            isJumping = false;
        }
    }

}