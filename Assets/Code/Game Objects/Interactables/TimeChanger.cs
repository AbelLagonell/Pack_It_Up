using System.Collections;
using UnityEditor;
using UnityEngine;

public class TimeChanger : Interactable {
    public float timerChange = 0f;

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }

    public override void HandleMessage(string flag, string value) { }

    public override void NetworkedStart() { }

    public override void OnUse() {
        if (!usable) return;
        usable = false;
        FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0].ChangeTime(timerChange);
    }
}