using NETWORK_ENGINE;
using UnityEngine;

public struct ActorFlags {
    public const string HEALTH = "HEALTH";
}

public abstract class Actor : NetworkComponent {
    public bool IsDetained = false;
    [SerializeField] protected Animator MyAnimator;
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected int Health = 10;
    [SerializeField] protected int MaxHealth = 10;

    public void UpdateHealth(int difference) {
        Health += difference;
        SendUpdate(ActorFlags.HEALTH, Health.ToString());
        if (Health > 0) return;
        OnDeath();
        MyCore.NetDestroyObject(MyId.NetId);
    }
    protected abstract void OnDeath();
}