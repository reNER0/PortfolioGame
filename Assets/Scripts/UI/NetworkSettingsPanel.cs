using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;


public class NetworkSettingsPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField maximumErrorInputField;
    [SerializeField]
    private TMP_InputField additivePingInputField;
    [SerializeField]
    private TMP_InputField additiveJitterInputField;

    [SerializeField]
    private Toggle showServerStatesToggle;

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

        additivePingInputField.text = NetworkSettings.AdditivePing.ToString();
        additivePingInputField.onValueChanged.AddListener(OnAdditivePingInputField);

        additiveJitterInputField.text = NetworkSettings.AdditiveJitter.ToString();
        additiveJitterInputField.onValueChanged.AddListener(OnAdditiveJitterInputField);

        syncForceSlider.SetValueWithoutNotify(NetworkSettings.SyncForce);
        syncForceSlider.onValueChanged.AddListener(OnSyncForceInputField);

        showServerStatesToggle.SetIsOnWithoutNotify(NetworkSettings.ShowServerStates);
        showServerStatesToggle.onValueChanged.AddListener(OnShowServerStatesToggle);
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

    private void OnAdditivePingInputField(string input)
    {
        if (!int.TryParse(input, out int additivePing))
            return;

        NetworkSettings.AdditivePing = additivePing;
    }

    private void OnAdditiveJitterInputField(string input)
    {
        if (!int.TryParse(input, out int additiveJitter))
            return;

        NetworkSettings.AdditiveJitter = additiveJitter;
    }

    private void OnShowServerStatesToggle(bool isOn)
    {
        NetworkSettings.ShowServerStates = isOn;
        NetworkBus.OnShowServerStates?.Invoke(isOn);
    }

    private void OnSyncForceInputField(float input)
    {
        NetworkSettings.SyncForce = input;
    }
}
