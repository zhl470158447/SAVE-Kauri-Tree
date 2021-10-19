using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractionPointDistribution
{
    
    //from https://github.com/bcrespy/unity-growing-tree/blob/master/Assets/Scripts/Generator.cs
    public List<Vector3> GenerateAttractorsSpherical(int numPoints, float distRadius, Vector3 startPos)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            float radius = Random.Range(0f, 1f);
            radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI / 2f), 0.8f);
            radius *= distRadius;
            // 2 angles are generated from which a direction will be computed
            float alpha = Random.Range(0f, Mathf.PI);
            float theta = Random.Range(0f, Mathf.PI * 2f);

            Vector3 pt = new Vector3(
                radius * Mathf.Cos(theta) * Mathf.Sin(alpha),
                radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
                radius * Mathf.Cos(alpha)
            );

            // translation to match the parent position
            pt += startPos - new Vector3(0, distRadius, 0);

            points.Add(pt);
        }
        return points;
    }
    public List<Vector3> GenerateAttractorsHemisphere(int numPoints, float distrRadius, Vector3 startPos)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            float radius = Random.Range(0f, 1f);
            radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI / 2f), 0.8f);
            radius *= distrRadius;
            // 2 angles are generated from which a direction will be computed
            float alpha = Random.Range(0f, Mathf.PI);
            float theta = Random.Range(0f, Mathf.PI);

            Vector3 pt = new Vector3(
                radius * Mathf.Cos(theta) * Mathf.Sin(alpha),
                radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
                radius * Mathf.Cos(alpha)
            );

            // translation to match the parent position
            pt += startPos + new Vector3(0, distrRadius, 0);

            points.Add(pt);
        }
        return points;
    }

    public List<Vector3> GenerateAttractorsMatureBranches(int numPoints, float distrRadius, Vector3 startPos)
    {
        Vector3 center = startPos + new Vector3(0, distrRadius/2, 0);
        List<Vector3> points = new List<Vector3>();
        while(points.Count<numPoints) //use while instead of for since some points will be discarded
        {
            float radius = Random.Range(0f, 1f);
            radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI / 2f), 0.8f);
            radius *= distrRadius;
            // 2 angles are generated from which a direction will be computed
            float alpha = Random.Range(0f, Mathf.PI);
            float theta = Random.Range(0f, Mathf.PI * 2f);

            Vector3 pt = new Vector3(
                radius * Mathf.Cos(theta) * Mathf.Sin(alpha),
                radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
                radius * Mathf.Cos(alpha)
            );

            if (pt.y < -distrRadius/3) //discard points with heights below threshold
            {
                continue;
            }
            else if(pt.y < 0) //for points below the half way point
            {
                Vector2 pt2D = new Vector2(pt.x, pt.z);
                Vector2 center2D = new Vector2(startPos.x, startPos.z);
                if (Vector2.Distance(pt2D, center2D) < 2) //discard if within radius of 2
                {
                    continue;
                }
            }

            // translation to match the parent position
            pt += center;

            points.Add(pt);
        }
        return points;
    }

    public List<Vector3> GenerateAttractorsCube(int numPoints, float distRadius, Vector3 startPos) //can be used as a starting point for complex distributions where you throw out points not in the shape
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            //random x, y, z
            float x = Random.Range(-distRadius, distRadius);
            float y = Random.Range(-distRadius, distRadius);
            float z = Random.Range(-distRadius, distRadius);
            Vector3 point = startPos + new Vector3(x, y, z) + new Vector3(0, -distRadius, 0);
            points.Add(point);
        }
        return points;
    }

    public List<Vector3> GenerateAttractorsCone(int numPoints, float height, Vector3 startPos)
    {
        List<Vector3> points = new List<Vector3>();
        while (points.Count < numPoints)
        {
            float pointHeight = Random.Range(0f, height); //vertical height of the point
            float radius = Mathf.Tan(Mathf.Deg2Rad*30)*(height-pointHeight); //30 degrees as stand in, modify to take a radius input as well as height and use them to find angle
            radius = Random.Range(0f, radius); // point is between center and radius at current height
            float angle = Random.Range(0f, Mathf.PI * 2f); //rotate point by random angle
            points.Add(startPos + new Vector3(radius*Mathf.Sin(angle), pointHeight, radius*Mathf.Cos(angle)));
        }
        return points;
    }
}
