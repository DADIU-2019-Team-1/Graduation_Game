

using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button), typeof(TextMeshProUGUI))]
public class QualityButton : MonoBehaviour
{
    [SerializeField] private int quality;
    private Button btn;
    private TextMeshProUGUI text;
    private UIMenu[] menus;
    private void Start()
    {
        menus = Resources.FindObjectsOfTypeAll<UIMenu>();

    }

    private void QualityChange()
    {

    }
}
