using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class VotingObject : MonoBehaviour {
    public Sprite[] characterSprite;
    public int characterIndex;
    public string playerName;
    public bool isOn;

    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image playerImage;

    private void Update() {
        playerNameText.text = "Vote " + playerName;
        playerImage.sprite = characterSprite[characterIndex];
    }

    public void setOn(bool on) {
        isOn = on;
    }
}