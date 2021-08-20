using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public Vector3 startingPoint;

    class Branch
    {
        Vector3 start;
        Vector3 end;
        Branch parent;
        List<Branch> children;
        int distanceFromRoot;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
