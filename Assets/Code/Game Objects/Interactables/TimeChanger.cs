using System.Collections;
using UnityEngine;

public class TimeChanger : Interactable {
    public float timerChange = 0f;

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }
    
    public override void HandleMessage(string flag, string value) {
    }
    public override void NetworkedStart() {
    }
    protected override void OnUse() {
    }
}