using Unity.Entities;
using UnityEngine;

public class EntityHelper : MonoBehaviour
{
    public static Entity GetEntityOfComponent(ComponentType componentType, EntityManager entityManager)
    {
        Unity.Collections.NativeArray<Entity> entities = entityManager.GetAllEntities();

        for (int i = 0; i < entities.Length; i++)
        {
            if (entityManager.HasComponent(entities[i], componentType))
            {
                return entities[i];
            }
        }

        entities.Dispose();

        return Entity.Null;
    }
}
