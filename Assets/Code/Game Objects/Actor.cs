using NETWORK_ENGINE;
using UnityEngine;

public abstract class Actor : NetworkComponent {
    protected bool IsDetained = false;
    [SerializeField] protected float speed = 10f;
    protected int Health = 10;

    public void UpdateHealth(int difference) {
        Health += difference;
        if (Health > 0) return;
        OnDeath();
        MyCore.NetDestroyObject(MyId.NetId);
    }
    protected abstract void OnDeath();
}