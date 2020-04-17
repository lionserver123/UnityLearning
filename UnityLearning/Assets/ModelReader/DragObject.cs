using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{
    public float rotSpeed = 720f;
    public bool Listen;
    private Vector3 moffset;
    private float mZCoord;
    private bool FirstBool;
    public GameObject Model;
    public GameObject MyCamera;
    public KeyCode keyRotX;
    public KeyCode keyRotY;
    public KeyCode ketRotObject;
    public KeyCode keyTransform;


    void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(Model.transform.position).z;
        moffset = Model.transform.position - GetMouseWorldPos();

    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);


    }
    void OnMouseDrag()
    {

        if (Input.GetKey(keyTransform))
        {
            if (FirstBool == false)
            {
                mZCoord = Camera.main.WorldToScreenPoint(Model.transform.position).z;
                moffset = Model.transform.position - GetMouseWorldPos();
                FirstBool = true;
            }
            Model.transform.position = GetMouseWorldPos() + moffset;

        }
        else
        {
            FirstBool = false;
            float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * rotSpeed * Mathf.Deg2Rad;
            if (Input.GetKey(ketRotObject))
            {
                if (Input.GetKey(keyRotX))
                {
                    Model.transform.Rotate(Vector3.up, -rotX);
                }
                else if (Input.GetKey(keyRotY))
                {
                    Model.transform.Rotate(Vector3.right, -rotY);
                }
                else
                {
                    Model.transform.Rotate(Vector3.up, -rotX);
                    Model.transform.Rotate(Vector3.right, -rotY);
                }


            }
            else
            {
                if (Input.GetKey(keyRotX))
                {
                    MyCamera.transform.Rotate(Vector3.up, rotX);
                }
                else if (Input.GetKey(keyRotY))
                {
                    MyCamera.transform.Rotate(Vector3.right, -rotY);
                }
                else
                {
                    MyCamera.transform.Rotate(Vector3.up, rotX);
                    MyCamera.transform.Rotate(Vector3.right, -rotY);
                }

            }
           
        }
        
    }

    
}
