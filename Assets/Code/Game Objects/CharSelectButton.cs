using UnityEngine;
using UnityEngine.UI;

public class CharSelectButton : MonoBehaviour
{
    public Button[] Buttons;
    public Sprite takenImg;
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
                Buttons[i].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = takenImg;
                Buttons[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = new Vector4(255,255,255,0);
                Buttons[i].interactable = false;
            }
            CharsTaken[i] = GameManager.CharsTaken[i];
        }
    }
}
