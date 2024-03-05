using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public TestHandler handler;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            gameObject.transform.position = new Vector3(-handler.startWidth / 2 - 10, 25, handler.startHeight / 2);
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        } else if(Input.GetKeyDown(KeyCode.D))
        {
            gameObject.transform.position = new Vector3(-9, 15, -9);
            gameObject.transform.rotation = Quaternion.Euler(35, 45, 0);
        }
    }
}
