using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardFlip : MonoBehaviour
{
  [SerializeField] internal Sprite cardImage;
  [SerializeField] internal Button Card_Button;
  [SerializeField] private GambleController gambleController;
  private RectTransform Card_transform;

  private void Start()
  {
    Card_transform = Card_Button.GetComponent<RectTransform>();
    if (Card_Button) Card_Button.onClick.RemoveAllListeners();
    if (Card_Button) Card_Button.onClick.AddListener(()=>
    {
      StartCoroutine(OnUserSelectCard());
    });
  }

  private IEnumerator OnUserSelectCard()
  {
    if (gambleController.Flipped)
    {
      yield break;
    }
    gambleController.Flipped = true;

    yield return gambleController.GambleDraw();
    gambleController.CardFlipped(this);
    FlipMyCard(gambleController.PlayerCardSprite);
    yield return new WaitForSecondsRealtime(1f);
    gambleController.FlipAllCards();
  }
  internal void FlipMyCard(Sprite CardSprite)
  {
    Card_transform.localEulerAngles = new Vector3(0, 180, 0);
    Card_transform.DORotate(new Vector3(0, 0, 0), 1, RotateMode.FastBeyond360);
    DOVirtual.DelayedCall(0.3f, () => ChangeSprite(CardSprite));
  }
  private void ChangeSprite(Sprite sprite)
  {
    if (Card_Button)
    {
      Card_Button.image.sprite = sprite;
    }
  }
}
