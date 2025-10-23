using TMPro;
using UnityEngine;


public class NetworkSettingsPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField maximumErrorInputField;


    private void Awake()
    {
        maximumErrorInputField.text = NetworkSettings.MaximumError.ToString();
        maximumErrorInputField.onValueChanged.AddListener(OnMaximumErrorInputField);
    }


    private void OnMaximumErrorInputField(string input)
    {
        if (!int.TryParse(input, out int maximumError))
            return;

        NetworkSettings.MaximumError = maximumError;
    }
}
