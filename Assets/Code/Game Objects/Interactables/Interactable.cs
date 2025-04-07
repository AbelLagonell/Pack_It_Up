using NETWORK_ENGINE;

public struct InteractableFlags {
    public const string USABLE = "USABLE";
}

public abstract class Interactable : NetworkComponent {
    public bool usable = true;
    protected abstract void OnUse();
}