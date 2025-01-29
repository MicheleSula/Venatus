using UnityEngine;

[CreateAssetMenu(fileName = "NewMove", menuName = "Game Data/Move")]
public class MoveData : ScriptableObject
{
    public string moveName;
    [TextArea] public string description;
    public DamageType damageType;
    public int basePower;
    public int accuracy;
    public int staminaCost;

    public void ExecuteMove(Creature attacker, Creature target, string targetPart)
{
    if (!target.bodyPartSlots.ContainsKey(targetPart))
    {
        Debug.LogWarning($"[ExecuteMove] Parte del corpo {targetPart} non trovata nel bersaglio {target.creatureData.creatureName}.");
        return;
    }

    var part = target.bodyPartSlots[targetPart];
    if (part.IsDestroyed())
    {
        Debug.Log($"[ExecuteMove] {targetPart} è già distrutta. Attacco inutile.");
        return;
    }

    int damage = Mathf.Max((attacker.finalStats.attack + basePower) - target.finalStats.defense, 0);
    target.TakeDamage(targetPart, damage);

    Debug.Log($"{attacker.creatureData.creatureName} usa {moveName}! Infligge {damage} danni a {targetPart} di {target.creatureData.creatureName}.");
}

}
