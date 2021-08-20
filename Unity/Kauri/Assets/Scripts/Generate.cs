using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//based off of https://ciphrd.com/2019/09/11/generating-a-3d-growing-tree-using-a-space-colonization-algorithm/
public class Generate : MonoBehaviour
{

    class Branch
    {
        public Vector3 start;
        public Vector3 end;
        Vector3 direction;
        Branch parent;
        List<Branch> children;
        int distanceFromRoot;

        public Branch(Vector3 start, Vector3 end, Vector3 direction, Branch parent)
        {
            this.start = start;
            this.end = end;
            this.direction = direction;
            this.parent = parent;
        }
    }

    [Header("Branch parameters")]
    public Vector3 startingPoint = new Vector3(0, 0, 0);
    public float segmentLength=0.5f;
    public float killDistance=0.5f;
    public int numAttracionPoints = 400;

    [Header("Root parameters")]
    public bool generateRoots=true;
    public Vector3 startingPointR = new Vector3(0, 0, 0);
    public float segmentLengthR = 0.5f;
    public float killDistanceR = 0.5f;
    public int numAttracionPointsR = 400;

    List<Branch> branches = new List<Branch>();
    List<Vector3> attractionPointsBranhces = new List<Vector3>();
    List<Vector3> attractionPointsRoots = new List<Vector3>();

    void GenerateAttractorsRoots(int n, float r)
    {
        for (int i = 0; i < n; i++)
        {
            float radius = Random.Range(0f, 1f);
            radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI / 2f), 0.8f);
            radius *= r;
            // 2 angles are generated from which a direction will be computed
            float alpha = Random.Range(0f, Mathf.PI);
            float theta = Random.Range(0f, Mathf.PI * 2f);

            Vector3 pt = new Vector3(
                radius * Mathf.Cos(theta) * Mathf.Sin(alpha),
                radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
                radius * Mathf.Cos(alpha)
            );

            // translation to match the parent position
            pt += startingPointR;

            attractionPointsRoots.Add(pt);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateAttractorsRoots(numAttracionPointsR, 5f);
        Branch root = new Branch(startingPoint, startingPoint + new Vector3(0, segmentLength, 0), new Vector3(0, segmentLength, 0), null);
        branches.Add(root);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        foreach(Branch b in branches)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(b.start, b.end);
        }
        foreach (Vector3 pt in attractionPointsRoots)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pt, 0.05f);
        }
    }
}
