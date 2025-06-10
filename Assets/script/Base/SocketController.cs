using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;

public class SocketController : MonoBehaviour
{

    internal SocketModel socketModel = new SocketModel();

    [SerializeField] private Slot_Manager slotManager;
    //WebSocket currentSocket = null;
    internal bool isResultdone = false;

    private SocketManager manager;


    protected string SocketURI = null;

    [SerializeField] internal JSFunctCalls JSManager;

    internal GameData InitialData = null;
    internal UiData UIData = null;
    internal Root ResultData = null;
    internal Player PlayerData = null;
    [SerializeField]
    internal List<string> bonusdata = null;


    // TODO: PM to be changed
    // protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
    // protected string TestSocketURI = "https://7p68wzhv-5000.inc1.devtunnels.ms/";
    protected string TestSocketURI = "https://m88wskhs-5001.inc1.devtunnels.ms/";
    // protected string SocketURI = "http://localhost:5000";
    protected string nameSpace = "playground"; //BackendChanges
    private Socket gameSocket; //BackendChanges
    [SerializeField]
    private string testToken;

    // [x]: PM to be added
    // protected string gameID = "";
    protected string gameID = "SL-PM";

    internal bool isLoading;
    internal bool SetInit = false;
    private const int maxReconnectionAttempts = 6;
    private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

   // internal Action<List<Symbol>, List<List<int>>, PlayerData> InitiateUI;
    internal Action ShowDisconnectionPopUp = null;
    internal Action ShowAnotherDevicePopUp = null;

    internal bool isExit;
    private bool firstTime = true;
    private void Awake()
    {
        isLoading = true;
        SetInit = false;
        // OpenSocket();

        // Debug.unityLogger.logEnabled = false;
    }

    private void Start()
    {
       
         OpenSocket();
    }




    //void ReceiveAuthToken(string jsonData)
    //{
    //    Debug.Log("Received data: " + jsonData);

    //    // Parse the JSON data
    //    var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
    //    SocketURI = data.socketURL;
    //    Debug.Log("socekt url: "+data.socketURL);
    //    myAuth = data.cookie;
    //    nameSpace = data.nameSpace;
    //    // Proceed with connecting to the server using myAuth and socketURL
    //}

    string myAuth = null;

    internal void OpenSocket()
    {
        // Create and setup SocketOptions
        SocketOptions options = new SocketOptions();
        options.ReconnectionAttempts = maxReconnectionAttempts;
        options.ReconnectionDelay = reconnectionDelay;
        options.Reconnection = true;

        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket; //BackendChanges

        // Application.ExternalCall("window.parent.postMessage", "authToken", "*");

#if UNITY_WEBGL && !UNITY_EDITOR
    string url = Application.absoluteURL;
    Debug.Log("Unity URL : " + url);
    ExtractUrlAndToken(url);

    Func<SocketManager, Socket, object> webAuthFunction = (manager, socket) =>
    {
      return new
      {
        token = testToken,
      };
    };
    options.Auth = webAuthFunction;
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = testToken,
            };
        };
        options.Auth = authFunction;
