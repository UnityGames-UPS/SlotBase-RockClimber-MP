using UnityEngine;
using TMPro;

public class BonusController : MonoBehaviour
{
  [SerializeField] internal SlotBehaviour slotManager;
  [SerializeField] private AudioController _audioManager;
  [SerializeField] private GameObject bonus_game;
  [SerializeField] private TMP_Text m_Score;
  private double TotalBonusWin = 0;
  internal bool isOpening;
  internal bool isFinished;
  internal bool WaitForBonusResult = false;

  internal void StartBonus()
  {
    _audioManager.SwitchBGSound(true);

    TotalBonusWin = 0;
    m_Score.text = TotalBonusWin.ToString();
    isOpening = false;
    isFinished = false;
    WaitForBonusResult = false;

    if (bonus_game) bonus_game.SetActive(true);
  }
  
  internal void UpdateTotalWin(double value)
  {
    TotalBonusWin += value;
    m_Score.text = TotalBonusWin.ToString();
  }

  internal void GameOver()
  {
    isFinished = true;
    Invoke("OnGameOver", 1.5f);
  }

  void OnGameOver()
  {
    _audioManager.SwitchBGSound(false);
    if (bonus_game) bonus_game.SetActive(false);
    slotManager.WaitForBonus = false;
  }

  internal void PlayWinSound()
  {
    if (_audioManager) _audioManager.PlayBonusAudio("win");
  }

  internal void PlayLoseSound()
  {
    if (_audioManager) _audioManager.PlayBonusAudio("lose");
  }
}
