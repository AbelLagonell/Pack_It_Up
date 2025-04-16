using System.Collections;
using UnityEngine;
using UnityEngine.Events;

struct ComplexInteractableFlag {
    public const string UPDATE = "UPDATE";
}

public class ComplexInteractable : Interactable {
    public UnityEvent OnUseEvent;
    
    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }
    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case ComplexInteractableFlag.UPDATE:
                OnUseEvent?.Invoke();
                break;
        }
    }
    public override void NetworkedStart() {
    }
    
    public override void OnUse() {
        Debug.Log("Complex use");
        usable = false;
        OnUseEvent?.Invoke();
        SendUpdate(ComplexInteractableFlag.UPDATE, "");
    }

    public void SetUsable() {
        usable = true;
    }
}