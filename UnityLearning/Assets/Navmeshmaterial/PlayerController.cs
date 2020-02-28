using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log(Input.mousePosition);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);   //創造一個射線從視窗發射到mousePosition
            //Debug.Log(ray);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit)) //如果有射到東西(含有Collider)
            {
                //Debug.Log(hit.point);
                agent.SetDestination(hit.point);
            }
        }
    }
}
