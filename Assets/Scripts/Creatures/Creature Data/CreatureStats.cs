[System.Serializable]
public class CreatureStats
{
    public int attack;
    public int defense;
    public int speed;
    public int maxHealth;

    public void Reset()
    {
        attack = 0;
        defense = 0;
        speed = 0;
        maxHealth = 0;
    }

    public override string ToString()
    {
        return $"ATK: {attack}, DEF: {defense}, SPD: {speed}, HP: {maxHealth}";
    }
}