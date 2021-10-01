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
        public Vector3 direction;
        public Limb parent;
        public List<Limb> children = new List<Limb>();
        public List<Vector3> attractors = new List<Vector3>();
        public int distanceFromRoot;

        public Limb(Vector3 start, Vector3 end, Vector3 direction, Limb parent)
        {
            this.start = start;
            this.end = end;
            this.direction = direction;
            this.parent = parent;
        }
    }

    public enum TreeStage { Young, Ricker, Mature };

    [Header("Generation paramters")]
    public float attractionRange = 1f;
    public float _timeBetweenIterations = .5f;
    public float _randomGrowth = 0.1f;
    public TreeStage stage = TreeStage.Mature;

    [Header("Branch parameters")]
    public Vector3 startingNodeB = new Vector3(0, 0, 0);
    public float radiusB = 5f;
    public float segmentLengthB=0.2f;
    public float killDistanceB=0.5f;
    public int numAttracionPointsB = 400;

    [Header("Root parameters")]
    public bool generateRoots=true;
    public Vector3 startingNodeR = new Vector3(0, 0, 0);
    public float radiusR = 5f;
    public float segmentLengthR = 0.2f;
    public float killDistanceR = 0.5f;
    public int numAttracionPointsR = 400;

    List<Limb> branches = new List<Limb>();
    List<Limb> branchExtremities = new List<Limb>();
    List<Limb> roots = new List<Limb>();
    List<Limb> rootExtremities = new List<Limb>();
    List<Vector3> attractionPointsBranches = new List<Vector3>();
    List<Vector3> attractionPointsRoots = new List<Vector3>();
    float _timeSinceLastIteration = 0f;
    AttractionPointDistribution attrDist = new AttractionPointDistribution();

    //From https://github.com/bcrespy/unity-growing-tree/blob/master/Assets/Scripts/Generator.cs
    Vector3 RandomGrowthVector()
    {
        float alpha = Random.Range(0f, Mathf.PI);
        float theta = Random.Range(0f, Mathf.PI * 2f);

        Vector3 pt = new Vector3(
            Mathf.Cos(theta) * Mathf.Sin(alpha),
            Mathf.Sin(theta) * Mathf.Sin(alpha),
            Mathf.Cos(alpha)
        );

        return pt * _randomGrowth;
    }

    void growLimbs(List<Limb> limbs, List<Limb> extremities, List<Vector3> attractors, float killDistance, float segmentLength)
    {
        if (attractors.Count>0)
        {
            bool attractionActive = false;
            
            List<Vector3> pointsToRemove = new List<Vector3>();
            foreach (Vector3 point in attractors)
            {
                Limb closest=null;
                float closestDistance = int.MaxValue;
                foreach (Limb l in limbs)
                {
                    float distance = Vector3.Distance(l.end, point);
                    if (distance <= attractionRange)
                    {
                        if (distance <= killDistance)
                        {
                            pointsToRemove.Add(point);
                            break;
                        }
                        else
                        {
                            if (distance < closestDistance)
                            {
                                closest = l;
                                closestDistance = distance;
                            }
                            if (!attractionActive)
                            {
                                attractionActive = true;
                            }
                        }
                    }
                }
                if (closest != null)
                {
                    closest.attractors.Add(point);
                }
            }
            foreach(Vector3 point in pointsToRemove)
            {
                attractors.Remove(point);
            }
            if (attractionActive)
            {
                extremities.Clear();
                List<Limb> newLimbs = new List<Limb>();
                foreach (Limb l in limbs)
                {
                    if (l.attractors.Count > 0)
                    {
                        Vector3 growthDirection = new Vector3(0, 0, 0);
                        foreach (Vector3 attr in l.attractors)
                        {
                            growthDirection += (attr - l.end).normalized;
                        }
                        growthDirection /= l.attractors.Count;
                        //growthDirection += RandomGrowthVector();
                        growthDirection.Normalize();
                        Limb newLimb = new Limb(l.end, l.end + growthDirection * segmentLength, growthDirection, l);
                        l.children.Add(newLimb);
                        newLimbs.Add(newLimb);
                        extremities.Add(newLimb);
                        l.attractors.Clear();
                    }
                    else
                    {
                        if (l.children.Count == 0)
                        {
                            extremities.Add(l);
                        }
                    }
                }
                limbs.AddRange(newLimbs);
            }
            else
            {
                for (int i = 0; i < extremities.Count; i++)
                {
                    Limb l = extremities[i];
                    Limb current = new Limb(l.end, l.end + l.direction, l.direction, l);
                    limbs.Add(current);
                    extremities[i] = current;
                    l.children.Add(current);
                }
            } 
        }
    }

    void initiliazeMatureKauri()
    {
        if (generateRoots)
        {
            attractionPointsRoots = attrDist.GenerateAttractorsCube(numAttracionPointsR, radiusR, startingNodeR);
            Limb baseRoot = new Limb(startingNodeR, startingNodeR + new Vector3(0, -segmentLengthB, 0), new Vector3(0, -segmentLengthB, 0), null);
            roots.Add(baseRoot);
            rootExtremities.Add(baseRoot);
        }
        attractionPointsBranches = attrDist.GenerateAttractorsMatureBranches(numAttracionPointsB, radiusB, startingNodeB);
        Limb baseBranch = new Limb(startingNodeB, startingNodeB + new Vector3(0, segmentLengthB, 0), new Vector3(0, segmentLengthB, 0), null);
        branches.Add(baseBranch);
        branchExtremities.Add(baseBranch);
    }

    // Start is called before the first frame update
    void Start()
    {
        switch (stage)
        {
            case TreeStage.Mature:
                initiliazeMatureKauri();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _timeSinceLastIteration += Time.deltaTime;

        // we check if we need to run a new iteration 
        if (_timeSinceLastIteration > _timeBetweenIterations)
        {
            _timeSinceLastIteration = 0f;
            if (generateRoots)
            {
                growLimbs(roots, rootExtremities, attractionPointsRoots, killDistanceR, segmentLengthR); 
            }
            growLimbs(branches, branchExtremities, attractionPointsBranches, killDistanceB, segmentLengthB);
        }
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
            Gizmos.color = Color.gray;
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
       /* foreach(Limb l in branchExtremities)
        {
            Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
            Gizmos.DrawSphere(l.end, attractionRange);
        }
        foreach(Limb l in rootExtremities)
        {
            Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
            Gizmos.DrawSphere(l.end, attractionRange);
        }*/
    }
}
