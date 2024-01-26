using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    private CinemachineConfiner2D pcamconfiner;
    void Start()
    {
        if (instance != null)
        {
            Debug.LogWarning($"Invalid configuration. Duplicate Instances found! First one: {instance.name} Second one: {name}. Destroying second one.");
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        pcamconfiner = GetComponentInChildren<CinemachineConfiner2D>();
        //pcamconfiner.InvalidatePathCache();
        pcamconfiner.m_BoundingShape2D = GameObject.FindGameObjectWithTag("Bounds").GetComponent<CompositeCollider2D>();
    }
}
