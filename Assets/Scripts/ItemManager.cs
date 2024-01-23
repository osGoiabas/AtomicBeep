using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemManager : MonoBehaviour
{
    [SerializeField] public static ItemManager instance;
    [SerializeField] private TextMeshProUGUI text;
    int radsCollected;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) {
            instance = this;
        }
    }

    public void ChangeRadsCollected(int radAmount) {
        radsCollected += radAmount;
        text.text = "x" + radsCollected.ToString();
    }
}
