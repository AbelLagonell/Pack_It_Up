using System;
using UnityEngine;

public class StartAudio : MonoBehaviour {
    private void Start() {
        AudioManager.Instance.PlayBGM("Main_Menu_Theme");
    }
}