using System;
using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;

public struct TransformFlags {
    public const string Position = "POS";
    public const string Rotation = "ROT";
}

public class NetTransform : NetworkComponent {
    //Sync
    private Vector3 _lastPosition;
    private Vector3 _lastRotation;

    //Unsync
    private const float EThreshold = 1f;
    private const float Threshold = 0.01f;
    public float Speed = 1;


    public override IEnumerator SlowUpdate() {
        while (true) {
            if (IsServer) {
                float distance = (_lastPosition - transform.position).magnitude;

                bool posDesync = distance > Threshold;
                bool rotDesync = ((transform.rotation.eulerAngles - _lastRotation).magnitude) > Threshold;


                if (posDesync) {
                    SendUpdate(TransformFlags.Position, transform.position.ToString());
                    _lastPosition = transform.position;
                }

                if (rotDesync) {
                    SendUpdate(TransformFlags.Rotation, transform.rotation.eulerAngles.ToString());
                    _lastRotation = transform.rotation.eulerAngles;
                }

                if (IsDirty) {
                    SendUpdate(TransformFlags.Position, transform.position.ToString());
                    SendUpdate(TransformFlags.Rotation, transform.rotation.ToString());
                    IsDirty = false;
                }
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case TransformFlags.Position:
                _lastPosition = NetworkCore.Vector3FromString(value);
                break;
            case TransformFlags.Rotation:
                _lastRotation = NetworkCore.Vector3FromString(value);
                break;
        }
    }

    public override void NetworkedStart() { }

    private void Update() {
        if (IsServer) return;
        float distance = (transform.position - _lastPosition).magnitude;

        if (distance > EThreshold) { transform.position = _lastPosition; } else {
            transform.position = Vector3.Lerp(transform.position, _lastPosition, Time.deltaTime * Speed);
        }


        transform.rotation = Quaternion.Euler(_lastRotation);
    }
}