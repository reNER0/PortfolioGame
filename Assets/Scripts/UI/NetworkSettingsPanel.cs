using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NetworkSettingsPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField maximumErrorInputField;

    [SerializeField]
    private Slider syncForceSlider;


    [SerializeField]
    private TMP_Dropdown clientSidePredictionDropdown;
    [SerializeField] 
    private TMP_Dropdown errorCorrectionDropdown;


    private void Awake()
    {
        SetupDropdowns();

        maximumErrorInputField.text = NetworkSettings.MaximumError.ToString();
        maximumErrorInputField.onValueChanged.AddListener(OnMaximumErrorInputField);

        syncForceSlider.SetValueWithoutNotify(NetworkSettings.SyncForce);
        syncForceSlider.onValueChanged.AddListener(OnSyncForceInputField);
    }


    private void SetupDropdowns() 
    {
        var options = Enum.GetNames(typeof(ErrorCorrectionType)).ToList();

        clientSidePredictionDropdown.ClearOptions();
        errorCorrectionDropdown.ClearOptions();

        clientSidePredictionDropdown.AddOptions(options);
        errorCorrectionDropdown.AddOptions(options);

        clientSidePredictionDropdown.onValueChanged.AddListener(OnClientSidePredictionDropdown);
        errorCorrectionDropdown.onValueChanged.AddListener(OnErrorCorrectionDropdown);

        clientSidePredictionDropdown.SetValueWithoutNotify((int)NetworkSettings.ClientSidePredictionType);
        errorCorrectionDropdown.SetValueWithoutNotify((int)NetworkSettings.ErrorCorrectionType);
    }


    private void OnClientSidePredictionDropdown(int index)
    {
        NetworkSettings.ClientSidePredictionType = (ErrorCorrectionType)index;
    }

    private void OnErrorCorrectionDropdown(int index)
    {
        NetworkSettings.ErrorCorrectionType = (ErrorCorrectionType)index;
    }

    private void OnMaximumErrorInputField(string input)
    {
        if (!int.TryParse(input, out int maximumError))
            return;

        NetworkSettings.MaximumError = maximumError;
    }

    private void OnSyncForceInputField(float input)
    {
        NetworkSettings.SyncForce = input;
    }
}
