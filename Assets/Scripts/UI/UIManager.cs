using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
  [Header("Menu UI")]
  [SerializeField]
  private Button Menu_Button;
  [SerializeField]
  private GameObject Menu_Object;

  [SerializeField]
  private Button Settings_Button;
  [SerializeField]
  private GameObject Settings_Object;
  [SerializeField]
  private RectTransform Settings_RT;

  [SerializeField]
  private Button Exit_Button;
  [SerializeField]
  private GameObject Exit_Object;

  [SerializeField]
  private Button Paytable_Button;
  [SerializeField]
  private GameObject Paytable_Object;
  [SerializeField]
  private RectTransform Paytable_RT;
  [SerializeField]
  private Button GameExit_Button;
  [SerializeField]
  private Button SkipWinAnimation;

  [Header("Popus UI")]
  [SerializeField]
  private GameObject MainPopup_Object;

  [Header("Paytable Popup")]
  [SerializeField]
  private GameObject PaytablePopup_Object;
  [SerializeField]
  private Button PaytableExit_Button;
  [SerializeField]
  private TMP_Text[] SymbolsText;

  [SerializeField]
  private TMP_Text wildText;

  [SerializeField]
  private TMP_Text m_Bonus_Text;
  [SerializeField]
  private Button Right_Button;
  [SerializeField]
  private Button Left_Button;
  [SerializeField]
  private GameObject[] Info_Screens;
  int screenCounter = 0;

  [Header("Settings Popup")]
  [SerializeField]
  private GameObject SettingsPopup_Object;
  [SerializeField]
  private Button SettingsExit_Button;
  [SerializeField]
  private Slider Sound_slider;
  [SerializeField]
  private Slider Music_slider;

  [Header("Megawin Popup")]
  [SerializeField] private GameObject megawin;
  [SerializeField] private TMP_Text megawin_text;
  [SerializeField] private Image Win_Image;
  [SerializeField] private Sprite HugeWin_Sprite;
  [SerializeField] private Sprite BigWin_Sprite;
  [SerializeField] private Sprite MegaWin_Sprite;

  [Header("LowBalance Popup")]
  [SerializeField]
  private Button LBExit_Button;
  [SerializeField]
  private GameObject LBPopup_Object;

  [Header("Disconnection Popup")]
  [SerializeField]
  private Button CloseDisconnect_Button;
  [SerializeField]
  private GameObject DisconnectPopup_Object;

  [Header("AnotherDevice Popup")]
  [SerializeField]
  private Button CloseAD_Button;
  [SerializeField]
  private GameObject ADPopup_Object;

  [Header("Quit Popup")]
  [SerializeField]
  private GameObject QuitPopup_Object;
  [SerializeField]
  private Button YesQuit_Button;
  [SerializeField]
  private Button NoQuit_Button;
  [SerializeField]
  private Button CrossQuit_Button;
  [SerializeField]
  private AudioController audioController;
  [SerializeField]
  private SlotBehaviour slotBehaviour;
  [SerializeField]
  private SocketIOManager socketManager;
  [SerializeField] Button m_AwakeGameButton;
  private bool isExit = false;
  internal int FreeSpins;

  private Tween WinPopupTextTween;
  private Tween ClosePopupTween;

  private void Start()
  {
    if (Menu_Button) Menu_Button.onClick.RemoveAllListeners();
    if (Menu_Button) Menu_Button.onClick.AddListener(OpenMenu);

    if (Exit_Button) Exit_Button.onClick.RemoveAllListeners();
    if (Exit_Button) Exit_Button.onClick.AddListener(CloseMenu);

    if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
    if (Settings_Button) Settings_Button.onClick.AddListener(delegate { OpenPopup(SettingsPopup_Object); });

    if (SettingsExit_Button) SettingsExit_Button.onClick.RemoveAllListeners();
    if (SettingsExit_Button) SettingsExit_Button.onClick.AddListener(delegate { ClosePopup(SettingsPopup_Object); });

    if (Paytable_Button) Paytable_Button.onClick.RemoveAllListeners();
    if (Paytable_Button) Paytable_Button.onClick.AddListener(delegate { screenCounter = 1; ChangePage(false); OpenPopup(PaytablePopup_Object); });

    if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
    if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });

    if (Sound_slider) Sound_slider.onValueChanged.RemoveAllListeners();
    if (Sound_slider) Sound_slider.onValueChanged.AddListener(delegate { ChangeSound(); });

    if (Music_slider) Music_slider.onValueChanged.RemoveAllListeners();
    if (Music_slider) Music_slider.onValueChanged.AddListener(delegate { ChangeMusic(); });
    if (Music_slider) Music_slider.value = 0.3f;

    if (audioController) audioController.ToggleMute(false);

    if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
    if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { OpenPopup(QuitPopup_Object); });

    if (NoQuit_Button) NoQuit_Button.onClick.RemoveAllListeners();
    if (NoQuit_Button) NoQuit_Button.onClick.AddListener(delegate { if (!isExit) ClosePopup(QuitPopup_Object); });

    if (CrossQuit_Button) CrossQuit_Button.onClick.RemoveAllListeners();
    if (CrossQuit_Button) CrossQuit_Button.onClick.AddListener(delegate { if (!isExit) ClosePopup(QuitPopup_Object); });

    if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
    if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

    if (YesQuit_Button) YesQuit_Button.onClick.RemoveAllListeners();
    if (YesQuit_Button) YesQuit_Button.onClick.AddListener(CallOnExitFunction);

    if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
    if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(delegate { CallOnExitFunction();}); //BackendChanges

    if (CloseAD_Button) CloseAD_Button.onClick.RemoveAllListeners();
    if (CloseAD_Button) CloseAD_Button.onClick.AddListener(CallOnExitFunction);

    if (Right_Button) Right_Button.onClick.RemoveAllListeners();
    if (Right_Button) Right_Button.onClick.AddListener(delegate { ChangePage(true); audioController.PlayButtonAudio(); });

    if (Left_Button) Left_Button.onClick.RemoveAllListeners();
    if (Left_Button) Left_Button.onClick.AddListener(delegate { ChangePage(false); audioController.PlayButtonAudio(); });

    if (SkipWinAnimation) SkipWinAnimation.onClick.RemoveAllListeners();
    if (SkipWinAnimation) SkipWinAnimation.onClick.AddListener(delegate { SkipWin(); Debug.Log("Clicked.."); });

  }


  void SkipWin()
  {
    Debug.Log("Skip win called");
    if (ClosePopupTween != null)
    {
      ClosePopupTween.Kill();
      ClosePopupTween = null;
    }
    if (WinPopupTextTween != null)
    {
      WinPopupTextTween.Kill();
      WinPopupTextTween = null;
    }
    ClosePopup(megawin);
    slotBehaviour.CheckPopups = false;
  }

  internal void LowBalPopup()
  {
    OpenPopup(LBPopup_Object);
  }

  internal void ADfunction()
  {
    OpenPopup(ADPopup_Object);
  }

  internal void DisconnectionPopup()
  {
    if (!isExit)
    {
      OpenPopup(DisconnectPopup_Object);
    }
  }

  internal void InitialiseUIData(Paylines symbolsText)
  {
    PopulateSymbolsPayout(symbolsText);
  }

  private void PopulateSymbolsPayout(Paylines paylines)
  {
    // for (int i = 0; i < SymbolsText.Length; i++)
    // {
    //   string text = null;
    //   if (paylines.symbols[i].Multiplier[0][0] != 0)
    //   {
    //     text += "5x - " + paylines.symbols[i].Multiplier[0][0] + "x";
    //   }
    //   if (paylines.symbols[i].Multiplier[1][0] != 0)
    //   {
    //     text += "\n4x - " + paylines.symbols[i].Multiplier[1][0] + "x";
    //   }
    //   if (paylines.symbols[i].Multiplier[2][0] != 0)
    //   {
    //     text += "\n3x - " + paylines.symbols[i].Multiplier[2][0] + "x";
    //   }
    //   if (SymbolsText[i]) SymbolsText[i].text = text;
    // }
    // for (int i = 0; i < paylines.symbols.Count; i++)
    // {
    //   if (paylines.symbols[i].Name.ToUpper() == "BONUS")
    //   {
    //     if (m_Bonus_Text) m_Bonus_Text.text = paylines.symbols[i].description.ToString();
    //   }
    //   if (paylines.symbols[i].Name.ToUpper() == "WILD")
    //   {
    //     if (wildText) wildText.text = paylines.symbols[i].description.ToString();
    //   }

    // }
  }

  private void CallOnExitFunction()
  {
    isExit = true;
    audioController.PlayButtonAudio();
    slotBehaviour.CallCloseSocket();
  }

  internal void PopulateWin(int type, double amount)
  {
    double initAmount = 0;
    double originalAmount = amount;
    switch (type)
    {
      case 1:
        if (Win_Image) Win_Image.sprite = BigWin_Sprite;
        break;
      case 2:
        if (Win_Image) Win_Image.sprite = HugeWin_Sprite;
        break;
      case 3:
        if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
        break;
    }
    if (megawin) megawin.SetActive(true);
    if (MainPopup_Object) MainPopup_Object.SetActive(true);

    WinPopupTextTween = DOTween.To(() => initAmount, (val) => initAmount = val, amount, 5f).OnUpdate(() =>
    {
      if (megawin_text) megawin_text.text = initAmount.ToString("f3");
    });

    ClosePopupTween = DOVirtual.DelayedCall(6f, () =>
    {
      if (MainPopup_Object) MainPopup_Object.SetActive(false);
      if (megawin) megawin.SetActive(false);
      if (megawin_text) megawin_text.text = "0";
      slotBehaviour.CheckPopups = false;

    });
  }

  private void OpenMenu()
  {
    if (audioController) audioController.PlayButtonAudio();
    if (Menu_Object) Menu_Object.SetActive(false);
    if (Exit_Object) Exit_Object.SetActive(true);
    if (Paytable_Object) Paytable_Object.SetActive(true);
    if (Settings_Object) Settings_Object.SetActive(true);

    DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x + 150, Paytable_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
    {
      LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
    });

    DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x + 300, Settings_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
    {
      LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
    });
  }

  private void CloseMenu()
  {
    if (audioController) audioController.PlayButtonAudio();
    DOTween.To(() => Paytable_RT.anchoredPosition, (val) => Paytable_RT.anchoredPosition = val, new Vector2(Paytable_RT.anchoredPosition.x - 150, Paytable_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
    {
      LayoutRebuilder.ForceRebuildLayoutImmediate(Paytable_RT);
    });

    DOTween.To(() => Settings_RT.anchoredPosition, (val) => Settings_RT.anchoredPosition = val, new Vector2(Settings_RT.anchoredPosition.x - 300, Settings_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
    {
      LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_RT);
    });

    DOVirtual.DelayedCall(0.1f, () =>
     {
       if (Menu_Object) Menu_Object.SetActive(true);
       if (Exit_Object) Exit_Object.SetActive(false);
       if (Paytable_Object) Paytable_Object.SetActive(false);
       if (Settings_Object) Settings_Object.SetActive(false);
     });
  }

  private void OpenPopup(GameObject Popup)
  {
    if (audioController) audioController.PlayButtonAudio();
    if (Popup) Popup.SetActive(true);
    if (MainPopup_Object) MainPopup_Object.SetActive(true);
  }

  private void ClosePopup(GameObject Popup)
  {
    if (audioController) audioController.PlayButtonAudio();
    if (Popup) Popup.SetActive(false);
    if (!DisconnectPopup_Object.activeSelf)
    {
      if (MainPopup_Object) MainPopup_Object.SetActive(false);
    }
  }

  private void ChangeSound()
  {
    audioController.ChangeVolume("wl", Sound_slider.value);
    audioController.ChangeVolume("button", Sound_slider.value);
  }

  private void ChangeMusic()
  {
    audioController.ChangeVolume("bg", Music_slider.value);
  }

  private void ChangePage(bool Increment)
  {
    foreach (GameObject t in Info_Screens)
    {
      t.SetActive(false);
    }

    if (Increment)
    {
      if (screenCounter == Info_Screens.Length - 1)
      {
        screenCounter = 0;
      }
      else
      {
        screenCounter++;
      }
    }
    else
    {
      if (screenCounter == 0)
      {
        screenCounter = Info_Screens.Length - 1;
      }
      else
      {
        screenCounter--;
      }
    }
    Info_Screens[screenCounter].SetActive(true);
  }
}
