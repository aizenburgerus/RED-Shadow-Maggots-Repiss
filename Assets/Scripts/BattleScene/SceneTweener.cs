using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using static Facade;

public class SceneTweener : BasicSingleton<SceneTweener>
{
    [SerializeField] Transform worldCenter;

    [SerializeField] Vector3 characterDestinationOffset = new Vector3();

    [Header("Skill Tween Props")]
    [SerializeField] float effectTickTime = 2;
    public float EffectTickTime => effectTickTime;
    [SerializeField] float postEffectTickTime = 1;
    public float PostEffectTickTime => postEffectTickTime;

    [SerializeField] float skillEffectApplyDelay = 0.5f;
    public float SkillEffectApplyDelay => skillEffectApplyDelay;

    [Header("Character Tween Props")]
    [SerializeField] float characterTweenBackDelay = 0.15f;
    [SerializeField] float characterTweenTime = 0.5f;
    [SerializeField] float postCharacterTweenWaitTime = 0.5f;

    [SerializeField] float tweenTime = 0;

    [SerializeField] float camTweenTime = 0;

    [SerializeField] float returnTweenDelay = 1;

    [SerializeField] float waveTransitionDelay = 1;

    [Header("Camera")]
    [SerializeField] Camera sceneCamera;
    public Camera SceneCamera => sceneCamera;

    [SerializeField] CinemachineVirtualCamera playerCam;
    /// <summary>
    /// The player-centric camera
    /// </summary>
    public CinemachineVirtualCamera PlayerCamera => playerCam;
    [SerializeField] CinemachineSmoothPath playerPath;

    [SerializeField] CinemachineVirtualCamera lerpCam;
    public CinemachineVirtualCamera LerpCamera => lerpCam;

    [SerializeField] CinemachineVirtualCamera enemyCam;
    [SerializeField] CinemachineSmoothPath enemyPath;
    CinemachineTrackedDolly enemyDolly;

    [SerializeField] CinemachineVirtualCamera specialCam;

    [SerializeField] CinemachineVirtualCamera playerTeamCam;
    public CinemachineVirtualCamera PlayerTeamCam => playerTeamCam;

    CinemachineComposer composer;
    CinemachineTrackedDolly playerDolly;

    Animator anim;

    public static Action OnBattleTransition;
    public static Action OnSkillUntween;
    public static Action OnBattleEntered;

    private void OnEnable()
    {
        GlobalEvents.OnCharacterUseSuperCritical += OnCharacterUseSuperCritical;
        //GlobalEvents.OnCharacterFinishSuperCritical += OnCharacterFinishSuperCritical;
        ui.OffenseBar.OnExecuteQuickTime += OnExecuteQuickTime;
    }

    private void OnDisable()
    {
        GlobalEvents.OnCharacterUseSuperCritical -= OnCharacterUseSuperCritical;
        //GlobalEvents.OnCharacterFinishSuperCritical -= OnCharacterFinishSuperCritical;

        // References may be lost on Application Quit
        if (ui)
        {
            if (ui.OffenseBar)
            {
                ui.OffenseBar.OnExecuteQuickTime -= OnExecuteQuickTime;
            }
        }
    }

    private IEnumerator Start()
    {
        anim = GetComponent<Animator>();

        yield return null;

        composer = playerCam.GetCinemachineComponent<CinemachineComposer>();
        
        //EnterBattle();
    }

    // Update is called once per frame
    void Update()
    {
        // Логика для игрока
        if (!playerDolly && playerCam != null)
        {
            playerDolly = playerCam.GetCinemachineComponent<CinemachineTrackedDolly>();
        }

        if (playerDolly)
        {
            playerDolly.m_PathPosition = lerpValue;
        }

        // Исправленная логика для врага
        if (!enemyDolly && enemyCam != null)
        {
            enemyDolly = enemyCam.GetCinemachineComponent<CinemachineTrackedDolly>();

            // Если попытались найти, но на этом кадре всё еще null
            if (!enemyDolly)
            {
                Debug.LogWarning("Cannot find enemyDolly! Somehow that invisible cm object doesn't instantiate!");
            }
        }

        // Если enemyDolly успешно существует (нашелся ранее или только что)
        if (enemyDolly)
        {
            enemyDolly.m_PathPosition = lerpValue;
        }
    }

    public void EnterBattleAfterDelay() => Invoke(nameof(EnterBattle), waveTransitionDelay);

