using System;
using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;

struct CallAudioFlags {
    public const string BGM = "BGM";
    public const string UI = "UI";
    public const string SFX = "SFX";
}

public class CallAudio : NetworkComponent {
    AudioManager audioManager;
    private NetworkComponent _networkComponentImplementation;

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(0.5f);
    }
    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case CallAudioFlags.BGM:
                audioManager.PlayBGM(value);
                break;
            case CallAudioFlags.UI:
                audioManager.PlayUI(value);
                break;
            case CallAudioFlags.SFX:
                audioManager.PlaySFX(value);
                break;
        }
    }
    public override void NetworkedStart() {
        audioManager = AudioManager.Instance;
    }

    public void PlayBGM(string audioName) {
        SendUpdate(CallAudioFlags.BGM, audioName);
    }
    
    public void PlaySFX(string soundName) {
        SendUpdate(CallAudioFlags.SFX, soundName);
    }

    public void PlayUI(string uiName) {
        SendUpdate(CallAudioFlags.UI, uiName);
    }
}