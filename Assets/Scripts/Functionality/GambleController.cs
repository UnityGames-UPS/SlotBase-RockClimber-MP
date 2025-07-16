using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GambleController : MonoBehaviour
{
  [SerializeField] internal SocketIOManager socketManager;
  [SerializeField] private AudioController audioController;
  [SerializeField] private SlotBehaviour slotController;
  [SerializeField] private GameObject GambleParentUI;
  [SerializeField] private GameObject GambleButtonsParent;
  [SerializeField] private GameObject LoadingScreenUI;
  [SerializeField] private Button GambleStartButton;
  [SerializeField] private Button GambleDoubleButton;
  [SerializeField] private Button GambleCollectButton;
  [SerializeField] internal List<CardFlip> allcards = new List<CardFlip>();
  [SerializeField] private CardFlip DealerCard_Script;
  [SerializeField] private TMP_Text GambleWinText;
  [SerializeField] private Sprite[] HeartSpriteList;
  [SerializeField] private Sprite[] ClubSpriteList;
  [SerializeField] private Sprite[] SpadeSpriteList;
  [SerializeField] private Sprite[] DiamondSpriteList;
  [SerializeField] private Sprite cardCover;
  [SerializeField] private Image slider;
  [SerializeField] private bool WasAutoSpinOnBeforeGamble;
  private Sprite DealerCardSprite;
  private Sprite RandomCardSprite1;
  private Sprite RandomCardSprite2;
  private Tweener Gamble_Tween_Scale = null;
  private CardFlip SelectedCard = null;
  internal Sprite PlayerCardSprite;
  internal bool WaitForGambleResult = true;
  internal bool isResult = false;
  internal bool Flipped = false;

  private void Start()
  {
    if (GambleStartButton) GambleStartButton.onClick.RemoveAllListeners();
    if (GambleStartButton) GambleStartButton.onClick.AddListener(delegate
    {
      StartGamblegame(true);
    });
    if (GambleDoubleButton) GambleDoubleButton.onClick.RemoveAllListeners();
    if (GambleDoubleButton) GambleDoubleButton.onClick.AddListener(delegate
    {
      audioController.PlayButtonAudio();
      ToggleButton(GambleDoubleButton, false);
      OnGambleDoubleReset();
    });
    if (GambleCollectButton) GambleCollectButton.onClick.RemoveAllListeners();
    if (GambleCollectButton) GambleCollectButton.onClick.AddListener(() =>
    {
      audioController.PlayButtonAudio();
      ToggleButton(GambleCollectButton, false);
      StartCoroutine(OnGambleCollectReset());
    });
    ToggleButton(GambleCollectButton, false);
    ToggleButton(GambleDoubleButton, false);
  }

  void StartGamblegame(bool GambleInit = false)
  {
    if (audioController && !GambleInit) audioController.PlayButtonAudio();
    GambleButtonsParent.gameObject.SetActive(false);
    ToggleButton(GambleCollectButton, false);
    ToggleButton(GambleDoubleButton, false);
    ToggleGambleButton(false);
    GambleWinText.text = "0";

    if (GambleInit)
    {
      WasAutoSpinOnBeforeGamble = slotController.IsAutoSpin;
      slotController.DeactivateAutoSpinOnGamble();
    }

    if (GambleParentUI) GambleParentUI.SetActive(true);
    StartCoroutine(InitGamble(GambleInit));
  }

  IEnumerator InitGamble(bool init = false)
  {
    LoadingScreenUI.SetActive(true);
    float fillAmount = 0;
    while (fillAmount < 0.9)
    {
      fillAmount += Time.deltaTime;
      slider.fillAmount = fillAmount;
      if (fillAmount == 0.9) yield break;
      yield return null;
    }

    if (init)
    {
      WaitForGambleResult = true;
      socketManager.OnGambleInit();
      yield return new WaitUntil(() => !WaitForGambleResult);
    }

    slider.fillAmount = 1;
    yield return new WaitForSeconds(1f);
    LoadingScreenUI.SetActive(false);

    Flipped = false;
  }

  internal IEnumerator GambleDraw()
  {
    WaitForGambleResult = true;
    socketManager.OnGambleDraw();
    yield return new WaitUntil(() => !WaitForGambleResult);

    ComputeCards();
  }

  IEnumerator GambleEnd(bool lost = false)
  {
    if (lost)
      yield return new WaitForSeconds(2f);

    slotController.UpdateBalanceAndTotalWin();
    if (GambleParentUI) GambleParentUI.SetActive(false);
    allcards.ForEach((element) =>
    {
      element.Card_Button.image.sprite = cardCover;
    });
    DealerCard_Script.Card_Button.image.sprite = cardCover;

    if (WasAutoSpinOnBeforeGamble)
    {
      WasAutoSpinOnBeforeGamble = false;
      slotController.AutoSpin();
    }
  }

  private void ComputeCards()
  {
    DealerCardSprite = GetCardForValue(socketManager.GambleData.payload.cards.dealerCard - 1);
    PlayerCardSprite = GetCardForValue(socketManager.GambleData.payload.cards.playerCard - 1);
    RandomCardSprite1 = GetCardForValue(Random.Range(0, 13));
    RandomCardSprite2 = GetCardForValue(Random.Range(0, 13));
  }

  private Sprite GetCardForValue(int value)
  {
    // Debug.Log("Card Value: " + value);
    List<Sprite[]> CartSuites = new()
    {
      HeartSpriteList,
      ClubSpriteList,
      SpadeSpriteList,
      DiamondSpriteList
    };

    int randomSuiteIndex = Random.Range(0, CartSuites.Count);
    Sprite[] selectedSuite = CartSuites[randomSuiteIndex];

    return selectedSuite[value];
  }

  internal void FlipAllCards()
  {
    int i = 1;
    allcards.ForEach((element) =>
    {
      if (element != SelectedCard && i == 1)
      {
        i++;
        element.FlipMyCard(RandomCardSprite1);
      }
      else if (element != SelectedCard && i == 2)
      {
        element.FlipMyCard(RandomCardSprite2);
      }
    });
    DealerCard_Script.FlipMyCard(DealerCardSprite);

    if (socketManager.GambleData.payload.playerWon)
    {
      GambleWinText.text = "YOU WIN" + "\n" + socketManager.GambleData.payload.winAmount.ToString("F3");
      ToggleButton(GambleCollectButton, true);
      ToggleButton(GambleDoubleButton, true);
      GambleButtonsParent.gameObject.SetActive(true);
    }
    else
    {
      GambleWinText.text = "YOU LOSE" + "\n" + "0";
      StartCoroutine(GambleEnd(true));
    }
  }

  internal void CardFlipped(CardFlip card)
  {
    SelectedCard = card;
  }

  private void OnGambleDoubleReset()
  {
    StartGamblegame();
    allcards.ForEach((element) =>
    {
      element.Card_Button.image.sprite = cardCover;
    });
    DealerCard_Script.Card_Button.image.sprite = cardCover;
    Flipped = false;
  }

  private IEnumerator OnGambleCollectReset()
  {
    WaitForGambleResult = true;
    socketManager.OnGambleCollect();
    yield return new WaitUntil(() => !WaitForGambleResult);

    if (WasAutoSpinOnBeforeGamble)
    {
      WasAutoSpinOnBeforeGamble = false;
      slotController.AutoSpin();
    }
    StartCoroutine(GambleEnd());
  }

  internal void ToggleGambleButton(bool toggle)
  {
    if (toggle)
    {
      Gamble_Tween_Scale = GambleStartButton.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.18f, 1.18f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
    }
    else
    {
      Gamble_Tween_Scale.Kill();
      GambleStartButton.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
    }
    GambleStartButton.interactable = toggle;
  }

  internal void ToggleButton(Button button, bool toggle)
  {
    button.interactable = toggle;
  }
}
