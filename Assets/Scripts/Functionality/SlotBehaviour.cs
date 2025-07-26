using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
  [SerializeField] private SocketIOManager SocketManager;
  [SerializeField] private AudioController audioController;
  [SerializeField] private UIManager uiManager;
  [SerializeField] private BonusController bonusController;
  [SerializeField] private GambleController gambleController;
  [Header("Sprites")]
  [SerializeField] private Sprite[] myImages;

  [Header("Slot Images")]
  [SerializeField] private List<SlotImage> images;
  [SerializeField] private List<SlotImage> Tempimages;

  [Header("Slots Elements")]
  [SerializeField] private LayoutElement[] Slot_Elements;

  [Header("Slots Transforms")]
  [SerializeField] private Transform[] Slot_Transform;

  [Header("Buttons")]
  [SerializeField] private Button SlotStart_Button;

  [Header("Miscellaneous UI")]
  [SerializeField] private TMP_Text Balance_text;
  [SerializeField] private TMP_Text TotalBet_text;
  [SerializeField] private TMP_Text TotalWin_text;

  [Header("Games buttongroup UI")]
  [SerializeField] private Button AutoSpin_Button;
  [SerializeField] private Button AutoSpinStop_Button;
  [SerializeField] private Button MaxBet_Button;
  [SerializeField] private Button Betone_button;
  [SerializeField] private Button Turbo_Button;
  [SerializeField] private Button StopSpin_Button;
  [SerializeField] private Sprite[] TurboToggleSprites;
  [SerializeField] private PayoutCalculation PayCalculator;
  [SerializeField] private List<Tweener> alltweens = new List<Tweener>();
  [SerializeField] private List<ImageAnimation> WinLineSymbolList;
  [SerializeField] private int IconSizeFactor = 100;
  [SerializeField] private int SpaceFactor = 0;
  private Coroutine AutoSpinRoutine = null;
  private Coroutine tweenroutine = null;
  private Coroutine FreeSpinRoutine = null;
  private int numberOfSlots = 5;
  private int tweenHeight = 0;
  private Tween BalanceTween;
  protected int Lines = 9;
  private bool IsFreeSpin = false;
  private bool IsSpinning = false;
  private double currentBalance = 0;
  private double currentTotalBet = 0;
  private bool StopSpinToggle;
  private float SpinDelay = 0.2f;
  private bool IsTurboOn;
  private bool WasAutoSpinOn;
  internal bool IsAutoSpin = false;
  internal bool CheckPopups = false;
  internal bool WaitForBonus = false;
  internal int BetCounter = 0;
  internal double currentBet = 0;

  private void Start()
  {
    if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
    if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

    if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
    if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

    if (Betone_button) Betone_button.onClick.RemoveAllListeners();
    if (Betone_button) Betone_button.onClick.AddListener(OnBetOne);

    if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
    if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

    if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
    if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(() =>
    {
      audioController.PlayButtonAudio();
      StopAutoSpin();
    });

    if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
    if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() =>
    {
      audioController.PlayButtonAudio();
      StopSpinToggle = true;
      StopSpin_Button.gameObject.SetActive(false);
    });

    if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
    if (Turbo_Button) Turbo_Button.onClick.AddListener(() =>
    {
      audioController.PlayButtonAudio();
      TurboToggle();
    });

    tweenHeight = (9 * IconSizeFactor) - 280;
  }

  void TurboToggle()
  {
    if (IsTurboOn)
    {
      IsTurboOn = false;
      Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
      Turbo_Button.image.sprite = TurboToggleSprites[0];
      Turbo_Button.image.color = new Color(0.86f, 0.86f, 0.86f, 1);
    }
    else
    {
      IsTurboOn = true;
      Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
      Turbo_Button.image.color = new Color(1, 1, 1, 1);
    }
  }

  internal void AutoSpin()
  {
    if (audioController) audioController.PlaySpinButtonAudio();
    if (!IsAutoSpin)
    {
      IsAutoSpin = true;
      if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
      if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

      if (AutoSpinRoutine != null)
      {
        StopCoroutine(AutoSpinRoutine);
        AutoSpinRoutine = null;
      }
      AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
    }
  }

  private void ShuffleInitialMatrix()
  {
    for (int i = 0; i < Tempimages.Count; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
        Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
      }
    }
  }

  internal void SetInitialUI()
  {
    ShuffleInitialMatrix();
    BetCounter = 0;
    if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.gameData.bets[BetCounter] * Lines).ToString();
    if (TotalWin_text) TotalWin_text.text = "0.000";
    if (Balance_text) Balance_text.text = SocketManager.initialData.player.balance.ToString("F3");
    currentBalance = SocketManager.initialData.player.balance;
    currentTotalBet = SocketManager.initialData.gameData.bets[BetCounter] * Lines;
    CompareBalance();
    uiManager.InitialiseUIData(SocketManager.initialData.uiData.paylines);
  }

  private void StopAutoSpin()
  {
    if (audioController) audioController.PlaySpinButtonAudio();
    if (IsAutoSpin)
    {
      IsAutoSpin = false;
      if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
      if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
      StartCoroutine(StopAutoSpinCoroutine());
    }
  }

  private IEnumerator AutoSpinCoroutine()
  {
    while (IsAutoSpin)
    {
      StartSlots(IsAutoSpin);
      yield return tweenroutine;
      yield return new WaitForSeconds(SpinDelay);
    }
    WasAutoSpinOn = false;
  }

  private IEnumerator StopAutoSpinCoroutine()
  {
    yield return new WaitUntil(() => !IsSpinning);
    CheckAndActivateGamble();
    ToggleButtonGrp(true);
    if (AutoSpinRoutine != null || tweenroutine != null)
    {
      StopCoroutine(AutoSpinRoutine);
      StopCoroutine(tweenroutine);
      tweenroutine = null;
      AutoSpinRoutine = null;
      StopCoroutine(StopAutoSpinCoroutine());
    }
  }

  internal void FreeSpin(int spins)
  {
    if (!IsFreeSpin)
    {
      IsFreeSpin = true;
      ToggleButtonGrp(false);

      if (FreeSpinRoutine != null)
      {
        StopCoroutine(FreeSpinRoutine);
        FreeSpinRoutine = null;
      }
      FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
    }
  }

  private IEnumerator FreeSpinCoroutine(int spinchances)
  {
    int i = 0;
    while (i < spinchances)
    {
      uiManager.FreeSpins--;
      StartSlots();
      yield return tweenroutine;
      yield return new WaitForSeconds(SpinDelay);
      i++;
    }
    if (WasAutoSpinOn)
    {
      AutoSpin();
    }
    else
    {
      ToggleButtonGrp(true);
    }
    IsFreeSpin = false;
  }

  private void CompareBalance()
  {
    if (currentBalance < currentTotalBet)
    {
      uiManager.LowBalPopup();
    }
  }

  private void MaxBet()
  {
    if (audioController) audioController.PlayButtonAudio();
    BetCounter = SocketManager.initialData.gameData.bets.Count - 1;
    if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.gameData.bets[BetCounter] * Lines).ToString();
    currentTotalBet = SocketManager.initialData.gameData.bets[BetCounter] * Lines;
  }

  void OnBetOne()
  {
    if (audioController) audioController.PlayButtonAudio();
    if (BetCounter < SocketManager.initialData.gameData.bets.Count - 1)
    {
      BetCounter++;
    }
    else
    {
      BetCounter = 0;
    }
    if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.gameData.bets[BetCounter] * Lines).ToString();
    currentTotalBet = SocketManager.initialData.gameData.bets[BetCounter] * Lines;
  }

  private void OnApplicationFocus(bool focus)
  {
    audioController.CheckFocusFunction(focus, IsSpinning);
  }

  private void StartSlots(bool autoSpin = false)
  {
    if (TotalWin_text) TotalWin_text.text = "0.000";
    if (audioController) audioController.PlaySpinButtonAudio();
    gambleController.ToggleGambleButton(false);
    if (!autoSpin)
    {
      if (AutoSpinRoutine != null)
      {
        StopCoroutine(AutoSpinRoutine);
        StopCoroutine(tweenroutine);
        tweenroutine = null;
        AutoSpinRoutine = null;
      }
    }
    PayCalculator.DontDestroyLines.Clear();

    if (WinLineSymbolList.Count > 0)
    {
      ResetWinLineSymbols();
    }
    PayCalculator.ResetStaticLine();
    tweenroutine = StartCoroutine(TweenRoutine());
  }

  private IEnumerator TweenRoutine()
  {
    currentBet = SocketManager.initialData.gameData.bets[BetCounter];

    if (currentBalance < currentTotalBet && !IsFreeSpin)
    {
      CompareBalance();
      StopAutoSpin();
      yield return new WaitForSeconds(1);
      ToggleButtonGrp(true);
      yield break;
    }
    if (audioController) audioController.PlayWLAudio("spin");
    IsSpinning = true;
    ToggleButtonGrp(false);

    if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
    {
      StopSpin_Button.gameObject.SetActive(true);
    }

    for (int i = 0; i < numberOfSlots; i++)
    {
      InitializeTweening(Slot_Transform[i]);
      yield return new WaitForSeconds(0.1f);
    }

    if (!IsFreeSpin)
    {
      BalanceTween?.Kill();
      BalanceTween = TextAnimation(Balance_text, currentBalance, currentBalance - currentTotalBet, 0.5f);
    }
    SocketManager.AccumulateResult(BetCounter);
    yield return new WaitUntil(() => SocketManager.isResultdone);

    for (int i = 0; i < SocketManager.resultData.matrix.Count; i++)
    {
      for (int j = 0; j < SocketManager.resultData.matrix[i].Count; j++)
      {
        Tempimages[j].slotImages[i].sprite = myImages[int.Parse(SocketManager.resultData.matrix[i][j])];
      }
    }
    if (IsTurboOn || IsFreeSpin)
    {
      StopSpinToggle = true;
    }
    else
    {
      for (int i = 0; i < 5; i++)
      {
        yield return new WaitForSeconds(0.1f);
        if (StopSpinToggle)
        {
          break;
        }
      }
      StopSpin_Button.gameObject.SetActive(false);
    }
    for (int i = 0; i < numberOfSlots; i++)
    {
      yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
    }
    audioController.StopWLAaudio();
    StopSpinToggle = false;
    yield return alltweens[^1].WaitForCompletion();
    KillAllTweens();
    yield return new WaitForSeconds(0.1f);

    if (SocketManager.resultData.payload.winAmount > 0)
    {
      UpdateBalanceAndTotalWin();
      SpinDelay = 1.2f;
    }
    else
    {
      SpinDelay = 0.2f;
    }

    ProcessWinLines(SocketManager.resultData.payload.wins);

    if (SocketManager.resultData.bonus.isTriggered)
    {
      yield return new WaitForSeconds(1f);
      WaitForBonus = true;
      bonusController.StartBonus();
      yield return new WaitUntil(() => !WaitForBonus);
    }

    CheckPopups = true;
    CheckAndTriggerWinPopups();
    yield return new WaitUntil(() => !CheckPopups);

    currentBalance = SocketManager.playerdata.balance;

    if (!IsAutoSpin)
    {
      ToggleButtonGrp(true);
      CheckAndActivateGamble();
    }
    IsSpinning = false;
  }

  private void CheckAndActivateGamble()
  {
    if (SocketManager.resultData.payload.winAmount > 0)
    {
      gambleController.ToggleGambleButton(true);
    }
  }

  internal void DeactivateAutoSpinOnGamble()
  {
    if (IsAutoSpin)
    {
      StopAutoSpin();
    }
  }

  internal void CheckAndTriggerWinPopups()
  {
    if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 5 && SocketManager.resultData.payload.winAmount < currentTotalBet * 10)
    {
      uiManager.PopulateWin(1, SocketManager.resultData.payload.winAmount);
    }
    else if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 10 && SocketManager.resultData.payload.winAmount < currentTotalBet * 15)
    {
      uiManager.PopulateWin(2, SocketManager.resultData.payload.winAmount);
    }
    else if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 15)
    {
      uiManager.PopulateWin(3, SocketManager.resultData.payload.winAmount);
    }
    else
    {
      CheckPopups = false;
    }
  }

  void ToggleButtonGrp(bool toggle)
  {
    if (SlotStart_Button) SlotStart_Button.interactable = toggle;
    if (Betone_button) Betone_button.interactable = toggle;
    if (MaxBet_Button) MaxBet_Button.interactable = toggle;
    if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
  }

  internal void UpdateBalanceAndTotalWin()
  {
    double currentTotalBalance = double.Parse(Balance_text.text, System.Globalization.CultureInfo.InvariantCulture);
    double currentTotalWin = double.Parse(TotalWin_text.text, System.Globalization.CultureInfo.InvariantCulture);
    BalanceTween?.Kill();
    BalanceTween = TextAnimation(Balance_text, currentTotalBalance, SocketManager.resultData.player.balance, 0.5f);
    TextAnimation(TotalWin_text, currentTotalWin, SocketManager.resultData.payload.winAmount, 0.5f);
  }

  private void ToggleWinLineSymbolsON(GameObject animObjects)
  {
    animObjects.transform.GetChild(0).gameObject.SetActive(true);
    animObjects.transform.GetChild(1).gameObject.SetActive(true);
    ImageAnimation IA = animObjects.transform.GetChild(0).GetComponent<ImageAnimation>();
    IA.StartAnimation();
    WinLineSymbolList.Add(IA);
  }

  private void ResetWinLineSymbols()
  {
    for (int i = 0; i < WinLineSymbolList.Count; i++)
    {
      WinLineSymbolList[i].StopAnimation();
      if (WinLineSymbolList[i].transform.parent.childCount > 0)
      {
        WinLineSymbolList[i].transform.parent.GetChild(0).gameObject.SetActive(false);
        WinLineSymbolList[i].transform.parent.GetChild(1).gameObject.SetActive(false);
      }
    }
    WinLineSymbolList.Clear();
    WinLineSymbolList.TrimExcess();
  }

  internal void CallCloseSocket()
  {
    StartCoroutine(SocketManager.CloseSocket());
  }

  private void ProcessWinLines(List<Win> wins)
  {
    if (wins.Count <= 0)
      return;

    audioController.PlayWLAudio("win");

    List<KeyValuePair<int, int>> coords = new();
    for (int j = 0; j < wins.Count; j++)
    {
      for (int k = 0; k < wins[j].positions.Count; k++)
      {
        int rowIndex = SocketManager.initialData.gameData.lines[wins[j].line][k];
        int columnIndex = k;
        coords.Add(new KeyValuePair<int, int>(rowIndex, columnIndex));
      }
    }

    foreach (var coord in coords)
    {
      int rowIndex = coord.Key;
      int columnIndex = coord.Value;
      // Debug.Log($"Win Line: Column {columnIndex}, Row {rowIndex}");
      ToggleWinLineSymbolsON(Tempimages[columnIndex].slotImages[rowIndex].gameObject);
    }
  }


  #region TweeningCode
  private void InitializeTweening(Transform slotTransform)
  {
    slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
    Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
    tweener.Play();
    alltweens.Add(tweener);
  }

  private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop)
  {
    alltweens[index].Pause();
    int tweenpos = (reqpos * (IconSizeFactor + SpaceFactor)) - (IconSizeFactor + (2 * SpaceFactor));
    slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
    alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpaceFactor > 0 ? SpaceFactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
    if (!isStop)
    {
      yield return new WaitForSeconds(0.2f);
    }
    else
    {
      yield return null;
    }
  }

  private void KillAllTweens()
  {
    for (int i = 0; i < numberOfSlots; i++)
    {
      alltweens[i].Kill();
    }
    alltweens.Clear();
  }
  #endregion

  Tween TextAnimation(TMP_Text text, double startValue, double endValue, double duration)
  {
    // Debug.Log($"Text Animation: Start Value: {startValue}, End Value: {endValue}, Duration: {duration}");
    return DOTween.To(() => startValue, (x) => startValue = x, endValue, (float)duration).OnUpdate(() =>
    {
      if (text)
      {
        text.text = startValue.ToString("F3");
      }
    });
  }
}

[Serializable]
public class SlotImage
{
  public List<Image> slotImages = new List<Image>(10);
}
