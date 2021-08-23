using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;



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

    RaycastHit2D leftHit;
    RaycastHit2D rightHit;

    //-----------------------------------------------------------------------------------------------------
    // GROUND MOVEMENT
    //-----------------------------------------------------------------------------------------------------
    [Header("Ground Movement")]
    public float groundAcceleration = 168.75f;
    public float groundTopSpeed = 360f;
    public float speedLimit = 960f;

    bool olhandoDireita = true;

    float friction = 168.75f;
    float abaixadoFriction = 337.50f; //84.375f;
    float deceleration = 1800f;

    float peleGrossura = 0.01f;
    float slopeFactor = 450f;
    float sideRaycastOffset = -4f;
    float sideRaycastDist = 10f;
    float groundRaycastDist = 24f;
    float fallVelocityThreshold = 180f;

    private float groundVelocity;
    private bool hControlLock = false;
    private float hControlLockTime = 0f;
    private GroundInfo currentGroundInfo;
    private GroundMode groundMode = GroundMode.Floor;


    //-----------------------------------------------------------------------------------------------------
    // AIR MOVEMENT
    //-----------------------------------------------------------------------------------------------------   
    float airAcceleration = 170f; //340f
    float jumpVelocity = 390f;
    float jumpReleaseThreshold = 240f;
    float gravity = -790f;
    float terminalVelocity = 960f;
    //float airDrag = 1f;

    //-----------------------------------------------------------------------------------------------------
    // ANIMAÇÃO
    //-----------------------------------------------------------------------------------------------------
    private int standHash;
    private int speedHash;
    private int caindoHash;

    private int groundedHash;

    private int freandoHash;

    private int empurrandoHash;
    private int grudadoParedeHash;

    //-----------------------------------------------------------------------------------------------------
    // OUTROS
    //-----------------------------------------------------------------------------------------------------
    [Header("Outros")]
    public ParticleSystem dust;

    bool spinReady = false;

    bool mudarDireção = false;
    float mudarDireçãoDelay = 0.5f;

    float tempoPirueta = 0;

    bool doubleJumpReady = false;
    float doubleJumpDelay = 0;
    float doubleJumpDelayTotal = 0.1f;

    float wallJumpDelay = 0;
    float wallJumpDelayTotal = 0.5f;

    float fatorLentidão = 0.2f;
    bool estáEmBulletTime = false;
    AfterImage afterImage;
    public GameObject luzGlobalBranca;
    public GameObject luzGlobalAzul;

    //Awake() is called immediately upon the start of the game,
    //independentemente do script estar ativado ou não.

    //Start() só é chamada quando o script for ativado,
    //e só será chamada essa única vez por toda a vida do script,
    //mesmo que ele seja desativado e reativado posteriormente.
    //Para essa última função, usar OnEnable()


    void Awake()
    {
        afterImage = gameObject.GetComponentInChildren<AfterImage>();
        //luzGlobalBranca = GameObject.Find("White Global Light 2D").GetComponent<Light2D>();
        //luzGlobalAzul = GameObject.Find("Blue Global Light 2D").GetComponent<Light2D>();

        standLeftRPos = new Vector2(-standWidthHalf + peleGrossura, 0f);
        standRightRPos = new Vector2(standWidthHalf - peleGrossura, 0f);

        standHash = Animator.StringToHash("Stand");
        speedHash = Animator.StringToHash("Speed");
        caindoHash = Animator.StringToHash("Caindo");

        groundedHash = Animator.StringToHash("Grounded");

        freandoHash = Animator.StringToHash("Freando");

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
            GUILayout.BeginArea(new Rect(10, 10, 220, 240), "Stats", "Window");

            //GUILayout.Label("hControlLockTime: " + hControlLockTime);
            //GUILayout.Label("hControlLock: " + (hControlLock ? "SIM" : "NÃO"));

            GUILayout.Label("mudarDireçãoDelay: " + mudarDireçãoDelay);
            //GUILayout.Label("tempoPirueta: " + tempoPirueta);

            //GUILayout.Label("spinReady: " + (spinReady ? "SIM" : "NÃO"));

            //GUILayout.Label("doubleJumpDelay: " + doubleJumpDelay);

            //GUILayout.Label("grudadoParede: " + grudadoParede);
            GUILayout.Label("wallJumpDelay: " + wallJumpDelay);

            GUILayout.Label("Bullet Time: " + (estáEmBulletTime ? "SIM" : "NÃO"));

            GUILayout.Label("Grounded: " + (grounded ? "SIM" : "NÃO"));
            //GUILayout.Label("Freando: " + (freando ? "SIM" : "NÃO"));
            GUILayout.Label("Olhando para: " + (olhandoDireita ? "DIREITA" : "ESQUERDA"));
            GUILayout.Label("Pulou: " + (pulou ? "SIM" : "NÃO"));
            GUILayout.Label("Caindo: " + (caindo ? "SIM" : "NÃO"));
            // GUILayout.Label("Empurrando: " + (empurrando ? "SIM" : "NÃO"));
            GUILayout.Label("Ground Mode: " + (groundMode));
            if (currentGroundInfo != null && currentGroundInfo.valid && grounded)
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { debug = !debug; print("debug!"); }
        if (Input.GetKeyDown(KeyCode.E)) { BulletTime(); }

        //#TODO colocar input de pulo aqui também
    }

    //FixedUpdate() is called before each internal physics update
    //(moving things due to physics, e.g., gravity)
    //Unity’s fixed timestep defaults to 0.02
    //This leads to FixedUpdate being called 50 times per second
    void FixedUpdate()
    {
        //INPUT
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (doubleJumpDelay > 0) {
            doubleJumpDelay -= Time.fixedDeltaTime;
        }

        //-----------------------------------------------------------------------------------------------------
        // ESTÁ NO CHÃO?
        //-----------------------------------------------------------------------------------------------------

        if (grounded)
        {
            //-----------------------------------------------------------------------------------------------------
            // SPINDASH
            //-----------------------------------------------------------------------------------------------------
            //tá parado, grounded, abaixado e apertou pulo?
            //comece a carregar o spindash
            if (groundVelocity == 0 && abaixado && !mudarDireção 
                && Input.GetButton("Jump") 
                && groundMode == GroundMode.Floor
                && leftHit.collider == null 
                && rightHit.collider == null)
            {
                spinReady = true;
                //mudarDireção = false;
                CreateDust();
                animator.Play("robot_spindash");
            }

            //soltou a seta pra baixo? simbora
            if (spinReady && !abaixado)
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
                spinReady = false;
            }

            //-----------------------------------------------------------------------------------------------------
            // RAMPAS
            //-----------------------------------------------------------------------------------------------------
            // ADICIONA AO groundVelocity O FATOR DE RAMPA APROPRIADO, AJUSTADO PELO TEMPO QUE PASSOU
            /* slow down when going uphill and speed up when going downhill.
            Fortunately, this is simple to achieve - with something called the Slope Factor. 
            Just subtract Slope Factor*sin(Ground Angle) from Ground Speed at the beginning of every step.
            */
            groundVelocity += slopeFactor * -Mathf.Sin(currentGroundInfo.angle) * Time.fixedDeltaTime;

            //-----------------------------------------------------------------------------------------------------
            // ABAIXAR
            //-----------------------------------------------------------------------------------------------------

            if (input.y < 0) { abaixado = true; }
            else { abaixado = false; }

            //-----------------------------------------------------------------------------------------------------
            // CAIR DA PAREDE/TETO
            //-----------------------------------------------------------------------------------------------------

            bool lostFooting = false;

            // SE NÃO ESTÁ NO FLOOR, MAS NÃO TEM VELOCIDADE PRA CORRER NA PAREDE/TETO
            if (groundMode != GroundMode.Floor && Mathf.Abs(groundVelocity) < fallVelocityThreshold)
            {
                groundMode = GroundMode.Floor; // VIRE PRO CHÃO
                grounded = false; // NÃO TÁ NO CHÃO
                //ESCORREGAR

                //TRAVE O CONTROLE NA HORIZONTAL
                traveControleH();

                lostFooting = true;
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
                doubleJumpReady = true;
                doubleJumpDelay = doubleJumpDelayTotal;

                if ((velocity.x > 0 && olhandoDireita) || (velocity.x < 0 && !olhandoDireita))
                {
                    animator.Play("robot_basic_jump");
                }
                else
                {
                    animator.Play("robot_backflip_TESTE");
                    tempoPirueta = 0.75f;
                    //#TODO variar tempoPirueta e velocidade da animação baseado na velocidade horizontal
                }

                /*
                //LONG JUMP
                if (Mathf.Abs(groundVelocity) >= fallVelocityThreshold) {
                    if (input.x > 0 && olhandoDireita) { velocity.x += 150; print("LONGJUMP direita!"); }
                    else if (input.x < 0 && !olhandoDireita) { velocity.x -= 150; print("LONGJUMP esquerda!"); }
                }*/

            }
            else
            {
                //timer do control lock
                if (hControlLock)
                {
                    hControlLockTime -= Time.fixedDeltaTime;
                    if (hControlLockTime < 0f)
                    {
                        hControlLock = false;
                        hControlLockTime = 0;
                    }
                }

                //-----------------------------------------------------------------------------------------------------
                // FRICÇÃO
                //-----------------------------------------------------------------------------------------------------

                // NÃO HÁ INPUT E/OU ESTÁ ABAIXADO? aplique fricção.
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

                if (!hControlLock && Mathf.Abs(input.x) >= 0.005f)
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
                        if (groundVelocity > -groundTopSpeed)
                        {
                            groundVelocity = Mathf.Max(-groundTopSpeed, groundVelocity + (input.x * acceleration) * Time.deltaTime);
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
                        if (groundVelocity < groundTopSpeed)
                        {
                            groundVelocity = Mathf.Min(groundTopSpeed, groundVelocity + (input.x * acceleration) * Time.deltaTime);
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
                /*
                if (velocity.y > 0f && velocity.y < 4f && Mathf.Abs(velocity.x) > 7.5f)
                {
                    velocity.x *= airDrag; //airDrag atual é 1f, então esse trecho todo é inútil
                }*/

                //-----------------------------------------------------------------------------------------------------
                // GRAVIDADE E VELOCIDADE TERMINAL
                //-----------------------------------------------------------------------------------------------------
                if (!grudadoParede) {
                    velocity.y = Mathf.Max(velocity.y + (gravity * Time.fixedDeltaTime), -terminalVelocity);
                }
            }

            if (doubleJumpReady && doubleJumpDelay <= 0 && Input.GetButtonDown("Jump") && !lowCeiling && !grudadoParede)
            {
                float jumpVel = jumpVelocity;
                velocity.y = jumpVel;
                grounded = false;
                pulou = true;
                doubleJumpReady = false;

                CreateDust();
                animator.Play("robot_basic_jump");
            }

            //-----------------------------------------------------------------------------------------------------
            // ACELERAR NO AR
            //-----------------------------------------------------------------------------------------------------

            // MOVA-SE, USANDO A airAcceleration AO INVÉS DA ACELERAÇÃO DO CHÃO
            if (Mathf.Abs(input.x) >= 0.005f)
            {
                if ((input.x < 0f && velocity.x > -groundTopSpeed) || (input.x > 0f && velocity.x < groundTopSpeed))
                {
                    float airAcc = /*underwater ? uwAirAcceleration :*/ airAcceleration;
                    velocity.x = Mathf.Clamp(velocity.x + (input.x * airAcc * Time.fixedDeltaTime), -groundTopSpeed, groundTopSpeed);
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

        WallCheck(sideRaycastDist, grounded ? sideRaycastOffset : 0f, out leftHit, out rightHit);

        if (leftHit.collider != null && rightHit.collider != null)
        {
            Debug.Log("GOT SQUASHED"); // Got squashed
        }
        else if (leftHit.collider != null)
        {
            //print("BATEU NA ESQUERDA: " + groundMode);
            //traveControleH();

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
            //traveControleH();

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
            //hControlLock = false;
        }

        //-----------------------------------------------------------------------------------------------------
        // WALLJUMP: GRUDAR NA PAREDE 
        //-----------------------------------------------------------------------------------------------------
        if ((leftHit.collider != null || rightHit.collider != null)
             && !grounded
             && groundMode == GroundMode.Floor
             && ((characterAngle >= 0 && characterAngle < 1)
             || characterAngle == 180))
        {
            grudadoParede = true;
            animator.Play("robot_wallgrab");
            velocity.y = 0;
            doubleJumpReady = true;
            if (leftHit.collider != null) { olhandoDireita = true; }
            else if (rightHit.collider != null) { olhandoDireita = false; }
        }
        else 
        {
            grudadoParede = false; 
            wallJumpDelay = wallJumpDelayTotal;
        }

        //-----------------------------------------------------------------------------------------------------
        // CAIR DA PAREDE
        //-----------------------------------------------------------------------------------------------------
        if (grudadoParede)
        {
            if (wallJumpDelay > 0 && !Input.GetButtonDown("Jump"))
            {
                wallJumpDelay -= Time.deltaTime;
            }
            else if (wallJumpDelay <= 0)
            {
                if (leftHit.collider != null) { velocity.x += 100; }
                else if (rightHit.collider != null) { velocity.x -= 100; }
                wallJumpDelay = wallJumpDelayTotal;
            }
            else if (Input.GetButtonDown("Jump")) {
                if (leftHit.collider != null) { velocity.x += 200; }
                else if (rightHit.collider != null) { velocity.x -= 200; }
                velocity.y += 300;
                animator.Play("robot_basic_jump");
            }
        }
        //#TODO de fato pular da parede
        //#TODO deslizar pra cima na parede quando pula do chão grudadinho nela

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
                // ATERRISSAR / LANDING (VERIFICA SE JÁ TOCOU NO CHÃO)
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
        { caindo = true; }
        else
        { caindo = false; }


        if (tempoPirueta > 0)
        { tempoPirueta -= Time.fixedDeltaTime; }
        else
        { tempoPirueta = 0; }


        if (grounded && !mudarDireção && !spinReady && velocity.x == 0)
        { animator.SetTrigger("Stand"); }

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

        if (Mathf.Abs(input.x) > 0.05f && grounded && !freando && !spinReady)
        {
            // indo prum lado mas olhando pro outro
            if ((input.x < 0 && olhandoDireita) || (input.x > 0 && !olhandoDireita))
            {
                mudarDireção = true;
                animator.Play("robot_turn");
            }
            // se tiver apertado certo
            else
            {
                mudarDireção = false;
            }
        }

        if (mudarDireção && grounded)
        {
            mudarDireçãoDelay -= Time.fixedDeltaTime;
            if (mudarDireçãoDelay < 0)
            {
                animator.SetTrigger("Stand");
                mudarDireção = false;
                mudarDireçãoDelay = 0.5f;
                olhandoDireita = !olhandoDireita;
                print("ACABOU a virada");
            }
        }
        else
        {
            mudarDireção = false;
            mudarDireçãoDelay = 0.5f;
        }

        transform.localScale = new Vector3(olhandoDireita ? 1 : -1, 1, 1);

        //-----------------------------------------------------------------------------------------------------
        // FREANDO
        //-----------------------------------------------------------------------------------------------------
        //nos jogos clássicos frear é meramente visual, a desaceleração é igual sempre        
        /*
        if (grounded)
        {
            if (freando == false && Mathf.Abs(groundVelocity) >= freandoLimiteVirar
                                 && ((groundVelocity < 0 && input.x > 0) ||
                                     (groundVelocity > 0 && input.x < 0)))
            {
                //animator.SetTrigger("Freando");
                freando = true;
                traveControleH();
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
                traveControleH();
            }
            else if (groundVelocity == 0 || ((groundVelocity > 0 && input.x > 0) || (groundVelocity < 0 && input.x < 0)))
            {
                hControlLock = false;
            }
        }
        */

        //linha que mostra a velocidade do boneco
        Debug.DrawLine(transform.position, new Vector3(transform.position.x + velocity.x / 5,
                                                       transform.position.y + velocity.y / 5, 0), Color.red);
    }

    bool freando = false;
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
        Quaternion rot = Quaternion.Euler(0f, 0f, (90f * (int)groundMode));
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
                    if (angle <= 315f && angle > 225f) { groundMode = GroundMode.LeftWall; }
                    else if (angle > 45f && angle <= 180f) { groundMode = GroundMode.RightWall; }
                }
                else {
                    if (angle > 35f && angle <= 325f && !hControlLock)
                    {
                        traveControleH();
                    }
                }
                //print("Y antes: " + pos.y);
                pos.y = info.point.y + heightHalf;
                //print("Y depois: " + pos.y);
                break;
            case GroundMode.RightWall:
                if (angle <= 45f && angle > 0f) { groundMode = GroundMode.Floor; }
                else if (angle > 135f && angle <= 270f) { groundMode = GroundMode.Ceiling; }
                pos.x = info.point.x - heightHalf;
                break;
            case GroundMode.Ceiling:
                if (angle <= 135f && angle > 45f) { groundMode = GroundMode.RightWall; }
                else if (angle > 225f && angle <= 360f) { groundMode = GroundMode.LeftWall; }
                pos.y = info.point.y - heightHalf;
                break;
            case GroundMode.LeftWall:
                if (angle <= 225f && angle > 45f) { groundMode = GroundMode.Ceiling; }
                else if (angle > 315f) { groundMode = GroundMode.Floor; }
                pos.x = info.point.x + heightHalf;
                break;
            default:
                break;
        }


        transform.position = pos;
    }


    //-----------------------------------------------------------------------------------------------------
    // CONTROL LOCK
    //-----------------------------------------------------------------------------------------------------
    /*
     Falling and Slipping Down Slopes
    At this point, slope movement will work rather well, 
    but it's not enough just to slow the Player down on steep slopes. 
    They need to slip down when it gets too steep and you are moving too slowly.

    The angle range of slopes for slipping is when your Ground Angle
    is within the range 46° to 315° (223~32) ($DF~$20) inclusive.
    In addition, the game will check if absolute Ground Speed falls below 2.5 ($280).

    So, when these conditions are met, what happens? Well, the Player will slip. 
    This achieved by detaching the Player from the floor (clearing the grounded state), 
    setting Ground Speed to 0, and employing the control lock timer.
    */

    //esse timer também é usado nas molas horizontais
    public void traveControleH() {
        hControlLock = true;
        hControlLockTime = 0.5f;
        //groundVelocity = 0;
    }


    //-----------------------------------------------------------------------------------------------------
    // BULLET TIME
    //-----------------------------------------------------------------------------------------------------
    public void BulletTime()
    {
        
        estáEmBulletTime = !estáEmBulletTime;
        if (estáEmBulletTime)
        {
            Time.timeScale = fatorLentidão;
            //print("ZA WARUDO! toki wo tomare!");
            luzGlobalBranca.SetActive(false);

            luzGlobalAzul.SetActive(true);
            afterImage.makeGhost = true;
            //se o tempo está 50% mais lento, a velocidade do jogador dobra
            //jogadorVelocidade /= fatorLentidão;

            //esse valor me parece arbitrário, melhor pesquisar
            Time.fixedDeltaTime = Time.timeScale * 0.02f; 
        }
        else
        {
            //print("Fim do Bullet Time.");

            //esse deltaTime é absoluto, não fica mais lento
            //Time.timeScale += (1f / duraçãoLentidão) * Time.unscaledDeltaTime;
            Time.timeScale = 1;
            //print("toki wa ugoki dasu");
            afterImage.makeGhost = false;
            luzGlobalAzul.SetActive(false);

            luzGlobalBranca.SetActive(true);
        }        
    }

    //-----------------------------------------------------------------------------------------------------
    // PÓ E PARTÍCULAS
    //-----------------------------------------------------------------------------------------------------

    void CreateDust() {
        dust.Play();
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
