using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public struct PlayerFlags {
    public const string NAME = "NAME";
    public const string CHAR = "CHAR";
    public const string BAG = "BAG";
    public const string BAGINFO = "BAGINFO";
    public const string DETAIN = "DETAIN";
    public const string DAMAGE = "DAMAGE";
}


public class Player : Actor {
    //UI
    [SerializeField] public GameObject inGameUI;
    [SerializeField] private GameObject bagInfo;
    public TMP_Text bagTextInfo;
    [SerializeField] private Slider healthBar;

    [SerializeField] private Text PlayerName;
    [SerializeField] private string PName = "<Default>";
    public NetworkPlayerManager _myNpm;

    //Action Display
    [SerializeField] private Sprite[] primaryActions;
    [SerializeField] private Sprite[] secondaryActions;
    [SerializeField] private Image primaryActionImage;
    [SerializeField] private Image secondaryActionImage;
    private NetControls _myControls;


    public bool hasBag;
    private Bag _currentBag;
    private bool GameStart = true;

    public override IEnumerator SlowUpdate() {
        while (true) {
            if (GameManager._gameStart && GameStart) {
                TeleportStart();
            }

            if (IsServer) {
                if (IsDirty) {
                    SendUpdate(PlayerFlags.BAG, hasBag.ToString());
                    SendUpdate(ActorFlags.HEALTH, Health.ToString());
                    SendUpdate(PlayerFlags.NAME, PName);
                    SendUpdate(PlayerFlags.BAGINFO, bagTextInfo.text);
                    IsDirty = false;
                }
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case PlayerFlags.NAME:
                if (IsLocalPlayer) {
                    PName = value;
                }

                if (IsServer) {
                    SendUpdate(PlayerFlags.NAME, value);
                }

                if (IsClient) {
                    PlayerName.text = value;
                }

                break;
            case PlayerFlags.BAG:
                if (IsLocalPlayer) {
                    hasBag = bool.Parse(value);
                }

                break;
            case ActorFlags.HEALTH:
                if (IsClient) {
                    Health = int.Parse(value);
                }

                if (IsLocalPlayer) {
                    Health = int.Parse(value);
                    healthBar.value = (float)Health / MaxHealth;
                }

                if (IsServer) {
                    SendUpdate(ActorFlags.HEALTH, value);
                }

                break;
            case PlayerFlags.BAGINFO:
                bagTextInfo.text = value;
                break;
            case PlayerFlags.DETAIN:
                IsDetained = bool.Parse(value);
                break;
            case PlayerFlags.DAMAGE:
                if (IsServer) {
                    SendUpdate(PlayerFlags.DAMAGE, value);
                }

                if (IsClient) {
                    Debug.Log(value + " Health: " + Health);
                    if (_myNpm.playerChar.ToString() == value) {
                        UpdateHealth(-1);
                        if (Health <= 0) {
                            //kill player
                        }
                    }

                    SendCommand(ActorFlags.HEALTH, Health.ToString());
                }

                break;
        }
        //TODO make necessary flags
    }

    public override void NetworkedStart() {
        foreach (var npm in FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None)) {
            if (npm.Owner == Owner) {
                _myNpm = npm;
                PName = npm.playerName;
                npm.player = this;
                //Get Whatever you need from it
            }
        }
        inGameUI.SetActive(true);
        //TODO Make what happens when they connect to the server
    }

    protected override void OnDeath() {
        //TODO Make do a death thing
        _myNpm.inGame = false;
        ReleaseBag();
        if(IsServer) SendUpdate(PlayerFlags.DETAIN, true.ToString()); 
        IsDetained = true;
    }

    public void Escape() {
        _myNpm.inGame = false;
    }

    public void AssignBag(Bag bag) {
        _currentBag = bag;
        _currentBag.AssignOwner(gameObject);
        hasBag = true;
        SendUpdate(PlayerFlags.BAG, hasBag.ToString());
    }

    //Server AddItem
    public void AddItem(Item item) {
        _currentBag.AddItem(item);
        item.SetDestroy();
    }

    public bool PossibleAdd(Item item) {
        return item.weight + _currentBag.weight <= _currentBag.maxWeight;
    }

    public void ReleaseBag() {
        //To be called on release
        _currentBag.ReleaseOwner();
        hasBag = false;
        SendUpdate(PlayerFlags.BAG, hasBag.ToString());
    }

    private void Start() {
        inGameUI.SetActive(false);
        _myControls = GetComponent<NetControls>();
    }

    private void Update() {
        if (IsServer)
            if (hasBag) {
                bagTextInfo.text = $"${_currentBag.money}\n{_currentBag.weight}/{_currentBag.maxWeight}";
                SendUpdate(PlayerFlags.BAGINFO, bagTextInfo.text);
            }

        if (IsLocalPlayer) {
            inGameUI.SetActive(!GameManager.GamePaused);
            bagInfo.SetActive(hasBag);
            primaryActionImage.sprite = primaryActions[(int)_myControls._pAction];
            secondaryActionImage.sprite = secondaryActions[(int)_myControls._sAction];
            Button primaryButton = primaryActionImage.GetComponent<Button>();
            primaryButton.onClick.AddListener(OnClickPrimary);
            Button secondaryButton = primaryActionImage.GetComponent<Button>();
            secondaryButton.onClick.AddListener(OnClickSecondary);
        }
    }

    public void OnClickPrimary() {
        primaryActionImage.sprite = primaryActions[(int)_myControls._pAction + 4];
    }

    private void OnClickSecondary() {
        primaryActionImage.sprite = primaryActions[(int)_myControls._pAction + 3];
    }

    public bool GetDetained() {
        return IsDetained;
    }

    public void SetDetained(bool value) {
        IsDetained = value;
        SendUpdate(PlayerFlags.DETAIN, value.ToString());
        if (value && hasBag) {
            ReleaseBag();
        }
    }

    public void TeleportStart() {
        transform.position = new Vector3(-104, -16, 0);
        GameStart = false;
    }

    public void TeleportEnd() {
        transform.position = new Vector3(0, 0, 0);
        GameStart = true;
    }
}