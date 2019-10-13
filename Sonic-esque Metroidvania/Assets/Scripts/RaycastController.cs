using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    //[HideInInspector] public BoxCollider2D colliderr;

    public virtual void Start()
    {
        //colliderr = GetComponent<BoxCollider2D>();
        UpdateRaycastOrigins();
        CalculateRaySpacing();
    }

    //-----------------------------------------------------------------------------------------------------
    // DETERMINA ORIGEM DOS RAIOS DENTRO DA "PELE" DO COLLIDER
    //-----------------------------------------------------------------------------------------------------

    public struct RaycastOrigins
    {
        public Vector3 topLeft, topRight;
        public Vector3 bottomLeft, bottomRight;
    }

    public Transform supEsq;
    public Transform supDir;
    public Transform infEsq;
    public Transform infDir;


    public RaycastOrigins raycastOrigins;
    public const float espessuraPele = 0.015f;

    public void UpdateRaycastOrigins()
    {
        //cria "pele" na beira do collider
        //Bounds bounds = colliderr.bounds;
        //bounds.Expand (skinWidth * -2);

        //define a posicao dos quatro vetores do futuro raycast como as pontas dessa pele
        //raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        //raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        //raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        //raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

        //define a posição dos quatro vetores do futuro raycast como transforms específicos de gameObjects vazios
        raycastOrigins.topLeft = new Vector2(supEsq.position.x + espessuraPele, supEsq.position.y - espessuraPele);
        raycastOrigins.topRight = new Vector2(supDir.position.x - espessuraPele, supDir.position.y - espessuraPele);
        raycastOrigins.bottomLeft = new Vector2(infEsq.position.x + espessuraPele, infEsq.position.y + espessuraPele);
        raycastOrigins.bottomRight = new Vector2(infDir.position.x - espessuraPele, infDir.position.y + espessuraPele);
    }

    //-----------------------------------------------------------------------------------------------------
    // TRATA DA QUANTIDADE DE RAIOS
    //-----------------------------------------------------------------------------------------------------

    [HideInInspector] public int horizontalRayCount = 4;
    [HideInInspector] public int verticalRayCount = 4; 

    [HideInInspector] public float horizontalRaySpacing;
    [HideInInspector] public float verticalRaySpacing;

    const float dstBetweenRays = .25f;

    public void CalculateRaySpacing()
    {
        //Bounds bounds = colliderr.bounds;
        //bounds.Expand (skinWidth * -2);

        float largura = Vector3.Distance(raycastOrigins.bottomLeft, raycastOrigins.bottomRight);
        float altura = Vector3.Distance(raycastOrigins.topRight, raycastOrigins.bottomRight);

        //horizontalRayCount = Mathf.RoundToInt (altura / dstBetweenRays);
        //verticalRayCount = Mathf.RoundToInt (largura / dstBetweenRays);

        //horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        //verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        horizontalRaySpacing = altura / (horizontalRayCount - 1);
        verticalRaySpacing = largura / (verticalRayCount - 1);
    }
}
