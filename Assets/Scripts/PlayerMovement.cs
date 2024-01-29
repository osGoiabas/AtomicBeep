using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
//using UnityEngine.Experimental.Rendering.LWRP;

public class GroundInfo
{
    public float height;
    public Vector3 point;
    public float distance;
    public Vector3 normal;
    public float angle;
    public bool valid = false;
}

public class PlayerMovement : MonoBehaviour
{
    #region variáveis

    enum GroundMode
    {
        Floor,
        RightWall,
        Ceiling,
        LeftWall,
    }

    //-----------------------------------------------------------------------------------------------------
    // GERAL
    //-----------------------------------------------------------------------------------------------------

    [SerializeField] private GameInput gameInput;

    public bool grounded { get; private set; }
    public bool estáCaindo { get; private set; }
    public bool estáPulando { get; private set; }
    public bool estáPulandoNormal { get; private set; }
    public bool estáPiruetando { get; private set; }

    public bool abaixado { get; private set; }

    public bool empurrando { get; private set; }
    public bool estáWallSliding { get; private set; }
    public bool estáLedgeGrabbing { get; private set; }

    public bool lostFooting = false;

    public bool estáAtacando = false;

    //public bool IsInvulnerable = false;

    //float standingHeight = 40f;
    private float heightHalf = 20f;
    private float standWidthHalf = 10f;

    private int tileSideLength = 48;

    private Vector2 velocity;
    private float characterAngle;
    private bool lowCeiling;

    private Vector2 standLeftRPos;
    private Vector2 standRightRPos;

    RaycastHit2D leftHit;
    RaycastHit2D rightHit;

    RaycastHit2D ledgeLeft;
    RaycastHit2D ledgeRight;


    //-----------------------------------------------------------------------------------------------------
    // HABILIDADES
    //-----------------------------------------------------------------------------------------------------
    [Header("Habilidades")]
    [SerializeField] private bool pegouSpinDash = false;
    [SerializeField] private bool pegouWallJump = false;
    [SerializeField] private bool pegouDoubleJump = false;
    [SerializeField] private bool pegouBulletTime = false;

    //-----------------------------------------------------------------------------------------------------
    // GROUND MOVEMENT
    //-----------------------------------------------------------------------------------------------------
    [Header("Ground Movement")]
    float groundAcceleration = 150f; //old = 300f | SONIC? = 168.75f;
    float groundTopSpeed = 150f; //old = 300f | SONIC? = 360f;
    float speedLimit = 300f; //old = 650f | SONIC? = 960f; //MAIS RÁPIDO QUE 600 ELE FICA TRAVANDO NAS CURVAS ÀS VEZES, ACHO QUE É BUG DA UNITY
    float velocidadePura; //só usada para debug e pra checar se é pra makeGhost

    bool olhandoDireita = true;

    private float friction = 250f; //old = 500f | SONIC? = 168.75f;
    private float abaixadoFriction = 350f; //old = 350f | SONIC? = 84.375f;
    private float deceleration = 1500f; //old = 2999f | SONIC? = 1800f;

    private float dashSpeed = 1000f;

    private float peleGrossura = 0.05f;
    private float slopeFactor = 200f; //old = 450f

    private float ledgeHeightOffset = 10f;
    private float sideRaycastOffset = -4f;
    private float sideRaycastDist = 10f;
    private float groundRaycastDist = 24f;
    private float fallVelocityThreshold = 90f; //old = 180f | SONIC? = ???

    float groundVelocity = 0f;
    bool hControlLock = false;
    float hControlLockTimer = 0f;
    float hControlLockTime = 0.5f;
    GroundInfo currentGroundInfo;
    GroundMode groundMode = GroundMode.Floor;

    private float tempoPiruetaTotal = 0.5f; //0.75f;

    //-----------------------------------------------------------------------------------------------------
    // AIR MOVEMENT
    //-----------------------------------------------------------------------------------------------------   
    private float airAcceleration = 150f; //340f
    private float jumpVelocity = 300f; //old = 390f | SONIC? = 390f;
    private float jumpReleaseThreshold = 240f;
    private float gravity = -600f; //old = 790f | SONIC? = -790f;
    private float terminalVelocity = 500f; //old = 960f | SONIC? = 960f;
    //private float airDrag = 0.5f; //1f; //celeste tem muito airDrag, pesquisar

    //-----------------------------------------------------------------------------------------------------
    // ANIMAÇÃO
    //-----------------------------------------------------------------------------------------------------
    private Animator animator;
    private int speedHash;
    private int standHash;
    private int groundedHash;

    private int caindoHash;
    private int pulandoNormalHash;
    private int piruetandoHash;

    private int spinReadyHash;

    private int wallSlidingHash;
    private int ledgeGrabHash;
    private int ledgeClimbHash;

    private int brakeHash;
    private int freandoAgachadoHash;
    private int empurrandoHash;

    private int hitHash;
    private int atacandoHash;

    //-----------------------------------------------------------------------------------------------------
    // OUTROS
    //-----------------------------------------------------------------------------------------------------
    [Header("Outros")]
    public ParticleSystem dust;

    private bool spinReady = false;

    private bool mudarDireção = false;
    private float mudarDireçãoDelay = 0.3f;
    private float mudarDireçãoTimer;

    private float tempoPirueta = 0;

    private bool doubleJumpReady = false;
    private float doubleJumpDelay = 0;
    private float doubleJumpDelayTotal = 0.1f;

    private float wallJumpDelay;
    private float wallJumpDelayTotal = 0.5f;
    private float wallSlideSpeed = 80f;
    private float wallJumpVelocity = 300f; //old = 300f

    Vector2 posLedge1;
    Vector2 posLedge2;
    float ledgeClimbTimer;
    float ledgeClimbTimerTotal = 0.3f;
    bool estáLedgeClimbing = false;

