using System.Net.Mime;
using UnityEngine;
using UnityEngine.Video;


/*
 * This script stores each location as a ScriptableObject.
 */

[CreateAssetMenu(fileName = "LocationData", menuName = "RIM/Location Data")]
public class LocationData : ScriptableObject
{
    public string locationName;

    [TextArea(4, 8)]
    public string description;

    public float x_cm;
    public float z_cm;

    public ContentType contentType;

    public Sprite image;
    public AudioClip audio;
    public VideoClip video;
    public GameObject model3D;
}


public enum ContentType
{
    TextOnly,
    Image,
    Audio,
    Video,
    Model3D
}
