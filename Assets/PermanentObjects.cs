using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermanentObjects : MonoBehaviour
{
    public static PermanentObjects instance;
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
        DontDestroyOnLoad(gameObject);
    }
}