    private float fatorLentidão = 0.2f;
    private bool estáEmBulletTime = false;
    private AfterImage afterImage;
    public GameObject luzGlobalBranca;
    public GameObject luzGlobalAzul;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private float jumpBuffer = 0.1f;
    private float jumpBufferCounter;

    /// <Summary>
    /// True if the character is currently in the hit state.
    /// </Summary>
    public bool isHit { get; private set; }
    private float hitTimer = 0f;
    private float hitDuration = 0.5f;
    public bool IsInvulnerable { get { return isHit || hitTimer > 0f; } }


    private CinemachineImpulseSource impulseSource;

    #endregion

    #region setup
    //Awake() is called immediately upon the start of the game,
    //independentemente do script estar ativado ou não.

    //Start() só é chamada quando o script for ativado,
    //e só será chamada essa única vez por toda a vida do script,
    //mesmo que ele seja desativado e reativado posteriormente.
    //Para essa última função, usar OnEnable()

    void Awake()
    {
        animator = GetComponent<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        afterImage = gameObject.GetComponentInChildren<AfterImage>();
        //luzGlobalBranca = GameObject.Find("White Global Light 2D").GetComponent<Light2D>();
        //luzGlobalAzul = GameObject.Find("Blue Global Light 2D").GetComponent<Light2D>();

        standLeftRPos = new Vector2(-standWidthHalf + peleGrossura, 0f);
        standRightRPos = new Vector2(standWidthHalf - peleGrossura, 0f);

        standHash = Animator.StringToHash("Stand");
        speedHash = Animator.StringToHash("Speed");
        groundedHash = Animator.StringToHash("Grounded");

        caindoHash = Animator.StringToHash("Caindo");
        pulandoNormalHash = Animator.StringToHash("PulandoNormal");
        piruetandoHash = Animator.StringToHash("Piruetando");

        spinReadyHash = Animator.StringToHash("SpinReady");

        wallSlidingHash = Animator.StringToHash("WallSliding");
        ledgeGrabHash = Animator.StringToHash("LedgeGrabbing"); 
        ledgeClimbHash = Animator.StringToHash("LedgeClimbing");

        brakeHash = Animator.StringToHash("Brake");
        freandoAgachadoHash = Animator.StringToHash("freandoAgachado");

        hitHash = Animator.StringToHash("Hit");
        atacandoHash = Animator.StringToHash("Atacando");

        empurrandoHash = Animator.StringToHash("Empurrando");
    }
    #endregion

    #region debugWindow
    private bool debug = false;
    void OnGUI()
    {
        if (debug)
        {
            GUILayout.BeginArea(new Rect(10, 10, 220, 300), "Stats", "Window");

            //GUILayout.Label("lostFooting: " + (lostFooting ? "SIM" : "NÃO"));
            //GUILayout.Label("hControlLockTimer: " + hControlLockTimer);
            //GUILayout.Label("hControlLock: " + (hControlLock ? "SIM" : "NÃO"));

            //GUILayout.Label("mudarDireção: " + (mudarDireção ? "SIM" : "NÃO"));
            //GUILayout.Label("mudarDireçãoTimer: " + mudarDireçãoTimer);
            GUILayout.Label("Olhando para: " + (olhandoDireita ? "DIREITA" : "ESQUERDA"));

            //GUILayout.Label("spinReady: " + (spinReady ? "SIM" : "NÃO"));
            //GUILayout.Label("doubleJumpDelay: " + doubleJumpDelay);
            //GUILayout.Label("Bullet Time: " + (estáEmBulletTime ? "SIM" : "NÃO"));

            //GUILayout.Label("estáWallSliding: " + (estáWallSliding ? "SIM" : "NÃO"));
            //GUILayout.Label("wallJumpDelay: " + wallJumpDelay);
            //GUILayout.Label("colliderParede?: " + ((leftHit.collider != null || rightHit.collider != null) ? "SIM" : "NÃO"));

            //GUILayout.Label("charAngle: " + characterAngle);
            GUILayout.Label("grounded: " + (grounded ? "SIM" : "NÃO"));
            //GUILayout.Label("freandoAgachado: " + (freandoAgachado ? "SIM" : "NÃO"));
            GUILayout.Label("estáCaindo: " + (estáCaindo ? "SIM" : "NÃO"));

            GUILayout.Label("isHit?: " + (isHit ? "SIM" : "NÃO"));
            GUILayout.Label("hitTimer: " + hitTimer);            

            //GUILayout.Label("lowCeiling: " + (lowCeiling ? "SIM" : "NÃO"));
            GUILayout.Label("estáPulando: " + (estáPulando ? "SIM" : "NÃO"));
            //GUILayout.Label("estáPulandoNormal: " + (estáPulandoNormal ? "SIM" : "NÃO"));
            //GUILayout.Label("estáPiruentado: " + (estáPiruentado ? "SIM" : "NÃO"));
            GUILayout.Label("tempoPirueta: " + tempoPirueta);
            //GUILayout.Label("coyoteTimeCounter: " + coyoteTimeCounter);
            //GUILayout.Label("jumpBufferCounter: " + jumpBufferCounter);

            //GUILayout.Label("abaixado: " + (abaixado ? "SIM" : "NÃO"));
            //GUILayout.Label("estáLedgeGrabbing: " + (estáLedgeGrabbing ? "SIM" : "NÃO"));
            //GUILayout.Label("estáLedgeClimbing: " + (estáLedgeClimbing ? "SIM" : "NÃO"));

            // GUILayout.Label("empurrando: " + (empurrando ? "SIM" : "NÃO"));

            GUILayout.Label("velocidadePura: " + velocidadePura);
            

            GUILayout.Label("Ground Mode: " + (groundMode));
            if (currentGroundInfo != null && currentGroundInfo.valid && grounded)
            {
                GUILayout.Label("Ground Speed: " + groundVelocity);
                GUILayout.Label("Angle (Deg): " + (currentGroundInfo.angle * Mathf.Rad2Deg));
            }
            GUILayout.EndArea();
        }
    }
    #endregion

