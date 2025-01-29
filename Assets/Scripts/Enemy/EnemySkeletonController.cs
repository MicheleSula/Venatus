using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySkeletonController : MonoBehaviour
{
    [System.Serializable]
    public class BodyPart
    {
        public string partName; // Nome della parte
        public GameObject bodyGameObject; // GameObject della parte
        public SpriteRenderer spriteRenderer; // SpriteRenderer per il rendering
    }

    private CreatureData creatureData;
    public Transform bodyParent; // Nodo genitore per le parti del corpo
    public List<BodyPart> bodyParts = new List<BodyPart>();

    public void InitializeSkeleton(CreatureData creatureData)
    {
        if (creatureData == null || creatureData.defaultBodyParts == null)
        {
            Debug.LogError("CreatureData non valido o mancano le body parts.");
            return;
        }

        this.creatureData = creatureData;

        foreach (var partData in creatureData.defaultBodyParts)
        {
            // Verifica se la parte del corpo è già presente
            BodyPart existingPart = bodyParts.Find(bp => bp.partName == partData.name);
            if (existingPart != null)
            {
                if (existingPart.bodyGameObject == null && partData.currentHealth > 0)
                {
                    // Ricrea il GameObject se era stato distrutto
                    GameObject partGO = new GameObject(partData.name);
                    partGO.transform.SetParent(bodyParent != null ? bodyParent : transform);
                    partGO.transform.localPosition = Vector3.zero;

                    // Aggiungi SpriteRenderer
                    SpriteRenderer spriteRenderer = partGO.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = partData.IsDamaged() ? partData.damagedSprite : partData.healthySprite;

                    existingPart.bodyGameObject = partGO;
                    existingPart.spriteRenderer = spriteRenderer;

                    Debug.Log($"[EnemySkeletonController] Body part ricreata: {partData.name}");
                }
                else if (existingPart.bodyGameObject != null)
                {
                    // Aggiorna lo sprite se necessario
                    SpriteRenderer sr = existingPart.spriteRenderer;
                    if (sr != null)
                    {
                        if (partData.IsDestroyed())
                        {
                            sr.sprite = null; // O un sprite di distruzione
                            Destroy(existingPart.bodyGameObject);
                            existingPart.bodyGameObject = null;
                            Debug.Log($"[EnemySkeletonController] Body part distrutta: {partData.name}");
                        }
                        else if (partData.IsDamaged())
                        {
                            sr.sprite = partData.damagedSprite;
                        }
                        else
                        {
                            sr.sprite = partData.healthySprite;
                        }
                    }
                }
            }
            else
            {
                if (partData.currentHealth > 0)
                {
                    // Crea una nuova parte del corpo
                    GameObject partGO = new GameObject(partData.name);
                    partGO.transform.SetParent(bodyParent != null ? bodyParent : transform);
                    partGO.transform.localPosition = Vector3.zero;

                    // Aggiungi SpriteRenderer
                    SpriteRenderer spriteRenderer = partGO.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = partData.IsDamaged() ? partData.damagedSprite : partData.healthySprite;

                    BodyPart bodyPart = new BodyPart
                    {
                        partName = partData.name,
                        bodyGameObject = partGO,
                        spriteRenderer = spriteRenderer
                    };
                    bodyParts.Add(bodyPart);

                    Debug.Log($"[EnemySkeletonController] Body part creata: {partData.name}");
                }
            }
        }
    }

    public void DamageBodyPart(string partName, int damage)
    {
        var bodyPart = bodyParts.Find(bp => bp.partName == partName);
        if (bodyPart != null)
        {
            var partData = creatureData.defaultBodyParts.FirstOrDefault(p => p.name == partName);
            if (partData == null) return;

            partData.TakeDamage(damage);

            if (partData.IsDestroyed())
            {
                if (bodyPart.bodyGameObject != null)
                {
                    Destroy(bodyPart.bodyGameObject);
                    bodyPart.bodyGameObject = null;
                }
                Debug.Log($"[EnemySkeletonController] Parte del corpo {partName} distrutta.");
            }
            else if (partData.IsDamaged())
            {
                if (bodyPart.spriteRenderer != null)
                {
                    bodyPart.spriteRenderer.sprite = partData.damagedSprite;
                }
                Debug.Log($"[EnemySkeletonController] Parte del corpo {partName} danneggiata.");
            }
        }
    }

    private Sprite GetDamagedSprite(string partName)
    {
        var partData = bodyParts.Find(bp => bp.partName == partName)?.spriteRenderer.sprite;
        return creatureData?.defaultBodyParts?.FirstOrDefault(p => p.name == partName)?.damagedSprite;
    }
}