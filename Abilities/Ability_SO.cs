using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Ability", fileName = "Ability_")]
public class Ability_SO : ScriptableObject
{
    public AbilityType abilityType;
    public Sprite icon;
}