    private void Start() {
        gameInput.OnPulo += GameInput_OnPulo;
        gameInput.OnBulletTime += GameInput_OnBulletTime;
        gameInput.OnAtaque += GameInput_OnAtaque;
    }

    public void Update()
    {
        //TODO: tirar isso do Update, só fazer OnSceneLoad ou algo do tipo
        gameInput = GameObject.FindGameObjectWithTag("Input").GetComponent<GameInput>();
    }

    private void GameInput_OnAtaque(object sender, System.EventArgs e) {
        FindObjectOfType<SoundManager>().PlaySFX("beepSwing");
        estáAtacando = true;
    }

    public void AcabarAtaque() {
        estáAtacando = false;
    }

    private void GameInput_OnBulletTime(object sender, System.EventArgs e) {

        //SetHitState(new Vector2(0, 0), 0);

        if (//Input.GetKeyDown(KeyCode.E) && 
            pegouBulletTime) 
        { 
            BulletTime(); 
        }
    }

    private void GameInput_OnPulo(object sender, System.EventArgs e) { 
        //Debug.Log("PRESSED JUMP!");

        if (grounded && !spinReady && jumpBufferCounter > 0)
            Pule();
        else if (grounded && !abaixado && !lowCeiling && !estáLedgeGrabbing && !estáLedgeClimbing && !spinReady)
            Pule();
        else if (!grounded && coyoteTimeCounter > 0 && !lowCeiling && !spinReady)
            Pule();

        //TÔ BOTANDO TODOS Input.GetButton("Jump") AQUI 
        //SÓ FALTA PULO VARIÁVEL
        
        //spin dash
        if (grounded
            && pegouSpinDash
            && groundVelocity == 0
            && !mudarDireção
            && abaixado
            && groundMode == GroundMode.Floor
            && leftHit.collider == null
            && rightHit.collider == null)
        {
            spinReady = true;
            //mudarDireção = false;
        }

        if (!grounded)
        {
            jumpBufferCounter = jumpBuffer;

            //-----------------------------------------------------------------------------------------------------
            // DOUBLE JUMP
            //-----------------------------------------------------------------------------------------------------
            #region doubleJump
            if (pegouDoubleJump
                && doubleJumpReady
                && doubleJumpDelay <= 0
                && !lowCeiling
                && !estáWallSliding)
            {
                Debug.Log("Double Jump!");
                float jumpVel = jumpVelocity;
                velocity.y = jumpVel;
                grounded = false;
                estáPulando = true;
                estáPulandoNormal = true;
                doubleJumpReady = false;
                CreateDust();
            }
            #endregion


            //-----------------------------------------------------------------------------------------------------
            // ALTURA DE PULO VARIÁVEL
            //-----------------------------------------------------------------------------------------------------

            //define valor máximo do pulo
            float jumpReleaThreshold = /*underwater ? uwJumpReleaseThreshold :*/ jumpReleaseThreshold;

            // JÁ PULOU, SOLTOU O BOTÃO, ESTÁ NO AR E A VELOCIDADE VERTICAL A SER APLICADA PASSOU DO LIMITE MÁXIMO?
            // VOLTA A APLICAR SÓ O MÁXIMO (a gravidade eventualmente o puxará pra baixo)
            if (estáPulando && velocity.y > jumpReleaThreshold)
            {
                //velocity.y = jumpReleaThreshold;
                //print("velocity.y = jumpReleaThreshold: " + velocity.y);
            }

        }

        //-----------------------------------------------------------------------------------------------------
        // CAIR DA PAREDE ou PULAR DA PAREDE / DAR WALLJUMP
        //-----------------------------------------------------------------------------------------------------
        if (estáWallSliding && !grounded)
        {
            if (leftHit.collider != null) { velocity.x += wallJumpVelocity/2; }
            else if (rightHit.collider != null) { velocity.x -= wallJumpVelocity/2; }   
            velocity.y += wallJumpVelocity;
            estáWallSliding = false;
            estáPulandoNormal = true;
        }

    }


    public void SetHitState(Vector2 source, int damage)
    {
        isHit = true;
        FindObjectOfType<SoundManager>().PlaySFX("beepHurt");
        CameraShakeManager.instance.CameraShake(impulseSource, 5f);
        FindObjectOfType<HitStop>().Stop(0.05f);

        hitTimer = hitDuration;
        //vida -= damage;

        characterAngle = 0f;
        grounded = false;
        estáPulando = false;
        spinReady = false;
        //isBraking = false;
        //ReleaseSpinDash(launch: false);

        // Jumping resets the horizontal control lock
        hControlLock = false;
        hControlLockTimer = 0f;
        
        
        //float positionDif = transform.position.x - source.x;

        Debug.Log(damage);

        //Vector2 hitStateVelocity = new Vector2(0, 0);
        //velocity = new Vector2(hitStateVelocity.x, hitStateVelocity.y);

        /*
        // If the damage source is nearly directly above or below us, default to getting knocked away from where we are facing at a lower speed
        if (Mathf.Abs(positionDif) < 1f)
        {
            velocity = new Vector2(hitStateVelocity.x * -FacingDirection, hitStateVelocity.y);
        }
        else
        {
            velocity = new Vector2(hitStateVelocity.x * Mathf.Sign(positionDif), hitStateVelocity.y);
        }
        
        animator.SetBool(springJumpHash, false);
        animator.SetBool(brakeHash, false);
        animator.SetFloat(speedHash, 0.1f);
        */
    }


