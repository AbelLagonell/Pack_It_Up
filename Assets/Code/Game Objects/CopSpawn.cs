using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;

public class CopSpawn : NetworkComponent
{
    private bool HasSpawned = false;
    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {
        
        HasSpawned = false;
    }

    public override IEnumerator SlowUpdate()
    {
        while(true)
        {
            //if(GameManager._timeOut && !HasSpawned)
            if(GameManager._timeOut && !HasSpawned)
            {
                Debug.Log("COP SPAWN");
                MyCore.NetCreateObject(20, NetId, transform.position, transform.rotation);
                HasSpawned = true;
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }
}
