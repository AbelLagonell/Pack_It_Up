using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;

public class Item : NetworkComponent {
    public float money;
    public float weight;
    public bool destroy = false;
    
    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }

    public override void HandleMessage(string flag, string value) { }
    
    public override void NetworkedStart() { }

    public void SetDestroy() {
        if (IsServer) {
            if (destroy)
                MyCore.NetDestroyObject(MyId.NetId);
        }
    }
}