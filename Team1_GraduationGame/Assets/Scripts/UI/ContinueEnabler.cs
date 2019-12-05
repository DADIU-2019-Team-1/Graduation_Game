// Code Owner: Jannik Neerdal & Jakob Husted
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ContinueEnabler : MonoBehaviour
{
    private Button continueBtn;
    void Start()
    {
        continueBtn = GetComponent<Button>();

        if (PlayerPrefs.GetInt("previousGame") == 1)
            continueBtn.interactable = true;
        else
            continueBtn.interactable = false;
    }
}
