using System;
using UnityEngine;
using UnityEngine.UI;

public class RadioGroup : MonoBehaviour {
    public int GetSelected() {
        VotingObject[] objects = GetComponentsInChildren<VotingObject>();
        foreach (var votingObject in objects) {
            if (votingObject.isOn) {
                return votingObject.characterIndex;
            }
        }

        return -1;
    }

    public void SetInactive(bool value) {
        var children = GetComponentsInChildren<Toggle>();
        foreach (var toggle in children) {
            toggle.interactable = !value;
        }
    }
}