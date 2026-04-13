using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public static PlayerController instance;
    public float moveSpeed, gravityModifier,jumpPower, runSpeed = 12f;
    public CharacterController charCon;

    private Vector3 moveInput;
    public Transform camTrans;

    public float mouseSensitivity;
    public bool invertX;
    public bool invertY;

    private bool canJump, canDoubleJump;
    public Transform groundCheckPoint;
    public LayerMask whatIsGround;

    public Animator anim;

    public GameObject bullet;
    public Transform firePoint;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {

        //sabt y velocity
        float yStore = moveInput.y;

        //Control Player Move

        //agar injori code benevisim player ma harekat mikone vali agar 90 daraje becharkhe dg forward un forward nist 
        // va dar asl be samte forwarde global harekat mikone na forward khodesh

        //moveInput.x = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        //moveInput.z = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        //---------------------------------------------------------------------------------------
        //dar in code ba charkhesh doorbin forward ma ham taghir mikone
        Vector3 vertMove = transform.forward * Input.GetAxis("Vertical");
        Vector3 horiMove = transform.right * Input.GetAxis("Horizontal");

        moveInput = horiMove + vertMove;
        moveInput.Normalize();


        // Sprint
        if(Input.GetKey(KeyCode.LeftShift))
        {
            moveInput = moveInput * runSpeed;
        }
        else
        {
        moveInput = moveInput * moveSpeed;
        }

        moveInput.y = yStore;

        //(moveInput.y = moveInput.y + Physics.gravity.y * gravityModifier...; =>)
        moveInput.y += Physics.gravity.y * gravityModifier * Time.deltaTime;
        //ye soal rajebe gravity daram inja ke badan net ha vasl shod bayad beporsam(Ep15).
        //dar kol baes mishe ke jazebe nesbat be ertefa sanjide beshe yani ertefa balatar bashe jazebe oftadan bishtar va baraks
        if(charCon.isGrounded)
        {
            moveInput.y = Physics.gravity.y * gravityModifier * Time.deltaTime;
        }

        //Handle Jumping

        //(OverlapSphere)=> baraye in hast ke in method ye kore farzi dore sphere man mikeshe ke ma nmitonim bbinim 
        //va karesh baresi kardan ine ke aya barkhordi ba layer soorat gerefte ya na ya har barkhord ya collideri
        canJump = Physics.OverlapSphere(groundCheckPoint.position, .25f, whatIsGround).Length > 0;
        //in khat be ine manie k mikhad check kone ma overlapsphere darim dar in point,size va check mikone ke ru ground hast ya na 
        //va lenght > 0 be in manie ke man niaz nadaram bedonm be chand ta jesm collide karde harchi bood okeye.

        if(canJump)
        {
            canDoubleJump = false;
        }

        if(Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            moveInput.y = jumpPower;

            canDoubleJump = true;
        }
        else if(canDoubleJump && Input.GetKeyDown(KeyCode.Space))
        {
            moveInput.y = jumpPower;

            canDoubleJump = false;
        }

        charCon.Move(moveInput * Time.deltaTime);

        //Control Camera Rotation
        Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

        if(invertX)
        {
            mouseInput.x = -mouseInput.x;
        }
        if(invertY)
        {
            mouseInput.y = -mouseInput.y;
        }

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f,mouseInput.x,0f));

        camTrans.rotation = Quaternion.Euler(camTrans.rotation.eulerAngles + new Vector3(-mouseInput.y,0f,0f));


        //Handle Shooting
        if(Input.GetMouseButtonDown(0))
        {

            RaycastHit hit;
            if(Physics.Raycast(camTrans.position, camTrans.forward,out hit,50f))
            {
                if(Vector3.Distance(camTrans.position, hit.point) > 2f)
                {
                    firePoint.LookAt(hit.point);
                }
            }
            else
            {
                firePoint.LookAt(camTrans.position + (camTrans.forward * 30f));
            }
            


            Instantiate(bullet, firePoint.position, firePoint.rotation);
        }



        //Apply Anim
        anim.SetFloat("moveSpeed", moveInput.magnitude);
        anim.SetBool("onGround", canJump);
    }
}
