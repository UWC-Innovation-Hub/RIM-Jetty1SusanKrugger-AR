using UnityEngine;


/*
 * This script creates a single database asset that would store all 30 locations' info.
 */

[CreateAssetMenu(fileName = "LocationDatabase", menuName = "RIM/Location Database")]
public class LocationDatabase : ScriptableObject
{
    public LocationData[] locations;
}
