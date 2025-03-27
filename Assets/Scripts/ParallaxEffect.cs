using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private float _startingPosX; //This is starting position of the sprites.
    private float _startingPosY; //This is starting position of the sprites.
    [SerializeField] private float amountOfParallax;  //This is amount of parallax scroll. 
    [SerializeField] private Camera MainCamera;   //Reference of the camera.

    private void Start()
    {
        //Getting the starting X position of sprite.
        _startingPosX = transform.position.x;
        _startingPosY = transform.position.y;
    }

    private void Update()
    {
        Vector3 position = MainCamera.transform.position;
        float distanceX = position.x * amountOfParallax;
        float distanceY = position.y * amountOfParallax * 0.5f;

        Vector3 newPosition = new(_startingPosX + distanceX,
                                  transform.position.y,//_startingPosY + distanceY, #TODO: implementar parallax vertical
                                  transform.position.z);

        transform.position = newPosition;
    }
}
