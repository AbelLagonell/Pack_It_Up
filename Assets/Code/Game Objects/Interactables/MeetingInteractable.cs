using System.Collections;
using UnityEngine;

public class MeetingInteractabl : Interactable {
    public override IEnumerator SlowUpdate() {
        while (true) {
            if (IsServer)
                if (IsDirty) {
                    SendUpdate(InteractableFlags.USABLE, usable.ToString());   
                }
                yield return new WaitForSeconds(1f);
        }
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case InteractableFlags.USABLE:
                usable = bool.Parse(value);
                break;
        }
    }

    public override void NetworkedStart() { }

    protected override void OnUse() {
        if (usable) return;
        usable = false;
        SendUpdate(InteractableFlags.USABLE, usable.ToString());
        Debug.Log("MeetingInteraction");
    }
}