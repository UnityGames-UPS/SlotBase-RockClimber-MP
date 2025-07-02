using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class BonusBreakGem : MonoBehaviour
{
  [SerializeField] private Button gem;
  [SerializeField] private ImageAnimation imageAnimation;
  [SerializeField] private List<Sprite> idleAnimation;
  [SerializeField] private List<Sprite> breakAnimation;
  [SerializeField] internal double value;
  [SerializeField] BonusController bonusController;
  [SerializeField] private TMP_Text valueText;
  void Start()
  {
    //Reset();
    if (gem) gem.onClick.RemoveAllListeners();
    if (gem) gem.onClick.AddListener(Break_gem);
  }

  void Break_gem()
  {
    if (bonusController.currentBreakCount < bonusController.maxBreakCount)
    {
      //HACK: Activating RayCast Panel
      bonusController.raycastPanel.SetActive(true);
      valueText.gameObject.SetActive(true);

      //HACK: Stop the current playing animation and turn off the interaction after click
      imageAnimation.StopAnimation();
      imageAnimation.doLoopAnimation = false;
      gem.interactable = false;

      //HACK: Assigning the new animation list to the texture array.
      imageAnimation.textureArray = breakAnimation;

      //HACK: Starting the new game animation.
      imageAnimation.StartAnimation();
      value = bonusController.OnBreakGem();

      if (value > 0)
      {
        valueText.text = "+" + value.ToString();
        bonusController.PlayWinSound();
      }
      else
      {
        valueText.text = "Game Over";
        bonusController.PlayLoseSound();
      }

      valueText.transform.DOLocalMoveY(300, 0.65f).onComplete = () =>
      {
        valueText.gameObject.SetActive(false);
        valueText.transform.localPosition = Vector2.zero;
        valueText.text = "0";
      };
      DOVirtual.DelayedCall(0.66f, () =>
      {
        bonusController.raycastPanel.SetActive(false);
      });
      if (value <= 0)
      {
        bonusController.GameOver();
      }
    }
  }

  void Reset()
  {
    gem.interactable = true;
    imageAnimation.textureArray = idleAnimation;
    imageAnimation.doLoopAnimation = true;
  }

  private void OnDisable()
  {
    value = 0;
    imageAnimation.StopAnimation();
  }
  private void OnEnable()
  {
    Reset();
    if (imageAnimation.textureArray.Count > 0)
      imageAnimation.StartAnimation();
  }
}
