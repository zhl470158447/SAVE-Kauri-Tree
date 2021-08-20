using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//based off of https://ciphrd.com/2019/09/11/generating-a-3d-growing-tree-using-a-space-colonization-algorithm/
public class Generate : MonoBehaviour
{

    class Limb
    {
        public Vector3 start;
        public Vector3 end;
        Vector3 direction;
        Limb parent;
        List<Limb> children;
        int distanceFromRoot;

        public Limb(Vector3 start, Vector3 end, Vector3 direction, Limb parent)
        {
            this.start = start;
            this.end = end;
            this.direction = direction;
            this.parent = parent;
        }
    }

    [Header("Branch parameters")]
    public Vector3 startingNode = new Vector3(0, 0, 0);
    public float generationHeight = 5;
    public float segmentLength=0.5f;
    public float killDistance=0.5f;
    public int numAttracionPoints = 400;

    [Header("Root parameters")]
    public bool generateRoots=true;
    public Vector3 startingNodeR = new Vector3(0, 0, 0);
    public float generationDepth = 5;
    public float segmentLengthR = 0.5f;
    public float killDistanceR = 0.5f;
    public int numAttracionPointsR = 400;

    List<Limb> branches = new List<Limb>();
    List<Limb> roots = new List<Limb>();
    List<Vector3> attractionPointsBranches = new List<Vector3>();
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
            pt += startingNodeR - new Vector3(0, generationDepth, 0);

            attractionPointsRoots.Add(pt);
        }
    }
    void GenerateAttractors(int n, float r)
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
            pt += startingNode + new Vector3(0, generationHeight, 0);

            attractionPointsBranches.Add(pt);
        }
    }

    void growRoots()
    {

    }

    void growBranches()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        if (generateRoots)
        {
            GenerateAttractorsRoots(numAttracionPointsR, 5f);
            Limb baseRoot = new Limb(startingNodeR, startingNodeR + new Vector3(0, -segmentLength, 0), new Vector3(0, -segmentLength, 0), null);
            roots.Add(baseRoot);
            growRoots();
        }
        GenerateAttractors(numAttracionPointsR, 5f);
        Limb baseBranch = new Limb(startingNode, startingNode + new Vector3(0, segmentLength, 0), new Vector3(0, segmentLength, 0), null);
        branches.Add(baseBranch);
        growRoots();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        foreach(Limb b in branches)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(b.start, b.end);
        }
        foreach (Limb r in roots)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(r.start, r.end);
        }
        foreach (Vector3 pt in attractionPointsRoots)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(pt, 0.05f);
        }
        foreach (Vector3 pt in attractionPointsBranches)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pt, 0.05f);
        }
    }
}
