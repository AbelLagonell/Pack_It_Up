using System;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Hitbox : MonoBehaviour {
    public int damage = 1;
    public string enemyTag;
    private Collider _myCollider;

    private void Awake() {
        _myCollider = GetComponent<Collider>();
        _myCollider.isTrigger = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;  
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(enemyTag)) {
            Actor actor = other.gameObject.GetComponent<Actor>();
            actor.UpdateHealth(-1 * damage);
        }
    }

    private void OnDrawGizmos() {
        // Get the collider if we don't have it yet
        if (_myCollider == null) {
            _myCollider = GetComponent<Collider>();
        }

        // If we have a collider, draw based on its actual shape
        if (_myCollider != null) {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

            // Different handling based on collider type
            if (_myCollider is BoxCollider boxCollider) {
                // Draw the box collider at its exact position and size
                Matrix4x4 originalMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(
                                              transform.TransformPoint(boxCollider.center),
                                              transform.rotation,
                                              Vector3.Scale(transform.localScale, boxCollider.size)
                                             );
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = originalMatrix;
            } else if (_myCollider is SphereCollider sphereCollider) {
                // For sphere colliders
                Vector3 center = transform.TransformPoint(sphereCollider.center);
                float radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x,
                                                                 Mathf.Max(transform.lossyScale.y,
                                                                           transform.lossyScale.z));
                Gizmos.DrawSphere(center, radius);
            } else {
                // Generic fallback for other collider types
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}