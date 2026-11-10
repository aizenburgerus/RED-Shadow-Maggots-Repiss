using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using JSAM;
using static Facade;

public class CanteenUI : BaseGameUI
{
    [Header("Animation Properties")] [SerializeField]
    float uiTweenTime = 0.35f;

    [SerializeField] float distanceFromCamera = 0;
    [SerializeField] float canteenShakeTime = 0.25f;
    [SerializeField] int canteenShakeVibrato = 10;
    [SerializeField] float canteenScaleTime = 0.075f;
    [SerializeField] float canteenMaxSpeed = 10;
    [SerializeField] float canteenSmoothTime = 0.5f;

    [SerializeField] float filledAnimTime = 0.15f;
    [SerializeField] RectTransform filledDestination;
    [SerializeField] float filledScaleTime = 0.1f;
    [SerializeField] Ease filledEase = Ease.InOutExpo;

    [Header("Object References")] [SerializeField]
    Canvas viewportBillboardCanvas;

    [SerializeField] Camera cam;
    [SerializeField] TextMeshProUGUI canteenChargeText;
    [SerializeField] TextMeshProUGUI canteenCountText;
    [SerializeField] RectTransform canteen;
    [SerializeField] Image canteenFill;
    [SerializeField] GameObject[] canteenIcons;
    [SerializeField] Renderer canteenRenderer;
    [SerializeField] OptimizedButton cancelButton;

    private void OnEnable()
    {
        CanteenSystem.OnSetCharge += OnSetCharge;
        CanteenSystem.OnAvailableChargeChanged += OnSetCharge;

        BattleSystem.OnStartPhaseLate[BattlePhases.PlayerTurn.ToInt()] += ShowUI;
        UIManager.OnEnterSkillTargetMode += HideUI;
        UIManager.OnExitSkillTargetMode += ShowUI;
        UIManager.OnShowBattleUI += ShowUI;
        UIManager.OnHideBattleUI += OnHideBattleUI;

        UpdateCanteenCount();

        cancelButton.Hide();
    }

    private void OnDisable()
    {
        CanteenSystem.OnSetCharge -= OnSetCharge;
        CanteenSystem.OnAvailableChargeChanged -= OnSetCharge;

        BattleSystem.OnStartPhaseLate[BattlePhases.PlayerTurn.ToInt()] -= ShowUI;
        UIManager.OnEnterSkillTargetMode -= HideUI;
        UIManager.OnExitSkillTargetMode -= ShowUI;
        UIManager.OnShowBattleUI -= ShowUI;
        UIManager.OnHideBattleUI -= OnHideBattleUI;
    }

    void OnHideBattleUI()
    {
        cancelButton.Hide();
        HideUI();
    }

    public override void ShowUI()
    {
        optimizedCanvas.Show();
    }

    public override void HideUI()
    {
        optimizedCanvas.Hide();
    }

    private void OnSetCharge()
    {
        UpdateCanteenCount();
    }

    public void UpdateCanteenCount()
    {
        var count = canteenSystem.AvailableCanteens;
        canteenCountText.text = count.ToString();

        // ЗАЩИТА: Проверяем, назначен ли вообще массив иконок в инспекторе
        if (canteenIcons == null || canteenIcons.Length == 0)
        {
            Debug.LogError("[CanteenUI] Массив canteenIcons пуст или не привязан в Инспекторе Unity!");
            return;
        }

        // ИСПРАВЛЕНО: Цикл не пойдет дальше, чем есть элементов в массиве canteenIcons
        for (int i = 0; i < count; i++)
        {
            if (i >= canteenIcons.Length)
            {
                Debug.LogWarning($"[CanteenUI] Количество доступных фляг ({count}) превышает размер массива UI-иконок ({canteenIcons.Length})!");
                break; // Экстренно выходим из цикла, чтобы не вызвать IndexOutOfRangeException
            }

            if (canteenIcons[i] != null)
            {
                canteenIcons[i].SetActive(true);
            }
        }

        // Второй цикл оставляем как есть, он безопасен
        for (int i = count; i < canteenIcons.Length; i++)
        {
            if (i >= 0 && canteenIcons[i] != null)
            {
                canteenIcons[i].SetActive(false);
            }
        }
    }


    public void UseCanteen()
    {
        if (canteenSystem.AvailableCanteens <= 0 || battleSystem.ActivePlayer.CanUseSuperCrit) return;
        canteenSystem.BorrowCritCharge();
        AudioManager.PlaySound(BattleSceneSounds.CanteenUse);
        UpdateCanteenCount();
        cancelButton.Show();
    }

    public void CancelCanteenUse()
    {
        canteenSystem.CancelCanteenUse();
        AudioManager.PlaySound(BattleSceneSounds.CanteenDrop);
        UpdateCanteenCount();
        cancelButton.Hide();
    }
}