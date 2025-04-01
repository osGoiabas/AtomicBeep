using UnityEngine;

public class Outline : MonoBehaviour
{
  public PlayerMovement playerMovement;
  public Material material;


  void Start() {
    playerMovement = GetComponentInParent<PlayerMovement>();
    material = GetComponent<SpriteRenderer>().material;
  }

  void Update() {
    if (playerMovement.estáMagnetizado) {
      material.SetColor("_Color", Color.magenta);
    } else {
      material.SetColor("_Color", Color.clear);
    }
  }
}
