using UnityEngine;

//Created for IsLocalPlayer only
public class MenuAudio : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    public void OnClick()
    {
        AudioSource.PlayClipAtPoint(clip,transform.position);
    }
}
