using NETWORK_ENGINE;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public abstract class Interactable : NetworkComponent {
    public bool usable = true;
    
    /// <summary>
    /// This Function will only ever be called on the server
    /// </summary>
    public abstract void OnUse();
}