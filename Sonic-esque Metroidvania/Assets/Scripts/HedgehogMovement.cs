﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GroundInfo
{
    public float height;
    public Vector3 point;
    public float distance;
    public Vector3 normal;
    public float angle;
    public bool valid = false;
}

public class HedgehogMovement : MonoBehaviour
{
    enum GroundMode
    {
        Floor,
        RightWall,
        Ceiling,
        LeftWall,
    }

    public Animator animator;


    //-----------------------------------------------------------------------------------------------------
    // GERAL
    //-----------------------------------------------------------------------------------------------------

    public bool grounded { get; private set; }
    public bool jumped { get; private set; }
    public bool caindo { get; private set; }
    public bool deslizando { get; private set; }
    public bool freando { get; private set; }
    public bool empurrando { get; private set; }

    //public bool rolling { get; private set; }


    public float standingHeight = 40f;
    //public float ballHeight = 30f;
    private float heightHalf
    {
        get
        {
            //if (rolling || jumped) { return ballHeight / 2f; }
            //else {
            return standingHeight / 2f; //}
        }
    }

    public float standWidthHalf = 9f;
    //public float spinWidthHalf = 7f;

    private Vector2 velocity;
    private float characterAngle;
    private bool lowCeiling;
    //private bool underwater;

    private Vector2 standLeftRPos;
    private Vector2 standRightRPos;
    //private Vector2 spinLeftRPos;
    //private Vector2 spinRightRPos;


    //-----------------------------------------------------------------------------------------------------
    // GROUND MOVEMENT
    //-----------------------------------------------------------------------------------------------------
    public float groundAcceleration = 168.75f;
    public float groundTopSpeed = 360f;
    public float speedLimit = 960f;
    //public float rollingMinSpeed = 61.875f;
    //public float unrollThreshold = 30f;
    public float friction = 168.75f;
    //public float rollingFriction = 84.375f;
    public float deceleration = 1800f;
    //public float rollingDeceleration = 450f;
    public float slopeFactor = 450f;
    //public float rollUphillSlope = 281.25f;
    //public float rollDownhillSlope = 1125f;
    public float sideRaycastOffset = -4f;
    public float sideRaycastDist = 11f;
    public float groundRaycastDist = 36f;
    public float fallVelocityThreshold = 180f;

    private float groundVelocity;
    private bool hControlLock;
    private float hControlLockTime = 0.5f;
    private GroundInfo currentGroundInfo;
    private GroundMode groundMode = GroundMode.Floor;


    //-----------------------------------------------------------------------------------------------------
    // AIR MOVEMENT
    //-----------------------------------------------------------------------------------------------------
    public float airAcceleration = 337.5f;
    public float jumpVelocity = 390f;
    public float jumpReleaseThreshold = 240f;
    public float gravity = -787.5f;
    public float terminalVelocity = 960f;
    public float airDrag = 0.96875f;


    /*-----------------------------------------------------------------------------------------------------
    // UNDERWATER
    //----------------------------------------------------------------------------------------------------- 
    public float uwAcceleration = 84.375f;
    public float uwDeceleration = 900f;
    public float uwFriction = 84.375f;
    public float uwRollingFriction = 42.1875f;
    public float uwGroundTopSpeed = 180f;
    public float uwAirAcceleration = 168.75f;
    public float uwGravity = -225f;
    public float uwJumpVelocity = 210f;
    public float uwJumpReleaseThreshold = 120f; 
    
    private Transform waterLevel; */


    //-----------------------------------------------------------------------------------------------------
    // ANIMAÇÃO
    //-----------------------------------------------------------------------------------------------------
    private int speedHash;
    private int standHash;
    private int jumpHash;
    private int cairHash;
    //private int spinHash;
    private int deslizandoHash;
    private int freandoHash;
    private int empurrarHash;
    private int wallStickHash;

    void Awake()
    {
        //waterLevel = GameObject.FindWithTag("WaterLevel").transform;

        standLeftRPos = new Vector2(-standWidthHalf, 0f);
        standRightRPos = new Vector2(standWidthHalf, 0f);
        //spinLeftRPos = new Vector2(-spinWidthHalf, 0f);
        //spinRightRPos = new Vector2(spinWidthHalf, 0f);

        speedHash = Animator.StringToHash("Speed");
        standHash = Animator.StringToHash("Stand");
        jumpHash = Animator.StringToHash("Jump");
        cairHash = Animator.StringToHash("Fall");
        //spinHash = Animator.StringToHash("Spin");
        deslizandoHash = Animator.StringToHash("Deslizando");
        freandoHash = Animator.StringToHash("Freando");
        empurrarHash = Animator.StringToHash("Push");
        wallStickHash = Animator.StringToHash("Grudado na Parede");
    }




