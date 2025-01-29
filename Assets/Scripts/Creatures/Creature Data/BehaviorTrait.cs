using UnityEngine;

[CreateAssetMenu(fileName = "NewBehaviorTrait", menuName = "Game Data/Behavior Trait")]
public class BehaviorTrait : ScriptableObject
{
    public string traitName;
    public int aggressionLevel;  
    public int cautionLevel;

    public bool ShouldFlee(int myCurrentHP, int enemyHP)
    {
        if (cautionLevel > aggressionLevel && enemyHP > myCurrentHP)
        {
            return true;
        }
        return false;
    }

    // TODO ALTRI METODI SHOULDHEAL, SHOULDCALLFORHELP, ETC.
}