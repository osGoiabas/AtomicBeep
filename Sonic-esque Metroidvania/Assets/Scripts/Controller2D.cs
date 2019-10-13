using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// COMECEI DIA 25/09/2019

public class Controller2D : RaycastController {

    public override void Start() {
        base.Start();
        collisions.faceDir = 1;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = false;
    }

    public void Move(Vector3 moveAmount) {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.moveAmountOld = moveAmount;

        //if (moveAmount.y < 0) {
        //    DescendSlope(ref moveAmount);
        //}

        if (moveAmount.x != 0) {
            collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        HorizontalCollisions(ref moveAmount);
        //if (moveAmount.y != 0) {
            VerticalCollisions(ref moveAmount);
        //}

        //print("o X é: " + moveAmount.x);

        FliparPersonagem();

        if (collisions.below) {
            Inclinar(); //NOVIDADE------------------------------------
        }

        transform.Translate(moveAmount);
    }
          
    //-----------------------------------------------------------------------------------------------------
    // DETECTOR DE COLISÕES
    //-----------------------------------------------------------------------------------------------------

    public CollisionInfo collisions;
    
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;

        public Vector3 moveAmountOld;

        public int faceDir;


        public void Reset() {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }


    public bool estáBalançando = false;

    public LayerMask collisionMask;
    float maxSlopeAngle = 1000;

    void HorizontalCollisions(ref Vector3 moveAmount) {

        float directionX = collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + espessuraPele;

        if (Mathf.Abs(moveAmount.x) < espessuraPele) {
            rayLength = 2 * espessuraPele;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector3 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += transform.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, transform.right * directionX, rayLength, collisionMask);

            // DESENHA RAIOS PARA FINS DE TESTE
            Debug.DrawRay(rayOrigin, transform.right * directionX, Color.green);

            if (hit) {

                if (hit.distance == 0) {
                    continue;
                }

                /*
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                
                if (i == 0 && slopeAngle <= maxSlopeAngle) {
                    if (collisions.descendingSlope) {
                        collisions.descendingSlope = false;
                        moveAmount = collisions.moveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - espessuraPele;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }*/

                //if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle) {
                    moveAmount.x = (hit.distance - espessuraPele) * directionX;
                    rayLength = hit.distance;

                    /*
                    if (collisions.climbingSlope) {
                        moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    } */

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                //}
            }
        }
    }

    void VerticalCollisions(ref Vector3 moveAmount) {
        //float directionY = Mathf.Sign (moveAmount.y);
        float directionY = moveAmount.y == 0 ? -1 : Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs (moveAmount.y) + espessuraPele;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector3 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += transform.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, transform.up * directionY, rayLength, collisionMask);

            // DESENHA RAIOS PARA FINS DE TESTE
            Debug.DrawRay(rayOrigin, transform.up * directionY, Color.green);

