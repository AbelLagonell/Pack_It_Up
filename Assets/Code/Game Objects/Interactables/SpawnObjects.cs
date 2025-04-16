using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;

public class SpawnObjects : NetworkComponent {
    public int typeIndex = 0;
    public Vector3 offset;
    public int count = 0;
    
    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }
    public override void HandleMessage(string flag, string value) { }
    public override void NetworkedStart() { }

    public void SpawnObject() {
        Vector3 totalOffset = transform.forward * offset.z + transform.right * offset.x +
                                 transform.up * offset.y;
        for (int i = 0; i < count; i++) {
            MyCore.NetCreateObject(typeIndex, 0, totalOffset, default);
        }
    }
}