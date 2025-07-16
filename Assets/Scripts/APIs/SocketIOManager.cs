using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;

public class SocketIOManager : MonoBehaviour
{
  [SerializeField]
  private SlotBehaviour slotManager;

  [SerializeField]
  private UIManager uiManager;

  [SerializeField] private GambleController gambleController;
  [SerializeField] private BonusController bonusController;

  internal Root initialData = new();
  internal UIData initUIData = null;
  internal Root resultData = new();
  internal Player playerdata = null;
  internal Message myMessage = null;
  internal Root GambleData = new();
  internal double GambleLimit = 0;
  [SerializeField]
  internal Root bonusData = null;
  private SocketManager manager;
  protected string SocketURI = null;
  // protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
  protected string TestSocketURI = "http://localhost:5000/";
  // protected string nameSpace="game"; //BackendChanges
  protected string nameSpace = "playground"; //BackendChanges
  private Socket gameSocket; //BackendChanges
  [SerializeField]
  private string testToken;
  internal bool isResultdone = false;
  [SerializeField] internal JSFunctCalls JSManager;
  // protected string gameID = "";
  protected string gameID = "SL-RC";

  internal bool SetInit = false;

  private const int maxReconnectionAttempts = 6;
  private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);
  string myAuth = null;

  private void Start()
  {
    SetInit = false;
    OpenSocket();
  }

  void ReceiveAuthToken(string jsonData)
  {
    Debug.Log("Received data: " + jsonData);

    // Parse the JSON data
    var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
    SocketURI = data.socketURL;
    myAuth = data.cookie;
    nameSpace = data.nameSpace;
  }


  private void OpenSocket()
  {
    //Create and setup SocketOptions
    SocketOptions options = new SocketOptions();
    options.ReconnectionAttempts = maxReconnectionAttempts;
    options.ReconnectionDelay = reconnectionDelay;
    options.Reconnection = true;
    options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket; //BackendChanges

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = testToken
      };
    };
    options.Auth = authFunction;

    SetupSocketManager(options);
