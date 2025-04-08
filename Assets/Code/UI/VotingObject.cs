using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class VotingObject : MonoBehaviour {
    public Sprite[] characterSprite;
    public int characterIndex;
    public string playerName;
    

    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image playerImage;
    private Toggle _toggle;

    private void Start() {
        _toggle = GetComponent<Toggle>();
        _toggle.group = GetComponentInParent<ToggleGroup>();
    }
    
    private void Update() {
        playerNameText.text = "Vote " + playerName;
        playerImage.sprite = characterSprite[characterIndex];
    }
}