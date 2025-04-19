using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;

public class Bullet : NetworkComponent
{
    [SerializeField] Rigidbody myRig;
    private bool KillBullet;
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        if(KillBullet)
        {
            MyCore.NetDestroyObject(this.NetId);
        }
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        KillBullet = false;
        myRig.linearVelocity = new Vector3(transform.forward.x*5,0,0);
        StartCoroutine(KillTime());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator KillTime()
    {
        yield return new WaitForSeconds(3f);
        KillBullet = true;
    }
}
