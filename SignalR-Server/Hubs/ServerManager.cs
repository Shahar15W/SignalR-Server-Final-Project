using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Security.AccessControl;

namespace SignalR_Server.Hubs
{
    public class ServerManager
    {
        private static readonly ServerManager _instance = new ServerManager();
        private readonly Dictionary<string, Lobby> lobbies = new Dictionary<string, Lobby>();
        private readonly object locker = new object();

        public static ServerManager Instance => _instance;

        public event Action<string, string> PlayerJoinedLobby;
        public event Action<string, string> PlayerLeftLobby;
        public event Action<string> LobbyCreated;

        private ServerManager()
        {
            // Initialize any other resources if needed
        }

        public void CreateLobby(string lobbyId, string gameType)
        {
            lock (locker)
            {
                if (!lobbies.ContainsKey(lobbyId))
                {
                    string json = File.ReadAllText("F:/מדעי המחשב 2023/פרויקט/SignalR-Server/SignalR-Server/Creations/" + gameType + "/data.json");
                    Console.WriteLine(json);
                    lobbies[lobbyId] = new Lobby(json, gameType, new List<string>());
                    LobbyCreated?.Invoke(lobbyId);
                }
            }
        }

        public bool JoinLobby(string lobbyId, string playerName)
        {
            lock (locker)
            {
                if (lobbies.ContainsKey(lobbyId))
                {
                    lobbies[lobbyId].Players.Add(playerName);
                    PlayerJoinedLobby?.Invoke(lobbyId, playerName);
                    Console.Write($"Players in lobby {lobbyId}: {string.Join(", ", GetPlayersInLobby(lobbyId))}");
                    Console.WriteLine();
                    return true;
                }
                return false; // Lobby not found
            }
        }

        public void LeaveLobby(string lobbyId, string playerName)
        {
            lock (locker)
            {
                if (lobbies.ContainsKey(lobbyId))
                {
                    lobbies[lobbyId].Players.Remove(playerName);
                    PlayerLeftLobby?.Invoke(lobbyId, playerName);
                }
            }
        }

        public List<string> GetPlayersInLobby(string lobbyId)
        {
            lock (locker)
            {
                return lobbies.ContainsKey(lobbyId) ? new List<string>(lobbies[lobbyId].Players) : null;
            }
        }

        public List<Tuple<string,string, dynamic>> GetLobbies()
        {
            lock (locker)
            {
                var ListOfLobbies = new List<Tuple<string,string, dynamic>>(lobbies.Keys.Select(x => new Tuple<string, string, dynamic>(x, lobbies[(string)x].GameType, lobbies[(string)x].GameState)));
                Console.WriteLine($"All lobbies: {string.Join(", ", ListOfLobbies)}");
                return ListOfLobbies;
            }
        }

        public dynamic GetGameState(string lobbyId)
        {
            lock (locker)
            {
                string gameState = lobbies.ContainsKey(lobbyId) ? lobbies[lobbyId].GameState : null;
                Console.WriteLine($"Gamestate of lobby {lobbyId}:\n {gameState}");
                return gameState;
            }
        } 

        // Implement other methods as needed

        // Notify clients in a lobby about changes
        public void NotifyClientsInLobby(string lobbyId, string message)
        {
            // You can implement this method if needed
        }

        // Create a class to represent a lobby with properties
        public class Lobby
        {
            public dynamic GameState { get; set; }
            public string GameType { get; set; }
            public List<string> Players { get; set; }

            public Lobby(dynamic gameState, string gameType, List<string> players)
            {
                GameState = gameState;
                GameType = gameType;
                Players = players;
            }

            public Lobby(Lobby other)
            {
                GameState = other.GameState;
                GameType = other.GameType;
                Players = other.Players;
            }
        }
    }
}