    void FixedUpdate()
    {
        //INPUT
        //Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Vector2 inputVector = gameInput.GetMovementVector();


        if (Input.GetKeyDown(KeyCode.Tab)) { debug = !debug; print("debug!"); }


        if (doubleJumpDelay > 0) {
            doubleJumpDelay -= Time.deltaTime;
        }


        if (spinReady)
        {
            CreateDust();
        }


        if (hitTimer >= 0f)
        {
            hitTimer -= Time.fixedDeltaTime;
            if (hitTimer <= 0f)
            {
                isHit = false;
                hitTimer = 0;
            }
        }



        //-----------------------------------------------------------------------------------------------------
        // ESTÁ NO CHÃO?
        //-----------------------------------------------------------------------------------------------------


        if (grounded)
        {
            //-----------------------------------------------------------------------------------------------------
            // SPINDASH
            //-----------------------------------------------------------------------------------------------------
            #region spinDash

            //soltou a seta pra baixo? simbora
            if (spinReady && !abaixado)
            {
                if (olhandoDireita)
                {
                    groundVelocity += dashSpeed;
                }
                else
                {
                    groundVelocity -= dashSpeed;
                }
                spinReady = false;
            }
            #endregion

            //-----------------------------------------------------------------------------------------------------
            // RAMPAS
            //-----------------------------------------------------------------------------------------------------
            #region rampas
            // ADICIONA AO groundVelocity O FATOR DE RAMPA APROPRIADO, AJUSTADO PELO TEMPO QUE PASSOU
            /* slow down when going uphill and speed up when going downhill.
            Fortunately, this is simple to achieve - with something called the Slope Factor. 
            Just subtract Slope Factor*sin(Ground Angle) from Ground Speed at the beginning of every step.
            */
            groundVelocity += slopeFactor * -Mathf.Sin(currentGroundInfo.angle) * Time.deltaTime;
            #endregion

            //-----------------------------------------------------------------------------------------------------
            // ABAIXAR
            //-----------------------------------------------------------------------------------------------------
            if (inputVector.y < 0) { abaixado = true; }
            else { abaixado = false; }

            //-----------------------------------------------------------------------------------------------------
            // CAIR DA PAREDE/TETO
            //-----------------------------------------------------------------------------------------------------

            lostFooting = false;

            // SE NÃO ESTÁ NO FLOOR, MAS NÃO TEM VELOCIDADE PRA CORRER NA PAREDE/TETO
            if (groundMode != GroundMode.Floor && Mathf.Abs(groundVelocity) < fallVelocityThreshold)
            {
                if (pegouWallJump && (groundMode == GroundMode.LeftWall || groundMode == GroundMode.RightWall))
                {
                    characterAngle = 0;
                    transform.rotation = Quaternion.identity;
                    if (olhandoDireita) { transform.position += new Vector3(10f, 0f, 0f); } 
                    else { transform.position -= new Vector3(10f, 0f, 0f); }
                }
                else 
                {
                    //TRAVE O CONTROLE NA HORIZONTAL
                    TraveControleH();
                    lostFooting = true;
                }
                groundMode = GroundMode.Floor; // VIRE PRO CHÃO
                grounded = false; // NÃO TÁ NO CHÃO
            }



            //-----------------------------------------------------------------------------------------------------
            // PULAR
            //-----------------------------------------------------------------------------------------------------



            coyoteTimeCounter = coyoteTime;

            if (!estáPulando) 
            {
                //timer do control lock
                if (hControlLock)
                {
                    hControlLockTimer -= Time.deltaTime;
                    if (hControlLockTimer < 0f)
                    {
                        hControlLock = false;
                        hControlLockTimer = 0;
                    }
                }

                //-----------------------------------------------------------------------------------------------------
                // FRICÇÃO
                //-----------------------------------------------------------------------------------------------------

                // NÃO HÁ INPUT E/OU ESTÁ ABAIXADO? aplique fricção.
                if (abaixado || Mathf.Abs(inputVector.x) < 0.005f)
                {
                    // Mostly because I don't like chaining ternaries 
                    float dePéFric = /*underwater ? uwFriction :*/ friction;
                    float abaixadoFric = /*underwater ? uwRollingFriction :*/ abaixadoFriction;

                    float frc = abaixado ? abaixadoFric : dePéFric;

                    if (groundVelocity > 0f)
                    {
                        groundVelocity -= frc * Time.deltaTime;
                        if (groundVelocity < 0f) { groundVelocity = 0f; }
                    }
                    else if (groundVelocity < 0f)
                    {
                        groundVelocity += frc * Time.deltaTime;
                        if (groundVelocity > 0f) { groundVelocity = 0f; }
                    }
                }


                //-----------------------------------------------------------------------------------------------------
                // INPUT = MOVIMENTO!
                //-----------------------------------------------------------------------------------------------------


                if (!hControlLock && Mathf.Abs(inputVector.x) >= 0.005f && !abaixado)
                {
                    float accel = /*underwater ? uwAcceleration :*/ groundAcceleration;
                    float decel = /*underwater ? uwDeceleration :*/ deceleration;

                    //-----------------------------------------------------------------------------------------------------
                    // ESQUERDA
                    //-----------------------------------------------------------------------------------------------------
                    if (inputVector.x < 0f)
                    {
                        float acceleration = 0f;
                        if (groundVelocity > 0.005f)
                        { acceleration = decel; } // FREAR
                        else
                        { acceleration = accel; } // ACELERAR

                        // ACELERAR OU DESACELERAR, CONFORME ACIMA, RESPEITANDO O SPEEDCAP ATUAL (água ou terra)
                        if (groundVelocity > -groundTopSpeed)
                        {
                            groundVelocity = Mathf.Max(-groundTopSpeed, groundVelocity + (inputVector.x * acceleration) * Time.fixedDeltaTime);
                        }
                    }

                    //-----------------------------------------------------------------------------------------------------
                    // DIREITA
                    //-----------------------------------------------------------------------------------------------------
                    else if (inputVector.x > 0f)
                    {
                        float acceleration = 0f;
                        if (groundVelocity < -0.005f)
                        { acceleration = decel; } // FREAR
                        else
                        { acceleration = accel; } // ACELERAR

                        // ACELERAR OU DESACELERAR, CONFORME ACIMA, RESPEITANDO O SPEEDCAP ATUAL (água ou terra)
                        if (groundVelocity < groundTopSpeed)
                        {
                            groundVelocity = Mathf.Min(groundTopSpeed, groundVelocity + (inputVector.x * acceleration) * Time.fixedDeltaTime);
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


                velocidadePura = Vector3.Distance(transform.position,
                                                  new Vector3(transform.position.x + velocity.x,
                                                              transform.position.y + velocity.y, 0));

                //if (Mathf.Abs(groundVelocity) > groundTopSpeed)
                if (velocidadePura > groundTopSpeed + 1f)
                {
                    afterImage.makeGhost = true;
                    CreateDust();
                }
                else if (!estáEmBulletTime)
                {
                    afterImage.makeGhost = false;
                }

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
            if (coyoteTimeCounter > 0)
                coyoteTimeCounter -= Time.deltaTime;
            else 
                coyoteTimeCounter = 0;

            if (jumpBufferCounter > 0)
                jumpBufferCounter -= Time.deltaTime;
            else
                jumpBufferCounter = 0;


            //-----------------------------------------------------------------------------------------------------
            // ALTURA DE PULO VARIÁVEL
            //-----------------------------------------------------------------------------------------------------

            //define valor máximo do pulo
            float jumpReleaThreshold = /*underwater ? uwJumpReleaseThreshold :*/ jumpReleaseThreshold;

            // JÁ PULOU, SOLTOU O BOTÃO, ESTÁ NO AR E A VELOCIDADE VERTICAL A SER APLICADA PASSOU DO LIMITE MÁXIMO?
            // VOLTA A APLICAR SÓ O MÁXIMO (a gravidade eventualmente o puxará pra baixo)
            if (estáPulando && velocity.y > jumpReleaThreshold && Input.GetButtonUp("Jump"))
            {
                velocity.y = jumpReleaThreshold;
                print("velocity.y = jumpReleaThreshold: " + velocity.y);
            }
            else
            {
                //-----------------------------------------------------------------------------------------------------
                // RESISTÊNCIA DO AR
                //-----------------------------------------------------------------------------------------------------
                // POR QUE ESSES VALORES???
                
                /*if (velocity.y > 0f && velocity.y < 4f && Mathf.Abs(velocity.x) > 7.5f)
                {
                    velocity.x *= airDrag; //airDrag atual é 1f, então esse trecho todo é inútil
                }*/

                //-----------------------------------------------------------------------------------------------------
                // GRAVIDADE E VELOCIDADE TERMINAL
                //-----------------------------------------------------------------------------------------------------
                if (!estáWallSliding && !estáLedgeGrabbing) {
                    velocity.y = Mathf.Max(velocity.y + (gravity * Time.deltaTime), -terminalVelocity);
                }
            }



            //-----------------------------------------------------------------------------------------------------
            // ACELERAR NO AR
            //-----------------------------------------------------------------------------------------------------

            // MOVA-SE, USANDO A airAcceleration AO INVÉS DA ACELERAÇÃO DO CHÃO
            if (Mathf.Abs(inputVector.x) >= 0.005f)
            {
                if ((inputVector.x < 0f && velocity.x > -groundTopSpeed) || (inputVector.x > 0f && velocity.x < groundTopSpeed))
                {
                    float airAcc = /*underwater ? uwAirAcceleration :*/ airAcceleration;
                    velocity.x = Mathf.Clamp(velocity.x + (inputVector.x * airAcc * Time.deltaTime), -groundTopSpeed, groundTopSpeed);
                }
            }
        }

        // CLAMP VELOCITY TO THE GLOBAL SPEED LIMIT (going any faster could result in passing through things)
        velocity.x = Mathf.Clamp(velocity.x, -speedLimit, speedLimit);
        velocity.y = Mathf.Clamp(velocity.y, -speedLimit, speedLimit);

        //-----------------------------------------------------------------------------------------------------
        // MOVA-SE. Digo, mude a posição agora, antes de calcular as colisões
        //-----------------------------------------------------------------------------------------------------
        transform.position += new Vector3(velocity.x, velocity.y, 0f) * Time.deltaTime;


        //-----------------------------------------------------------------------------------------------------
        // PAREDES
        //-----------------------------------------------------------------------------------------------------
        #region paredes
        WallCheck(sideRaycastDist + peleGrossura, grounded ? sideRaycastOffset : 0f, out leftHit, out rightHit);
        LedgeCheck(sideRaycastDist + 10, ledgeHeightOffset, out ledgeLeft, out ledgeRight);

        if (leftHit.collider != null && rightHit.collider != null)
        {
            Debug.Log("GOT SQUASHED"); // Got squashed
        }
        else if (leftHit.collider != null)
        {
            transform.position = new Vector2(leftHit.point.x + standWidthHalf, transform.position.y);
            if (velocity.x < 0f)
            {
                velocity.x = 0f;
                groundVelocity = 0f;
                //if (grounded && (characterAngle == 0 || characterAngle == 180)) { empurrando = true; }
                //else { empurrando = false; }
            }
            //else if (velocity.x == 0) { empurrando = false; }
        }
        else if (rightHit.collider != null)
        {
            transform.position = new Vector2(rightHit.point.x - standWidthHalf, transform.position.y);
            if (velocity.x > 0f)
            {
                velocity.x = 0f;
                groundVelocity = 0f;
                //if (grounded && (characterAngle == 0 || characterAngle == 180)) { empurrando = true; }
                //else { empurrando = false; }
            }
            //else if (velocity.x == 0) { empurrando = false; }
        }
        else
        {
            //empurrando = false;
        }
        #endregion


        //-----------------------------------------------------------------------------------------------------
        // LEDGE GRAB e CLIMB 
        //-----------------------------------------------------------------------------------------------------
        #region ledge
        
        if (!grounded && rightHit.collider != null && ledgeRight.collider == null)
        {
            olhandoDireita = true;
            estáLedgeGrabbing = true;
        } 
        else if (!grounded && leftHit.collider != null && ledgeLeft.collider == null)
        {
            olhandoDireita = false;
            estáLedgeGrabbing = true;
        }
        else 
        {
            estáLedgeGrabbing = false;
            estáLedgeClimbing = false;
        }

        if (estáLedgeGrabbing && !estáLedgeClimbing) 
        {
            if (!grounded && rightHit.collider != null && ledgeRight.collider == null) 
            {
                velocity.y = 0;
                posLedge1.y = rightHit.point.y - 1 + (tileSideLength/6 - rightHit.point.y % (tileSideLength/6));
                transform.position = new Vector2(transform.position.x, posLedge1.y);

                if (inputVector.x > 0.05f /*|| Input.GetButton("Jump")*/)
                {
                    estáLedgeClimbing = true;
                    ledgeClimbTimer = ledgeClimbTimerTotal;
                }
                else
                {
                    estáLedgeClimbing = false;
                }
            }
            else if (!grounded && leftHit.collider != null && ledgeLeft.collider == null)
            {
                velocity.y = 0;
                posLedge1.y = leftHit.point.y - 1 + (tileSideLength/6 - leftHit.point.y % (tileSideLength/6));
                transform.position = new Vector2(transform.position.x, posLedge1.y);

                if (inputVector.x < -0.05f /*|| Input.GetButton("Jump")*/)
                {
                    estáLedgeClimbing = true;
                    ledgeClimbTimer = ledgeClimbTimerTotal;
                }
                else
                {
                    estáLedgeClimbing = false;
                }
            }

        }

        if (estáLedgeClimbing)
        {
            ledgeClimbTimer -= Time.deltaTime;
            if (ledgeClimbTimer <= 0)
            {
                if (rightHit.collider != null && ledgeRight.collider == null)
                {
                    posLedge2 = new Vector2(transform.position.x + 22, posLedge1.y + 18);
                    transform.position = new Vector2(posLedge2.x, posLedge2.y);
                    estáLedgeClimbing = false;
                }
                if (leftHit.collider != null && ledgeLeft.collider == null)
                {
                    posLedge2 = new Vector2(transform.position.x - 22, posLedge1.y + 18);
                    transform.position = new Vector2(posLedge2.x, posLedge2.y);
                    estáLedgeClimbing = false;
                }
            }
        }

        #endregion
        //-----------------------------------------------------------------------------------------------------
        // WALLSLIDE: GRUDAR NA PAREDE 
        //-----------------------------------------------------------------------------------------------------
        #region wallJump
        if (pegouWallJump
            && (leftHit.collider != null || rightHit.collider != null)
            && !estáLedgeGrabbing
            && !grounded
            && groundMode == GroundMode.Floor
            && ((characterAngle >= 0 && characterAngle <= 5)
            || (characterAngle >= 355 && characterAngle <= 360) 
            || (characterAngle >= 175 && characterAngle <= 185)))
        {
            estáWallSliding = true;
            doubleJumpReady = true;
            if (leftHit.collider != null) { olhandoDireita = true; }
            else if (rightHit.collider != null) { olhandoDireita = false; }


            velocity = new Vector2(velocity.x, velocity.y * 0.90f);

            //velocity.y -= friction * Time.deltaTime;

            
            if (velocity.y > 0)
            {
                velocity.y -= friction * Time.deltaTime;
            }
            else 
            { 
                //velocity.y = 0;
            }
        }
        else 
        {
            estáWallSliding = false; 
            wallJumpDelay = wallJumpDelayTotal;
        }

        //-----------------------------------------------------------------------------------------------------
        // CAIR DA PAREDE ou PULAR DA PAREDE / DAR WALLJUMP
        //-----------------------------------------------------------------------------------------------------
        if (estáWallSliding && !grounded)
        {
            if (wallJumpDelay > 0 && velocity.y <= 0 )
            {
                //wallJumpDelay -= Time.deltaTime;
                transform.position = new Vector2(transform.position.x, transform.position.y - wallSlideSpeed * Time.deltaTime);
            }
            else if (wallJumpDelay < 0)
            { // solta ele da parede se o tempo acabar; cód está atualmente INATIVO
                if (leftHit.collider != null) { velocity.x += wallSlideSpeed; }
                else if (rightHit.collider != null) { velocity.x -= wallSlideSpeed; }
                wallJumpDelay = wallJumpDelayTotal;
            }
        }

        //#TODO fazer jumpBuffer pro wallJump também, a la Celeste, prov será com outro Raycast, 2 pixels mais longo ou algo do tipo
        //#TODO deslizar pra cima na parede quando pula do chão grudadinho nela, a la Fancy Pants
        #endregion

        //-----------------------------------------------------------------------------------------------------
        // COLISÃO EM CIMA E EM BAIXO
        //-----------------------------------------------------------------------------------------------------
        #region cima e baixo
        bool groundedLeft = false;
        bool groundedRight = false;

        bool ceilingLeft = false;
        bool ceilingRight = false;

        int ceilDirection = (int)groundMode + 2; // se groundMode for 0, o teto será 2
        if (ceilDirection > 3) { ceilDirection -= 4; } // se groundMode for 3, o teto será 5-4, isto é, 1.

        GroundInfo ceil = GroundedCheck(groundRaycastDist, (GroundMode)ceilDirection, out ceilingLeft, out ceilingRight);

        if (grounded) // ESTÁ NO CHÃO?
        {
            //não sei bem pra que serve essa parte aqui, mas ela corta um frame do delay grouded. Falta cortar mais um.
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
                    estáPulando = false;

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

        // o certo é não precisar disso, mas enfim
        if (estáPulandoNormal || estáPiruetando)
        estáPulando = true;

        if (grounded)
        {
            StickToGround(currentGroundInfo);

            // ANIMAÇÃO
            animator.SetFloat(speedHash, Mathf.Abs(groundVelocity));

            // A CABEÇA DO SPRITE ESTÁ DENTRO DO TETO?
            lowCeiling = ceil.valid && transform.position.y > ceil.point.y - 25f;
        }
        #endregion

        //-----------------------------------------------------------------------------------------------------
        // ANIMAÇÕES
        //-----------------------------------------------------------------------------------------------------

        // CAIR FALLING
        if (!estáWallSliding && !grounded && velocity.y < 0 && tempoPirueta == 0)
        { estáCaindo = true; }
        else
        { estáCaindo = false; }

        if (grounded || estáCaindo || estáWallSliding || estáLedgeGrabbing || estáLedgeClimbing)
            estáPulando = false;

        if (!estáPulando) {
            estáPulandoNormal = false;
            estáPiruetando = false;
            tempoPirueta = 0;
        }


        if (tempoPirueta > 0) { 
            tempoPirueta -= Time.deltaTime;
            estáPiruetando = true;
        }
        else
        { tempoPirueta = 0; }



        if (grounded && !mudarDireção && !spinReady && velocity.x == 0)
        { 
            //animator.SetTrigger("Stand");
        }

        //animator.SetBool(groundedHash, grounded);
        animator.SetBool(pulandoNormalHash, estáPulandoNormal);
        animator.SetBool(piruetandoHash, estáPiruetando);
        animator.SetBool(caindoHash, estáCaindo);        

        animator.SetBool(wallSlidingHash, estáWallSliding);
        animator.SetBool(ledgeGrabHash, estáLedgeGrabbing);
        animator.SetBool(ledgeClimbHash, estáLedgeClimbing);

        animator.SetBool(hitHash, isHit);
        animator.SetBool(atacandoHash, estáAtacando);

        animator.SetBool(spinReadyHash, spinReady);

        animator.SetBool(brakeHash, mudarDireção);
        //animator.SetBool(freandoAgachadoHash, freandoAgachado);
        //animator.SetBool(abaixadoHash, abaixado);
        //animator.SetBool(empurrandoHash, empurrando);

        //-----------------------------------------------------------------------------------------------------
        // ROTAÇÃO ROTATE
        //-----------------------------------------------------------------------------------------------------
        if (grounded) 
        {
            // rotaciona personagem perfeitamente, como em Sonic Mania e Freedom Planet, não em frações de 45º
            transform.rotation = Quaternion.Euler(0f, 0f, characterAngle); 
        }
        else // não está no chão? rode para a posição de queda normal.
        { 
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 5 * Time.deltaTime);
            characterAngle = 0;
        }

        //-----------------------------------------------------------------------------------------------------
        // DIREÇÃO E mudarDireção, VIRAR, TURN      
        //-----------------------------------------------------------------------------------------------------
        #region direção

        if (Mathf.Abs(inputVector.x) > 0.05f && grounded && !freandoAgachado && !spinReady)
        {
            // indo prum lado mas olhando pro outro
            if ((inputVector.x < 0.05f && olhandoDireita) || (inputVector.x > 0.05f && !olhandoDireita))
            {
                mudarDireção = true;
            } 
        }

        if (mudarDireção && grounded)
        {
            CreateDust();
            mudarDireçãoTimer -= Time.deltaTime;
            if (mudarDireçãoTimer < 0)
            {
                animator.SetTrigger("BrakeEnd");
                mudarDireção = false;
                mudarDireçãoTimer = mudarDireçãoDelay;
                olhandoDireita = !olhandoDireita;
            }
        }
        else
        {
            mudarDireção = false;
            mudarDireçãoTimer = mudarDireçãoDelay;
        }

        transform.localScale = new Vector3(olhandoDireita ? 1 : -1, 1, 1);
        #endregion

        //-----------------------------------------------------------------------------------------------------
        // freandoAgachado
        //-----------------------------------------------------------------------------------------------------
        #region frear
        //nos jogos clássicos frear é meramente visual, a desaceleração é igual sempre        
        /*
        if (grounded)
        {
            if (freandoAgachado == false && Mathf.Abs(groundVelocity) >= freandoAgachadoLimiteVirar
                                 && ((groundVelocity < 0 && inputVector.x > 0) ||
                                     (groundVelocity > 0 && inputVector.x < 0)))
            {
                //animator.SetTrigger("freandoAgachado");
                freandoAgachado = true;
                TraveControleH();
                FindObjectOfType<SoundManager>().Play("freandoAgachado");
            }
            else if (freandoAgachado && Mathf.Abs(groundVelocity) < freandoAgachadoLimiteVirar)
            {
                if (Mathf.Abs(inputVector.x) > 0.05f)
                {
                    olhandoDireita = !olhandoDireita;
                    animator.SetTrigger("freandoAgachadoVirar");
                }
                freandoAgachado = false;
                TraveControleH();
            }
            else if (groundVelocity == 0 || ((groundVelocity > 0 && inputVector.x > 0) || (groundVelocity < 0 && inputVector.x < 0)))
            {
                hControlLock = false;
            }
        }
        */
        #endregion

        //linha que mostra a velocidade do boneco
        Debug.DrawLine(transform.position, new Vector3(transform.position.x + velocity.x / 5,
                                                       transform.position.y + velocity.y / 5, 0), Color.red);
    }

    [Header ("Frear")]
    [SerializeField] AudioSource somfreandoAgachado;
    bool freandoAgachado = false;
    //bool freandoAgachadoVirar = false;
    //float freandoAgachadoLimiteVirar = 100;




    #region pule
    void Pule() {
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
        float jumpVel = jumpVelocity;


        //print("velocity.y: " + velocity.y);

        velocity.x -= jumpVel * (Mathf.Sin(currentGroundInfo.angle));
        velocity.y = jumpVel * (Mathf.Cos(currentGroundInfo.angle));
        grounded = false;
        estáPulando = true;

        if (groundMode == GroundMode.LeftWall || groundMode == GroundMode.RightWall) {
            velocity.y += jumpVel/2;
        }
        //print("velocity.y: " + velocity.y);

        doubleJumpReady = true;
        doubleJumpDelay = doubleJumpDelayTotal;

        if ((velocity.x > 0 && olhandoDireita) || (velocity.x < 0 && !olhandoDireita) || Mathf.Abs(velocity.x) <= 0.05f)
        {
            estáPulandoNormal = true;
            estáPiruetando = false;
        }
        else
        {
            estáPulandoNormal = false;
            estáPiruetando = true;
            tempoPirueta = tempoPiruetaTotal;
            //#TODO variar tempoPirueta e velocidade da animação baseado na velocidade horizontal
        }

        /*
        //LONG JUMP
        if (Mathf.Abs(groundVelocity) >= fallVelocityThreshold) {
            if (inputVector.x > 0 && olhandoDireita) { velocity.x += 150; print("LONGJUMP direita!"); }
            else if (inputVector.x < 0 && !olhandoDireita) { velocity.x -= 150; print("LONGJUMP esquerda!"); }
        }*/
    }
    #endregion

    [SerializeField] public LayerMask máscaraColisão;
    void WallCheck(float distance, float heightOffset, out RaycastHit2D hitLeft, out RaycastHit2D hitRight)
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y + heightOffset);

        hitLeft = Physics2D.Raycast(pos, Vector2.left, distance, máscaraColisão);
        hitRight = Physics2D.Raycast(pos, Vector2.right, distance, máscaraColisão);

        Debug.DrawLine(pos, pos + (Vector2.left * distance), Color.blue);
        Debug.DrawLine(pos, pos + (Vector2.right * distance), Color.blue);
    }

    
    void LedgeCheck(float ledgeDistance, float ledgeHeightOffset, out RaycastHit2D ledgeLeft, out RaycastHit2D ledgeRight) 
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y + ledgeHeightOffset);

        ledgeLeft = Physics2D.Raycast(pos, Vector2.left, ledgeDistance, máscaraColisão);
        ledgeRight = Physics2D.Raycast(pos, Vector2.right, ledgeDistance, máscaraColisão);

        Debug.DrawLine(pos, pos + (Vector2.left * ledgeDistance), Color.blue);
        Debug.DrawLine(pos, pos + (Vector2.right * ledgeDistance), Color.blue);
    }
    

