// Code Owner: Jannik Neerdal

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Localizer : MonoBehaviour
{
    private Button _thisBtn;
    private HubMenu _menu;
    private string _gameObjectName;
    private TextMeshProUGUI _textElement;
    [HideInInspector] public static readonly string[] languages = 
    {
        "English", // 0
        "Danish", // 1
        "Emojis (kill me)", // 2
        "German", // 3
        "Hungarian", // 4
        "Bisaya" // 5
    };

    [SerializeField] [NamedArray(new[] {"English", "Danish", "Emojis (kill me)", "German", "Hungarian", "Bisaya"})] 
    [TextArea] private string[] _localizedTexts;
    private void Awake()
    {
        _gameObjectName = gameObject.name.ToLower();
        if (GetComponent<TextMeshProUGUI>() != null)
        {
            _textElement = GetComponent<TextMeshProUGUI>();
        }
        else if (GetComponentsInChildren<TextMeshProUGUI>().Length ==  1)
        {
            _textElement = GetComponentInChildren<TextMeshProUGUI>();
        }
        else if (GetComponentsInChildren<TextMeshProUGUI>().Length > 1)
        {
            Debug.LogError("More than 1 of Localizer object " + gameObject.name + "'s children have text components on them!\n" +
                           "Assign each individual text object with it's own localizer script, so the desired text is assigned to the appropriate object.");
        }
        else
        {
            Debug.LogError("No text elements were found on or as children of " + gameObject.name + ", which has a localizer script on it!");
        }

        if (GetComponent<Button>() != null)
            _thisBtn = GetComponent<Button>();

        if (_menu == null)
            _menu = FindObjectOfType<HubMenu>();


        LanguageChanged(PlayerPrefs.GetInt("Language",0));
    }

    private void Start()
    {
        _menu.languageChangeEvent += LanguageChanged;
    }

    private void LanguageChanged(int languageIndex)
    {
        if (_textElement != null)
        {
            if (_localizedTexts.Length > 0)
                _textElement.text = _localizedTexts[languageIndex];
            else
            {
                _textElement.text = _localizedTexts[0];
                Debug.LogError("Language was changed to " + languages[languageIndex] + ", but object " + gameObject.name + " does not have any text in it's localizer for that language!" +
                               "\nWriting the content of default language text in it's container...");
            }
        }
        else
        {
            Debug.LogError("No text elements were found on or as children of " + gameObject.name + ", which has a localizer script on it!");
        }
    }

    private void SetInteractable(bool val) // TODO: EVENTIZE IT!!
    {
        if (_thisBtn != null) // This is a button
        {
            _thisBtn.interactable = val;
            if (val)
                GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            else
                GetComponentInChildren<TextMeshProUGUI>().color = Color.grey;
        }
        else
        {
            Debug.LogError("SetInteractable() was called for an object without an (initialized) button component!");
        }
    }
}
