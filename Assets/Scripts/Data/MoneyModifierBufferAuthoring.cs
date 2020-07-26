using Unity.Entities;
using UnityEngine;

public class MoneyModifierBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<MoneyModifierBufferElement>(entity);
    }
}
