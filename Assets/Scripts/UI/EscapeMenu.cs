using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private float showTime = 1;
    [SerializeField]
    private Button resumeButton;

    public static bool IsOpen { get; private set; }


    private void Start()
    {
        resumeButton.onClick.AddListener(Show);
        PlayerInputController.inputSystem.Inputs.Escape.performed += Show;

        Show(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        PlayerInputController.inputSystem.Inputs.Escape.performed -= Show;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Show(InputAction.CallbackContext context)
    {
        Show();
    }

    private void Show()
    {
        Show(!IsOpen);
    }

    private void Show(bool show)
    {
        IsOpen = show;

        canvasGroup.DOKill();

        if (show)
        {
            gameObject.SetActive(show);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        canvasGroup.DOFade(show ? 0 : 1, 0);
        canvasGroup.DOFade(show ? 1 : 0, showTime).OnComplete(() => gameObject.SetActive(show));
    }
}
