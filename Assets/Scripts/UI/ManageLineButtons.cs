using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class ManageLineButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField]
  private int num;
  private bool isActive = false;
  [SerializeField]
  private PayoutCalculation payoutManager;
  private Button btn;

  private void Start()
  {
    btn = this.GetComponent<Button>();
  }

  public void OnPointerEnter(PointerEventData eventData)
  {

    if (num < 9)
    {
      isActive = true;
      btn.interactable = true;
    }
    else
    {
      isActive = false;
      btn.interactable = false;
    }
    if (isActive)
      payoutManager.GeneratePayoutLinesBackend(num);
  }
  public void OnPointerExit(PointerEventData eventData)
  {
    if (isActive)
      payoutManager.ResetStaticLine();
  }
  public void OnPointerDown(PointerEventData eventData)
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
    {
      this.gameObject.GetComponent<Button>().Select();
      Debug.Log("run on pointer down");
      payoutManager.GeneratePayoutLinesBackend(num);
    }
  }
  public void OnPointerUp(PointerEventData eventData)
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
    {
      Debug.Log("run on pointer up");
      payoutManager.ResetStaticLine();
      DOVirtual.DelayedCall(0.1f, () =>
      {
        this.gameObject.GetComponent<Button>().spriteState = default;
        EventSystem.current.SetSelectedGameObject(null);
      });
    }
  }
}
