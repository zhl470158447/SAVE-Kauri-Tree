using UnityEngine;
using System.Collections;public class mesh_test : MonoBehaviour {
    public MeshFilter tree_mesh;
    // Use this for initialization
    void Start () {
        Mesh tree = this.tree_mesh.mesh;//传递进来的模型的MeshFilter组件的Mesh赋值给Mesh类型的变量
        
        Mesh self_mesh = this.GetComponent<MeshFilter>().mesh;
        self_mesh.Clear();
        self_mesh.vertices = tree.vertices;
        self_mesh.triangles = tree.triangles;
        self_mesh.normals = tree.normals;
        self_mesh.uv = tree.uv;
        self_mesh.tangents = tree.tangents;

        self_mesh.RecalculateBounds();
       }

    // Update is called once per frame
    void Update () {
    
    }
}