    //-----------------------------------------------------------------------------------------------------
    // DEBUG WINDOW
    //-----------------------------------------------------------------------------------------------------

    bool debug = true;

    void OnGUI()
    {
        if (debug)
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 240), "Stats", "Window");
            GUILayout.Label("Olhando pra direita: " + (olhandoDireita ? "SIM" : "NÃO"));            
            //GUILayout.Label("Jumped: " + (jumped ? "SIM" : "NÃO"));
            GUILayout.Label("Caindo: " + (caindo ? "SIM" : "NÃO"));
            GUILayout.Label("Deslizando: " + (deslizando ? "SIM" : "NÃO"));
            GUILayout.Label("Empurrando: " + (empurrando ? "SIM" : "NÃO"));
            GUILayout.Label("Grudado: " + (grudadoParede ? "SIM" : "NÃO"));
            //GUILayout.Label("Bullet Time: " + (estáEmBulletTime ? "SIM" : "NÃO"));
            //GUILayout.Label("Rolling: " + (rolling ? "YES" : "NO"));
            //GUILayout.Label("Underwater: " + (underwater ? "YES" : "NO"));
            if (currentGroundInfo != null && currentGroundInfo.valid)
            {
                GUILayout.Label("Ground Speed: " + groundVelocity);
                GUILayout.Label("Angle (Deg): " + (currentGroundInfo.angle * Mathf.Rad2Deg));
            }
            GUILayout.EndArea();
        }
    }



    //-----------------------------------------------------------------------------------------------------
    // COISAS DO UPDATE
    //-----------------------------------------------------------------------------------------------------

    bool olhandoDireita = true;
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { debug = !debug; }
        if (Input.GetKeyDown(KeyCode.E)) { 
            estáEmBulletTime = !estáEmBulletTime;
            BulletTime();
        }
        

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float accelSpeedCap = /*underwater ? uwGroundTopSpeed :*/ groundTopSpeed;

        //-----------------------------------------------------------------------------------------------------
        // ESTÁ NO CHÃO?
        //-----------------------------------------------------------------------------------------------------

        if (grounded)
        {
            //-----------------------------------------------------------------------------------------------------
            // ROLL
            //-----------------------------------------------------------------------------------------------------

            /*if (!rolling && input.y < -0.005f && Mathf.Abs(groundVelocity) >= rollingMinSpeed)
            {
                rolling = true;
                transform.position -= new Vector3(0f, 5f);
            }*/


            //-----------------------------------------------------------------------------------------------------
            // RAMPAS
            //-----------------------------------------------------------------------------------------------------

            float slope; //= 0f; // APAGAR? a única utilidade dessa variável era o código abaixo.
                         /*if (rolling)
                         {
                             float sin = Mathf.Sin(currentGroundInfo.angle);
                             bool uphill = (sin >= 0f && groundVelocity >= 0f) || (sin <= 0f && groundVelocity <= 0);
                             slope = uphill ? rollUphillSlope : rollDownhillSlope;
                         }
                         else {*/
            slope = slopeFactor; //}

            // ADICIONA AO groundVelocity O FATOR DE RAMPA APROPRIADO, AJUSTADO PELO TEMPO QUE PASSOU
            groundVelocity += (slope * -Mathf.Sin(currentGroundInfo.angle)) * Time.fixedDeltaTime;



            //-----------------------------------------------------------------------------------------------------
            // CAIR
            //-----------------------------------------------------------------------------------------------------

            bool lostFooting = false;

            // SE NÃO ESTÁ NO CHÃO, MAS NÃO TEM VELOCIDADE PRA CORRER NA PAREDE/TETO
            if (groundMode != GroundMode.Floor && Mathf.Abs(groundVelocity) < fallVelocityThreshold)
            {
                groundMode = GroundMode.Floor; // VIRE PRO CHÃO
                grounded = false; // NÃO TÁ NO CHÃO
                //hControlLock = true; // TRAVE O CONTROLE NA HORIZONTAL
                //hControlLockTime = 0.5f;
                lostFooting = true;
                //return; // deveria?
            }


            //-----------------------------------------------------------------------------------------------------
            // PULAR
            //-----------------------------------------------------------------------------------------------------

            if (Input.GetButtonDown("Jump") && !lowCeiling)
            {
                //float jumpVel = underwater ? uwJumpVelocity : jumpVelocity;
                float jumpVel = jumpVelocity;
                velocity.x -= jumpVel * (Mathf.Sin(currentGroundInfo.angle));
                velocity.y += jumpVel * (Mathf.Cos(currentGroundInfo.angle));
                grounded = false;
                jumped = true;
            }
            else
            {
                //-----------------------------------------------------------------------------------------------------
                // FRICÇÃO
                //-----------------------------------------------------------------------------------------------------

                if (/*rolling ||*/ Mathf.Abs(input.x) < 0.005f)
                {
                    // Mostly because I don't like chaining ternaries 
                    float fric = /*underwater ? uwFriction :*/ friction;
                    //float rollFric = underwater ? uwRollingFriction : rollingFriction;

                    float frc = /*rolling ? rollFric :*/ fric;

                    if (groundVelocity > 0f)
                    {
                        groundVelocity -= frc * Time.fixedDeltaTime;
                        if (groundVelocity < 0f) { groundVelocity = 0f; }
                    }
                    else if (groundVelocity < 0f)
                    {
                        groundVelocity += frc * Time.fixedDeltaTime;
                        if (groundVelocity > 0f) { groundVelocity = 0f; }
                    }
                }


                //-----------------------------------------------------------------------------------------------------
                // CONTROL LOCK????
                //-----------------------------------------------------------------------------------------------------

                /*if (hControlLock)
                {
                    hControlLockTime -= Time.fixedDeltaTime;
                    if (hControlLockTime <= 0f)
                    {
                        hControlLock = false;
                    }
                }*/
                if (/*!hControlLock && !deslizando && */Mathf.Abs(input.x) >= 0.005f)
                {
                    float accel = /*underwater ? uwAcceleration :*/ groundAcceleration;
                    float decel = /*underwater ? uwDeceleration :*/ deceleration;

                    //-----------------------------------------------------------------------------------------------------
                    // ESQUERDA
                    //-----------------------------------------------------------------------------------------------------
                    if (input.x < 0f)
                    {
                        olhandoDireita = false;
                        float acceleration = 0f;
                        //if (rolling && groundVelocity > 0f) { acceleration = rollingDeceleration; } else
                        if (/*!rolling &&*/ groundVelocity > 0f) { acceleration = decel; }
                        else
                        if (/*!rolling &&*/ groundVelocity <= 0f) { acceleration = accel; }

                        // ACELERAR OU DESACELERAR, CONFORME ACIMA, RESPEITANDO O SPEEDCAP ATUAL (água ou terra)
                        if (groundVelocity > -accelSpeedCap)
                        {
                            groundVelocity = Mathf.Max(-accelSpeedCap, groundVelocity + (input.x * acceleration) * Time.deltaTime);
                        }
                    }

                    //-----------------------------------------------------------------------------------------------------
                    // DIREITA
                    //-----------------------------------------------------------------------------------------------------
                    else
                    {
                        olhandoDireita = true;
                        float acceleration = 0f;
                        //if (rolling && groundVelocity < 0f) { acceleration = rollingDeceleration; } else 
                        if (/*!rolling &&*/ groundVelocity < 0f) { acceleration = decel; }
                        else
                        if (/*!rolling &&*/ groundVelocity >= 0f) { acceleration = accel; }

                        // ACELERAR OU DESACELERAR, CONFORME ACIMA, RESPEITANDO O SPEEDCAP ATUAL (água ou terra)
                        if (groundVelocity < accelSpeedCap)
                        {
                            groundVelocity = Mathf.Min(accelSpeedCap, groundVelocity + (input.x * acceleration) * Time.deltaTime);
                        }
                    }
                }


                //-----------------------------------------------------------------------------------------------------
                // IMPEDE A VELOCIDADE NO CHÃO DE PASSAR DO LIMITE GLOBAL DE VELOCIDADE
                //-----------------------------------------------------------------------------------------------------

                // APAGAR?
                if (groundVelocity > speedLimit) { groundVelocity = speedLimit; }
                else if (groundVelocity < -speedLimit) { groundVelocity = -speedLimit; }


                /*-----------------------------------------------------------------------------------------------------
                // UNROLL
                //-----------------------------------------------------------------------------------------------------

                if (rolling && Mathf.Abs(groundVelocity) < unrollThreshold)
                {
                    rolling = false;
                    transform.position += new Vector3(0f, 5f);
                }*/


                //-----------------------------------------------------------------------------------------------------
                // VELOCIDADE ANGULAR????
                //-----------------------------------------------------------------------------------------------------

                Vector2 angledSpeed = new Vector2(groundVelocity * Mathf.Cos(currentGroundInfo.angle),
                                                  groundVelocity * Mathf.Sin(currentGroundInfo.angle));
                velocity = angledSpeed;

                //-----------------------------------------------------------------------------------------------------
                // DESGRUDOU DO TETO/PAREDE? ENTÃO RESETA O MOVIMENTO.
                //-----------------------------------------------------------------------------------------------------
                if (lostFooting)
                {
                    groundVelocity = 0f;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------
        // NÃO ESTÁ NO CHÃO?
        //-----------------------------------------------------------------------------------------------------
        else
        {
            //-----------------------------------------------------------------------------------------------------
            // ALTURA DE PULO VARIÁVEL
            //-----------------------------------------------------------------------------------------------------

            //define valor máximo do pulo
            float jumpReleaThreshold = /*underwater ? uwJumpReleaseThreshold :*/ jumpReleaseThreshold;

            // JÁ PULOU, SOLTOU O BOTÃO E A VELOCIDADE VERTICAL A SER APLICADA PASSOU DO LIMITE MÁXIMO?
            // VOLTA A APLICAR SÓ O MÁXIMO (a gravidade eventualmente o puxará pra baixo)
            if (jumped && velocity.y > jumpReleaThreshold && Input.GetButtonUp("Jump"))
            {
                velocity.y = jumpReleaThreshold;
            }
            else
            {
                //-----------------------------------------------------------------------------------------------------
                // RESISTÊNCIA DO AR
                //-----------------------------------------------------------------------------------------------------

                // POR QUE ESSES VALORES???
                if (velocity.y > 0f && velocity.y < 4f && Mathf.Abs(velocity.x) > 7.5f)
                {
                    velocity.x *= airDrag;
                }


                //-----------------------------------------------------------------------------------------------------
                // GRAVIDADE E VELOCIDADE TERMINAL
                //-----------------------------------------------------------------------------------------------------
                float grv = /*underwater ? uwGravity :*/ gravity;

                velocity.y = Mathf.Max(velocity.y + (grv * Time.fixedDeltaTime), -terminalVelocity);
            }


            //-----------------------------------------------------------------------------------------------------
            // ACELERAR NO AR
            //-----------------------------------------------------------------------------------------------------

            // CAIU DO PRECIPÍCIO ROLANDO (rolling) E TENTOU SE MOVER?
            // OU SERÁ QUE FEZ UM PULO (jumped) NORMAL, SEM ROLAR, E TENTOU SE MOVER?
            // MOVA-SE, USANDO A airAcceleration AO INVÉS DA ACELERAÇÃO DO CHÃO

            if (!(/*rolling &&*/ jumped) && Mathf.Abs(input.x) >= 0.005f)
            {
                if ((input.x < 0f && velocity.x > -accelSpeedCap) || (input.x > 0f && velocity.x < accelSpeedCap))
                {
                    float airAcc = /*underwater ? uwAirAcceleration :*/ airAcceleration;
                    velocity.x = Mathf.Clamp(velocity.x + (input.x * airAcc * Time.fixedDeltaTime), -accelSpeedCap, accelSpeedCap);
                }
            }
        }

        // CLAMP VELOCITY TO THE GLOBAL SPEED LIMIT (going any faster could result in passing through things)
        velocity.x = Mathf.Clamp(velocity.x, -speedLimit, speedLimit);
        velocity.y = Mathf.Clamp(velocity.y, -speedLimit, speedLimit);


        //-----------------------------------------------------------------------------------------------------
        // CHEQUE COLISÕES
        //-----------------------------------------------------------------------------------------------------
        TetoPiso(input);
        Paredes();


        //-----------------------------------------------------------------------------------------------------
        // ANIMAÇÃO DE CAIR
        //-----------------------------------------------------------------------------------------------------
        /*if (!grounded && velocity.y < 0) 
        {
            caindo = true;
            characterAngle = 0;
            groundMode = GroundMode.Floor;
        }
        else { caindo = false; }*/

        /*
        //-----------------------------------------------------------------------------------------------------
        // ANIMAÇÃO DE FREIO
        //-----------------------------------------------------------------------------------------------------

        if (groundVelocity != 0 && input.x != 0 && grounded && Mathf.Abs(groundVelocity) > 10
                                      && ((olhandoDireita && Mathf.Sign(groundVelocity) == -1)
                                      || (!olhandoDireita && Mathf.Sign(groundVelocity) == 1)))
        {
            freando = true;
        }
        else
        {
            freando = false;
        }*/

        //-----------------------------------------------------------------------------------------------------
        // ANIMAÇÃO DE DESLIZAR NA RAMPA
        //-----------------------------------------------------------------------------------------------------

        if (groundVelocity != 0 && grounded && ((olhandoDireita && Mathf.Sign(groundVelocity) == -1)
                                             || (!olhandoDireita && Mathf.Sign(groundVelocity) == 1)))
        {
            deslizando = true;
            print("deslizando, veloc é: " + groundVelocity);
        }
        else
        {
            deslizando = false;
            print("não tá deslizando, veloc é: " + groundVelocity + ". grounded? " + grounded + ". groundMode? " + groundMode);
        }


        //-----------------------------------------------------------------------------------------------------
        // APLICAR ANIMAÇÕES
        //-----------------------------------------------------------------------------------------------------
        animator.SetBool(jumpHash, jumped);
        animator.SetBool(cairHash, caindo);
        animator.SetBool(empurrarHash, empurrando);
        animator.SetBool(wallStickHash, grudadoParede);
        animator.SetBool(deslizandoHash, deslizando);
        animator.SetBool(freandoHash, freando);


        //-----------------------------------------------------------------------------------------------------
        // ÁGUA
        //-----------------------------------------------------------------------------------------------------
        //if (!underwater && transform.position.y <= waterLevel.position.y) { EnterWater(); }
        //else if (underwater && transform.position.y > waterLevel.position.y) { ExitWater(); }



        //-----------------------------------------------------------------------------------------------------
        // ROTAÇÃO
        //-----------------------------------------------------------------------------------------------------
        // rotaciona personagem em incrementos de 45°, assim como em Sonic 1, 2, 3&K
        //transform.localRotation = Quaternion.Euler(0f, 0f, SnapAngle(characterAngle)); 

        // rotaciona personagem perfeitamente, assim como em Sonic Mania e Freedom Planet
        if (grounded)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, characterAngle);
        }
        else 
        {
            //if (characterAngle <= 60 || 300 <= characterAngle)  
            //{
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, 5 * Time.fixedDeltaTime);
            //}           
        }

        //-----------------------------------------------------------------------------------------------------
        // DIREÇÃO
        //-----------------------------------------------------------------------------------------------------
        transform.localScale = new Vector3(olhandoDireita ? 1 : -1, 1, 1);

        //-----------------------------------------------------------------------------------------------------
        // MOVA-SE, DE FATO.
        //-----------------------------------------------------------------------------------------------------
        transform.position += new Vector3(velocity.x, velocity.y, 0f) * Time.fixedDeltaTime;
    }

    /* void EnterWater() {
        underwater = true;
        groundVelocity *= 0.5f;
        velocity.x *= 0.5f;
        velocity.y *= 0.25f;
    }

    void ExitWater() {
        underwater = false;
        velocity.y *= 2f;
    } */


    //-----------------------------------------------------------------------------------------------------
    // PAREDES
    //-----------------------------------------------------------------------------------------------------

    public bool grudadoParede { get; private set; }
    float tempoMáximoGrudado = 0.5f;
    float quantoFaltaDesgrudar = 0.5f;

    void Paredes()
    {
        RaycastHit2D leftHit;
        RaycastHit2D rightHit;
        WallCheck(sideRaycastDist, grounded ? sideRaycastOffset : 0f, out leftHit, out rightHit);

        if (leftHit.collider != null && rightHit.collider != null)
        {
            Debug.Log("GOT SQUASHED"); // Got squashed
        }
        else if (leftHit.collider != null)
        {
            olhandoDireita = false;
            transform.position = new Vector2(leftHit.point.x + sideRaycastDist, transform.position.y);
            if (velocity.x < 0f)
            {
                velocity.x = 0f;
                groundVelocity = 0f;
                if (grounded && (characterAngle == 0 || characterAngle == 180)) { empurrando = true; }
                else { empurrando = false; }
            } else if (velocity.x == 0) { empurrando = false; }
        }
        else if (rightHit.collider != null)
        {
            olhandoDireita = true;
            transform.position = new Vector2(rightHit.point.x - sideRaycastDist, transform.position.y);
            if (velocity.x > 0f)
            {
                velocity.x = 0f;
                groundVelocity = 0f;
                if (grounded && (characterAngle == 0 || characterAngle == 180)) { empurrando = true; }
                else { empurrando = false; }
            } else if (velocity.x == 0) { empurrando = false; }
        } else { empurrando = false; }









        //-----------------------------------------------------------------------------------------------------
        // GRUDAR NA PAREDE
        //-----------------------------------------------------------------------------------------------------


        if ((leftHit.collider != null || rightHit.collider != null) 
             && !grounded 
             && groundMode == GroundMode.Floor 
             && (characterAngle == 0 || characterAngle == 180))
        {
            grudadoParede = true;
            velocity.y = 0;
        }
        else { grudadoParede = false; }


        //-----------------------------------------------------------------------------------------------------
        // CAIR DA PAREDE
        //-----------------------------------------------------------------------------------------------------
        //&& Input.GetKeyDown(KeyCode.Space)
        if (grudadoParede) {
            if (quantoFaltaDesgrudar > 0 && !Input.GetButtonDown("Jump")) 
            {
                quantoFaltaDesgrudar -= Time.deltaTime;
            } 
            else if (quantoFaltaDesgrudar <= 0 || Input.GetButtonDown("Jump")) 
            {
                if (leftHit.collider != null) { velocity.x += 50; }
                else if (rightHit.collider != null) { velocity.x -= 50; }
                quantoFaltaDesgrudar = tempoMáximoGrudado;
            }
        }
    }

    public LayerMask máscaraColisão;

    void WallCheck(float distance, float heightOffset, out RaycastHit2D hitLeft, out RaycastHit2D hitRight)
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y + heightOffset);

        hitLeft = Physics2D.Raycast(pos, Vector2.left, distance, máscaraColisão);
        hitRight = Physics2D.Raycast(pos, Vector2.right, distance, máscaraColisão);

        Debug.DrawLine(pos, pos + (Vector2.left * distance), Color.yellow);
        Debug.DrawLine(pos, pos + (Vector2.right * distance), Color.yellow);
    }

    //-----------------------------------------------------------------------------------------------------
    // TETO E PISO
    //-----------------------------------------------------------------------------------------------------

    bool groundedLeft = false;
    bool groundedRight = false;

    void TetoPiso(Vector2 input) {

        bool ceilingLeft = false;
        bool ceilingRight = false;

        int ceilDirection = (int)groundMode + 2; // se groundMode for 0, o teto será 2
        if (ceilDirection > 3) { ceilDirection -= 4; } // se groundMode for 3, o teto será 5-4, isto é, 1.

        GroundInfo ceil = GroundedCheck(groundRaycastDist, (GroundMode)ceilDirection, out ceilingLeft, out ceilingRight);

        if (grounded) // ESTÁ NO CHÃO?
        {   
            currentGroundInfo = GroundedCheck(groundRaycastDist, groundMode, out groundedLeft, out groundedRight);
            grounded = groundedLeft || groundedRight;
            if (!grounded) { return; }

            StickToGround(currentGroundInfo);

            // ANIMAÇÃO
            animator.SetFloat(speedHash, Mathf.Abs(groundVelocity));

            // o teto é baixo?
            lowCeiling = ceil.valid && transform.position.y > ceil.point.y - 25f;
        }
        else
        {
            if (ceil.valid && velocity.y > 0f) { DanarCabeçaNoTeto(ceil); }
            else { Aterrissar(); } // TETO NÃO É VÁLIDO OU SONIC TÁ CAINDO 
        }
    }

    //-----------------------------------------------------------------------------------------------------
    // DANAR CABEÇA NO TETO
    //-----------------------------------------------------------------------------------------------------
    void DanarCabeçaNoTeto (GroundInfo ceil) {
        bool hitCeiling = transform.position.y >= (ceil.point.y - heightHalf);
        float angleDeg = ceil.angle * Mathf.Rad2Deg;

        // GRUDA NO TETO SE O ÂNGULO DELE NÃO FOR RETO (não gosto disso)
        /*if (hitCeiling && ((angleDeg >= 225f && angleDeg <= 270f) || (angleDeg >= 90f && angleDeg <= 135f)))
        {
            grounded = true;
            jumped = false;
            //rolling = false;
            currentGroundInfo = ceil;
            groundMode = GroundMode.Ceiling;

            groundVelocity = velocity.y * Mathf.Sign(Mathf.Sin(currentGroundInfo.angle));
            velocity.y = 0f; 
        }
        else*/
        if (hitCeiling)
        {
            // PASSOU DO TETO?
            if (transform.position.y > ceil.point.y - heightHalf)
            {
                // CORRIJA A POSIÇÃO
                transform.position = new Vector2(transform.position.x, ceil.point.y - heightHalf);
                // E PARE DE CAIR
                velocity.y = 0f;
            }
        }
    }
    
    //-----------------------------------------------------------------------------------------------------
    // ATERRISSAR
    //-----------------------------------------------------------------------------------------------------
    void Aterrissar () {
        // VERIFICA SE JÁ TOCOU NO CHÃO
        GroundInfo info = GroundedCheck(groundRaycastDist, GroundMode.Floor, out groundedLeft, out groundedRight);
        grounded = (groundedLeft || groundedRight) && velocity.y <= 0f && transform.position.y <= (info.height + heightHalf);

        // SE SIM, TRANSFORMA A VELOCIDADE NO AR EM VELOCIDADE NO CHÃO 
        if (grounded)
        {
            // If in a roll jump, add 5 to position upon landing
            //if (jumped) { transform.position += new Vector3(0f, 5f); }

            jumped = false;
            //rolling = false;

            currentGroundInfo = info;
            groundMode = GroundMode.Floor;
            float angleDeg = currentGroundInfo.angle * Mathf.Rad2Deg;

            // SE O ÂNGULO É BAIXO, SIMPLESMENTE USE A VELOCIDADE DO CHÃO
            if (angleDeg < 22.5f || (angleDeg > 337.5 && angleDeg <= 360f))
            {
                groundVelocity = velocity.x;
            }
            // SE O ÂNGULO É MAIOR, ATÉ 45º:
            else if ((angleDeg >= 22.5f && angleDeg < 45f) || (angleDeg >= 315f && angleDeg < 337.5f))
            {
                // MAS A VELOCIDADE X AINDA É MAIOR QUE A Y, CONTINUA USANDO SÓ A X
                if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y)) { groundVelocity = velocity.x; }
                // SE NÃO (está caindo/subindo muito rápido), USA METADE DA velocY, CORRIGIDA POR TRIGONOMETRIA
                else { groundVelocity = velocity.y * 0.5f * Mathf.Sign(Mathf.Sin(currentGroundInfo.angle)); }
            }
            // SE O ÂNGULO É AINDA MAIOR, ATÉ 90º:
            else if ((angleDeg >= 45f && angleDeg < 90f) || (angleDeg >= 270f && angleDeg < 315f))
            {
                // MAS A VELOCIDADE X AINDA É MAIOR QUE A Y, CONTINUA USANDO SÓ A X
                if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y)) { groundVelocity = velocity.x; }
                // SE NÃO (está caindo/subindo muito rápido), USA A velocY INTEIRA, CORRIGIDA POR TRIGONOMETRIA
                else { groundVelocity = velocity.y * Mathf.Sign(Mathf.Sin(currentGroundInfo.angle)); }
            }

            // FINALMENTE, PARE DE CAIR (você já está no chão)
            velocity.y = 0f;
        }
    }



    private Vector2 leftRaycastPos { get { //if (rolling || jumped) { return spinLeftRPos; } else { 
            return standLeftRPos; //} 
        }
    }
    private Vector2 rightRaycastPos { get { //if (rolling || jumped) { return spinRightRPos; } else { 
            return standRightRPos; //}
        }
    }

    GroundInfo GroundedCheck(float distance, GroundMode groundMode, out bool groundedLeft, out bool groundedRight)
    {
        Quaternion rot = Quaternion.Euler(0f, 0f, (90f * (int)groundMode)); // COMO QUE ELE CONVERTE groundMode EM INT?
        Vector2 dir = rot * Vector2.down;
        Vector2 leftCastPos = rot * leftRaycastPos;
        Vector2 rightCastPos = rot * rightRaycastPos;
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        RaycastHit2D leftHit = Physics2D.Raycast(pos + leftCastPos, dir, distance, máscaraColisão);
        groundedLeft = leftHit.collider != null;

        RaycastHit2D rightHit = Physics2D.Raycast(pos + rightCastPos, dir, distance, máscaraColisão);
        groundedRight = rightHit.collider != null;

        Debug.DrawLine(pos + leftCastPos, pos + leftCastPos + (dir * distance), Color.magenta);
        Debug.DrawLine(pos + rightCastPos, pos + rightCastPos + (dir * distance), Color.red);

        GroundInfo found = null;

        if (groundedLeft && groundedRight)
        {
            float leftCompare = 0f;
            float rightCompare = 0f;

            switch (groundMode)
            {
                case GroundMode.Floor:
                    leftCompare = leftHit.point.y;
                    rightCompare = rightHit.point.y;
                    break;
                case GroundMode.RightWall:
                    leftCompare = -leftHit.point.x;
                    rightCompare = -rightHit.point.x;
                    break;
                case GroundMode.Ceiling:
                    leftCompare = -leftHit.point.y;
                    rightCompare = -rightHit.point.y;
                    break;
                case GroundMode.LeftWall:
                    leftCompare = leftHit.point.x;
                    rightCompare = rightHit.point.x;
                    break;
                default:
                    break;
            }

            if (leftCompare >= rightCompare) { found = GetGroundInfo(leftHit); } // checa o lado com valor mais "alto", a depender do ângulo
            else { found = GetGroundInfo(rightHit); }
        }
        else if (groundedLeft) { 
            found = GetGroundInfo(leftHit); 
            // checar "raio do meio" e fazer animação de balanço
        }
        else if (groundedRight) { 
            found = GetGroundInfo(rightHit);
            // checar "raio do meio" e fazer animação de balanço
        }
        else { found = new GroundInfo(); }

        return found;   
    }
    GroundInfo GetGroundInfo(RaycastHit2D hit)
    {
        GroundInfo info = new GroundInfo();
        if (hit.collider != null)
        {
            info.height = hit.point.y;
            info.point = hit.point;
            info.normal = hit.normal;
            info.angle = Vector2ToAngle(hit.normal);
            info.valid = true;
        }

        return info;
    }

    void StickToGround(GroundInfo info)
    {
        float angle = info.angle * Mathf.Rad2Deg;
        characterAngle = angle;
        Vector3 pos = transform.position;

        // SE O ÂNGULO NÃO CONDIZER COM O groundMode ATUAL, MUDE-O.
        switch (groundMode) 
        {
            case GroundMode.Floor:
                if (angle < 315f && angle > 225f) { groundMode = GroundMode.LeftWall; }
                else if (angle > 45f && angle < 180f) { groundMode = GroundMode.RightWall; }
                pos.y = info.point.y + heightHalf;
                break;
            case GroundMode.RightWall:
                if (angle < 45f && angle > 0f) { groundMode = GroundMode.Floor; }
                else if (angle > 135f && angle < 270f) { groundMode = GroundMode.Ceiling; }
                pos.x = info.point.x - heightHalf;
                break;
            case GroundMode.Ceiling:
                if (angle < 135f && angle > 45f) { groundMode = GroundMode.RightWall; }
                else if (angle > 225f && angle < 360f) { groundMode = GroundMode.LeftWall; }
                pos.y = info.point.y - heightHalf;
                break;
            case GroundMode.LeftWall:
                if (angle < 225f && angle > 45f) { groundMode = GroundMode.Ceiling; }
                else if (angle > 315f) { groundMode = GroundMode.Floor; }
                pos.x = info.point.x + heightHalf;
                break;
            default:
                break;
        }


        transform.position = pos;
    }


    //-----------------------------------------------------------------------------------------------------
    // BULLET TIME
    //-----------------------------------------------------------------------------------------------------

    float fatorLentidão = 0.1f; //tempo do unity fica 50% mais lento
    float duraçãoLentidão = 2f;
    bool estáEmBulletTime = false;

    public void BulletTime()
    {
        if (estáEmBulletTime)
        {
            print("ZA WARUDO!");
            Time.timeScale = fatorLentidão;
            //jogadorVelocidade /= fatorLentidão; //se o tempo está 50% mais lento, a velocidade do jogador dobra
            Time.fixedDeltaTime = Time.timeScale * 0.02f; //esse valor me parece arbitrário, melhor pesquisar
        }
        else 
        {
            print("Fim do Bullet Time.");
            //Time.timeScale += (1f / duraçãoLentidão) * Time.unscaledDeltaTime; //esse deltaTime é absoluto, não fica mais lento
            Time.timeScale = 1;
        }
    }


    //-----------------------------------------------------------------------------------------------------
    // MATEMÁTICA
    //-----------------------------------------------------------------------------------------------------

    /// <summary>
    /// Retorna ângulo ajustado ao incremento de 45° mais próximo
    /// </summary>

    /*
    float SnapAngle(float angle)
    {
        int mult = (int)(angle + 22.5f);
        mult /= 45;
        return mult * 45f;
    }*/

    /// <summary>
    /// Converte um Vector2 em um ângulo de 0-360° Converts vector to 0-360 degree (counter-clockwise) angle, with a vector pointing straight up as zero.
    /// </summary>
    float Vector2ToAngle(Vector2 vector)
    {
        float angle = Mathf.Atan2(vector.y, vector.x) - (Mathf.PI / 2f);
        if (angle < 0f) { angle += Mathf.PI * 2f; }
        return angle;
    }
}
