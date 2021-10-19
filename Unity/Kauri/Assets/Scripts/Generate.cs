using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//based off of https://ciphrd.com/2019/09/11/generating-a-3d-growing-tree-using-a-space-colonization-algorithm/
public class Generate : MonoBehaviour
{
    public enum limbType { trunk, branch, root };

    class Limb
    {

        public Vector3 start;
        public Vector3 end;
        public Vector3 direction;
        public Limb parent;
        public List<Limb> children = new List<Limb>();
        public List<Vector3> attractors = new List<Vector3>();
        public int distanceFromRoot;
        public limbType type;

        public Limb(Vector3 start, Vector3 end, Vector3 direction, Limb parent, limbType type)
        {
            this.start = start;
            this.end = end;
            this.direction = direction;
            this.parent = parent;
            this.type = type;
            if(parent == null)
            {
                distanceFromRoot = 0;
            }
            else //increase the distance from root if not first limb
            {
                distanceFromRoot = parent.distanceFromRoot + 1;
            }
        }
    }

    public enum TreeStage { Young, Ricker, Mature };

    [Header("Environmental parameters")]
    [Range(0.0f, 100.0f)]
    public float diebackSlider;

    [Header("Generation parameters")]
    public float attractionRange = 1f;
    public float _randomGrowth = 0.1f;
    public TreeStage stage = TreeStage.Mature;
    public GameObject leafObject;

    [Header("Branch parameters")]
    public float radiusB = 5f;
    public float segmentLengthB=0.2f;
    public float killDistanceB=0.5f;
    public int numAttracionPointsB = 400;

    [Header("Root parameters")]
    public bool generateRoots=true;
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

    bool leavesGenerated = false; //keeps track of if leaves have been added to the tree
    List<GameObject> leaves = new List<GameObject>();
    Vector3 position;

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

    Quaternion randomRotation() //returns a random rotation in all 3 axes
    {
        float x = Random.Range(-Mathf.PI/2, Mathf.PI/2);
        float y = Random.Range(-Mathf.PI/2, Mathf.PI/2);
        float z = Random.Range(-Mathf.PI/2, Mathf.PI/2);
        return Quaternion.Euler(x, y, z);
    }

    void growTrunk()
    {
        while (branches[branches.Count - 1].end.y < position.y + radiusB)
        {
            Limb l = branches[branches.Count - 1];
            Limb current = new Limb(l.end, l.end + l.direction, l.direction, l, l.type);
            branches.Add(current);
            l.children.Add(current);
        }
    }

    void growLimbs(List<Limb> limbs, List<Limb> extremities, List<Vector3> attractors, float killDistance, float segmentLength, bool roots)
    {
        limbType type;
        if (roots)
        {
            type = limbType.root;
        }
        else
        {
            type = limbType.branch;
        }
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
                    if (distance <= killDistance)
                    {
                        pointsToRemove.Add(point);
                        break;
                    }
                    else if ((roots && l.end.y >= point.y) || (!roots && l.end.y <= point.y)) //roots can't grow up, branches can't grow down
                    {
                        if (distance < closestDistance) //keep track of the closest limb
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
                if (closest != null)
                {
                    closest.attractors.Add(point); //set attractor of closest limb equal to this point
                }
            }
            foreach(Vector3 point in pointsToRemove) //remove points from within kill distance
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
                        growthDirection += RandomGrowthVector();
                        growthDirection.Normalize();
                        Limb newLimb = new Limb(l.end, l.end + growthDirection * segmentLength, growthDirection, l, type);
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
            else //grow extremities in previous direction
            {
                for (int i = 0; i < extremities.Count; i++)
                {
                    Limb l = extremities[i];
                    Limb current = new Limb(l.end, l.end + l.direction, l.direction, l, l.type);
                    limbs.Add(current);
                    extremities[i] = current;
                    l.children.Add(current);
                }
            } 
        }
        else if(!roots && !leavesGenerated) //branches have grown, add leaves if none have been added yet (only apply to branches)
        {
            leavesGenerated = true;
            foreach(Limb l in extremities) //start by adding leaves at the end of each branch
            {
                leaves.Add(Instantiate(leafObject, l.end, Quaternion.LookRotation(l.direction)));
            }
        }
    }

    void initiliazeMatureKauri()
    {
        if (generateRoots)
        {
            attractionPointsRoots = attrDist.GenerateAttractorsCube(numAttracionPointsR, radiusR, position);
            Limb baseRoot = new Limb(position, position + new Vector3(0, -segmentLengthB, 0), new Vector3(0, -segmentLengthB, 0), null, limbType.root);
            roots.Add(baseRoot);
            rootExtremities.Add(baseRoot);
        }
        attractionPointsBranches = attrDist.GenerateAttractorsMatureBranches(numAttracionPointsB, radiusB, position);
        Limb baseBranch = new Limb(position, position + new Vector3(0, segmentLengthB, 0), new Vector3(0, segmentLengthB, 0), null, limbType.trunk);
        branches.Add(baseBranch);
    }

    void initializeYoungKauri()
    {
        if (generateRoots)
        {
            attractionPointsRoots = attrDist.GenerateAttractorsCube(numAttracionPointsR, radiusR, position);
            Limb baseRoot = new Limb(position, position + new Vector3(0, -segmentLengthB, 0), new Vector3(0, -segmentLengthB, 0), null, limbType.root);
            roots.Add(baseRoot);
            rootExtremities.Add(baseRoot);
        }
        attractionPointsBranches = attrDist.GenerateAttractorsCone(numAttracionPointsB, radiusB, position);
        Limb baseBranch = new Limb(position, position + new Vector3(0, segmentLengthB, 0), new Vector3(0, segmentLengthB, 0), null, limbType.trunk);
        branches.Add(baseBranch);
    }

    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
        switch (stage)
        {
            case TreeStage.Mature:
                initiliazeMatureKauri();
                break;
            case TreeStage.Young:
                initializeYoungKauri();
                break;
        }
        growTrunk();
    }

    // Update is called once per frame
    void Update()
    {
        if (generateRoots)
        {
            growLimbs(roots, rootExtremities, attractionPointsRoots, killDistanceR, segmentLengthR, true);
        }
        growLimbs(branches, branchExtremities, attractionPointsBranches, killDistanceB, segmentLengthB, false);
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
