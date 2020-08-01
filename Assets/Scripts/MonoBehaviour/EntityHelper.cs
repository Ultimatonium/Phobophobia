using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class EntityHelper : MonoBehaviour
{
    public static Entity GetEntityOfComponent(ComponentType componentType, EntityManager entityManager)
    {
        NativeArray<Entity> entities = entityManager.GetAllEntities();

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

    public static Entity GetEntityOfGameObject(GameObject gameObject, EntityManager entityManager)
    {
        NativeArray<Entity> entities = entityManager.GetAllEntities();

        for (int i = 0; i < entities.Length; i++)
        {
            if (!entityManager.HasComponent<Transform>(entities[i])) continue;
            if (entityManager.GetComponentObject<Transform>(entities[i]).gameObject == gameObject)
            {
                return entities[i];
            }
        }

        entities.Dispose();

        return Entity.Null;
    }
}
