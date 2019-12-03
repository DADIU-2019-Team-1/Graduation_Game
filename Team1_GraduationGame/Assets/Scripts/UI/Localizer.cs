// Code Owner: Jannik Neerdal - Optimized
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class Localizer : MonoBehaviour
{
    private Button _thisBtn;
    private TextMeshProUGUI _textElement;

    private static readonly string[] languages =
    {
        "English", // 0
        "Danish", // 1
        "Hungarian", // 2
    };

    // Display an array of text areas that have their index show the name of the language it pertains to
    [SerializeField] [NamedArray(new[] {"English", "Danish", "Hungarian"})]
    [TextArea] private string[] _localizedTexts;
    [Tooltip("Only use this for buttons that change the language of the game!")]
    [SerializeField] private int languageButtonIndex;


    private void Awake()
    {
        if (GetComponent<TextMeshProUGUI>() != null) // Find the text element on the object itself,
        {
            _textElement = GetComponent<TextMeshProUGUI>();
        }
        else if (GetComponentsInChildren<TextMeshProUGUI>().Length == 1) // Or find the text element in a child of the object,
        {
            _textElement = GetComponentInChildren<TextMeshProUGUI>();
        }
        else if (GetComponentsInChildren<TextMeshProUGUI>().Length > 1) // Unless there are multiple children with a text object
        {
            Debug.LogError("More than 1 of Localizer object " + gameObject.name +
                           "'s children have text components on them!\n" +
                           "Assign each individual text object with it's own localizer script, so the desired text is assigned to the appropriate object.");
        }
        else // If none of the above are true, either the text element is missing, or this object should not have a localizer script on it
        {
            Debug.LogError("No text elements were found on or as children of " + gameObject.name +
                           ", which has a localizer script on it!");
        }

        if (GetComponent<Button>() != null)
            _thisBtn = GetComponent<Button>();
    }

    private void Start()
    {
        // Load the current language and subscribe the LanguageChanged method to the languageChangeEvent
        LanguageChanged(PlayerPrefs.GetInt("Language", 0));
        UIMenu[] menus = Resources.FindObjectsOfTypeAll<UIMenu>();
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].languageChangeEvent += LanguageChanged;
        }
    }

    private void LanguageChanged(int languageIndex)
    {
        if (_thisBtn != null && languageButtonIndex > 0) // This behaviour is only for buttons that can change the language
        {
            if (languageIndex + 1 == languageButtonIndex) // Languages are 0-indexed in the event, but not in the inspector (since default is 0)
            {
                _thisBtn.interactable = false; // Button is of the same language as the event, so it should not be pressed, and is highlighted to show the current language
                if (_textElement != null)
                {
                    _textElement.color = Color.white;
                }
            }
            else // Button was not of the same language as requested language from the event, so it can be pressed but is not highlighted to show it is not the current language
            {
                _thisBtn.interactable = true;
                if (_textElement != null)
                {
                    _textElement.color = Color.grey;
                }
            }
        }

        if (_textElement != null)
        {

            if (_localizedTexts[languageIndex].Trim().Length > 0) // Set the text of the object to the requested language
            {
                _textElement.text = _localizedTexts[languageIndex];
            }
            else // If the language is missing in the localizer, it will instead set be set to the default language
            {
                _textElement.text = _localizedTexts[0];
                Debug.LogError("Language was changed to " + languages[languageIndex] + ", but object " +
                               gameObject.name + " does not have any text in it's localizer for that language!" +
                               "\nWriting the content of default language text in it's container...");
            }
        }
        else
        {
            Debug.LogError("No text elements were found on or as children of " + gameObject.name +
                           ", which has a localizer script on it!");
        }
    }
}