            if (hit) {
                moveAmount.y = directionY * (hit.distance - espessuraPele);
                rayLength = hit.distance;

                /*if (collisions.climbingSlope) {
                    moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                }*/

                collisions.below = directionY == -1;
                collisions.above = directionY == 1; 
            }
        }

        /*
        if (collisions.climbingSlope) {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + espessuraPele;
            Vector3 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + transform.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast (rayOrigin, transform.right * directionX, rayLength, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle) {
                    moveAmount.x = directionX * (hit.distance - espessuraPele);
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }*/
    }


    //-----------------------------------------------------------------------------------------------------
    // LIDA COM RAMPAS
    //-----------------------------------------------------------------------------------------------------

    /*
    void ClimbSlope(ref Vector3 moveAmount, float slopeAngle) {
        print("Tá escalando!");
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (moveAmount.y <= climbMoveAmountY) {
            moveAmount.y = climbMoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        } 
    }

    void DescendSlope(ref Vector3 moveAmount) {

        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, -transform.up, Mathf.Abs(moveAmount.y) + espessuraPele, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, -transform.up, Mathf.Abs(moveAmount.y) + espessuraPele, collisionMask);
        //SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
        //SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);

        if (!collisions.slidingDownMaxSlope) {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -transform.up, Mathf.Infinity, collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
                    if (Mathf.Sign(hit.normal.x) == directionX) {
                        if (hit.distance - espessuraPele <= Mathf.Tan(slopeAngle + Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x)) {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendMoveAmountY;

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector3 moveAmount) {

        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle) {
                moveAmount.x = hit.normal.x * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                collisions.slopeAngle = slopeAngle;
                collisions.slidingDownMaxSlope = true;
            }
        }
    }


    */

    SpriteRenderer spriteRenderer;

    void FliparPersonagem() {
        if (Mathf.Sign(collisions.faceDir) == 1) {
            spriteRenderer.flipX = false;
        } else if (Mathf.Sign(collisions.faceDir) == -1) {
            spriteRenderer.flipX = true;
        }
        //transform.localScale = new Vector3(transform.localScale.x * collisions.faceDir, transform.localScale.y, transform.localScale.z);

    }


    //-----------------------------------------------------------------------------------------------------
    // INCLINA PERSONAGEM
    //-----------------------------------------------------------------------------------------------------

    float comprimentoRaio = 1;

    

    void Inclinar() {

        Vector2 posicaoInfEsq = new Vector2(infEsq.position.x + espessuraPele, infEsq.position.y + espessuraPele);
        Vector2 posicaoInfDir = new Vector2(infDir.position.x - espessuraPele, infDir.position.y + espessuraPele);

        // LANÇA RAIO DOS CANTOS PRA BAIXO
        RaycastHit2D alvoInfoEsq = Physics2D.Raycast(posicaoInfEsq, -transform.up, comprimentoRaio, collisionMask);
        RaycastHit2D alvoInfoDir = Physics2D.Raycast(posicaoInfDir, -transform.up, comprimentoRaio, collisionMask);

        Debug.DrawRay(posicaoInfEsq, -transform.up * alvoInfoEsq.distance, Color.yellow);
        Debug.DrawRay(posicaoInfDir, -transform.up * alvoInfoDir.distance, Color.yellow);

        Debug.DrawRay(alvoInfoEsq.point, alvoInfoEsq.normal * 5, Color.red);
        Debug.DrawRay(alvoInfoDir.point, alvoInfoDir.normal * 5, Color.red);
        
        // ACHA E DESENHA O RAIO DO MEIO
        Vector2 mediaNormal = (alvoInfoEsq.normal + alvoInfoDir.normal) / 2;
        Vector2 mediaPonto = (alvoInfoEsq.point + alvoInfoDir.point) / 2;
        float mediaDistancia = (alvoInfoEsq.distance + alvoInfoDir.distance) / 2;
        Debug.DrawRay(mediaPonto, mediaNormal * mediaDistancia, Color.blue);

        transform.rotation = Quaternion.LookRotation(Vector3.forward, mediaNormal);
        //transform.rotation = Quaternion.Euler(0, 0, anguloRampa);





        Vector2 posicaoInfMeio = new Vector2((posicaoInfDir.x + posicaoInfEsq.x) / 2, (posicaoInfDir.y + posicaoInfEsq.y) / 2);
        RaycastHit2D alvoInfoMeio = Physics2D.Raycast(posicaoInfMeio, -transform.up, comprimentoRaio, collisionMask);
        Debug.DrawRay(alvoInfoMeio.point, alvoInfoMeio.normal * 5, Color.red);

        if ((alvoInfoEsq && !alvoInfoMeio && !alvoInfoDir) ||
            (!alvoInfoEsq && !alvoInfoMeio && alvoInfoDir)) {
            //print ("Tá quase caindo!");
            estáBalançando = true;
        } else {
            //print ("Não tá caindo.");
            estáBalançando = false;
        }

    }
}