    public void EnterBattle()
    {
        screenEffects.FadeFromBlack(ScreenEffects.EffectType.Fullscreen);

        bool useSpecialCam = playerDataManager.LoadedData.InBattle ?
            battleStateManager.LoadedData.UseSpecialCam[battleStateManager.LoadedData.WaveCount] :
            waveManager.CurrentWave.UseSpecialCam;

        playerCam.enabled = !useSpecialCam;
        specialCam.enabled = useSpecialCam;

        OnBattleTransition?.Invoke();
        OnBattleEntered?.Invoke();
    }

    float lerpValue;

    readonly Dictionary<Transform, Vector3> savedPositions = new Dictionary<Transform, Vector3>();
    readonly Dictionary<BaseCharacter, Quaternion> savedRotations = new Dictionary<BaseCharacter, Quaternion>();

    public void RememberPosition(BaseCharacter character)
    {
        if (!savedPositions.ContainsKey(character.transform))
            savedPositions.Add(character.transform, character.transform.position);
        if (character.CharacterMesh && !savedRotations.ContainsKey(character))
            savedRotations.Add(character, character.CharacterMesh.transform.rotation);
    }

    void RememberPosition(Transform character)
    {
        if (!savedPositions.ContainsKey(character)) savedPositions.Add(character, character.position);
    }
    public void MeleeTweenTo(Transform attacker, Transform target)
    {
        RememberPosition(attacker);
        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                enemyCam.enabled = false;
                playerCam.m_LookAt = attacker;
                attacker.transform.DOMove(target.position + characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic);
                //playerPath.transform.DOKill();
                //playerPath.transform.DOMove(target.position + characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.enabled = true;
                enemyCam.m_LookAt = attacker;
                attacker.transform.DOMove(target.position - characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic);
                enemyPath.transform.DOKill();
                enemyPath.transform.DOMove(target.position - characterDestinationOffset, characterTweenTime).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late);
                break;
        }

        var newTween = DOTween.To(() => lerpValue, x => lerpValue = x, 2, camTweenTime).SetEase(Ease.OutCubic);
    }

    public void MeleeMoveTo(Transform attacker, Transform target)
    {
        RememberPosition(attacker);
        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                enemyCam.enabled = false;
                playerCam.m_LookAt = attacker;
                attacker.transform.position = target.position + characterDestinationOffset;
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.enabled = true;
                enemyCam.m_LookAt = attacker;
                attacker.transform.position = target.position - characterDestinationOffset;
                enemyPath.transform.DOKill();
                enemyPath.transform.position = target.position - characterDestinationOffset;
                break;
        }
    }

    public void RangedTweenTo(Transform attacker, Transform target)
    {
        switch (BattleSystem.Instance.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                playerCam.m_LookAt = battleSystem.ActivePlayer.transform;
                attacker.LookAt(target.position);
                break;
            case BattlePhases.EnemyTurn:
                if (!BaseCharacter.IncomingAttack.isAOE)
                    attacker.LookAt(target.position);
                break;
        }
    }

    public void SkillTween(Transform user, float skillUseTime)
    {
        //playerCam.m_LookAt = user;
        //playerPath.transform.position = user.position;
        //DOTween.To(() => lerpValue, x => lerpValue = x, 1.25f, skillUseTime).SetEase(Ease.OutCubic);
    }

    public void SkillUntween()
    {
        lerpCam.enabled = false;
        OnSkillUntween?.Invoke();
        //playerCam.m_LookAt = worldCenter;
        //playerPath.transform.position = worldCenter.position;
        //DOTween.To(() => lerpValue, x => lerpValue = x, 0, 1.5f);
    }

    private void OnCharacterUseSuperCritical(BaseCharacter obj)
    {
        Transform target = null;
        Vector3 offset = Vector3.zero;
        CinemachineVirtualCamera cam;
        CinemachineSmoothPath path;
        switch (battleSystem.CurrentPhase)
        {
            case BattlePhases.PlayerTurn:
                enemyCam.enabled = false;
                path = playerPath;
                cam = playerCam;
                target = battleSystem.ActiveEnemy.transform;
                offset = characterDestinationOffset;
                break;
            case BattlePhases.EnemyTurn:
                enemyCam.enabled = false;
                path = enemyPath;
                cam = enemyCam;
                target = battleSystem.ActivePlayer.transform;
                offset = -characterDestinationOffset;
                break;
            default:
                return;
        }

        switch (obj.Reference.superCritRange)
        {
            case AttackRange.CloseRange:
                //cam.m_LookAt = obj.transform;
                RememberPosition(obj);
                Vector3 position = target.position + offset;
                obj.transform.position = position;
                //path.transform.position = position;
                //lerpValue = 2;
                break;
            case AttackRange.LongRange:
                obj.CharacterMesh.transform.LookAt(target.position);
                break;
        }
    }

    public void DisableAnim()
    {
        anim.enabled = false;
    }

    public void ReturnToPosition()
    {
        ReturnToPosition(CurrentAttacker());
    }

    BaseCharacter CurrentAttacker() => battleSystem.CurrentPhase == BattlePhases.PlayerTurn
        ? (BaseCharacter)battleSystem.ActivePlayer : battleSystem.ActiveEnemy;

    public void ReturnToPosition(BaseCharacter attacker) => StartCoroutine(CompleteAttack(attacker, true));

    private void OnExecuteQuickTime()
    {
        // Look at enemy whose getting hit by the attack
        if (BaseCharacter.IncomingAttack.attackRange == AttackRange.LongRange)
        {
            playerCam.m_LookAt = battleSystem.ActiveEnemy.transform;
        }
    }

    public void RotateBackInstantly()
    {
        battleSystem.ActiveEnemy.CharacterMesh.transform.eulerAngles = new Vector3(0, 90, 0);
        battleSystem.ActivePlayer.CharacterMesh.transform.eulerAngles = new Vector3(0, -90, 0);
    }

    public void RotateBack() => RotateBack(CurrentAttacker());

    public void RotateBack(BaseCharacter attacker) => StartCoroutine(CompleteAttack(attacker, false));

    IEnumerator CompleteAttack(BaseCharacter attacker, bool moveBack)
    {
        var phase = battleSystem.CurrentPhase;
        var target = battleSystem.OpposingCharacter;
        var camera = phase == BattlePhases.PlayerTurn ? playerCam : enemyCam;
        camera.enabled = true;
        if (attacker) camera.m_LookAt = attacker.transform;

        yield return new WaitForSeconds(returnTweenDelay);

        if (attacker)
        {
            var root = attacker.transform;
            if (savedPositions.TryGetValue(root, out var position) &&
                (moveBack || (root.position - position).sqrMagnitude > 0.0001f))
            {
                root.DOKill();
                attacker.PlayReturnAnimation();
                var tween = root.DOMove(position, characterTweenTime)
                    .SetDelay(characterTweenBackDelay).SetEase(Ease.OutCubic);
                yield return tween.WaitForCompletion();
                // A killed/replaced tween must not leave the actor between its slots.
                if (attacker) root.position = position;
            }

            if (attacker && attacker.CharacterMesh)
            {
                attacker.CharacterMesh.transform.DOKill();
                if (savedRotations.TryGetValue(attacker, out var rotation))
                    attacker.CharacterMesh.transform.rotation = rotation;
                attacker.ReturnToIdle();
            }
        }

        if (target && target.CharacterMesh)
            target.CharacterMesh.transform.eulerAngles = target.IsPlayer()
                ? new Vector3(0, -90, 0) : new Vector3(0, 90, 0);

        lerpValue = 0;
        playerCam.m_LookAt = worldCenter;
        enemyCam.m_LookAt = worldCenter;
        enemyCam.enabled = false;
        playerCam.enabled = true;

        yield return new WaitForSeconds(postCharacterTweenWaitTime);

        // Resolve the actor that started the return, not whichever target is selected later.
        if (battleSystem.CurrentPhase == phase && CurrentAttacker() == attacker)
            battleSystem.EndTurn();
    }

    public void TeleportBackToPosition()
    {
        var attacker = CurrentAttacker();
        if (attacker) TeleportBackToPosition(attacker.transform);
    }

    public void TeleportBackToPosition(Transform attacker)
    {
        if (attacker && savedPositions.TryGetValue(attacker, out var position))
        {
            attacker.DOKill();
            attacker.position = position;
        }
    }

    public void WaveClearSequence()
    {
        OnBattleTransition?.Invoke();
        anim.enabled = true;
        screenEffects.FadeToBlack(ScreenEffects.EffectType.Fullscreen, 1.5f);
        anim.SetTrigger("OpenGate");
    }

    public void MakePlayersWalk()
    {
        // Dummy anim event method
    }

    public void GateEntered()
    {
        BattleSystem.Instance.InitiateNextBattle();
    }
}
