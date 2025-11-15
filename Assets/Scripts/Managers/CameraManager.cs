using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    private CinemachineConfiner2D pcamconfiner;
    private CinemachineCamera cinemachineCamera;

    void Start()
    {
        
        if (instance != null)
        {
            //Debug.LogWarning($"Duplicate Instances found! First one: {instance.name} Second one: {name}. Destroying second one.");
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        //TODO: tirar isso do Update, só fazer OnSceneLoad ou algo do tipo
        pcamconfiner = GetComponentInChildren<CinemachineConfiner2D>();
        cinemachineCamera = GetComponentInChildren<CinemachineCamera>();
        pcamconfiner.BoundingShape2D = GameObject.FindGameObjectWithTag("Bounds").GetComponent<CompositeCollider2D>();
        cinemachineCamera.Follow = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }
}
