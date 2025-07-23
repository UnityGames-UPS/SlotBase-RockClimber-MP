using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;
public class BonusBreakGem : MonoBehaviour
{
  [SerializeField] internal int index;
  [SerializeField] private SocketIOManager socketManager;
  [SerializeField] private BonusController bonusController;
  [SerializeField] private ImageAnimation imageAnimation;
  [SerializeField] private TMP_Text WinValueText;
  [SerializeField] private Button GemButton;
  [SerializeField] private List<Sprite> idleAnimation;
  [SerializeField] private List<Sprite> breakAnimation;
  internal bool Selected = false;

  void Awake()
  {
    GemButton = GetComponent<Button>();
    WinValueText = transform.GetChild(0).GetComponent<TMP_Text>();
  }

  void Start()
  {
    //Reset();
    if (GemButton) GemButton.onClick.RemoveAllListeners();
    if (GemButton) GemButton.onClick.AddListener(OpenGem);
  }

  void OpenGem()
  {
    if (bonusController.isOpening || bonusController.isFinished || Selected)
      return;
    Selected = true;
    StartCoroutine(OpenGemCoroutine());
  }

  IEnumerator OpenGemCoroutine()
  {
    bonusController.isOpening = true;

    imageAnimation.StopAnimation();

    imageAnimation.textureArray = breakAnimation;

    Tween tween = transform.DOShakePosition(1f, new Vector3(15, 0, 0), 30, 90, true).SetLoops(-1, LoopType.Incremental);

    bonusController.WaitForBonusResult = true;
    socketManager.OnBonusCollect(index);
    yield return new WaitUntil(() => !bonusController.WaitForBonusResult);

    double winnings = socketManager.bonusData.payload.winAmount;
    if (socketManager.bonusData.payload.payout > 0)
    {
      bonusController.PlayWinSound();
      WinValueText.text = "+" + winnings.ToString();
      bonusController.UpdateTotalWin(winnings);
    }
    else
    {
      bonusController.PlayLoseSound();
      WinValueText.text = "Game Over";
      socketManager.resultData.payload.winAmount = socketManager.bonusData.payload.winAmount;
      socketManager.resultData.player.balance = socketManager.bonusData.player.balance;
      bonusController.slotManager.UpdateBalanceAndTotalWin();
    }

    tween.Kill();
    imageAnimation.doLoopAnimation = false;
    imageAnimation.StartAnimation();

    WinValueText.gameObject.SetActive(true);
    WinValueText.transform.DOLocalMoveY(300, 0.65f).onComplete = () =>
    {
      WinValueText.gameObject.SetActive(false);
      WinValueText.transform.localPosition = Vector2.zero;
      WinValueText.text = "0";
    };

    if (socketManager.bonusData.payload.payout <= 0)
    {
      bonusController.GameOver();
    }
    bonusController.isOpening = false;
  }

  void Reset()
  {
    imageAnimation.textureArray = idleAnimation;
    imageAnimation.doLoopAnimation = true;
  }

  private void OnDisable()
  {
    imageAnimation.StopAnimation();
  }
  private void OnEnable()
  {
    Reset();
    if (imageAnimation.textureArray.Count > 0)
      imageAnimation.StartAnimation();
  }
}
