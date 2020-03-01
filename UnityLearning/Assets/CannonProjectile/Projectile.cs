using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody bulletPrefabs;
    public GameObject cursor;
    public Transform Shotpoint;
    public LayerMask layer;
    public float Time = 1.5f;
    public LineRenderer lineVisual;
    public int lineSegment;

    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        lineVisual.positionCount = lineSegment;
    }

    // Update is called once per frame
    void Update()
    {
        LaunchProjectile();

        
    }

    void LaunchProjectile()
    {
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(camRay,out hit,100f, layer))
        {
            cursor.SetActive(true);
            cursor.transform.position = hit.point + Vector3.up * 0.1f;

            Vector3 V0 = CalculateVelocity(hit.point, Shotpoint.position, Time);

            Visualize(V0); //拋物線視覺化

            transform.rotation = Quaternion.LookRotation(V0);  //炮口調整至發射角

            if (Input.GetMouseButtonDown(0))
            {

                Rigidbody obj = Instantiate(bulletPrefabs, Shotpoint.position, Quaternion.identity);
                obj.velocity = V0; //給予發射角度的力
            }
        }
        else
        {
            cursor.SetActive(false);
        }


    }

    void Visualize(Vector3 v0)
    {
        for (int i=0; i <lineSegment; i++)
        {
            Vector3 pos = CalculatePosInTime(v0, i / (float)lineSegment);  // 「i/(float)lineSegment」秒的時候點在哪裡
            lineVisual.SetPosition(i, pos);
        }

    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0f;

        float Sy = distance.y;  //參考公式圖
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time; //只取重力的Y軸

        Vector3 result = distanceXZ.normalized;     //組合Velocity
        result *= Vxz;
        result.y = Vy;

        return result;

    }

    Vector3 CalculatePosInTime(Vector3 v0, float time) 
    {
        Vector3 Vxz = v0; //參考公式圖2
        Vxz.y = 0f;

        Vector3 result = Shotpoint.position + v0 * time;
        float sY = (-0.5f * Mathf.Abs(Physics.gravity.y) * (time * time)) + (v0.y * time) + Shotpoint.position.y;
        result.y = sY;

        return result;

    }

}
