using UnityEngine;


[CreateAssetMenu(menuName = "Managers/Example", fileName = "Example")]
public class ExampleSO : ScriptableObject
{
    public int lives = 10;

    public int score = 1000;

    public string currentLevel = "El Huesso";

    public string currentWeapon = "Giant nuke spider";
}
