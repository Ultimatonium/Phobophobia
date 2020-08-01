using Unity.Entities;

public class MoneySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref MoneyData moneyData, ref DynamicBuffer<MoneyModifierBufferElement> moneyModifiers) =>
        {
            for (int i = 0; i < moneyModifiers.Length; i++)
            {
                moneyData.money += moneyModifiers[i].value;
            }
            moneyModifiers.Clear();
        }
        ).Schedule();

        Entities.ForEach((ref MoneyData moneyData) =>
        {
            if (HUD.Instance != null)
            {
                HUD.Instance.SetMoney(moneyData.money);
            }
        }
        ).WithoutBurst().Run();
    }
}