#endif
  }

  private IEnumerator WaitForAuthToken(SocketOptions options)
  {
    // Wait until myAuth is not null
    while (myAuth == null)
    {
      yield return null;
    }

    // Once myAuth is set, configure the authFunction
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = myAuth
      };
    };
    options.Auth = authFunction;

    Debug.Log("Auth function configured with token: " + myAuth);

    // Proceed with connecting to the server
    SetupSocketManager(options);
  }

  private void SetupSocketManager(SocketOptions options)
  {
    // Create and setup SocketManager
#if UNITY_EDITOR
    this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
    this.manager = new SocketManager(new Uri(SocketURI), options);
#endif

    if (string.IsNullOrEmpty(nameSpace))
    {  //BackendChanges Start
      gameSocket = manager.Socket;
    }
    else
    {
      print("nameSpace: " + nameSpace);
      gameSocket = manager.GetSocket("/" + nameSpace);
    }
    // Set subscriptions
    gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
    gameSocket.On<string>(SocketIOEventTypes.Disconnect, OnDisconnected);
    gameSocket.On<string>(SocketIOEventTypes.Error, OnError);
    gameSocket.On<string>("game:init", OnListenEvent);
    gameSocket.On<string>("result", OnListenEvent);
    gameSocket.On<bool>("socketState", OnSocketState);
    gameSocket.On<string>("internalError", OnSocketError);
    gameSocket.On<string>("alert", OnSocketAlert);
    gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
  }

  // Connected event handler implementation
  void OnConnected(ConnectResponse resp)
  {
    Debug.Log("Connected!");
    SendPing();
  }

  private void OnDisconnected(string response)
  {
    Debug.Log("Disconnected from the server");
    StopAllCoroutines();
    uiManager.DisconnectionPopup();
  }

  private void OnError(string response)
  {
    Debug.LogError("Error: " + response);
  }

  private void OnListenEvent(string data)
  {
    ParseResponse(data);
  }
  private void OnSocketState(bool state)
  {
    if (state)
    {
      Debug.Log("my state is " + state);
    }
  }
  private void OnSocketError(string data)
  {
    Debug.Log("Received error with data: " + data);
  }
  private void OnSocketAlert(string data)
  {
    Debug.Log("Received alert with data: " + data);
  }

  private void OnSocketOtherDevice(string data)
  {
    Debug.Log("Received Device Error with data: " + data);
    uiManager.ADfunction();
  }

  private void SendPing()
  {
    InvokeRepeating("AliveRequest", 0f, 3f);
  }

  private void AliveRequest()
  {
    SendDataWithNamespace("YES I AM ALIVE");
  }

  private void ParseResponse(string jsonObject)
  {
    Debug.Log(jsonObject);
    Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

    playerdata = myData.player;

    string id = myData.id;
    switch (id)
    {
      case "initData":
        {
          initialData = myData;
          if (!SetInit)
          {
            InitUI();
            SetInit = true;
          }
          else
          {
            RefreshUI();
          }
          break;
        }
      case "ResultData":
        {
          resultData = myData;
          isResultdone = true;
          break;
        }
      case "gambleInit":
        {
          GambleData = myData;
          resultData.player = GambleData.player;
          resultData.payload = GambleData.payload;
          gambleController.WaitForGambleResult = false;
          break;
        }
      case "gambleCollect":
        {
          GambleData = myData;
          resultData.player = GambleData.player;
          resultData.payload = GambleData.payload;
          gambleController.WaitForGambleResult = false;
          slotManager.UpdateBalanceAndTotalWin();
          isResultdone = true;
          break;
        }
      case "gambleDraw":
        {
          GambleData = myData;
          resultData.player = GambleData.player;
          resultData.payload = GambleData.payload;
          gambleController.WaitForGambleResult = false;
          break;
        }
      case "bonusResult":
        {
          bonusData = myData;
          bonusController.WaitForBonusResult = false;
          break;
        }
      case "ExitUser":
        {
          if (gameSocket != null) //BackendChanges
          {
            gameSocket.Disconnect();
            manager.Close();
          }
          break;
        }
    }
  }

  private void RefreshUI()
  {
    uiManager.InitialiseUIData(initialData.uiData.paylines);
  }

  private void InitUI()
  {
    slotManager.SetInitialUI();
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnEnter");
#endif
  }

  internal void CloseSocket()
  {
    SendDataWithNamespace("game:exit");
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit");
#endif
  }

  internal void AccumulateResult(int currBet)
  {
    isResultdone = false;
    MessageData message = new()
    {
      type = "SPIN"
    };
    message.payload.betIndex = currBet;

    // Serialize message data to JSON
    string json = JsonUtility.ToJson(message);
    SendDataWithNamespace("request", json);
  }

  private void SendDataWithNamespace(string eventName, string json = null)
  {
    // Send the message
    if (gameSocket != null && gameSocket.IsOpen)
    {
      if (json != null)
      {
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);
      }
      else
      {
        gameSocket.Emit(eventName);
      }
    }
    else
    {
      Debug.LogWarning("Socket is not connected.");
    }
  }

  internal void OnGambleInit()
  {
    MessageData message = new MessageData();
    message.type = "GAMBLE";
    message.payload.betIndex = slotManager.BetCounter;
    message.payload.Event = "init";

    string json = JsonUtility.ToJson(message);
    Debug.Log("OnGambleInit Data Sent: " + json);
    SendDataWithNamespace("request", json);
  }

  internal void OnGambleCollect()
  {
    MessageData message = new MessageData();
    message.type = "GAMBLE";
    message.payload.betIndex = slotManager.BetCounter;
    message.payload.Event = "collect";

    string json = JsonUtility.ToJson(message);
    Debug.Log("OnGambleCollect Data Sent: " + json);
    SendDataWithNamespace("request", json);
  }

  internal void OnGambleDraw()
  {
    MessageData message = new MessageData();
    message.type = "GAMBLE";
    message.payload.betIndex = slotManager.BetCounter;
    message.payload.Event = "draw";

    string json = JsonUtility.ToJson(message);
    Debug.Log("OnGambleDraw Data Sent: " + json);
    SendDataWithNamespace("request", json);
  }

  internal void OnBonusCollect(int index)
  {
    isResultdone = false;

    MessageData message = new MessageData();
    message.type = "BONUS";
    message.payload.betIndex = slotManager.BetCounter;
    message.payload.index = index;
    message.payload.Event = "tap";

    string json = JsonUtility.ToJson(message);
    Debug.Log("OnBonusCollect Data Sent: " + json);
    SendDataWithNamespace("request", json);
  }

  private List<string> RemoveQuotes(List<string> stringList)
  {
    for (int i = 0; i < stringList.Count; i++)
    {
      stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
    }
    return stringList;
  }

  private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
  {
    List<string> resultList = new List<string>();

    foreach (List<int> innerList in listOfLists)
    {
      // Convert each integer in the inner list to string
      List<string> stringList = new List<string>();
      foreach (int number in innerList)
      {
        stringList.Add(number.ToString());
      }

      // Join the string representation of integers with ","
      string joinedString = string.Join(",", stringList.ToArray()).Trim();
      resultList.Add(joinedString);
    }

    return resultList;
  }

  private List<string> ConvertListOfListsToStrings(List<List<string>> inputList)
  {
    List<string> outputList = new List<string>();

    foreach (List<string> row in inputList)
    {
      string concatenatedString = string.Join(",", row);
      outputList.Add(concatenatedString);
    }

    return outputList;
  }

  private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
  {
    // Flattened list
    List<string> flattenedList = new List<string>();
    foreach (List<string> sublist in originalList)
    {
      flattenedList.AddRange(sublist);
    }

    // Remove recurring elements
    HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

    // Transformed list
    List<string> transformedList = new List<string>();
    foreach (string element in uniqueElements)
    {
      transformedList.Add(element.Replace(",", ""));
    }

    return transformedList;
  }
}

