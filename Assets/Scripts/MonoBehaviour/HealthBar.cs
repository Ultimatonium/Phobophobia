using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider healthBar;

    public void SetHealth(float health, float maxHealth)
    {
        if (healthBar == null) GetHealthBar();
        if (healthBar == null) return;
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }

    private void GetHealthBar()
    {
        if (name == "Player")
        {
            if (HUD.Instance == null) { Debug.LogWarning("HUD not loaded"); return; }
            healthBar = HUD.Instance.GetHealthBarPlayer();
        }
        if (name == "Bett")
        {
            if (HUD.Instance == null) { Debug.LogWarning("HUD not loaded"); return; }
            healthBar = HUD.Instance.GetHealthBarBase();
        }
        if (healthBar == null)
        {
            Debug.LogWarning("No healthbar for " + gameObject);
        }
    }
}
