// Code Owner: Jannik Neerdal
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class Localizer : MonoBehaviour
{
    private Button _thisBtn;
    private UIMenu _menu;
    private TextMeshProUGUI _textElement;

    [HideInInspector] public static readonly string[] languages =
    {
        "English", // 0
        "Danish", // 1
        "Hungarian", // 2
    };

    [SerializeField] private int languageButtonIndex;
    [SerializeField] [NamedArray(new[] {"English", "Danish", "Hungarian"})]
    [TextArea] private string[] _localizedTexts;


    private void Awake()
    {
        if (GetComponent<TextMeshProUGUI>() != null)
        {
            _textElement = GetComponent<TextMeshProUGUI>();
        }
        else if (GetComponentsInChildren<TextMeshProUGUI>().Length == 1)
        {
            _textElement = GetComponentInChildren<TextMeshProUGUI>();
        }
        else if (GetComponentsInChildren<TextMeshProUGUI>().Length > 1)
        {
            Debug.LogError("More than 1 of Localizer object " + gameObject.name +
                           "'s children have text components on them!\n" +
                           "Assign each individual text object with it's own localizer script, so the desired text is assigned to the appropriate object.");
        }
        else
        {
            Debug.LogError("No text elements were found on or as children of " + gameObject.name +
                           ", which has a localizer script on it!");
        }

        if (GetComponent<Button>() != null)
            _thisBtn = GetComponent<Button>();
    }

    private void Start()
    {
        if (_menu == null)
            _menu = FindObjectOfType<UIMenu>();
        LanguageChanged(PlayerPrefs.GetInt("Language", 0));
        _menu.languageChangeEvent += LanguageChanged;
    }

    private void LanguageChanged(int languageIndex)
    {
        if (_thisBtn != null && languageButtonIndex > 0)
        {
            if (languageIndex + 1 == languageButtonIndex)
            {
                _thisBtn.interactable = false;
                if (_textElement != null)
                {
                    _textElement.color = Color.white;
                }
            }
            else
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
            if (_localizedTexts[languageIndex].Length > 0)
            {
                _textElement.text = _localizedTexts[languageIndex];
            }
            else
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
