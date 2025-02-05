using UnityEngine;
using UnityEngine.UI;

public class MobileUI : MonoBehaviour
{
    public Button invenButton;
    public Button optionButton;
    
    private void Awake()
    {
        MobileUIOnClick();
    }

    private void MobileUIOnClick()
    {
        invenButton.onClick.AddListener(InvenButtonClicked);
        optionButton.onClick.AddListener(OptionButtonClicked);
    }
    
    private void InvenButtonClicked()
    {
        UIManager.Instance.OpenInventoryPanel();
    }

    private void OptionButtonClicked()
    {
        UIManager.Instance.OpenOptionPanel();
    }
}
