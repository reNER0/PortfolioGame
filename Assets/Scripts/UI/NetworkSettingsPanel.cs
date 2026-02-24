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
    private TMP_Dropdown dropdown;

    private void SetMode(int index)
    {
        NetworkSettings.ErrorCorrectionType = (ErrorCorrectionType)index;
    }

    private void Awake()
    {
        var options = Enum.GetNames(typeof(ErrorCorrectionType)).ToList();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        dropdown.onValueChanged.AddListener(SetMode);
        SetMode(dropdown.value);

        maximumErrorInputField.text = NetworkSettings.MaximumError.ToString();
        maximumErrorInputField.onValueChanged.AddListener(OnMaximumErrorInputField);

        syncForceSlider.value = NetworkSettings.SyncForce;
        syncForceSlider.onValueChanged.AddListener(OnSyncForceInputField);
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
