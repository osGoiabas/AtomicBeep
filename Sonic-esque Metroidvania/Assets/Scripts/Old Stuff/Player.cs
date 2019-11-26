using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

    //-----------------------------------------------------------------------------------------------------
    // FÍSICA
    //-----------------------------------------------------------------------------------------------------
    
    Controller2D controller;
    Animator animator;

    float maxJumpHeight = 7;
    float minJumpHeight = 2;
    float timeToJumpApex = 0.4f;

    float maxJumpVelocity;
    float minJumpVelocity;

    float gravity;


    void Start() {
        controller = GetComponent<Controller2D>();
        animator = GetComponentInChildren<Animator>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity)) * minJumpHeight;
    }


    Vector3 moveAmount;

    void FixedUpdate() { //******tutorial usa Update ao invés de Fixed, sei lá porque

        CalculateVelocity();
        HandleWallSliding();

        controller.Move(moveAmount * Time.fixedDeltaTime);

        FazerAnimação();
        Balançar();

        // SE TOCAR NO TETO OU NO CHÃO, PARA DE SUBIR/DESCER
        if (controller.collisions.above || controller.collisions.below) {
            if (!controller.collisions.slidingDownMaxSlope) {
                moveAmount.y = 0;
            } 
        }
    }

    float moveSpeed = 20;
    /*
    float velocidadeMáximaX = 30;

    float velocityXSmoothing;
    float accelerationTime = .5f; //aceleração do movimento

    float aceleraçãoX = 0.2f;
    float deceleraçãoX = 0.5f;
    float fricção = 0.2f;
    */

    void CalculateVelocity() {
        //float targetVelocityX = directionalInput.x * moveSpeed;
        //Mathf.Clamp(targetVelocityX, 0, velocidadeMáximaX);
        //moveAmount.x = Mathf.SmoothDamp(moveAmount.x, targetVelocityX, ref velocityXSmoothing, accelerationTime);
        
        moveAmount.x = directionalInput.x * moveSpeed;
        moveAmount.y += gravity * Time.deltaTime;
    }





    bool wallSliding;
    int wallDirX;
    float wallSlideSpeedMax = 3;
    float wallStickTime = .1f;
    float timeToWallUnstick;

    void HandleWallSliding() { 
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && moveAmount.y < 0) {
            wallSliding = true;
            if (moveAmount.y < -wallSlideSpeedMax) {
                moveAmount.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0) {
                // velocityXSmoothing = 0;
                moveAmount.x = 0;
                if (directionalInput.x != wallDirX && directionalInput.x != 0) { 
                    timeToWallUnstick -= Time.deltaTime;
                } else {
                    timeToWallUnstick = wallStickTime;
                }                 
            } else {
                timeToWallUnstick = wallStickTime;
            }
        }
    }




    void FazerAnimação() {
        animator.SetFloat("velocidade", Mathf.Abs(moveAmount.x));
        //animator.speed = Mathf.Clamp(Mathf.Abs(moveAmount.x) / 5, 1, 2);
    }



    public void Balançar() {
        if (moveAmount.x == 0 && controller.estáBalançando) {
            animator.SetBool("estáBalançando", true);
        } else {
            animator.SetBool("estáBalançando", false);
        }
    }



    //-----------------------------------------------------------------------------------------------------
    // INPUTS
    //-----------------------------------------------------------------------------------------------------

    Vector2 directionalInput;
    public void SetDirectionalInput(Vector2 input) {
        directionalInput = input;
    }

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public void OnJumpInputDown() {
        if (wallSliding) {
            if (wallDirX == directionalInput.x) {
                moveAmount.x = -wallDirX * wallJumpClimb.x;
                moveAmount.y = wallJumpClimb.y;
            } else if (directionalInput.x == 0) {
                moveAmount.x = -wallDirX * wallJumpOff.x;
                moveAmount.y = wallJumpOff.y;
            } else {
                moveAmount.x = -wallDirX * wallLeap.x;
                moveAmount.y = wallLeap.y;
            }
        }
        if (controller.collisions.below) {
            moveAmount.y = maxJumpVelocity;
        }
    }

    public void OnJumpInputUp() {
        if (moveAmount.y > minJumpVelocity) {
            moveAmount.y = minJumpVelocity;
        }
    }





    //-----------------------------------------------------------------------------------------------------
    // BULLET TIME
    //-----------------------------------------------------------------------------------------------------

    public float fatorLentidão = 0.1f; //tempo do unity fica 50% mais lento
    public float duraçãoLentidão = 2f;
    public bool estáEmBulletTime = false;

    public void ComeçarBulletTime()
    {
        print ("ZA WARUDO!");
        estáEmBulletTime = true;
        Time.timeScale = fatorLentidão;
        //jogadorVelocidade /= fatorLentidão; //se o tempo está 50% mais lento, a velocidade do jogador dobra
        Time.fixedDeltaTime = Time.timeScale * 0.02f; //esse valor me parece arbitrário, melhor pesquisar
    }

    public void PararBulletTime()
    {
        print("Fim do Bullet Time.");
        //Time.timeScale += (1f / duraçãoLentidão) * Time.unscaledDeltaTime; //esse deltaTime é absoluto, não fica mais lento
        Time.timeScale = 1;
        estáEmBulletTime = false;
    }

}
