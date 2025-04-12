using System;
using System.Collections;
using NETWORK_ENGINE;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

struct NetControlFlag {
    public const string MOVEINPUT = "Move";
    public const string LOOKINPUT = "Look";
    public const string PRIMARY = "Primary";
    public const string SECONDARY = "Secondary";
}

public enum PrimaryActions {
    PickupItem,
    PickupBag,
    Interaction,
    Arrest
}

public enum SecondaryActions {
    Attack,
    Release,
    Tamper
}


[RequireComponent(typeof(Rigidbody), typeof(PlayerInput), typeof(HitboxSpawner))]
public class NetControls : NetworkComponent {
    private Camera _cam;
    private Rigidbody _rb;
    public Player _player;

    [SerializeField] private float rayCastDistance = 2.5f;
    [SerializeField] private float linearSpeed = 5f;

    //Server only
    public Item _item;
    public Interactable _interactable;
    public Actor actor;
    private bool hasSomething = false;
    private bool lookingAt = false;
    public float attackCooldown;
    [SerializeField]private float _currentCooldown;

    //Sync Vars
    public Animator MyAnimator;
    public PrimaryActions _pAction;
    public SecondaryActions _sAction;
    private Vector2 _lastMoveInput;
    private Vector2 _lastLookInput;

    private HitboxSpawner _hitboxSpawner;

    private Vector2 Vector2FromString(string s) {
        string[] split = s.Trim().Trim('(', ')').Split(',');
        return new Vector2(float.Parse(split[0]), float.Parse(split[1]));
    }

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case NetControlFlag.MOVEINPUT:
                if (IsClient) {
                    if (Vector2FromString(value) == Vector2.zero) //Need to sync animations
                    {
                        MyAnimator.SetBool("walk", false);
                    } else {
                        MyAnimator.SetBool("walk", true);
                    }
                }

                if (IsServer)
                    _lastMoveInput = Vector2FromString(value);
                SendUpdate(NetControlFlag.MOVEINPUT, value);
                break;
            case NetControlFlag.LOOKINPUT:
                if (IsServer) {
                    _rb.angularVelocity = Vector3.zero;
                    _lastLookInput = Vector2FromString(value);
                }

