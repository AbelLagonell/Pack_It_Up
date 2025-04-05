using Unity.Mathematics;
using UnityEngine;

public class Name : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = new Quaternion(0 - this.transform.parent.transform.rotation.x,1- this.transform.parent.transform.rotation.y,0- this.transform.parent.transform.rotation.z,0 - this.transform.parent.transform.rotation.w);
    }
}