    //-----------------------------------------------------------------------------------------------------
    // TETO E PISO
    //-----------------------------------------------------------------------------------------------------
    private Vector2 leftRaycastPos { get { return standLeftRPos; }}
    private Vector2 rightRaycastPos { get { return standRightRPos; }}

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

            // checa o lado com valor mais "alto", a depender do ângulo
            if (leftCompare >= rightCompare) { found = GetGroundInfo(leftHit); }
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
                    if (angle > 45f && angle <= 315f && !hControlLock)
                    {
                        TraveControleH();
                        if (angle > 60f && angle <= 300f)
                        {
                            Escorregar("muito");
                        }
                        else
                        {
                            Escorregar("pouco");
                        }
                    }
                }
                pos.y = info.point.y + heightHalf;
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
    #region controlLock e escorregar
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
    public void TraveControleH() {
        hControlLock = true;
        hControlLockTimer = hControlLockTime;
        groundVelocity /= 2;
    }

    public void Escorregar(string intensidade) {
        float quantoEscorregará = 0f;

        if (intensidade == "muito") {
            quantoEscorregará = 200f;
        } else if (intensidade == "pouco") {
            quantoEscorregará = 100f;
        }

        if (characterAngle > 0.05f && characterAngle < 90f) 
        { groundVelocity -= quantoEscorregará; }
        else if (characterAngle < 359f && characterAngle > 180f)
        { groundVelocity += quantoEscorregará; }
    }


    #endregion

    //-----------------------------------------------------------------------------------------------------
    // BULLET TIME
    //-----------------------------------------------------------------------------------------------------
    public void BulletTime()
    {        
        estáEmBulletTime = !estáEmBulletTime;
        if (estáEmBulletTime)
        {
            Time.timeScale = fatorLentidão;
            Debug.Log("ZA WARUDO! toki wo tomare!");
            //luzGlobalBranca.SetActive(false);
            //luzGlobalAzul.SetActive(true);

            afterImage.makeGhost = true;
            //se o tempo está 50% mais lento, a velocidade do jogador dobra
            //jogadorVelocidade /= fatorLentidão;

            //esse valor me parece arbitrário, melhor pesquisar
            //Time.fixedDeltaTime = Time.timeScale * 0.02f; 
        }
        else
        {
            //esse deltaTime é absoluto, não fica mais lento
            //Time.timeScale += (1f / duraçãoLentidão) * Time.unscaledDeltaTime;
            Time.timeScale = 1;
            Debug.Log("toki wa ugoki dasu");
            afterImage.makeGhost = false;
            //luzGlobalAzul.SetActive(false);
            //luzGlobalBranca.SetActive(true);
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
