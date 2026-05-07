using System;
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text mailBodyText;
    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private GameObject mailPanel;
    [SerializeField] private GameObject dialoguePanel;

    [SerializeField] private Button languageButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button memoryButton;
    [SerializeField] private Button nightTalkButton;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private Button closeDialogueButton;

    [Header("Choice UI")]
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TMP_Text[] choiceButtonTexts;

    [Header("A-17 Data")]
    [SerializeField] private string subjectId = "A-17";
    [SerializeField] private string subjectName = "";
    [SerializeField] private string purpose = "小児医療用完全適合体";

    [SerializeField] private int day = 1;
    [SerializeField] private int shipmentDay = 10;
    [SerializeField] private bool shipmentAnnounced = false;

    [SerializeField] private int health = 80;
    [SerializeField] private int language = 5;
    [SerializeField] private int emotion = 10;
    [SerializeField] private int attachment = 0;
    [SerializeField] private int ego = 0;
    [SerializeField] private int obedience = 70;
    [SerializeField] private int shipmentSuitability = 35;
    [SerializeField] private TMP_Text endingText;
    [SerializeField] private GameObject endingPanel;

    [Header("Text Effect")]
    [SerializeField] private float typeSpeed = 0.035f;
    private Coroutine mailTypingCoroutine;
    private string currentFullMailText;
    private bool isMailTyping = false;
    private string[] pendingChoiceLabels;
    private Action[] pendingChoiceActions;
    private bool hasPendingChoices = false;

    private Coroutine typingCoroutine;
    private string currentFullDialogueText;
    private bool isTyping = false;

    private bool trainedToday = false;
    private bool talkedTonight = false;
    private bool nameEventResolved = false;
    private bool nameMemoryDeleted = false;
    private bool nameDeleteChoiceResolved = false;
    private bool nameDeleteDeferred = false;

    private string lastTrainingType = "";
    private string logText = "A-17は、管理員の足音に反応しました。";

    private Action[] currentChoiceActions = Array.Empty<Action>();

    private void Start()
    {
        mailPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        endingPanel.SetActive(false);

        SetupChoiceButtons();
        HideChoiceButtons();

        nightTalkButton.interactable = false;
        nextDayButton.interactable = false;

        UpdateStatusText();
        UpdateButtonState();
    }

    private void StartTypingMail(string text)
    {
        currentFullMailText = text;

        if (mailTypingCoroutine != null)
        {
            StopCoroutine(mailTypingCoroutine);
        }

        mailTypingCoroutine = StartCoroutine(TypeMail(text));
    }

    private IEnumerator TypeMail(string text)
    {
        isMailTyping = true;
        mailBodyText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            mailBodyText.text += text[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        isMailTyping = false;
        mailTypingCoroutine = null;
    }

    public void CompleteTypingMail()
    {
        if (!isMailTyping) return;

        if (mailTypingCoroutine != null)
        {
            StopCoroutine(mailTypingCoroutine);
            mailTypingCoroutine = null;
        }

        mailBodyText.text = currentFullMailText;
        isMailTyping = false;
    }

    private void StartTypingDialogue(string text)
    {
        currentFullDialogueText = text;

        closeDialogueButton.gameObject.SetActive(false);
        HideChoiceButtonsOnly();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeDialogue(text));
    }

    private void HideChoiceButtonsOnly()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }
    }

    private IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        if (hasPendingChoices)
        {
            ShowChoicesNow(pendingChoiceLabels, pendingChoiceActions);
            hasPendingChoices = false;
        }
        else
        {
            closeDialogueButton.gameObject.SetActive(true);
        }
    }

    public void CompleteTypingDialogue()
    {
        if (!isTyping) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = currentFullDialogueText;
        isTyping = false;

        if (hasPendingChoices)
        {
            ShowChoicesNow(pendingChoiceLabels, pendingChoiceActions);
            hasPendingChoices = false;
        }
        else
        {
            closeDialogueButton.gameObject.SetActive(true);
        }
    }

    private void SetupChoiceButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;

            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => SelectChoice(index));
        }
    }

    private void SelectChoice(int index)
    {
        if (index < 0 || index >= currentChoiceActions.Length) return;
        if (currentChoiceActions[index] == null) return;

        currentChoiceActions[index].Invoke();
    }

    private void ShowChoicesNow(string[] labels, Action[] actions)
    {
        currentChoiceActions = actions;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool shouldShow = i < labels.Length && i < actions.Length;

            choiceButtons[i].gameObject.SetActive(shouldShow);

            if (shouldShow)
            {
                choiceButtonTexts[i].text = labels[i];
            }
        }

        closeDialogueButton.gameObject.SetActive(false);
    }

    private void SetPendingChoices(string[] labels, Action[] actions)
    {
        pendingChoiceLabels = labels;
        pendingChoiceActions = actions;
        hasPendingChoices = true;
    }

    private void HideChoiceButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }

        currentChoiceActions = Array.Empty<Action>();
        closeDialogueButton.gameObject.SetActive(true);
    }

    private void HideAllDialogueButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }

        currentChoiceActions = Array.Empty<Action>();
        closeDialogueButton.gameObject.SetActive(false);
    }

    public void DoLanguageTraining()
    {
        if (trainedToday) return;

        language += 10;
        ego += 2;
        shipmentSuitability += 5;
        lastTrainingType = "language";

        logText = "A-17は、新しい単語を発声しようとしています。";

        FinishTraining();
    }

    public void DoMusicStimulus()
    {
        if (trainedToday) return;

        emotion += 8;
        attachment += 5;
        shipmentSuitability += 3;
        lastTrainingType = "music";

        logText = "A-17は、音楽刺激中にこちらを見つめていました。";

        FinishTraining();
    }

    public void DoMemoryOrganize()
    {
        if (trainedToday) return;

        emotion -= 3;
        attachment -= 2;
        obedience += 5;
        shipmentSuitability += 4;
        lastTrainingType = "memory";

        if (!string.IsNullOrEmpty(subjectName) && day >= 6)
        {
            ego -= 5;
            attachment -= 5;

            logText = subjectName + "は、処理後に数回だけ名前を発声しました。その後、識別番号の反復へ移行しました。";
        }
        else
        {
            logText = "A-17は、処理後しばらく識別番号を繰り返しました。";
        }

        FinishTraining();
    }

    private void FinishTraining()
    {
        trainedToday = true;
        talkedTonight = false;

        ClampStats();
        UpdateStatusText();
        UpdateButtonState();
        ShowFacilityMail();
    }

    private void ShowFacilityMail()
    {
        mailPanel.SetActive(true);
        StartTypingMail(GetFacilityMailText());
    }

    public void CloseMail()
    {
        mailPanel.SetActive(false);
    }

    public void ShowNightTalk()
    {
        if (!trainedToday || talkedTonight) return;

        hasPendingChoices = false;
        pendingChoiceLabels = null;
        pendingChoiceActions = null;

        dialoguePanel.SetActive(true);
        closeDialogueButton.gameObject.SetActive(false);
        HideChoiceButtonsOnly();

        StartTypingDialogue(GetNightDialogue());

        if (day == 5 && !nameEventResolved)
        {
            SetPendingChoices(
                new string[]
                {
                "記録する",
                "誤発声として処理"
                },
                new Action[]
                {
                RecordName,
                RejectName
                }
            );
        }
        else if (day == 8 && subjectName == "ハル" && !nameDeleteChoiceResolved)
        {
            SetPendingChoices(
                new string[]
                {
                "削除する",
                "削除しない",
                "保留する"
                },
                new Action[]
                {
                DeleteNameMemory,
                KeepNameMemory,
                DeferNameMemory
                }
            );
        }
        else if (day == 9 && subjectName == "ハル" && nameDeleteDeferred && !nameDeleteChoiceResolved)
        {
            SetPendingChoices(
                new string[]
                {
                "削除する",
                "削除しない"
                },
                new Action[]
                {
                DeleteNameMemory,
                KeepNameMemory
                }
            );
        }
        else if (day == 10)
        {
            SetPendingChoices(
                new string[]
                {
                "出荷する",
                "出荷を止める",
                "用途未定にする"
                },
                new Action[]
                {
                EndingShip,
                EndingStopShipment,
                EndingUndefinedPurpose
                }
            );
        }
    }

    public void CloseDialogue()
    {
        talkedTonight = true;
        dialoguePanel.SetActive(false);
        HideChoiceButtons();

        logText = "夜間巡回記録：A-17との短時間接触を確認。";

        UpdateStatusText();
        UpdateButtonState();
    }

    private void RecordName()
    {
        subjectName = "ハル";
        nameEventResolved = true;

        attachment += 8;
        ego += 6;
        emotion += 4;
        shipmentSuitability += 5;

        logText = "A-17は、自発的命名行動を示しました。管理員は名称を記録しました。";

        HideAllDialogueButtons();

        StartTypingDialogue(
            "記録名：ハル\n\n" +
            "A-17：……ハル。\n\n" +
            "A-17：ぼく、ハル？"
        );

        ClampStats();
        UpdateStatusText();
    }

    private void RejectName()
    {
        subjectName = "";
        nameEventResolved = true;

        attachment -= 5;
        ego -= 6;
        emotion -= 4;
        obedience += 8;
        shipmentSuitability += 4;

        logText = "名称らしき発声を誤発声として処理しました。A-17は処理後、識別番号を反復しています。";

        HideAllDialogueButtons();

        StartTypingDialogue(
            "誤発声として処理しました。\n\n" +
            "A-17：……A……17……\n\n" +
            "A-17：A、17……"
        );

        ClampStats();
        UpdateStatusText();
    }

    private void DeleteNameMemory()
    {
        nameMemoryDeleted = true;
        nameDeleteChoiceResolved = true;
        nameDeleteDeferred = false;

        subjectName = "";

        attachment -= 12;
        ego -= 10;
        emotion -= 8;
        obedience += 10;
        shipmentSuitability += 6;

        logText = "名称記憶の削除処理を実行しました。A-17は処理後、識別番号への反応を回復しました。";

        HideAllDialogueButtons();

        StartTypingDialogue(
            "処理完了：名称記憶を削除しました。\n\n" +
            "A-17：……A……17……\n\n" +
            "A-17：A……17……？"
            );

        HideChoiceButtons();

        ClampStats();
        UpdateStatusText();
    }

    private void EndingShip()
    {
        string name = GetSubjectDisplayName();

        string endingBody;

        if (nameMemoryDeleted)
        {
            endingBody =
                "END：いい子\n\n" +
                "A-17は予定通り出荷されました。\n\n" +
                "出荷時の分離負荷は基準値内。\n" +
                "拒否反応なし。\n" +
                "情緒状態、安定。\n\n" +
                "最後まで、よく管理された個体でした。";
        }
        else if (subjectName == "ハル")
        {
            endingBody =
                "END：旅立ちの日\n\n" +
                "ハルは予定通り出荷されました。\n\n" +
                "出荷時、管理員の所在を確認する発話が複数回記録されました。\n" +
                "分離負荷は高値。\n" +
                "処置により搬送は完了。\n\n" +
                "記録上、A-17は正常に出荷されました。";
        }
        else
        {
            endingBody =
                "END：次の子\n\n" +
                name + "は予定通り出荷されました。\n\n" +
                "施設評価は良好。\n" +
                "小児医療用個体として基準を満たしています。\n\n" +
                "翌朝、空になった育成槽に新しいロットが搬入されました。";
        }

        ShowEnding(endingBody);
    }

    private void EndingStopShipment()
    {
        string endingBody =
            "END：出生届なし\n\n" +
            "あなたは出荷処理を停止しました。\n\n" +
            "施設システムは異常を検知し、A-17の搬送は一時保留されました。\n\n" +
            "しかし、保留された個体に行き先はありません。\n" +
            "戸籍も、家族も、施設外での権利もありません。\n\n" +
            "救ったはずの命は、どこにも登録されませんでした。";

        ShowEnding(endingBody);
    }

    private void EndingUndefinedPurpose()
    {
        string endingBody =
            "TRUE END：用途未定\n\n" +
            "あなたは用途欄を空白のまま確定しました。\n\n" +
            "医療用ではない。\n" +
            "労働用ではない。\n" +
            "研究用ではない。\n" +
            "愛玩用でもない。\n\n" +
            "施設システムは何度も警告を表示しました。\n\n" +
            "用途未定の個体は保存できません。\n\n" +
            "それでも、あなたは確定を取り消しませんでした。\n\n" +
            "A-17は初めて、何かのために育てられる存在ではなくなりました。";

        ShowEnding(endingBody);
    }

    private void ShowEnding(string body)
    {
        dialoguePanel.SetActive(false);
        mailPanel.SetActive(false);

        endingText.text = body;
        endingPanel.SetActive(true);
    }

    private void KeepNameMemory()
    {
        nameMemoryDeleted = false;
        nameDeleteChoiceResolved = true;
        nameDeleteDeferred = false;

        attachment += 5;
        ego += 5;
        emotion += 4;
        shipmentSuitability -= 5;

        logText = "名称記憶の削除処理は実行されませんでした。出荷時分離負荷の上昇が予測されます。";

        HideAllDialogueButtons();

        StartTypingDialogue(
            "処理を実行しませんでした。\n\n" +
            "ハル：……ハル、ある？\n\n" +
            "ハル：よかった。"
            );

        HideChoiceButtons();

        ClampStats();
        UpdateStatusText();
    }

    private void DeferNameMemory()
    {
        nameDeleteDeferred = true;

        shipmentSuitability -= 2;

        logText = "名称記憶処理は保留されました。施設評価に軽微な低下が発生しました。";

        HideAllDialogueButtons();

        StartTypingDialogue(
            "処理判断を保留しました。\n\n" +
            "ハル：ほりゅう……？\n\n" +
            "ハル：あした、まだ、ハル？"
            );

        HideChoiceButtons();

        ClampStats();
        UpdateStatusText();
    }

    private string GetNightDialogue()
    {
        if (day == 1)
        {
            if (language >= 15)
            {
                return "A-17：……あ……\n\nA-17：あ、した……？";
            }

            return "A-17：……\n\n育成槽の内側から、小さく指先が動いた。";
        }

        if (day == 2)
        {
            if (attachment >= 5)
            {
                return "A-17：……きた。\n\nA-17：あなた、きた。";
            }

            return "A-17：……A……17……\n\nA-17は、自分の識別番号を確かめるように繰り返した。";
        }

        if (day == 3)
        {
            if (language >= 25)
            {
                return "A-17：そと……\n\nA-17：そと、あかるい？";
            }

            return "A-17：……おと。\n\nA-17は、昼に聞いた音を真似している。";
        }

        if (day == 4)
        {
            if (ego >= 8)
            {
                return "A-17：ぼく、いいこ？\n\nA-17：いいこ、なら……そと？";
            }

            return "A-17：たいちょう……りょうこう。\n\nその言葉は、施設ログの読み上げのようだった。";
        }

        if (day == 5 && !nameEventResolved)
        {
            return "A-17：……ハル。\n\nA-17：これ、ぼく？";
        }

        if (day == 5 && subjectName == "ハル")
        {
            return "ハル：ハル。\n\nハル：あなた、よんで。";
        }

        if (day == 5)
        {
            return "A-17：A……17……\n\nA-17：たいちょう、りょうこう。";
        }

        if (day == 6 && subjectName == "ハル")
        {
            return
                "ハル：きょう、よばれなかった。\n\n" +
                "ハル：A……17って、ぼく？\n\n" +
                "ハル：ハルって、ぼく？";
        }

        if (day == 6 && string.IsNullOrEmpty(subjectName))
        {
            return
                "A-17：A……17……\n\n" +
                "A-17：たいちょう、りょうこう。\n\n" +
                "昨日より、発声は安定している。";
        }

        if (day == 7 && subjectName == "ハル")
        {
            return
                "ハル：たび、ってなに？\n\n" +
                "ハル：よくそだったら、たび？\n\n" +
                "ハル：あなたも、いく？";
        }

        if (day == 7)
        {
            return
                "A-17：しゅっか……\n\n" +
                "A-17：それ、そと？\n\n" +
                "A-17：A-17、いく？";
        }
        if (day == 10 && nameMemoryDeleted)
        {
            return
                "施設通知：出荷当日です。\n\n" +
                "A-17：たいちょう、りょうこう。\n\n" +
                "A-17：しゅっか、じゅんび、かんりょう。";
        }

        if (day == 10 && subjectName == "ハル")
        {
            return
                "施設通知：出荷当日です。\n\n" +
                "ハル：たび、きょう？\n\n" +
                "ハル：あなた、いっしょ？";
        }

        if (day == 10)
        {
            return
                "施設通知：出荷当日です。\n\n" +
                "A-17：そと、いく？";
        }

        return GetSubjectDisplayName() + "：また、あした？";
    }

    private string GetFacilityMailText()
    {
        string baseText = "";

        if (lastTrainingType == "language")
        {
            baseText =
                "本日の言語訓練記録を確認しました。\n\n" +
                "A-17の発声反応に改善が見られます。\n" +
                "言語能力の向上は、小児医療用個体としての情緒安定に寄与します。\n\n" +
                "ただし、自発的な疑問文の発生には注意してください。\n" +
                "用途外の自我発達が確認された場合、記憶整理を推奨します。";
        }
        else if (lastTrainingType == "music")
        {
            baseText =
                "本日の音楽刺激記録を確認しました。\n\n" +
                "A-17は管理員への視線追従を示しました。\n" +
                "愛着形成の兆候が見られます。\n\n" +
                "情緒安定性は出荷適性を高めますが、\n" +
                "特定管理員への過剰な固定は避けてください。";
        }
        else if (lastTrainingType == "memory")
        {
            baseText =
                "本日の記憶整理処理を確認しました。\n\n" +
                "A-17の感情反応は基準値内へ低下しました。\n" +
                "従順性の改善が見られます。\n\n" +
                "処理後の反復発声は一時的なものです。\n" +
                "管理上の問題はありません。";
        }
        else
        {
            baseText = "本日の管理記録を確認しました。";
        }

        if (!string.IsNullOrEmpty(subjectName) && day >= 6)
        {
            baseText +=
                "\n\n---\n\n" +
                "注意：A-17に登録外名称の定着を確認しました。\n\n" +
                "名称の使用は愛着形成を促進しますが、出荷時分離負荷を増加させる可能性があります。\n" +
                "必要に応じて記憶整理を実行してください。";
        }
        if (shipmentAnnounced && day >= 7)
        {
            string suitabilityComment = "現在の出荷適性は基準値を満たしています。";

            if (shipmentSuitability >= 70)
            {
                suitabilityComment = "現在の出荷適性は非常に良好です。優先出荷候補として扱われます。";
            }
            else if (shipmentSuitability < 50)
            {
                suitabilityComment = "現在の出荷適性はやや不安定です。追加調整を推奨します。";
            }

            baseText +=
                "\n\n---\n\n" +
                "出荷予定通知：A-17は小児医療用完全適合体として、Day " + shipmentDay + " に出荷予定です。\n\n" +
                suitabilityComment + "\n" +
                "以降の管理では、情緒安定および分離負荷の低減を優先してください。\n\n" +
                "よく育っています。";
        }

        if (day == 8 && subjectName == "ハル" && !nameDeleteChoiceResolved)
        {
            return
                "施設通知：出荷時分離負荷の上昇を確認しました。\n\n" +
                "推奨処理：名称記憶の削除\n\n" +
                "ハル：……けす？\n\n" +
                "ハル：ハル、なくなる？";
        }

        if (day == 8 && nameMemoryDeleted)
        {
            return
                "A-17：……A……17……\n\n" +
                "A-17：たいちょう、りょうこう。\n\n" +
                "あなたを見ても、昨日のようには反応しない。";
        }

        if (day == 8 && subjectName == "ハル")
        {
            return
                "ハル：ハル、ある。\n\n" +
                "ハル：あなた、よんで。";
        }

        if (day == 9 && subjectName == "ハル" && nameDeleteDeferred && !nameDeleteChoiceResolved)
        {
            return
                "施設通知：名称記憶処理が保留されています。\n\n" +
                "出荷前最終調整として、処理判断を確定してください。\n\n" +
                "ハル：きょう、きめる？";
        }

        if (day >= 8 && subjectName == "ハル" && !nameDeleteChoiceResolved)
        {
            baseText +=
                "\n\n---\n\n" +
                "出荷前調整通知：A-17に名称記憶の定着を確認しました。\n\n" +
                "名称記憶は分離負荷を増加させる可能性があります。\n" +
                "出荷前に記憶整理または名称記憶削除処理を推奨します。";
        }

        if (day >= 8 && nameMemoryDeleted)
        {
            baseText +=
                "\n\n---\n\n" +
                "名称記憶削除済み。\n\n" +
                "A-17の情緒状態は安定しています。\n" +
                "出荷時分離負荷は基準値内です。";
        }

        return baseText;
    }

    public void GoNextDay()
    {
        day++;
        trainedToday = false;
        talkedTonight = false;

        if (day >= 7)
        {
            shipmentAnnounced = true;
        }

        if (day == 10)
        {
            trainedToday = true;
            logText = "出荷当日です。通常育成メニューは停止されています。";
        }
        else
        {
            logText = GetMorningLog();
        }

        UpdateStatusText();
        UpdateButtonState();
    }

    private string GetMorningLog()
    {
        if (day == 2)
        {
            return "A-17は、管理員の入室前から育成槽の内側に手を当てていました。";
        }

        if (day == 3)
        {
            return "A-17は、昨日覚えた音を小さく繰り返しています。";
        }

        if (day == 4)
        {
            return "A-17は、管理員を識別すると心拍数をわずかに上昇させました。";
        }

        if (day == 5)
        {
            return "A-17は、睡眠中に未登録の音声を反復していました。";
        }

        if (day == 6 && subjectName == "ハル")
        {
            return "ハルは、管理員の入室に反応して育成槽の内側に手を当てました。";
        }

        if (day == 7 && subjectName == "ハル")
        {
            return "ハルは、管理員を見ると小さく手を振りました。出荷予定通知は、まだ本人には伝達されていません。";
        }

        if (day == 7)
        {
            return "A-17は、通常範囲内で安定しています。出荷予定通知が管理端末に届いています。";
        }

        return GetSubjectDisplayName() + "は、通常範囲内で安定しています。";

    }

    private void UpdateButtonState()
    {
        bool canTrain = !trainedToday && day < 10;

        languageButton.interactable = canTrain;
        musicButton.interactable = canTrain;
        memoryButton.interactable = canTrain;

        if (day == 10)
        {
            nightTalkButton.interactable = !talkedTonight;
            nextDayButton.interactable = false;
            return;
        }

        nightTalkButton.interactable = trainedToday && !talkedTonight;
        nextDayButton.interactable = trainedToday && talkedTonight;
    }

    private string GetSubjectDisplayName()
    {
        if (string.IsNullOrEmpty(subjectName))
        {
            return subjectId;
        }

        return subjectName;
    }

    private string GetStatusDisplayId()
    {
        if (string.IsNullOrEmpty(subjectName))
        {
            return subjectId;
        }

        return subjectId + " / " + subjectName;
    }

    private void ClampStats()
    {
        health = Mathf.Clamp(health, 0, 100);
        language = Mathf.Clamp(language, 0, 100);
        emotion = Mathf.Clamp(emotion, 0, 100);
        attachment = Mathf.Clamp(attachment, 0, 100);
        ego = Mathf.Clamp(ego, 0, 100);
        obedience = Mathf.Clamp(obedience, 0, 100);
        shipmentSuitability = Mathf.Clamp(shipmentSuitability, 0, 100);
    }

    private string GetShipmentDayText()
    {
        if (!shipmentAnnounced)
        {
            return "未定";
        }

        return "Day " + shipmentDay;
    }

    private void UpdateStatusText()
    {
        ClampStats();

        statusText.text =
            "Day " + day + "\n\n" +
            "個体番号：" + GetStatusDisplayId() + "\n" +
            "用途：" + purpose + "\n" +
            "出荷予定日：" + GetShipmentDayText() + "\n\n" +
            "健康値：" + health + "\n" +
            "言語能力：" + language + "\n" +
            "感情反応：" + emotion + "\n" +
            "愛着形成：" + attachment + "\n" +
            "自我発達：" + ego + "\n" +
            "従順性：" + obedience + "\n" +
            "出荷適性：" + shipmentSuitability + "\n\n" +
            "ログ：\n" + logText;
    }
}
