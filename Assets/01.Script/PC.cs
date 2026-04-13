using UnityEngine;
using UnityEngine.UI;


public class PC : MonoBehaviour
{
    public bool isArrived = false;
    public Slider slider;
    public float usingTime;
    // private float[] earningTime = { 3f, 30f, 20f };
    public float earningTime = 20f;
    public Transform interactionPos;

    public bool isOccupied = false;

    // [SerializeField] private int level = 0;
    void Start()
    {
        slider = GetComponentInChildren<Slider>();
        slider.gameObject.SetActive(false);
    }

    public bool UpdateUsingTimer()
    {
        usingTime += Time.deltaTime;
        // float value = usingTime / earningTime[level];
        float value = usingTime / earningTime;


        slider.value = value;

        if(value >= 1)
        {
            usingTime = 0;
            return true;
        }
        return false;
    }
    
}
