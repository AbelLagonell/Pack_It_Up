using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

struct PlayerFlags {
    public const string NAME = "NAME";
    public const string CHAR = "CHAR";
    public const string BAG = "BAG";
    public const string BAGINFO = "BAGINFO";
    public const string DETAIN = "DETAIN";
}


public class Player : Actor {
    //UI
    [SerializeField] private GameObject inGameUI;
    [SerializeField] private GameObject bagInfo;
    public TMP_Text bagTextInfo;
    [SerializeField] private Slider healthBar;

    [SerializeField] private Text PlayerName;
    [SerializeField] private string PName = "<Default>";
    public NetworkPlayerManager _myNpm;

    public bool hasBag;
    private Bag _currentBag;

    public override IEnumerator SlowUpdate() {
        while (true) {
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
                    healthBar.value = (float)Health / MaxHealth;
                }

                break;
            case PlayerFlags.BAGINFO:
                bagTextInfo.text = value;
                break;
            case PlayerFlags.DETAIN:
                IsDetained = bool.Parse(value);
                Debug.Log(IsDetained);
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
        //TODO Make what happens when they connect to the server
    }

    protected override void OnDeath() {
        //TODO Make do a death thing
        _myNpm.inGame = false;
    }

    public void Escape()
    {
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

    public void ReleaseBag() {
        //To be called on release
        _currentBag.ReleaseOwner();
        hasBag = false;
        SendUpdate(PlayerFlags.BAG, hasBag.ToString());
    }

    private void Start() {
        inGameUI.SetActive(false);
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
        }
    }

    public bool GetDetained() {
        return IsDetained;
    }

    public void SetDetained(bool value) {
        if (IsServer) Debug.Log("Server Detained");
        IsDetained = value;
        SendUpdate(PlayerFlags.DETAIN, value.ToString());
        if (value && hasBag) {
            ReleaseBag();
        }
    }
}