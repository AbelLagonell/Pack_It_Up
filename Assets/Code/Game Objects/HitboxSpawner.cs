using UnityEngine;

public class HitboxSpawner : MonoBehaviour {
    public GameObject attackObject;
    public float attackDuration = 2f;
    public Vector3 attackSize;
    public Vector3 attackOffset;
    public Vector3 linearVelocity;
    public int damage = 1;
    private Quaternion _rotation;

    public void SpawnAttack() {
        Vector3 offset = transform.forward * attackOffset.z + transform.right * attackOffset.x +
                         transform.up * attackOffset.y;
        GameObject hitbox = Instantiate(attackObject, transform.position + offset, transform.rotation);
        hitbox.transform.localScale = attackSize;
        var script = hitbox.GetComponent<Hitbox>();
        script.damage = damage;
        var rb = hitbox.GetComponent<Rigidbody>();
        rb.linearVelocity = linearVelocity;
        Destroy(hitbox, attackDuration);
    }
}