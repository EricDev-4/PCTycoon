using UnityEngine;
using TMPro;
public class MoneyUI : MonoBehaviour
{
    private TextMeshProUGUI tmp;

    void OnEnable()
    {
        GameManager.OnMoneyChanged += UpdateText;
    }

    void OnDisable()
    {
        GameManager.OnMoneyChanged -= UpdateText;
    }
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();

        if(GameManager.instance != null)
        {
            UpdateText(GameManager.instance.money);
        }
    }

    void UpdateText(int amount)
    {
        tmp.text = $"{amount:N0}$";
    }

}
