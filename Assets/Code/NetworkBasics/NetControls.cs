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
    public const string SOUND = "SOUND";
    public const string ANIMATION = "Animation";
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
    private AudioManager AM;

    [SerializeField] private float rayCastDistance = 2.5f;
    [SerializeField] private float linearSpeed = 5f;

    //Server only
    public Item _item;
    public Interactable _interactable;
    public Actor actor;
    private bool hasSomething = false;
    private bool lookingAt = false;
    private bool HasBeenDetained = false;
    public float attackCooldown;
    [SerializeField] private float _currentCooldown;

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
        while (IsConnected) {
            if (IsServer)
                if (IsDirty) {
                    SendUpdate(NetControlFlag.PRIMARY, ((int)_pAction).ToString());
                    SendUpdate(NetControlFlag.SECONDARY, ((int)_sAction).ToString());
                    IsDirty = false;
                }
            if(_player.IsDetained && !HasBeenDetained)
            {
                HandleMessage(NetControlFlag.ANIMATION, "detain");
                HasBeenDetained = true;
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case NetControlFlag.ANIMATION:
                if (IsServer) break;
                MyAnimator.SetBool(value, !MyAnimator.GetBool(value));
                if(value == "detain")
                {
                    HasBeenDetained = true;
                }

                break;
            case NetControlFlag.SOUND:
                if (IsServer) break;
                AM.PlaySFX(value, transform.position);
                break;
            case NetControlFlag.MOVEINPUT:
                if (IsClient) {
                    MyAnimator.SetBool("walk", Vector2FromString(value) != Vector2.zero);
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
                    switch (_pAction) {
                        case PrimaryActions.PickupItem:
                            if (!_player.hasBag) return;
                            if (!_player.PossibleAdd(_item)) return;
                            _player.AddItem(_item);
                            SendUpdate(NetControlFlag.SOUND, "Item_Pickup");
                            break;
                        case PrimaryActions.PickupBag:
                            if (_player.hasBag) return;
                            if (_item is Bag bag) {
                                if (bag._hasOwner) return;
                                _player.AssignBag(bag);
                                SendUpdate(NetControlFlag.SOUND, "Bag");
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
                                    if (_player._myNpm.GetInformant())
                                        player.SetDetained(true);
                                    break;
                                case Civilian civilian:
                                    civilian.Detain();
                                    break;
                            }

                            break;
                    }
                }

                if (IsLocalPlayer) {
                    _pAction = (PrimaryActions)int.Parse(value);
                }

                break;

            case NetControlFlag.SECONDARY:
                if (IsServer) {
                    switch (_sAction) {
                        case SecondaryActions.Tamper:
                            if (_item is Bag bag) {
                                if (_currentCooldown > 0) break;
                                _currentCooldown = 2 * attackCooldown;
                                if (IsClient) break;
                                SendUpdate(NetControlFlag.SOUND, "Tamper");
                                bag.isTampered = true;
                            }

                            break;
                        case SecondaryActions.Attack:
                            if (_currentCooldown > 0) break;
                            _currentCooldown = attackCooldown;
                            if (IsClient) break;
                            _hitboxSpawner.SpawnAttack();
                            SendUpdate(NetControlFlag.ANIMATION, "attack");
                            StartCoroutine(StopTrigger("attack"));
                            break;
                        case SecondaryActions.Release:
                            _player.ReleaseBag();
                            SendUpdate(NetControlFlag.SOUND, "Bag");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (IsLocalPlayer) {
                    _sAction = (SecondaryActions)int.Parse(value);
                }

                break;
        }
    }

    public override void NetworkedStart() { }

    public void OnMoveAction(InputAction.CallbackContext mv) {
        if (IsServer) return;
        if (_player.GetDetained() || (GameManager.GamePaused && GameManager._gameStart)) return;
        if (MyAnimator.GetBool("attack")) return;
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

    public void Primary()
    {
        if (IsServer) return;
        if (_player.GetDetained() || GameManager.GamePaused) return;
        SendCommand(NetControlFlag.PRIMARY, "");
    }

    public void Secondary()
    {
        if (IsServer) return;
        if (_player.GetDetained() || GameManager.GamePaused) return;
        SendCommand(NetControlFlag.SECONDARY, "");
    }

    private IEnumerator StopTrigger(string trigger) {
        yield return new WaitForSeconds(.6f);
        SendUpdate(NetControlFlag.ANIMATION, trigger);
    }

    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main;
        _player = GetComponent<Player>();
        _hitboxSpawner = GetComponent<HitboxSpawner>();
        AM = FindFirstObjectByType<AudioManager>();
    }

    private void Update() {
        if (IsServer) {
            _rb.linearVelocity = new Vector3(_lastMoveInput.x, _lastMoveInput.y, 0) * linearSpeed;
            //Change look rot
            float angle = Mathf.Atan2(_lastLookInput.x, _lastLookInput.y) * Mathf.Rad2Deg;
            _rb.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            _currentCooldown -= Time.deltaTime;

            Ray ray = new Ray(transform.position, -transform.up);

            RaycastHit[] hits = Physics.RaycastAll(ray, rayCastDistance);
            RaycastHit? firstHit = null;
            float closestDistance = float.MaxValue;

            foreach (RaycastHit hit in hits) {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.isTrigger && hit.distance < closestDistance) {
                    closestDistance = hit.distance;
                    firstHit = hit;
                }
            }


            if (firstHit.HasValue) {
                RaycastHit hit = firstHit.Value;

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

            SendUpdate(NetControlFlag.PRIMARY, ((int)_pAction).ToString());
            SendUpdate(NetControlFlag.SECONDARY, ((int)_sAction).ToString());
            Debug.DrawRay(transform.position, -transform.up * rayCastDistance, Color.red);
        }

        if (IsLocalPlayer) {
            _cam.transform.position = transform.position - Vector3.forward * 5f;
        }

        //Animation
    }
}