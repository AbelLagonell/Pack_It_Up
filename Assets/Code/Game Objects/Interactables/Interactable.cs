using NETWORK_ENGINE;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public abstract class Interactable : NetworkComponent {
    public bool usable = true;
    public abstract void OnUse();
}