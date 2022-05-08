using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public float speed = 5f;
    public float scrollStrength = 5f;

    private const int EDGEPAN = 10;
    private Vector3 center;

    public static CameraHandler global;

    // Start is called before the first frame update
    void Start()
    {
        global = this;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x < EDGEPAN)
        {
            velocity += Vector3.left;
        }
        else if (mousePos.x > Screen.width - EDGEPAN)
        {
            velocity += Vector3.right;
        }

        if (mousePos.y < EDGEPAN)
        {
            velocity += Vector3.down;
        }
        else if (mousePos.y > Screen.height - EDGEPAN)
        {
            velocity += Vector3.up;
        }

        //Mouse scroll functions like a "Zoom"
        if (Input.mouseScrollDelta.y > 0)
        {
            velocity += Vector3.forward * scrollStrength;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            velocity += Vector3.back * scrollStrength;
        }
        transform.position += velocity * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Space))
        {
            transform.position = center;
        }
    }

    public static void SetCameraCenter(Vector3 position)
    {
        global.center = position;
    }

    public static void CenterCamera()
    {
        global.transform.position = global.center;
    }
}
