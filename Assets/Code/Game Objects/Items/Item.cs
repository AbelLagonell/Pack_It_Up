using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;

struct ItemFlags {
    public const string DESTROY = "DESTROY";
}

public class Item : NetworkComponent {
    public float money;
    public float weight;
    
    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case ItemFlags.DESTROY:
                if (IsServer)
                    MyCore.NetDestroyObject(MyId.NetId);
                break;
        }
    }
    public override void NetworkedStart() { }
}