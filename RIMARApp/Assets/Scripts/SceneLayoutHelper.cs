using UnityEngine;

public static class SceneLayoutHelper
{
    public static Vector3 GetCircularPosition(int index, int totalObjects, float radius, float height = 0f)
    {
        float angleStep = 360f / totalObjects;
        float angle = angleStep * index;
        float radians = angle * Mathf.Deg2Rad;

        float x = Mathf.Sin(radians) * radius;
        float z = Mathf.Cos(radians) * radius;

        return new Vector3(x, height, z);
    }

    public static Vector3 GetSpiralPosition(int index, float startRadius, float radiusIncrement, float heightIncrement = 0.2f)
    {
        float angle = index * 45f;
        float radians = angle * Mathf.Deg2Rad;
        float radius = startRadius + (index * radiusIncrement);

        float x = Mathf.Sin(radians) * radius;
        float z = Mathf.Cos(radians) * radius;
        float y = index * heightIncrement;

        return new Vector3(x, y, z);
    }

    public static Vector3 GetRandomPosition(float minRadius, float maxRadius, float minHeight, float maxHeight)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(minRadius, maxRadius);

        float x = Mathf.Sin(angle) * radius;
        float z = Mathf.Cos(angle) * radius;
        float y = Random.Range(minHeight, maxHeight);

        return new Vector3(x, y, z);
    }

    public static Vector3 GetGridPosition(int x, int z, float spacing, float height = 0f)
    {
        return new Vector3(x * spacing, height, z * spacing);
    }
}
