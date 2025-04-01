using UnityEngine;
using TMPro;

public class ItemManager : MonoBehaviour
{
    [SerializeField] public static ItemManager instance;
    [SerializeField] private TextMeshProUGUI text;
    public static int radsCollected;

    // Start is called before the first frame update
    void Awake()
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
        text = GameObject.FindGameObjectWithTag("RadsCollected").GetComponent<TextMeshProUGUI>();
        text.text = "x" + radsCollected.ToString();
    }

    public void ChangeRadsCollected(int radAmount) {
        radsCollected += radAmount;
        text.text = "x" + radsCollected.ToString();
    }
}
