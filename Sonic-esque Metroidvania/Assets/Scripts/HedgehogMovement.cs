using System.Collections;
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
    public bool pulou { get; private set; }
    public bool caindo { get; private set; }


    public bool abaixado { get; private set; }



    public bool empurrando { get; private set; }
    public bool grudadoParede { get; private set; }


    float standingHeight = 40f;
    private float heightHalf
    {
        get
        {
            return standingHeight / 2f; //}
        }
    }

    float standWidthHalf = 10f;

    private Vector2 velocity;
    private float characterAngle;
    private bool lowCeiling;

    private Vector2 standLeftRPos;
    private Vector2 standRightRPos;


    //-----------------------------------------------------------------------------------------------------
    // GROUND MOVEMENT
    //-----------------------------------------------------------------------------------------------------
    public float groundAcceleration = 168.75f;
    public float groundTopSpeed = 360f;
    public float speedLimit = 960f;

    float friction = 168.75f;
    float abaixadoFriction = 337.50f; //84.375f;
    float deceleration = 1800f;

    float slopeFactor = 450f;
    float sideRaycastOffset = -4f;
    float sideRaycastDist = 10f;
    float groundRaycastDist = 24f;
    float fallVelocityThreshold = 180f;

    private float groundVelocity;
    private GroundInfo currentGroundInfo;
    private GroundMode groundMode = GroundMode.Floor;


    //-----------------------------------------------------------------------------------------------------
    // AIR MOVEMENT
    //-----------------------------------------------------------------------------------------------------
    //public float airAcceleration = 340f;        
    public float airAcceleration = 170f;
    public float jumpVelocity = 390f;
    public float jumpReleaseThreshold = 240f;
    public float gravity = -790f;
    public float terminalVelocity = 960f;
    public float airDrag = 1f;


    //-----------------------------------------------------------------------------------------------------
    // ANIMAÇÃO
    //-----------------------------------------------------------------------------------------------------
    private int standHash;
    private int speedHash;
    private int caindoHash;

    private int groundedHash;

    private int freandoHash;
    private int abaixadoHash;

    private int empurrandoHash;
    private int grudadoParedeHash;

    float duraçãoAnimação = 1f;
    float timeToJumpApex = 0.5f;
    bool backfliping;

    float tempoPirueta = 0;


    //SKIN
    float peleGrossura = 0.05f;

    void Awake()
    {
        //waterLevel = GameObject.FindWithTag("WaterLevel").transform;

        standLeftRPos = new Vector2(-standWidthHalf + peleGrossura, 0f);
        standRightRPos = new Vector2(standWidthHalf - peleGrossura, 0f);

        standHash = Animator.StringToHash("Stand");
        speedHash = Animator.StringToHash("Speed");
        caindoHash = Animator.StringToHash("Caindo");

        groundedHash = Animator.StringToHash("Grounded");

        freandoHash = Animator.StringToHash("Freando");
        abaixadoHash = Animator.StringToHash("Abaixado");

        empurrandoHash = Animator.StringToHash("Empurrando");
        grudadoParedeHash = Animator.StringToHash("Grudado na Parede");
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
            GUILayout.Label("TempoPirueta: " + tempoPirueta);

            GUILayout.Label("Grounded: " + (grounded ? "SIM" : "NÃO"));
            GUILayout.Label("Freando: " + (freando ? "SIM" : "NÃO"));
            GUILayout.Label("Olhando pra direita: " + (olhandoDireita ? "SIM" : "NÃO"));
            GUILayout.Label("Collider: " + colliderNome);
            GUILayout.Label("Pulou: " + (pulou ? "SIM" : "NÃO"));
            GUILayout.Label("Caindo: " + (caindo ? "SIM" : "NÃO"));
            //GUILayout.Label("Empurrando: " + (empurrando ? "SIM" : "NÃO"));
            GUILayout.Label("Ground Mode: " + (groundMode));
            //GUILayout.Label("Bullet Time: " + (estáEmBulletTime ? "SIM" : "NÃO"));
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
    string colliderNome = "NADA";
    void FixedUpdate()
    {        
        if (Input.GetKeyDown(KeyCode.Tab)) { debug = !debug; }
        if (Input.GetKeyDown(KeyCode.E)) { estáEmBulletTime = !estáEmBulletTime; }

        BulletTime();

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float accelSpeedCap = /*underwater ? uwGroundTopSpeed :*/ groundTopSpeed;


        //-----------------------------------------------------------------------------------------------------
        // ESTÁ NO CHÃO?
        //-----------------------------------------------------------------------------------------------------

        if (grounded)
        {
            

            //-----------------------------------------------------------------------------------------------------
            // ABAIXAR e RAMPAS
            //-----------------------------------------------------------------------------------------------------

            if (input.y < -0.005f) { abaixado = true; }
            else { abaixado = false; }

            // ADICIONA AO groundVelocity O FATOR DE RAMPA APROPRIADO, AJUSTADO PELO TEMPO QUE PASSOU
            groundVelocity += (slopeFactor * -Mathf.Sin(currentGroundInfo.angle)) * Time.fixedDeltaTime;


            //-----------------------------------------------------------------------------------------------------
            // CAIR
            //-----------------------------------------------------------------------------------------------------

            bool lostFooting = false;

            // SE NÃO ESTÁ NO FLOOR, MAS NÃO TEM VELOCIDADE PRA CORRER NA PAREDE/TETO
            if (groundMode != GroundMode.Floor && Mathf.Abs(groundVelocity) < fallVelocityThreshold)
            {
                groundMode = GroundMode.Floor; // VIRE PRO CHÃO
                grounded = false; // NÃO TÁ NO CHÃO
                //hControlLock = true; // TRAVE O CONTROLE NA HORIZONTAL
                //hControlLockTime = 0.5f;
                lostFooting = true;
            }



            //SPIN DASH
            //tá no ground, abaixado e soltou o pulo?
            if (input.y < 0 && Input.GetButtonDown("Jump") && groundMode == GroundMode.Floor)
            {
                animator.Play("Spindash");
            }
            if (input.y < 0 && Input.GetButtonUp("Jump") && groundMode == GroundMode.Floor)
            {
                animator.SetTrigger("Stand");
                if (olhandoDireita)
                {
                    groundVelocity += 600f;
                }
                else
                {
                    groundVelocity -= 600f;
                }
            }


            //-----------------------------------------------------------------------------------------------------
            // PULAR
            //-----------------------------------------------------------------------------------------------------

            if (Input.GetButtonDown("Jump") && !abaixado && !lowCeiling)
            {
                float jumpVel = jumpVelocity;
                velocity.x -= jumpVel * (Mathf.Sin(currentGroundInfo.angle));
                velocity.y += jumpVel * (Mathf.Cos(currentGroundInfo.angle));
                grounded = false;
                pulou = true;
                animator.SetTrigger("Pulou");
                tempoPirueta = 0.75f;

                //LONG JUMP
                if (Mathf.Abs(groundVelocity) >= fallVelocityThreshold) {
                    if (input.x > 0) { velocity.x += 150; }
                    if (input.x < 0) { velocity.x -= 150; }
                }

            }
            else
            {
                //-----------------------------------------------------------------------------------------------------
                // FRICÇÃO
                //-----------------------------------------------------------------------------------------------------

                // NÃO HÁ INPUT? aplique fricção.
                if (abaixado || Mathf.Abs(input.x) < 0.005f)
                {
                    // Mostly because I don't like chaining ternaries 
                    float dePéFric = /*underwater ? uwFriction :*/ friction;
                    float abaixadoFric = /*underwater ? uwRollingFriction :*/ abaixadoFriction;

                    float frc = abaixado ? abaixadoFric : dePéFric;

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
                // INPUT = MOVIMENTO!
                //-----------------------------------------------------------------------------------------------------

                if (Mathf.Abs(input.x) >= 0.005f)
                {
                    float accel = /*underwater ? uwAcceleration :*/ groundAcceleration;
                    float decel = /*underwater ? uwDeceleration :*/ deceleration;

                    //-----------------------------------------------------------------------------------------------------
                    // ESQUERDA
                    //-----------------------------------------------------------------------------------------------------
                    if (input.x < 0f)
                    {
                        float acceleration = 0f;
                        //if (rolling && groundVelocity > 0f) { acceleration = rollingDeceleration; } else
                        if (!abaixado && groundVelocity > 0f) { acceleration = decel; } // FREAR
                        else
                        if (!abaixado && groundVelocity <= 0f) { acceleration = accel; } // ACELERAR

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
                        float acceleration = 0f;
                        if (!abaixado && groundVelocity < 0f) { acceleration = decel; }
                        else
                        if (!abaixado && groundVelocity >= 0f) { acceleration = accel; }

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


                //-----------------------------------------------------------------------------------------------------
                // APLICA groundVelocity À VELOCIDADE DO PERSONAGEM, LEVANDO EM CONTA O ÂNGULO DO CHÃO 
                //-----------------------------------------------------------------------------------------------------

                velocity = new Vector2(groundVelocity * Mathf.Cos(currentGroundInfo.angle),
                                       groundVelocity * Mathf.Sin(currentGroundInfo.angle));

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
            if (pulou && velocity.y > jumpReleaThreshold && Input.GetButtonUp("Jump"))
            {
                velocity.y = jumpReleaThreshold;
            }
            else
            {
                //-----------------------------------------------------------------------------------------------------
                // RESISTÊNCIA DO AR
                //-----------------------------------------------------------------------------------------------------
                // POR QUE ESSES VALORES???
                //if (velocity.y > 0f && velocity.y < 4f && Mathf.Abs(velocity.x) > 7.5f)
                //{
                //    velocity.x *= airDrag;
                //}

                //-----------------------------------------------------------------------------------------------------
                // GRAVIDADE E VELOCIDADE TERMINAL
                //-----------------------------------------------------------------------------------------------------
                float grv = /*underwater ? uwGravity :*/ gravity;

                velocity.y = Mathf.Max(velocity.y + (grv * Time.fixedDeltaTime), -terminalVelocity);
            }


            //-----------------------------------------------------------------------------------------------------
            // ACELERAR NO AR
            //-----------------------------------------------------------------------------------------------------

            // MOVA-SE, USANDO A airAcceleration AO INVÉS DA ACELERAÇÃO DO CHÃO
            if (Mathf.Abs(input.x) >= 0.005f)
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
        // MOVA-SE, DE FATO.
        //-----------------------------------------------------------------------------------------------------
        transform.position += new Vector3(velocity.x, velocity.y, 0f) * Time.fixedDeltaTime;



        //-----------------------------------------------------------------------------------------------------
        // PAREDES
        //-----------------------------------------------------------------------------------------------------

        //-----------------------------------------------------------------------------------------------------
        // COLISÃO NA DIREITA E NA ESQUERDA
        //-----------------------------------------------------------------------------------------------------

        RaycastHit2D leftHit;
        RaycastHit2D rightHit;
        
        WallCheck(sideRaycastDist, grounded ? sideRaycastOffset : 0f, out leftHit, out rightHit);

        if (leftHit.collider != null && rightHit.collider != null)
        {
            Debug.Log("GOT SQUASHED"); // Got squashed
        }
        else if (leftHit.collider != null)
        {
            colliderNome = "ESQUERDA";
            travaDireção = true;

            transform.position = new Vector2(leftHit.point.x + sideRaycastDist, transform.position.y);
            if (velocity.x < 0f)
            {
                velocity.x = 0f;
                groundVelocity = 0f;
                if (grounded && (characterAngle == 0 || characterAngle == 180)) { empurrando = true; }
                else { empurrando = false; }
            }
            else if (velocity.x == 0) { empurrando = false; }
        }
        else if (rightHit.collider != null)
        {
            colliderNome = "DIREITA";
            travaDireção = true;

            //if (velocity.x > rightHit.distance - 10f) { velocity.x = rightHit.distance - 10f; }
            //velocity.x = rightHit.distance - sideRaycastDist;
            //sideRaycastDist = rightHit.distance;

            
            transform.position = new Vector2(rightHit.point.x - sideRaycastDist, transform.position.y);
            if (velocity.x > 0f)
            {
                velocity.x = 0f;
                groundVelocity = 0f;
                if (grounded && (characterAngle == 0 || characterAngle == 180)) { empurrando = true; }
                else { empurrando = false; }
            }
            else if (velocity.x == 0) { empurrando = false; }
        }
        else 
        { 
            empurrando = false;
            travaDireção = false;
            colliderNome = "NADA";
        }


        //-----------------------------------------------------------------------------------------------------
        // GRUDAR NA PAREDE
        //-----------------------------------------------------------------------------------------------------

        /*
        if ((leftHit.collider != null || rightHit.collider != null)
             && !grounded
             && groundMode == GroundMode.Floor
             && (characterAngle == 0 || characterAngle == 180))
        {
            grudadoParede = true;
            velocity.y = 0;
        }
        else { grudadoParede = false; }
        */
        /*
        //-----------------------------------------------------------------------------------------------------
        // CAIR DA PAREDE
        //-----------------------------------------------------------------------------------------------------
        if (grudadoParede)
        {
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
        */

        //-----------------------------------------------------------------------------------------------------
        // COLISÃO EM CIMA E EM BAIXO
        //-----------------------------------------------------------------------------------------------------

        bool groundedLeft = false;
        bool groundedRight = false;

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
        }
        else
        {
            if (ceil.valid && velocity.y > 0f)
            {
                //-----------------------------------------------------------------------------------------------------
                // DANAR CABEÇA NO TETO
                //-----------------------------------------------------------------------------------------------------

                bool hitCeiling = transform.position.y >= (ceil.point.y - heightHalf);
                float angleDeg = ceil.angle * Mathf.Rad2Deg;

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
            else // TETO NÃO É VÁLIDO OU SONIC TÁ CAINDO 
            {
                //-----------------------------------------------------------------------------------------------------
                // ATERRISSAR (VERIFICA SE JÁ TOCOU NO CHÃO)
                //-----------------------------------------------------------------------------------------------------

                GroundInfo info = GroundedCheck(groundRaycastDist, GroundMode.Floor, out groundedLeft, out groundedRight);
                grounded = (groundedLeft || groundedRight)
                                && velocity.y <= 0f
                                && transform.position.y <= (info.height + heightHalf);

                // SE SIM, TRANSFORMA A VELOCIDADE NO AR EM VELOCIDADE NO CHÃO 
                if (grounded)
                {
                    // If in a roll jump, add 5 to position upon landing
                    //if (jumped) { transform.position += new Vector3(0f, 5f); }

                    pulou = false;
                    animator.ResetTrigger("Pulou");
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

            //currentGroundInfo = null;
            groundMode = GroundMode.Floor;
            lowCeiling = false;

            //isso é pra virar o personagem pra direita ou esquerda, mas eu não uso esse código
            if (Mathf.Abs(input.x) > 0.005f /*&& !(rolling && pulou)*/)
            {
                //Vector3 scale = Vector3.one;
                //scale.x *= Mathf.Sign(input.x);
                //transform.localScale = scale;
            }
        }


        if (grounded) 
        {
            StickToGround(currentGroundInfo);

            // ANIMAÇÃO
            animator.SetFloat(speedHash, Mathf.Abs(groundVelocity));

            // A CABEÇA DO SPRITE ESTÁ DENTRO DO TETO?
            lowCeiling = ceil.valid && transform.position.y > ceil.point.y - 25f;
        }


        //-----------------------------------------------------------------------------------------------------
        // ANIMAÇÕES
        //-----------------------------------------------------------------------------------------------------

        // CAIR FALLING
        if (!grounded && velocity.y < 0 && tempoPirueta == 0)
        { 
            caindo = true; 
        }
        else 
        { 
            caindo = false; 
        }


        if (tempoPirueta > 0)
        {
            tempoPirueta -= Time.fixedDeltaTime;
        }
        else {
            tempoPirueta = 0;
        }


        if (grounded && velocity.x == 0) 
            { animator.SetTrigger("Stand"); }
        else { animator.ResetTrigger("Stand"); }
        animator.SetBool(caindoHash, caindo);
        animator.SetBool(groundedHash, grounded);
        //animator.SetBool(freandoHash, freando);
        //animator.SetBool(abaixadoHash, abaixado);
        //animator.SetBool(empurrandoHash, empurrando);
        //animator.SetBool(grudadoParedeHash, grudadoParede);

        //-----------------------------------------------------------------------------------------------------
        // ROTAÇÃO
        //-----------------------------------------------------------------------------------------------------

        // rotaciona personagem perfeitamente, assim como em Sonic Mania e Freedom Planet
        if (grounded) { transform.rotation = Quaternion.Euler(0f, 0f, characterAngle); }
        // não está no chão? fique em posição de queda normal.
        else { transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 5 * Time.fixedDeltaTime); }

        //-----------------------------------------------------------------------------------------------------
        // DIREÇÃO
        //-----------------------------------------------------------------------------------------------------
        if (Mathf.Abs(input.x) > 0 && grounded && !freando && !travaDireção)
        {
            if (groundVelocity < 0) { olhandoDireita = false; }
            else { olhandoDireita = true; }
        }


        //-----------------------------------------------------------------------------------------------------
        // FREANDO
        //-----------------------------------------------------------------------------------------------------
        /*
        if (grounded)
        {
            if (freando == false && Mathf.Abs(groundVelocity) >= freandoLimiteVirar
                                 && ((groundVelocity < 0 && input.x > 0) ||
                                     (groundVelocity > 0 && input.x < 0)))
            {
                //animator.SetTrigger("Freando");
                freando = true;
                travaDireção = true;
                FindObjectOfType<SoundManager>().Play("freando");
            }
            else if (freando && Mathf.Abs(groundVelocity) < freandoLimiteVirar)
            {
                if (Mathf.Abs(input.x) > 0.05f)
                {
                    olhandoDireita = !olhandoDireita;
                    animator.SetTrigger("FreandoVirar");
                }
                freando = false;
                travaDireção = true;
            }
            else if (groundVelocity == 0 || ((groundVelocity > 0 && input.x > 0) || (groundVelocity < 0 && input.x < 0)))
            {
                travaDireção = false;
            }
        }
        */
        transform.localScale = new Vector3(olhandoDireita ? 1 : -1, 1, 1);

        //#TODO ver pra que serve essa linha
        Debug.DrawLine(transform.position, new Vector3(transform.position.x + velocity.x / 5,
                                               transform.position.y + velocity.y / 5, 0), Color.red);
    }

    bool freando = false;
    bool travaDireção = false;
    //bool freandoVirar = false;
    float freandoLimiteVirar = 100;
    public AudioSource somFreando;



    public LayerMask máscaraColisão;

    void WallCheck(float distance, float heightOffset, out RaycastHit2D hitLeft, out RaycastHit2D hitRight)
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y + heightOffset);

        hitLeft = Physics2D.Raycast(pos, Vector2.left, distance, máscaraColisão);
        hitRight = Physics2D.Raycast(pos, Vector2.right, distance, máscaraColisão);

        Debug.DrawLine(pos, pos + (Vector2.left * distance), Color.green);
        Debug.DrawLine(pos, pos + (Vector2.right * distance), Color.green);
    }

    //-----------------------------------------------------------------------------------------------------
    // TETO E PISO
    //-----------------------------------------------------------------------------------------------------


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

        Debug.DrawRay(pos + leftCastPos, dir * distance, Color.green);
        Debug.DrawRay(pos + rightCastPos, dir * distance, Color.green);

        //Debug.DrawLine(pos + leftCastPos, pos + leftCastPos + (dir * distance), Color.magenta);
        //Debug.DrawLine(pos + rightCastPos, pos + rightCastPos + (dir * distance), Color.red);

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
            //#TODO checar "raio do meio" e fazer animação de balanço
        }
        else if (groundedRight) { 
            found = GetGroundInfo(rightHit);
            //#TODO checar "raio do meio" e fazer animação de balanço
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
                if (Mathf.Abs(groundVelocity) >= fallVelocityThreshold) 
                {
                    if (angle < 315f && angle > 225f) { groundMode = GroundMode.LeftWall; }
                    else if (angle > 45f && angle < 180f) { groundMode = GroundMode.RightWall; }
                }
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
            //print("Fim do Bullet Time.");
            //Time.timeScale += (1f / duraçãoLentidão) * Time.unscaledDeltaTime; //esse deltaTime é absoluto, não fica mais lento
            Time.timeScale = 1;
        }
    }
    

    //-----------------------------------------------------------------------------------------------------
    // MATEMÁTICA
    //-----------------------------------------------------------------------------------------------------

    /// <summary>
    /// Converte um Vector2 em um ângulo de 0-360° (anti-horário) com um vetor zerado apontando pra cima.
    /// </summary>
    float Vector2ToAngle(Vector2 vector)
    {
        float angle = Mathf.Atan2(vector.y, vector.x) - (Mathf.PI / 2f);
        if (angle < 0f) { angle += Mathf.PI * 2f; }
        return angle;
    }
}
