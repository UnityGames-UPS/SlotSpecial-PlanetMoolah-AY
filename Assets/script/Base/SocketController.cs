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
    [SerializeField] private UI_Controller uiManager;
    //WebSocket currentSocket = null;
    internal bool isResultdone = false;

    private SocketManager manager;


    protected string SocketURI = null;

    [SerializeField] internal JSFunctCalls JSManager;

    internal GameData InitialData = null;
    internal UiData UIData = null;
    internal Root ResultData = null;
    internal Player PlayerData = null;
    internal Features FreeSpinInit = null;
    [SerializeField]
    internal List<string> bonusdata = null;


    // TODO: PM to be changed
    // protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
    // protected string TestSocketURI = "https://7p68wzhv-5000.inc1.devtunnels.ms/";
    [SerializeField] protected string TestSocketURI = "https://m88wskhs-5001.inc1.devtunnels.ms/";
    // protected string SocketURI = "http://localhost:5000";
    protected string nameSpace = "playground"; //BackendChanges
    private Socket gameSocket; //BackendChanges
    [SerializeField]
    private string testToken;

    // [x]: PM to be added
    // protected string gameID = "";
    protected string gameID = "SL-PML";

    internal bool isLoading;
    internal bool SetInit = false;
    private const int maxReconnectionAttempts = 6;
    private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

    // internal Action<List<Symbol>, List<List<int>>, PlayerData> InitiateUI;
    internal Action ShowDisconnectionPopUp = null;
    internal Action ShowAnotherDevicePopUp = null;

    internal bool isExit;
    private bool firstTime = true;

    private bool isConnected = false; //Back2 Start
    private bool hasEverConnected = false;
    private const int MaxReconnectAttempts = 5;
    private const float ReconnectDelaySeconds = 2f;

    private float lastPongTime = 0f;
    private float pingInterval = 2f;
    private float pongTimeout = 3f;
    private bool waitingForPong = false;
    private int missedPongs = 0;
    private const int MaxMissedPongs = 5;
    private Coroutine PingRoutine; //Back2 end
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




    void ReceiveAuthToken(string jsonData)
    {
        Debug.Log("Received data: " + jsonData);

        // Parse the JSON data
        var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
        SocketURI = data.socketURL;
        Debug.Log("socekt url: " + data.socketURL);
        myAuth = data.cookie;
        nameSpace = data.nameSpace;
        // Proceed with connecting to the server using myAuth and socketURL
    }

    string myAuth = null;

    internal void OpenSocket()
    {
        SocketOptions options = new SocketOptions(); //Back2 Start
        options.AutoConnect = false;
        options.Reconnection = false;
        options.Timeout = TimeSpan.FromSeconds(3); //Back2 end

        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket; //BackendChanges

        // Application.ExternalCall("window.parent.postMessage", "authToken", "*");

#if UNITY_WEBGL && !UNITY_EDITOR
            JSManager.SendCustomMessage("authToken");
            StartCoroutine(WaitForAuthToken(options));
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
    internal void closeSockets()
    {
        StartCoroutine(CloseSocket());
    }
    internal IEnumerator CloseSocket() //Back2 Start
    {
        uiManager.RaycastBlocker.SetActive(true);
        ResetPingRoutine();

        Debug.Log("Closing Socket");

        manager?.Close();
        manager = null;

        Debug.Log("Waiting for socket to close");

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
    } //Back2 end
    void CloseGame()
    {
        Debug.Log("Unity: Closing Game");
        StartCoroutine(CloseSocket());
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

    void OnConnected(ConnectResponse resp) //Back2 Start
    {
        Debug.Log("✅ Connected to server.");

        if (hasEverConnected)
        {
            uiManager.CheckAndClosePopups();
        }

        isConnected = true;
        hasEverConnected = true;
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        SendPing();
    } //Back2 end



    private void OnDisconnected() //Back2 Start
    {
        Debug.LogWarning("⚠️ Disconnected from server.");
        isConnected = false;
        ResetPingRoutine();
    } //Back2 end
    private void OnPongReceived(string data) //Back2 Start
    {
        Debug.Log("✅ Received pong from server.");
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
        Debug.Log($"📦 Pong payload: {data}");
    } //Back2 end
    private void SendPing() //Back2 Start
    {
        ResetPingRoutine();
        PingRoutine = StartCoroutine(PingCheck());
    }
    private IEnumerator PingCheck()
    {
        while (true)
        {
            Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

            if (missedPongs == 0)
            {
                uiManager.CheckAndClosePopups();
            }

            // If waiting for pong, and timeout passed
            if (waitingForPong)
            {
                if (missedPongs == 2)
                {
                    uiManager.ReconnectionPopup();
                }
                missedPongs++;
                Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

                if (missedPongs >= MaxMissedPongs)
                {
                    Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
                    isConnected = false;
                    uiManager.DisconnectionPopup();
                    yield break;
                }
            }

            // Send next ping
            waitingForPong = true;
            lastPongTime = Time.time;
            Debug.Log("📤 Sending ping...");
            SendDataWithNamespace("ping");
            yield return new WaitForSeconds(pingInterval);
        }
    } //Back2 end
    void ResetPingRoutine()
    {
        if (PingRoutine != null)
        {
            StopCoroutine(PingRoutine);
        }
        PingRoutine = null;
    }

    private void OnError(string response)
    {
        Debug.LogError("Error: " + response);
    }
    private void OnError()
    {
        Debug.LogError("Socket Error");
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
        gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected); //Back2 Start
        gameSocket.On(SocketIOEventTypes.Error, OnError); //Back2 Start
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("result", OnResult);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("pong", OnPongReceived); //Back2 Start
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
        manager.Open();
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
    JSManager.SendCustomMessage("OnExit");
#endif
    }
    //     internal void CloseSocket()
    //     {
    //         isExit = true;
    //         SendData("OnExit");

    // #if UNITY_WEBGL && !UNITY_EDITOR
    //                         JSManager.SendCustomMessage("OnExit");
    // #endif

    //         DOVirtual.DelayedCall(0.1f, () =>
    //         {
    //             if (this.manager != null)
    //             {
    //                 Debug.Log("Dispose my Socket");
    //                 this.manager.Close();
    //             }
    //         });
    //     }

    private void ParseResponse(string jsonObject)
    {


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
                FreeSpinInit = myData.features;
                // bonusdata = GetBonusData(myData.gameData.spinBonus); 
                slotManager.InitiateUI(UIData.paylines.symbols, myData.player.balance, FreeSpinInit.freeSpin);



#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif
                uiManager.RaycastBlocker.SetActive(false);
            }
        }
        else if (id == "ResultData")
        {
            // ResultData = myData;
            ResultData = myData;
            PlayerData = myData.player;
            isResultdone = true;
            foreach (var x in ResultData.payload.cascades)
            {
                for (int i = 0; i < x.winnings.Count; i++)
                {

                    x.winnings[i].positions = Helper.ConvertSymbolToPos(x.winnings[i].symbolsToEmit);
                }
            }
            // Debug.Log("Dev Test : ggoo" + ResultData.payload.cascades.Count);
        }
        else if (id == "ExitUser")
        {
            if (this.manager != null)
            {
                Debug.Log("Dispose my Socket");
                this.manager.Close();
            }
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("OnExit");
#endif
        }
    }

    List<string> GetBonusData(List<int> bonusData)
    {
        Debug.Log("Ashu Test: " + "0000000" + bonusData.Count);
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
        message.payload = new SentDeta();
        message.type = "SPIN";
        Debug.Log(slotManager.betCounter);
        message.payload.betIndex = slotManager.betCounter;
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
public class SentDeta
{
    public int betIndex;
    public string Event;
    public double lastWinning;
    public int index;
}



[Serializable]
public class MessageData
{
    public string type;

    public SentDeta payload;

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
    public string id { get; set; }
    public List<List<string>> matrix { get; set; }
    public Payload payload { get; set; }

    //Initial Data

    public GameData gameData { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }
    public Features features { get; set; }
}
public class Features
{
    public FreeSpin freeSpin { get; set; }
    public Jackpot jackpot { get; set; }
}
[Serializable]
public class Scatter
{
    public double amount { get; set; }
}
[Serializable]
public class Jackpot
{
    public bool enabled { get; set; }
    public int minSymbolCount { get; set; }
    public int defaultAmount { get; set; }
}
public class Payload
{
    public double totalWin { get; set; }
    public bool isJackpot { get; set; }
    public int jackpotWin { get; set; }
    public List<Cascade> cascades { get; set; }
    public int cascadeCount { get; set; }
    public double totalCascadeWin { get; set; }
    public bool isFreeSpin { get; set; }
    public int freeSpinCount { get; set; }
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
    // public List<Winning> winningLines { get; set; }
    public List<List<int>> symbolsToFill { get; set; }
    public double currentCascadeWin { get; set; }
    public int cascadeNumber { get; set; }
    public List<Winning> winnings { get; set; }

}


public class Winning
{
    public List<List<string>> newMatrix { get; set; }
    public int lineIndex { get; set; }
    public List<int> positions { get; set; }
    public List<int> line { get; set; }
    public List<string> symbolsToEmit { get; set; }
    public double win { get; set; }
}

public class FreeSpin
{
    public bool enabled { get; set; }
    public List<List<int>> multiplier { get; set; }
}