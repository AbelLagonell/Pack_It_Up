using UnityEngine;
using UnityEngine.UI;

public class CharSelectButton : MonoBehaviour
{
    public Button[] Buttons;
    public Image takenImg;
    private bool[] CharsTaken;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CharsTaken = GameManager.CharsTaken;
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < 8; i++)
        {
            if(CharsTaken[i])
            {
                Buttons[i].image = takenImg;
                Buttons[i].interactable = false;
                Destroy(Buttons[i]);
            }
            CharsTaken[i] = GameManager.CharsTaken[i];
        }
    }
}
