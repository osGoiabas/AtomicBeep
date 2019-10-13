using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorSoniquiano : MonoBehaviour
{

    void FixedUpdate() {

        CalcularDistânciaMovimento();


        Mover(distânciaMovimento * Time.fixedDeltaTime);

        // IMPEDE A GRAVIDADE DE ACUMULAR
        if (tocandoChão)
        {
            distânciaMovimento.y = 0;
        }

        // RESETA 
        tocandoChão = false;
    }


    Vector2 distânciaMovimento;
    float distânciaMovimentoChão;
    float velocidade = 0.5f;
    float gravidade = 20;

    void CalcularDistânciaMovimento() {

        // PRA QUE LADO ESTAMOS APERTANDO?
        float inputHorizontal = Input.GetAxisRaw("Horizontal"); // -1 se esquerda, 1 se direita, 0 se parado

        // MOVEREMOS PARA OS LADOS EXATAMENTE A velocidade POR pseudoframe
        //distânciaMovimento.x = inputHorizontal * velocidade;
        distânciaMovimentoChão = inputHorizontal * velocidade;

        // ACELERAR COM A GRAVIDADE
        if (!tocandoChão) { 
            distânciaMovimento.y -= gravidade * Time.fixedDeltaTime;
        }
    }

    void Mover(Vector2 distânciaMovimento) {

        // CHEQUE COLISÕES ABAIXO
        ChecarColisõesAbaixo(ref distânciaMovimento);

        // MOVA, DE FATO.
        transform.Translate(distânciaMovimento, Space.World); //exato mesmo efeito de [xpos += xsp] e [ypos += ysp]
    }





    public Transform infEsq;
    public Transform infDir;
    const float espessuraPele = 0.015f;
    public LayerMask máscaraColisão;

    public bool tocandoChão;

    void ChecarColisõesAbaixo(ref Vector2 distânciaMovimento) {

        float comprimentoRaio = Mathf.Abs(distânciaMovimento.y) + espessuraPele;

        // CALCULA POSIÇÃO DE ORIGEM DOS RAIOS
        Vector2 posicaoInfEsq = new Vector2(infEsq.position.x + espessuraPele, infEsq.position.y + espessuraPele);
        Vector2 posicaoInfDir = new Vector2(infDir.position.x - espessuraPele, infDir.position.y + espessuraPele);

        // LANÇA E DESENHA RAIO DOS CANTOS PRA BAIXO
        RaycastHit2D hitEsq = Physics2D.Raycast(posicaoInfEsq, -transform.up, comprimentoRaio, máscaraColisão);
        RaycastHit2D hitDir = Physics2D.Raycast(posicaoInfDir, -transform.up, comprimentoRaio, máscaraColisão);
        Debug.DrawRay(posicaoInfEsq, -transform.up * comprimentoRaio, Color.red);
        Debug.DrawRay(posicaoInfDir, -transform.up * comprimentoRaio, Color.red);

        float direçãoY = distânciaMovimento.y == 0 ? -1 : Mathf.Sign(distânciaMovimento.y);
        float distânciaEsqArred = Mathf.Round(hitEsq.distance * 1000) / 1000; // ARREDONDA PARA 3 CASAS DECIMAIS
        float distânciaDirArred = Mathf.Round(hitDir.distance * 1000) / 1000; // ARREDONDA PARA 3 CASAS DECIMAIS

        if (hitEsq && hitDir) { //SE HÁ DOIS ALVOS AO ALCANCE, FIQUE NO MAIS PRÓXIMO (MAIS ALTO)
            float menorDistânciaArred = Mathf.Min(distânciaEsqArred, distânciaDirArred);
            distânciaMovimento.y = direçãoY * (menorDistânciaArred - espessuraPele);
            tocandoChão = true;
        } else if (hitEsq) { // SE APENAS hitEsq ESTÁ AO ALCANCE, FIQUE NELE
            distânciaMovimento.y = direçãoY * (distânciaEsqArred - espessuraPele);
            tocandoChão = true;
        } else if (hitDir) { // SE APENAS hitDir ESTÁ AO ALCANCE, FIQUE NELE
            distânciaMovimento.y = direçãoY * (distânciaDirArred - espessuraPele);
            tocandoChão = true;
        }

        float comprimentoRaioMeio = 3;

        // LANÇA E DESENHA RAIO DO MEIO
        Vector2 posicaoInfMeio = new Vector2((posicaoInfDir.x + posicaoInfEsq.x) / 2, (posicaoInfDir.y + posicaoInfEsq.y) / 2);
        RaycastHit2D hitMeio = Physics2D.Raycast(posicaoInfMeio, -transform.up, comprimentoRaioMeio, máscaraColisão);
        Debug.DrawRay(hitMeio.point, hitMeio.normal * comprimentoRaioMeio, Color.green);

        if ((hitEsq && !hitMeio && !hitDir) ||
            (!hitEsq && !hitMeio && hitDir)) {
            //print ("Tá quase caindo!");
            //estáBalançando = true;
        } else {
            //print ("Não tá caindo.");
            //estáBalançando = false;
        }

        // DESCOBRE ÂNGULO DO PISO
        float ânguloPiso = Vector2.Angle(hitMeio.normal, Vector2.up);

        
        if (distânciaMovimentoChão != 0) {
            distânciaMovimento.x = distânciaMovimentoChão * Mathf.Cos(ânguloPiso * Mathf.Deg2Rad);
                                // se ângulo for zero, cos será um, logo GSP será aplicado direto no X
                                // se ângulo for 90°, cos será zero, logo nada do GSP será aplicado ao X
                                
            print ("distânciaMovimento.x == " + distânciaMovimento.x);

            distânciaMovimento.y = distânciaMovimentoChão * Mathf.Sin(ânguloPiso * Mathf.Deg2Rad);
                                // se ângulo for zero, sen será zero, logo nada do GSP será aplicado ao Y
                                // se ângulo for 90°, sen será um, logo GSP será aplicado direto no Y
            print ("distânciaMovimento.y == " + distânciaMovimento.y);
        }

        transform.rotation = Quaternion.LookRotation(Vector3.forward, hitMeio.normal);
    }
}
