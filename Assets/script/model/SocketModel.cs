using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

internal class SocketModel
{

    public PlayerData playerData;
    public UIData uIData;

    public InitGameData initGameData;

    public ResultGameData resultGameData;

    public int currentBetIndex=0;
    internal SocketModel(){

        this.playerData= new PlayerData();
        this.uIData= new UIData();
        this.initGameData= new InitGameData();
        this.resultGameData= new ResultGameData();
    }

}



[Serializable]
public class InitGameData
{
    public List<List<string>> Reel { get; set; }
    public List<List<int>> Lines { get; set; }
    public List<double> Bets { get; set; }
    public bool canSwitchLines { get; set; }
    public List<int> LinesCount { get; set; }
    public List<int> autoSpin { get; set; }

    public List<List<int>> lineData {get; set;}
}

[Serializable]
public class ResultGameData
{
    public List<List<int>> ResultReel { get; set; }
    public List<Cascading> cascadeData{get; set;}
    public List<int> autoSpin { get; set; }
    public List<int> linesToEmit { get; set; }
    public List<List<string>> symbolsToEmit { get; set; }
    public double WinAmout { get; set; }
    public double freeSpins { get; set; }
    public double jackpot { get; set; }
    public bool isBonus { get; set; }
    public double BonusStopIndex { get; set; }
}

[Serializable]
public class PlayerData
{
    public double Balance { get; set; }
    public int HaveWon { get; set; }
    public double CurrentWining { get; set; }
}

[Serializable]
public class UIData
{
    public List<Symbol> symbols { get; set; }

}



[Serializable]
public class Symbol
{
    public int ID { get; set; }
    public string Name { get; set; }
    public JToken Multiplier { get; set;}

    // [JsonProperty("multiplier")]
    // public object MultiplierObject { get; set; }

    // [JsonIgnore]
    // public List<List<int>> Multiplier { get; set; }

    // [OnDeserialized]
    // internal void OnDeserializedMethod(StreamingContext context)
    // {
 
    //     if (MultiplierObject is JObject)
    //     {
    //         Multiplier = new List<List<int>>();
    //     }
    //     else
    //     {

    //         Multiplier = JsonConvert.DeserializeObject<List<List<int>>>(MultiplierObject.ToString());
    //     }
    // }
    public object defaultAmount { get; set; }
    public object symbolsCount { get; set; }
    public object increaseValue { get; set; }
    public int freeSpin { get; set; }
}


[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
}

[Serializable]
public class Cascading
{

    public List<List<int>> symbolsToFill{get; set;}
    public List<List<string>> winingSymbols {get; set;}
    public List<int> lineToEmit{get; set;}
    
    public double currentWinning{get; set;}


}
