using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPromptBase : MonoBehaviour
{
    [SerializeField]
    Sprite keyboardIcon;

    [SerializeField]
    Sprite controllerIcon;

    Image icon;
    TextMeshProUGUI actionMeshText;

    PlayerController _playerControllerl;
    protected PlayerController player
    {
        get
        {
            if (_playerControllerl == null)
            {
                _playerControllerl = FindObjectOfType<PlayerController>();
            }

            return _playerControllerl;
        }
    }


    protected bool isKeyBoard = true;

    protected string actionText
    {
        get { return actionMeshText?.text ?? ""; }
        set { if (actionMeshText != null) actionMeshText.text = value; }
    }

    protected bool visible
    {
        get { return transform.GetChild(0).gameObject.activeSelf; }
        set { transform.GetChild(0).gameObject.SetActive(value); }
    }

    protected virtual void Awake()
    {
        icon = GetComponentInChildren<Image>();
        actionMeshText = GetComponentInChildren<TextMeshProUGUI>();
    }

    protected virtual void Start()
    {
        icon.sprite = keyboardIcon;
    }

    protected virtual void Update()
    {
        if (Input.anyKey)
        {
            if (CheckKeyboardInput())
            {
                isKeyBoard = true;
            }

            if (CheckJoysticButtons())
            {
                isKeyBoard = false;
            }

            icon.sprite = isKeyBoard ? keyboardIcon : controllerIcon;
        }
    }

    bool CheckJoysticButtons()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKey($"joystick button {i}"))
            {
                return true;
            }
        }

        return false;
    }

    bool CheckKeyboardInput()
    {
        return false
            || Input.GetKey(KeyCode.Space)
            || Input.GetKey(KeyCode.E)
            || Input.GetKey(KeyCode.Q)
            || Input.GetKey(KeyCode.Return)
            || Input.GetKey(KeyCode.Mouse0)
            || Input.GetKey(KeyCode.Escape);
    }
}
