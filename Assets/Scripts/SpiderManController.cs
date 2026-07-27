using UnityEngine;

public class SpiderManController : MonoBehaviour
{
    public static SpiderManController instance;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float gravityModifier = 2f;
    public float jumpPower = 12f;
    public float runSpeed = 12f;
    public float airControlSpeed = 5f;

    [Header("Web Swinging")]
    public float webSwingSpeed = 15f;
    public float webMaxLength = 20f;
    public float webLaunchPower = 10f;
    public LayerMask webableSurfaces;

    [Header("Wall Cling")]
    public float wallClingSpeed = 3f;
    public float wallSlideSpeed = 2f;
    public float wallJumpPower = 8f;

    [Header("References")]
    public CharacterController charCon;
    public Transform camTrans;
    public Transform groundCheckPoint;
    public Transform webCheckPoint;
    public LayerMask whatIsGround;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public bool invertX;
    public bool invertY;

    private Vector3 moveInput;
    private Vector3 webPoint;
    private bool canJump;
    private bool isWebSwinging;
    private bool isWallClinging;
    private bool wallClingLeft;
    private bool wallClingRight;

    public Animator anim;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        canJump = Physics.OverlapSphere(groundCheckPoint.position, .25f, whatIsGround).Length > 0;

        CheckWallCling();

        if (isWallClinging)
        {
            HandleWallClingMovement();
        }
        else if (isWebSwinging)
        {
            HandleWebSwing();
        }
        else
        {
            HandleNormalMovement();
        }

        HandleJumping();

        charCon.Move(moveInput * Time.deltaTime);

        HandleCamera();

        anim.SetFloat("moveSpeed", moveInput.magnitude);
        anim.SetBool("onGround", canJump);
        anim.SetBool("isWebSwinging", isWebSwinging);
        anim.SetBool("isWallClinging", isWallClinging);
    }

    void CheckWallCling()
    {
        if (canJump) return;

        RaycastHit leftHit, rightHit;
        wallClingLeft = Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.right * -1, out leftHit, 1f, webableSurfaces);
        wallClingRight = Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.right, out rightHit, 1f, webableSurfaces);

        if (wallClingLeft || wallClingRight)
        {
            if (moveInput.y < 0) moveInput.y = -wallSlideSpeed;
            isWallClinging = true;
            transform.rotation = Quaternion.FromToRotation(transform.forward, new Vector3(transform.forward.x, 0, transform.forward.z).normalized) * transform.rotation;
        }
        else
        {
            isWallClinging = false;
        }
    }

    void HandleWallClingMovement()
    {
        moveInput = Vector3.zero;
        moveInput.y = -wallSlideSpeed;
        moveInput += transform.forward * Input.GetAxis("Vertical") * wallClingSpeed;
        moveInput += transform.right * Input.GetAxis("Horizontal") * wallClingSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 jumpDir = (wallClingLeft ? transform.right : -transform.right) * wallJumpPower;
            jumpDir.y = jumpPower;
            moveInput = jumpDir;
            isWallClinging = false;
        }
    }

    void HandleNormalMovement()
    {
        Vector3 vertMove = transform.forward * Input.GetAxis("Vertical");
        Vector3 horiMove = transform.right * Input.GetAxis("Horizontal");
        moveInput = (vertMove + horiMove);
        moveInput.Normalize();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveInput *= runSpeed;
        }
        else if (!canJump)
        {
            moveInput = new Vector3(moveInput.x, moveInput.y * airControlSpeed, moveInput.z);
        }
        else
        {
            moveInput *= moveSpeed;
        }

        moveInput.y += Physics.gravity.y * gravityModifier * Time.deltaTime;

        if (charCon.isGrounded && moveInput.y < 0)
        {
            moveInput.y = Physics.gravity.y * gravityModifier * Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryWebShoot();
        }
    }

    void TryWebShoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(webCheckPoint.position, camTrans.forward, out hit, webMaxLength, webableSurfaces))
        {
            webPoint = hit.point;
            isWebSwinging = true;
            moveInput = (webPoint - transform.position).normalized * webLaunchPower;
            moveInput.y = 0;
        }
    }

    void HandleWebSwing()
    {
        float distance = Vector3.Distance(transform.position, webPoint);

        if (distance < 2f)
        {
            isWebSwinging = false;
            return;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 swingDir = Quaternion.Euler(0, 90, 0) * camTrans.forward;
            swingDir.y = 0;
            moveInput = swingDir * webSwingSpeed;

            moveInput += Physics.gravity * gravityModifier * 0.5f * Time.deltaTime;
        }
        else
        {
            isWebSwinging = false;
            moveInput += Physics.gravity * gravityModifier * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            moveInput.y = jumpPower;
            isWebSwinging = false;
        }
    }

    void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canJump && !isWebSwinging && !isWallClinging)
        {
            moveInput.y = jumpPower;
        }
    }

    void HandleCamera()
    {
        Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

        if (invertX) mouseInput.x = -mouseInput.x;
        if (invertY) mouseInput.y = -mouseInput.y;

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, mouseInput.x, 0f));
        camTrans.rotation = Quaternion.Euler(camTrans.rotation.eulerAngles + new Vector3(-mouseInput.y, 0f, 0f));
    }
}