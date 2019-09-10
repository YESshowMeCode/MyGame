using UnityEngine;
using UnityEngine.UI;
using System;

public class TestDialog : MonoBehaviour
{
    private Image m_Image = null;
    private InputField m_UserNameInputField = null;
    private Button m_CloseButton = null;
    private Toggle m_SexToggle = null;

    void Awake()
    {
        m_Image = transform.FindChild("Image").GetComponent<Image>();
        m_UserNameInputField = transform.FindChild("Image/UserNameInputField").GetComponent<InputField>();
        m_CloseButton = transform.FindChild("CloseButton").GetComponent<Button>();
        m_SexToggle = transform.FindChild("SexToggle").GetComponent<Toggle>();
    }

    void Start()
    {
        InitUIEvent();
    }

    private void InitUIEvent()
    {
        m_UserNameInputField.onEndEdit.AddListener(OnUserNameInputFieldEndEdit);
        m_CloseButton.onClick.AddListener(OnCloseButtonClick);
        m_SexToggle.onValueChanged.AddListener(OnSexToggleValueChanged);
    }

    private void OnUserNameInputFieldEndEdit(string arg0)
    {
        throw new NotImplementedException();
    }

    private void OnCloseButtonClick()
    {
        throw new NotImplementedException();
    }

    private void OnSexToggleValueChanged(bool arg0)
    {
        throw new NotImplementedException();
    }
}
