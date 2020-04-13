using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{
    public float rotSpeed = 720f;
    public bool Listen;
    private Vector3 moffset;
    private float mZCoord;
    public GameObject Model;
    
    void OnMouseDown()
    {
        
        if (Listen == true) {
            mZCoord = Camera.main.WorldToScreenPoint(Model.transform.position).z;
            moffset = Model.transform.position - GetMouseWorldPos();
        }
        else
        {

        }
        
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);


    }
    void OnMouseDrag()
    {

        if (Listen == true)
        {
            Model.transform.position = GetMouseWorldPos() + moffset;
        }
        else
        {
            float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;

            Model.transform.Rotate(Vector3.up, -rotX);
            Model.transform.Rotate(Vector3.right, -rotY);
        }
        
    }

    
}
