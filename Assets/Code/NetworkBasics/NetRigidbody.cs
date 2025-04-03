using System.Collections;
using System.Numerics;
using NETWORK_ENGINE;
using NUnit.Framework;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public struct RigidBodyFlags {
    public const string Velocity = "VEL";
    public const string AngularVelocity = "ANV";
}

[RequireComponent(typeof(Rigidbody))]
public class NetRigidbody : NetworkComponent {
    //Sync
    private Vector3 _lastVelocity;
    private Vector3 _lastAngularVelocity;
    private Vector3 _lastPosition;
    private Vector3 _lastRotation;

    //UnSync
    private Rigidbody _rb;
    private const float EThreshold = 0.25f;
    private const float Threshold = 0.1f;
    public Vector3 adjustVelocity;
    public bool useAdjustedVelocity;

    public override IEnumerator SlowUpdate() {
        if (IsClient) {
            _rb.useGravity = false;
        }

        while (true) {
            if (IsServer) {
                if ((_lastPosition - _rb.position).magnitude > Threshold) {
                    _lastPosition = _rb.position;
                    SendUpdate(TransformFlags.Position, _rb.position.ToString());
                }

                if ((_lastRotation - _rb.rotation.eulerAngles).magnitude > Threshold) {
                    _lastRotation = _rb.rotation.eulerAngles;
                    SendUpdate(TransformFlags.Rotation, _rb.rotation.eulerAngles.ToString());
                }

                if ((_lastVelocity - _rb.linearVelocity).magnitude > Threshold) {
                    _lastVelocity = _rb.linearVelocity;
                    SendUpdate(RigidBodyFlags.Velocity, _rb.linearVelocity.ToString());
                }

                if ((_lastAngularVelocity - _rb.angularVelocity).magnitude > Threshold) {
                    _lastAngularVelocity = _rb.angularVelocity;
                    SendUpdate(RigidBodyFlags.AngularVelocity, _rb.angularVelocity.ToString());
                }

                if (IsDirty) {
                    SendUpdate(TransformFlags.Position, _rb.position.ToString());
                    SendUpdate(TransformFlags.Rotation, _rb.rotation.eulerAngles.ToString());
                    SendUpdate(RigidBodyFlags.AngularVelocity, _rb.angularVelocity.ToString());
                    SendUpdate(RigidBodyFlags.Velocity, _rb.linearVelocity.ToString());
                    IsDirty = false;
                }
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case TransformFlags.Position:
                if (IsClient) {
                    _lastPosition = NetworkCore.Vector3FromString(value);

                    if (useAdjustedVelocity)
                        adjustVelocity = _lastPosition - _rb.position;

                    var d = (_rb.position - _lastPosition).magnitude;
                    if (d > EThreshold) {
                        adjustVelocity = Vector3.zero;
                        _rb.position = _lastPosition;
                    }
                }

                break;
            case TransformFlags.Rotation:
                if (IsClient) {
                    _lastRotation = NetworkCore.Vector3FromString(value);
                    _rb.rotation = Quaternion.Euler(_lastRotation);
                }

                break;

            case RigidBodyFlags.Velocity:
                if (IsClient) {
                    _lastVelocity = NetworkCore.Vector3FromString(value);

                    if (_lastVelocity.magnitude <= 0.1f) {
                        adjustVelocity = Vector3.zero;
                    }
                }

                break;

            case RigidBodyFlags.AngularVelocity:
                if (IsClient) {
                    _lastAngularVelocity = NetworkCore.Vector3FromString(value);
                }

                break;
        }
    }

    public override void NetworkedStart() { }

    private void Start() {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        if (IsServer) return;

        _rb.linearVelocity = _lastVelocity;

        bool isMoving = (_lastVelocity.magnitude < 0.01f);

        if (isMoving) {
            _rb.linearVelocity = Vector3.zero;
        }

        _rb.angularVelocity = _lastAngularVelocity;
    }
}