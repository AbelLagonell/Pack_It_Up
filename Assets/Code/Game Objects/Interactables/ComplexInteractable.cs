using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ComplexInteractable : Interactable {
    public UnityEvent OnUseEvent;
    
    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }
    public override void HandleMessage(string flag, string value) {
    }
    public override void NetworkedStart() {
    }
    
    public override void OnUse() {
        usable = false;
        OnUseEvent?.Invoke();
    }
}