                break;
            case NetControlFlag.PRIMARY:
                if (IsServer) {
                    Debug.Log("Primary");
                    switch (_pAction) {
                        case PrimaryActions.PickupItem:
                            if (!_player.hasBag) return;
                            _player.AddItem(_item);
                            break;
                        case PrimaryActions.PickupBag:
                            if (_player.hasBag) return;
                            if (_item is Bag bag) {
                                if (bag._hasOwner) return;
                                _player.AssignBag(bag);
                            }

                            break;
                        case PrimaryActions.Interaction:
                            if (!hasSomething) return;
                            if (!_interactable.usable) return;
                            _interactable.OnUse();
                            break;
                        case PrimaryActions.Arrest:
                            if (!lookingAt) return;
                            switch (actor) {
                                case Player player:
                                    Debug.Log("Setting Detained");
                                    if (_player._myNpm.GetInformant())
                                        player.SetDetained(true);
                                    break;
                                case Civilian civilian:
                                    civilian.Detain();
                                    break;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                break;

            case NetControlFlag.SECONDARY:
                if (IsServer) {
                    switch (_sAction) {
                        case SecondaryActions.Tamper:
                            if (_item is Bag bag) {
                                bag.isTampered = true;
                            }

                            break;
                        case SecondaryActions.Attack:
                            if (IsServer) Debug.Log("Attacking");
                            if (_currentCooldown > 0) break;
                            if (IsClient) break;
                            _hitboxSpawner.SpawnAttack();
                            _currentCooldown = attackCooldown;
                            break;
                        case SecondaryActions.Release:
                            _player.ReleaseBag();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                break;
        }
    }

    public override void NetworkedStart() {
    }

    public void OnMoveAction(InputAction.CallbackContext mv) {
        if (IsServer) return;
        if (_player.GetDetained() || (GameManager.GamePaused && GameManager._gameStart)) return;
        if (mv.performed || mv.started) {
            if (IsLocalPlayer) {
                MyAnimator.SetBool("walk", true);
            }

            SendCommand(NetControlFlag.MOVEINPUT, mv.ReadValue<Vector2>().ToString());
        }

        if (mv.canceled) {
            if (IsLocalPlayer) {
                MyAnimator.SetBool("walk", false);
            }

            SendCommand(NetControlFlag.MOVEINPUT, Vector2.zero.ToString());
        }
    }


    public void OnLookAction(InputAction.CallbackContext lk) {
        if (IsServer) return;
        if (lk.performed || lk.started) {
            var lkVec = lk.ReadValue<Vector2>();
            if (lk.control.device is Mouse) {
                lkVec.x /= Screen.width;
                lkVec.y /= Screen.height;
                lkVec.x -= 0.50f;
                lkVec.y -= 0.50f;
            } else
                lkVec = lkVec.normalized;

            lkVec.y = -lkVec.y;
            SendCommand(NetControlFlag.LOOKINPUT, lkVec.ToString());
        }

        if (lk.canceled) {
            SendCommand(NetControlFlag.LOOKINPUT, _lastLookInput.ToString());
        }
    }

    public void OnPrimaryAction(InputAction.CallbackContext pa) {
        if (IsServer) return;
        if (_player.GetDetained() || GameManager.GamePaused) return;
        if (pa.performed || pa.started) {
            SendCommand(NetControlFlag.PRIMARY, "");
        }
    }

    public void OnSecondaryAction(InputAction.CallbackContext sa) {
        if (IsServer) return;
        if (_player.GetDetained() || GameManager.GamePaused) return;
        if (sa.performed || sa.started) {
            SendCommand(NetControlFlag.SECONDARY, "");
        }
    }

    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main;
        _player = GetComponent<Player>();
        _hitboxSpawner = GetComponent<HitboxSpawner>();
    }

    private void Update() {
        if (IsServer) {
            _rb.linearVelocity = new Vector3(_lastMoveInput.x, _lastMoveInput.y, 0) * linearSpeed;
            //Change look rot
            float angle = Mathf.Atan2(_lastLookInput.x, _lastLookInput.y) * Mathf.Rad2Deg;
            _rb.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            _currentCooldown -= Time.deltaTime;

            Ray ray = new Ray(transform.position, -transform.up);

            if (Physics.Raycast(ray, out RaycastHit hit, rayCastDistance)) {
                switch (hit.collider.tag) {
                    case "Item":
                        _pAction = PrimaryActions.PickupItem;
                        _item = hit.collider.gameObject.GetComponent<Item>();
                        break;
                    case "Bag":
                        _pAction = PrimaryActions.PickupBag;
                        if (_player._myNpm.GetInformant()) {
                            _sAction = SecondaryActions.Tamper;
                        }

                        _item = hit.collider.gameObject.GetComponent<Bag>();
                        break;
                    case "Interactable":
                        hasSomething = true;
                        _pAction = PrimaryActions.Interaction;
                        _interactable = hit.collider.gameObject.GetComponent<Interactable>();
                        break;
                    case "Player":
                    case "Civilian":
                        lookingAt = true;
                        _pAction = PrimaryActions.Arrest;
                        actor = hit.collider.gameObject.GetComponent<Actor>();
                        break;
                }
            } else {
                _pAction = PrimaryActions.Interaction;
                _sAction = _player.hasBag ? SecondaryActions.Release : SecondaryActions.Attack;
                _item = null;
                _interactable = null;
                actor = null;
                hasSomething = false;
                lookingAt = false;
            }

            Debug.DrawRay(transform.position, -transform.up * rayCastDistance, Color.red);
        }

        if (IsLocalPlayer) {
            _cam.transform.position = transform.position - Vector3.forward * 5f;
        }

        //Animation
    }
}