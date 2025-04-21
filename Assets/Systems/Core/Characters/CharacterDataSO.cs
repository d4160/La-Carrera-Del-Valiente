using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "MiJuego/Data/Character Data")]
public class CharacterDataSO : ScriptableObject
{
    [Header("Identification")]
    public string CharacterName = "Nameless";
    // public Faction Faction; // Si tienes un sistema de facciones

    [Header("Core Stats")]
    public float MaxHealth = 100f;

    // ... otros datos base (resistencia, maná máximo, etc.)
}