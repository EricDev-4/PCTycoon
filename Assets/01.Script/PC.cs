using UnityEngine;
using UnityEngine.UI;


public class PC : MonoBehaviour
{
    public bool isArrived = false;
    public Slider slider;
    private float usingTime;
    private float[] earningTime = { 3f, 30f, 20f };

    private int level = 0;
    void Start()
    {
       slider = GetComponentInChildren<Slider>();
        slider.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public bool UsingPc()
    {
        usingTime += Time.deltaTime;
        float value = usingTime / earningTime[level];

        slider.value = value;

        if(value >= 1)
        {
            slider.gameObject.SetActive(false);
            usingTime = 0;
            return true;
        }
        return false;
    }


    
}
