using System;
using System.Collections;
using NETWORK_ENGINE;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

struct NetControlFlag {
    public const string MOVEINPUT = "Move";
    public const string LOOKINPUT = "Look";
    public const string PRIMARY = "Primary";
    public const string SECONDARY = "Secondary";
}

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class NetControls : NetworkComponent {
    private Rigidbody _rb;
    [SerializeField] private float angularSpeed = 5f;
    [SerializeField] private float linearSpeed = 5f;

    //Sync Vars
    private Vector2 _lastMoveInput;
    private Vector2 _lastLookInput;

    private Vector2 Vector2FromString(string s) {
        string[] split = s.Trim().Trim('(', ')').Split(',');
        return new Vector2(float.Parse(split[0]), float.Parse(split[1]));
    }

    public override IEnumerator SlowUpdate() {
        while (true) {
            if (IsServer) {
                //Primary Action
                //Secondary Action
            }

            if (IsLocalPlayer) {
                //Camera
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case NetControlFlag.MOVEINPUT:
                if (IsServer)
                    _lastMoveInput = Vector2FromString(value);
                break;
            case NetControlFlag.LOOKINPUT:
                if (IsServer) {
                    _lastLookInput = Vector2FromString(value);
                    Debug.Log(_lastLookInput);
                }

                break;
        }
    }

    public override void NetworkedStart() { }

    public void OnMoveAction(InputAction.CallbackContext mv) {
        if (IsServer) return;
        if (mv.performed || mv.started) {
            SendCommand(NetControlFlag.MOVEINPUT, mv.ReadValue<Vector2>().ToString());
        }

        if (mv.canceled) {
            SendCommand(NetControlFlag.MOVEINPUT, Vector2.zero.ToString());
        }
    }


    public void OnLookAction(InputAction.CallbackContext lk) {
        Debug.Log(lk.control.device.);
        if (IsServer) return;
        if (lk.performed || lk.started) {
            
            SendCommand(NetControlFlag.LOOKINPUT, new Vector2(_lastLookInput.x, _lastLookInput.y).normalized.ToString());
        }

        if (lk.canceled) {
            SendCommand(NetControlFlag.LOOKINPUT, Vector2.zero.ToString());
        }
    }

    private void Start() {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        if (IsServer) {
            _rb.linearVelocity = new Vector3(_lastMoveInput.x, _lastMoveInput.y, 0) * linearSpeed;
            //Change look rot
            float angle = Mathf.Atan2(_lastLookInput.x, _lastLookInput.y) * Mathf.Rad2Deg;
            _rb.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        //Animation
    }
}