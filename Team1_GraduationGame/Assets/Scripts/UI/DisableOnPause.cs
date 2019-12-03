// Code Owner: Jannik Neerdal - Optimized
using UnityEngine;

public class DisableOnPause : MonoBehaviour
{
    private void Start()
    {
        UIMenu[] menus = Resources.FindObjectsOfTypeAll<UIMenu>();
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].gamePauseState += SetActive;
        }
    }

    private void SetActive(bool activity)
    {
        gameObject.SetActive(!activity);
    }
}
