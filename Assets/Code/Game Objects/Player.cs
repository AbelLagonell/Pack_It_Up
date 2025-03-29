using System.Collections;
using UnityEngine;

public class Player : Actor {
    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public override void HandleMessage(string flag, string value) {
        //TODO make necessary flags
    }

    public override void NetworkedStart() {
        //TODO Make what happens when they connect to the server
    }

    protected override void OnDeath() {
        //TODO Make do a death thing
    }
}