[Serializable]
public class BetData
{
  public double currentBet;
  public double currentLines;
  public double spins;
}

[Serializable]
public class AuthData
{
  public string GameID;
}

[Serializable]
public class MessageData
{
  public string type;
  public Data payload = new();

}
[Serializable]
public class Data
{
  public int betIndex;
  public string Event;
  public int index;
  public int option;
}

[Serializable]
public class ExitData
{
  public string id;
}

[Serializable]
public class InitData
{
  public AuthData Data;
  public string id;
}

[Serializable]
public class AbtLogo
{
  public string logoSprite { get; set; }
  public string link { get; set; }
}

[Serializable]
public class GameData
{
  public List<List<int>> lines;
  public List<double> bets;
  public List<int> spinBonus;
}

[Serializable]
public class FreeSpins
{
  public int count { get; set; }
  public bool isNewAdded { get; set; }
}

[Serializable]
public class GambleData
{
  public string GAMBLETYPE;
}

[Serializable]
public class RiskData
{
  public GambleData data;
  public string id;
}

[Serializable]
public class Message
{
  public PlayerData PlayerData { get; set; }
  public HighCard highCard { get; set; }
  public LowCard lowCard { get; set; }
  public List<ExCard> exCards { get; set; }
  public bool playerWon { get; set; }
  public double Balance { get; set; }
  public double currentWining { get; set; }
  public double maxGambleBet { get; set; }
}

[SerializeField]
public class Bonus
{
  public bool isTriggered { get; set; }
}

[SerializeField]
public class Win
{
  public int line { get; set; }
  public List<int> positions { get; set; }
  public double amount { get; set; }
}

public class Payload
{
  public double winAmount { get; set; }
  public List<Win> wins { get; set; }
  public double payout { get; set; }
  public Cards cards { get; set; }
  public bool playerWon { get; set; }
}

[Serializable]
public class Cards
{
  public int dealerCard { get; set; }
  public int playerCard { get; set; }
}

[Serializable]
public class HighCard
{
  public string suit { get; set; }
  public string value { get; set; }
}

[Serializable]
public class LowCard
{
  public string suit { get; set; }
  public string value { get; set; }
}

[Serializable]
public class ExCard
{
  public string suit { get; set; }
  public string value { get; set; }
}

[Serializable]
public class Root
{
  public string id { get; set; }
  public List<List<string>> matrix = new();
  public Message message { get; set; }

  public GameData gameData { get; set; }
  public UiData uiData { get; set; }
  public Player player { get; set; }
  public Payload payload { get; set; }
  public Bonus bonus { get; set; }
}

[Serializable]
public class UiData
{
  public Paylines paylines { get; set; }
}

[Serializable]
public class Player
{
  public double balance { get; set; }
}

[Serializable]
public class UIData
{
  public Paylines paylines { get; set; }
  public AbtLogo AbtLogo { get; set; }
  public string ToULink { get; set; }
  public string PopLink { get; set; }
}

[Serializable]
public class Paylines
{
  public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Symbol
{
  public int id { get; set; }
  public string name { get; set; }
  public List<int> multiplier { get; set; }
  public string description { get; set; }
}

[Serializable]
public class PlayerData
{
  public double balance { get; set; }
  public double haveWon { get; set; }
  public double currentWining { get; set; }
}

[Serializable]
public class AuthTokenData
{
  public string cookie;
  public string socketURL;
  public string nameSpace;
}
