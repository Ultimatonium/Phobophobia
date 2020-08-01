using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    [SerializeField]
    private Slider healthBarBase;
    [SerializeField]
    private Slider healthBarPlayer;
    [SerializeField]
    private TextMeshProUGUI money;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    public void SetBaseHealth(float health, float maxHealth)
    {
        healthBarBase.maxValue = maxHealth;
        healthBarBase.value = health;
    }

    public void SetPlayerHealth(float health, float maxHealth)
    {
        healthBarPlayer.maxValue = maxHealth;
        healthBarPlayer.value = health;
    }

    public void SetMoney(int money)
    {
        this.money.text = money.ToString();
    }

    public Slider GetHealthBarBase()
    {
        return healthBarBase;
    }

    public Slider GetHealthBarPlayer()
    {
        return healthBarPlayer;
    }
}