#endif
        // Proceed with connecting to the server
        SetupSocketManager(options);
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
                token = myAuth,
                gameId = gameID
            };
        };
        options.Auth = authFunction;

        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);
    }
    private void OnSocketState(bool state)
    {
        if (state)
        {
            Debug.Log("my state is " + state);
            InitRequest("AUTH");
        }
        else
        {

        }
    }
    private void OnSocketError(string data)
    {
        Debug.Log("Received error with data: " + data);
    }
    private void OnSocketAlert(string data)
    {
        Debug.Log("Received alert with data: " + data);
        // AliveRequest("YES I AM ALIVE");
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        // uIManager.ADfunction();
        ShowAnotherDevicePopUp?.Invoke();
    }

    private void AliveRequest()
    {
        SendData("YES I AM ALIVE");
    }

    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Connected!");
        SendPing();

        //InitRequest("AUTH");
    }

    private void SendPing()
    {
        InvokeRepeating("AliveRequest", 0f, 3f);
    }

    private void OnDisconnected(string response)
    {
        Debug.Log("Disconnected from the server");
        StopAllCoroutines();
        if (!isExit)
            ShowDisconnectionPopUp?.Invoke();
        // uIManager.DisconnectionPopup();
    }

    private void OnError(string response)
    {
        Debug.LogError("Error: " + response);
    }

    private void OnListenEvent(string data)
    {
        ParseResponse(data);
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
            gameSocket = this.manager.Socket;
        }
        else
        {
            print("nameSpace: " + nameSpace);
            gameSocket = this.manager.GetSocket("/" + nameSpace);
        }
        // Set subscriptions
        gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        gameSocket.On<string>(SocketIOEventTypes.Disconnect, OnDisconnected);
        gameSocket.On<string>(SocketIOEventTypes.Error, OnError);
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("spin:result", OnResult);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
    }

    // Connected event handler implementation

    internal void InitRequest(string eventName)
    {
        var initmessage = new { Data = new { GameID = gameID }, id = "Auth" };
        SendData(eventName, initmessage);
    }
    internal void closeSocketReactnativeCall()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
    }
    internal void CloseSocket()
    {
        isExit = true;
        SendData("EXIT");

#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("onExit");
#endif

        DOVirtual.DelayedCall(0.1f, () =>
        {
            if (this.manager != null)
            {
                Debug.Log("Dispose my Socket");
                this.manager.Close();
            }
        });
    }

    private void ParseResponse(string jsonObject)
    {
        //Debug.Log(jsonObject);

        //JObject resp = JObject.Parse(jsonObject);

        //string messageId = resp["id"].ToString();

        //var message = resp["message"];
        //var gameData = message["GameData"];
        //socketModel.playerData = message["PlayerData"].ToObject<PlayerData>();

        Debug.Log(jsonObject);
        Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

        string id = myData.id;

        if (id == "initData")
        {
            if (firstTime)
            {
                firstTime = false;
                InitialData = myData.gameData;
                UIData = myData.uiData;
                PlayerData = myData.player;
               // bonusdata = GetBonusData(myData.gameData.spinBonus); 
                slotManager.InitiateUI(UIData.paylines.symbols, myData.player.balance);

            

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif

            }
        }
        else if (id == "ResultData")
        {
            // socketModel.resultGameData = gameData.ToObject<ResultGameData>();
            ResultData = myData;
            PlayerData = myData.player;
            isResultdone = true;

            Debug.Log(ResultData.cascades.Count);
        }
        else if (id == "ExitUser")
        {
            if (this.manager != null)
            {
                Debug.Log("Dispose my Socket");
                this.manager.Close();
            }
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("onExit");
#endif
        }
    }

    List<string> GetBonusData(List<int> bonusData)
    {
        Debug.Log("Ashu Test: " + "0000000"+bonusData.Count);
        List<string> bonusDataString = new List<string>();

        foreach (int data in bonusData)
        {
           
            bonusDataString.Add(data.ToString());
        }
        return bonusDataString;
    }

    public void ExtractUrlAndToken(string fullUrl)
    {
        Uri uri = new Uri(fullUrl);
        string query = uri.Query; // Gets the query part, e.g., "?url=http://localhost:5000&token=e5ffa84216be4972a85fff1d266d36d0"

        Dictionary<string, string> queryParams = new Dictionary<string, string>();
        string[] pairs = query.TrimStart('?').Split('&');

        foreach (string pair in pairs)
        {
            string[] kv = pair.Split('=');
            if (kv.Length == 2)
            {
                queryParams[kv[0]] = Uri.UnescapeDataString(kv[1]);
            }
        }
    }
        void OnResult(string data)
    {
        ParseResponse(data);
    }
    internal void AccumulateResult(int currBet)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.currentBet = currBet;
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("spin:request", json);
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

    internal void SendData(string eventName, object message = null)
    {

        if (gameSocket == null || !gameSocket.IsOpen)
        {
            Debug.LogWarning("Socket is not connected.");
            return;
        }
        if (message == null)
        {
            gameSocket.Emit(eventName);
            return;
        }
        isResultdone = false;
        string json = JsonConvert.SerializeObject(message);
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);

    }

    IEnumerator DelayedCall()
    {
        Debug.Log("Start Delay");
        yield return new WaitForSeconds(1f);
        Debug.Log("After Delay");


    }

}





[Serializable]
public class MessageData
{
    public int currentBet;
}

[Serializable]
public class GameData
{
    public List<List<int>> lines { get; set; }
    public List<double> bets { get; set; }
    public List<int> spinBonus { get; set; }
}



[Serializable]
public class FreeSpins
{
    public int count { get; set; }
    public bool isFreeSpin { get; set; }
}

[SerializeField]
public class Bonus
{
    public int BonusSpinStopIndex { get; set; }
    public double amount { get; set; }
}

[Serializable]
public class Root
{
    //Result Data
    public bool success { get; set; }
    public List<List<string>> matrix { get; set; }
    public string name { get; set; }
    public Payload payload { get; set; }
    public Bonus bonus { get; set; }
    public Jackpot jackpot { get; set; }
    public Scatter scatter { get; set; }
    public FreeSpins freeSpin { get; set; }
    public List<Cascade> cascades { get; set; }
    public double totalWin { get; set; }
    //Initial Data
    public string id { get; set; }
    public GameData gameData { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }
}
[Serializable]
public class Scatter
{
    public double amount { get; set; }
}
[Serializable]
public class Jackpot
{
    public bool isTriggered { get; set; }
    public double amount { get; set; }
}
[Serializable]
public class Payload
{
    public double winAmount { get; set; }
    public List<Win> wins { get; set; }
}

[Serializable]
public class Win
{
    public int line { get; set; }
    public List<int> positions { get; set; }
    public double amount { get; set; }
}

[Serializable]
public class UiData
{
    public Paylines paylines { get; set; }
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
public class Player
{
    public double balance { get; set; }
}

[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
    public string nameSpace; //BackendChanges
}


[Serializable]
public class Cascade
{
    public int cascadeIndex { get; set; }
    public List<WinningLine> winningLines { get; set; }
    public List<List<string>> symbolsToFill { get; set; }
    public double currentCascadeWin { get; set; }
}

public class WinningLine
{
    public int lineIndex { get; set; }
    public string symbols { get; set; }
    public List<int> positions { get; set; }
}
