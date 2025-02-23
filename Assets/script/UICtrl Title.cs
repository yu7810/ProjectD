using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using I2.Loc;

public class UICtrlTitle : MonoBehaviour
{
    public GameObject settingUI;
    public TextMeshProUGUI versionUI;
    public TMP_Dropdown LanguageDropdown; // 語言選項下拉UI

    private void Start()
    {
        // 首頁版本號
        versionUI.text = Application.version;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            CallSettingUI(); // 首頁的設定頁面
    }

    public void CallSettingUI()
    {
        if (settingUI.activeSelf)
            settingUI.SetActive(false);
        else
        {
            StartLanguageDropdown();
            settingUI.SetActive(true);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // 語系選擇
    public void SelectLenguage()
    {
        int index = LanguageDropdown.value;
        string selectedOption = LanguageDropdown.options[index].text;
        string targetLanguage;

        if (selectedOption == "繁體中文")
            targetLanguage = "Chinese (Taiwan)";
        else if (selectedOption == "English")
            targetLanguage = "English";
        else if (selectedOption == "简体中文")
            targetLanguage = "Chinese (Simplified)";
        else
            return;

        if (LocalizationManager.HasLanguage(targetLanguage))
            LocalizationManager.CurrentLanguage = targetLanguage;
    }

    // 語系下拉式選單初始化
    private void StartLanguageDropdown()
    {
        if (LocalizationManager.CurrentLanguage == "English")
            LanguageDropdown.value = 0;
        else if (LocalizationManager.CurrentLanguage == "Chinese (Taiwan)")
            LanguageDropdown.value = 1;
        else if (LocalizationManager.CurrentLanguage == "Chinese (Simplified)")
            LanguageDropdown.value = 2;
    }
}
