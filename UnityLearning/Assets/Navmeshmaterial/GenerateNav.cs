using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenerateNav : MonoBehaviour
{

    public GameObject wall;
    public GameObject Mother;
    public NavMeshSurface Surface;
    void Start()
    {//產生牆壁 地板時 需將模型的Read/Write開始
    //先將地板給Bake 再產生牆壁
        Instantiate(wall,Mother.transform);
        Surface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
