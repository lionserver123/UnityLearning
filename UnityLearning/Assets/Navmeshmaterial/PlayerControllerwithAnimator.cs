using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerControllerwithAnimator : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public ThirdPersonCharacter character;

    void Start()
    {
        agent.updateRotation = false; //把navmesh的rotation關起來
            
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log(Input.mousePosition);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);   //創造一個射線從視窗發射到mousePosition
            //Debug.Log(ray);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) //如果有射到東西(含有Collider)
            {
                //Debug.Log(hit.point);
                agent.SetDestination(hit.point);
            }
        }
        if(agent.remainingDistance > agent.stoppingDistance) //剩下的距離是否大於停止的距離
        {
           // Debug.Log(agent.remainingDistance); //在最後點對點前 都是infinity
          //Debug.Log(agent.desiredVelocity); //向量方向
            character.Move(agent.desiredVelocity, false, false);
        }
        else
        {
            character.Move(new Vector3(0, 0, 0), false, false);
        }
   

    }
}
