using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BonusController : MonoBehaviour
{
  [SerializeField] private GameObject bonus_game;
  [SerializeField] private SlotBehaviour slotManager;
  [SerializeField] internal GameObject raycastPanel;
  [SerializeField] private AudioController _audioManager;
  [SerializeField] private TMP_Text m_Score;
  [SerializeField] private List<int> gemValues;
  [SerializeField] internal int currentBreakCount = 0;
  [SerializeField] internal int maxBreakCount = 0;
  private double TotalBonusWin = 0;

  internal double OnBreakGem()
  {
    currentBreakCount++;
    double m_value = gemValues[currentBreakCount - 1] * slotManager.currentBet;
    TotalBonusWin += m_value;
    Debug.Log(string.Concat("<color=cyan><b>", slotManager.currentBet, m_value, "</b></color>"));
    m_Score.text = TotalBonusWin.ToString();
    return m_value;
  }

  internal void GameOver()
  {
    currentBreakCount = 0;
    maxBreakCount = 0;
    Invoke("OnGameOver", 1.5f);
  }

  void OnGameOver()
  {
    slotManager.CheckPopups = false;
    _audioManager.SwitchBGSound(false);
    if (bonus_game) bonus_game.SetActive(false);
  }

  internal void StartBonus(List<int> values)
  {

    gemValues.Clear();
    gemValues.TrimExcess();
    gemValues = values;
    _audioManager.SwitchBGSound(true);
    if (bonus_game) bonus_game.SetActive(true);
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
