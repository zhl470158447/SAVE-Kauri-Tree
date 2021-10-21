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
        public float size;
        public float _lastSize;
        public bool grown = false;
        public int _verticesId;

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
    public float trunkHeight = 5;
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

    [Header("Leaf materials")]
    public Material healthy;
    public Material unhealthy1;
    public Material unhealthy2;
    public Material unhealthy3;

    [Header("Mesh generation")]
    [Range(0, 20)]
    public int radialSubdivisions = 10;
    [Range(0f, 1f), Tooltip("The size at the extremity of the branches")]
    public float _extremitiesSize = 0.05f;
    [Range(0f, 5f), Tooltip("Growth power, of the branches size")]
    public float invertGrowth = 2f;

    MeshFilter _filter;

    /**
	 * Generates n attractors and stores them in the attractors array
	 * The points are generated within a sphere of radius r using a random distribution
	 **/
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
            pt += transform.position;

            attractionPointsBranches.Add(pt);
        }
    }
    public List<int> _activeAttractors = new List<int>();
    List<Limb> branches = new List<Limb>();
    List<Limb> branchExtremities = new List<Limb>();
    List<Limb> roots = new List<Limb>();
    List<Limb> rootExtremities = new List<Limb>();
    List<Vector3> attractionPointsBranches = new List<Vector3>();
    List<Vector3> attractionPointsRoots = new List<Vector3>();
    AttractionPointDistribution attrDist = new AttractionPointDistribution();

    bool leavesGenerated = false; //keeps track of if leaves have been added to the tree
    List<GameObject> leaves = new List<GameObject>();
    List<GameObject> removedLeaves = new List<GameObject>();
    int leavesCount;
    Vector3 position;

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

    void growTrunk(float height)
    {
        while (branches[branches.Count - 1].end.y < height) //trunk too short, keep growing
        {
            Limb l = branches[branches.Count - 1]; //get latest part of trunk
            Limb current = new Limb(l.end, l.end + l.direction, l.direction, l, limbType.trunk); //create new limb of trunk type
            branches.Add(current); //add to branches list so branches can grow off of trunk
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
                        //limb starts from parent's end, grows in the computed direction a distance of segment length, has l as a parent
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
                    //limb starts from parent's end, grows in the same direction
                    Limb current = new Limb(l.end, l.end + l.direction * segmentLength, l.direction, l, l.type);
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
            leavesCount = leaves.Count;
        }
    }

    void initiliazeMatureKauri()
    {
        _filter = GetComponent<MeshFilter>();
        float bareTrunk = trunkHeight * 0.7f; //portion of trunk to have no branches
        if (generateRoots)
        {
            attractionPointsRoots = attrDist.GenerateAttractorsSpherical(numAttracionPointsR, radiusR, position);
            Limb baseRoot = new Limb(position, position + new Vector3(0, -segmentLengthB, 0), new Vector3(0, -segmentLengthB, 0), null, limbType.root); //initial root, just straight down
            roots.Add(baseRoot);
            rootExtremities.Add(baseRoot);
        }
        attractionPointsBranches = attrDist.GenerateAttractorsMatureBranches(numAttracionPointsB, radiusB, position + new Vector3(0, bareTrunk, 0));
        Limb baseBranch = new Limb(position, position + new Vector3(0, segmentLengthB, 0), new Vector3(0, segmentLengthB, 0), null, limbType.trunk); //initial trunk piece, straight upwards
        branches.Add(baseBranch);
        growTrunk(position.y + trunkHeight); //grow trunk to the trunk height
       
    }

    void initializeYoungKauri()
    {
        if (generateRoots)
        {
            attractionPointsRoots = attrDist.GenerateAttractorsSpherical(numAttracionPointsR, radiusR, position);
            Limb baseRoot = new Limb(position, position + new Vector3(0, -segmentLengthB, 0), new Vector3(0, -segmentLengthB, 0), null, limbType.root); //initial root, just straight down
            roots.Add(baseRoot);
            rootExtremities.Add(baseRoot);
        }
        float bareTrunk = trunkHeight * .2f; //portion of trunk to have no branches
        attractionPointsBranches = attrDist.GenerateAttractorsCone(numAttracionPointsB, trunkHeight - bareTrunk, position+new Vector3(0,bareTrunk,0));
        Limb baseBranch = new Limb(position, position + new Vector3(0, segmentLengthB, 0), new Vector3(0, segmentLengthB, 0), null, limbType.trunk); //initial trunk piece, straight upwards
        branches.Add(baseBranch);
        growTrunk(position.y + trunkHeight);    //grow trunk to the trunk height
        _filter = GetComponent<MeshFilter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
        
        switch (stage)
        {
            case TreeStage.Mature:
                initiliazeMatureKauri();
                _filter = GetComponent<MeshFilter>();
                break;
            case TreeStage.Young:
                initializeYoungKauri();
                _filter = GetComponent<MeshFilter>();
                break;
            case TreeStage.Ricker:
                //TODO
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (generateRoots && attractionPointsRoots.Count>0) //roots need to be generated
        {
            growLimbs(roots, rootExtremities, attractionPointsRoots, killDistanceR, segmentLengthR, true);
        }
        if (!leavesGenerated) //leaves are only generated once all attraction points are gone
        {
            growLimbs(branches, branchExtremities, attractionPointsBranches, killDistanceB, segmentLengthB, false);
        }
        else if(diebackSlider>0 || removedLeaves.Count>0) //dieback active
        {
            int toRemove = (int)((diebackSlider / 100.0f) * leavesCount); //find what number of leaves should be removed for current dieback level
            int currentLeaves = leavesCount - toRemove;
            while(currentLeaves < leaves.Count) //too many, remove
            {
                int index = Random.Range(0, leaves.Count); //pick randomly from leaves
                GameObject current = leaves[index];
                leaves.RemoveAt(index);
                current.SetActive(false); //keep game objects, just disable
                removedLeaves.Add(current); //keep track of removed so they can be reenabled and added back
            }
            while(currentLeaves > leaves.Count) //not enough, add (basically the opposite of removal)
            {
                int index = Random.Range(0, removedLeaves.Count); //pick randomly from removed
                GameObject current = removedLeaves[index];
                removedLeaves.RemoveAt(index);
                current.SetActive(true); //re-enable game object
                leaves.Add(current);
            }
        }
        ToMesh();
    }
    void ToMesh()
    {
        Mesh treeMesh = new Mesh();

        // we first compute the size of each branch 
        for (int i = branches.Count - 1; i >= 0; i--)
        {
            float size = 0f;
            Limb b = branches[i];
            if (b.children.Count == 0)
            {
                size = _extremitiesSize;
            }
            else
            {
                foreach (Limb bc in b.children)
                {
                    size += Mathf.Pow(bc.size, invertGrowth);
                }
                size = Mathf.Pow(size, 1f / invertGrowth);
            }
            b.size = size;
        }

        Vector3[] vertices = new Vector3[(branches.Count + 1) * radialSubdivisions];
        int[] triangles = new int[branches.Count * radialSubdivisions * 6];

        // construction of the vertices 
        for (int i = 0; i < branches.Count; i++)
        {
            Limb b = branches[i];

            // the index position of the vertices
            int vid = radialSubdivisions * i;
            b._verticesId = vid;

            // quaternion to rotate the vertices along the branch direction
            Quaternion quat = Quaternion.FromToRotation(Vector3.up, b.direction);

            // construction of the vertices 
            for (int s = 0; s < radialSubdivisions; s++)
            {
                // radial angle of the vertex
                float alpha = ((float)s / radialSubdivisions) * Mathf.PI * 2f;

                // radius is hard-coded to 0.1f for now
                Vector3 pos = new Vector3(Mathf.Cos(alpha) * b.size, 0, Mathf.Sin(alpha) * b.size);
                pos = quat * pos; // rotation

                // if the branch is an extremity, we have it growing slowly
                if (b.children.Count == 0 && !b.grown)
                {
                    
                }
                else
                {
                    pos += b.end;
                }

                vertices[vid + s] = pos - transform.position; // from tree object coordinates to [0; 0; 0]

                // if this is the tree root, vertices of the base are added at the end of the array 
                if (b.parent == null)
                {
                    vertices[branches.Count * radialSubdivisions + s] = b.start + new Vector3(Mathf.Cos(alpha) * b.size, 0, Mathf.Sin(alpha) * b.size) - transform.position;
                }
            }
        }

        // faces construction; this is done in another loop because we need the parent vertices to be computed
        for (int i = 0; i < branches.Count; i++)
        {
            Limb b = branches[i];
            int fid = i * radialSubdivisions * 2 * 3;
            // index of the bottom vertices 
            int bId = b.parent != null ? b.parent._verticesId : branches.Count * radialSubdivisions;
            // index of the top vertices 
            int tId = b._verticesId;

            // construction of the faces triangles
            for (int s = 0; s < radialSubdivisions; s++)
            {
                // the triangles 
                triangles[fid + s * 6] = bId + s;
                triangles[fid + s * 6 + 1] = tId + s;
                if (s == radialSubdivisions - 1)
                {
                    triangles[fid + s * 6 + 2] = tId;
                }
                else
                {
                    triangles[fid + s * 6 + 2] = tId + s + 1;
                }

                if (s == radialSubdivisions - 1)
                {
                    // if last subdivision
                    triangles[fid + s * 6 + 3] = bId + s;
                    triangles[fid + s * 6 + 4] = tId;
                    triangles[fid + s * 6 + 5] = bId;
                }
                else
                {
                    triangles[fid + s * 6 + 3] = bId + s;
                    triangles[fid + s * 6 + 4] = tId + s + 1;
                    triangles[fid + s * 6 + 5] = bId + s + 1;
                }
            }
        }

        treeMesh.vertices = vertices;
        treeMesh.triangles = triangles;
        treeMesh.RecalculateNormals();
        _filter.mesh = treeMesh;
    }

    private void OnDrawGizmos() //for drawing in the scene view
    {
        foreach(Limb b in branches)
        {
            if(b.type == limbType.trunk)
            {
                Gizmos.color = Color.black;
            }
            else
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
