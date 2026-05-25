using UnityEngine;


/*
 * Stores a story pathway.
 * Each pathway contains ordered locations and their clues.
 */

[CreateAssetMenu(fileName = "PathwayData", menuName = "RIM/Pathway Data")]
public class PathwayData : ScriptableObject
{
    public string pathwayName;

    [TextArea(3, 6)]
    public string pathwayDescription;

    public PathwayStep[] steps;
}


[System.Serializable]
public class PathwayStep
{
    [TextArea(2, 5)]
    public string clueText;

    public LocationData locationData;
}
