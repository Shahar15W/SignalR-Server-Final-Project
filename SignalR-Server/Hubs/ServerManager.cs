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
using System.Media;
using System.Runtime.Remoting.Contexts;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;

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

        public Dictionary<string, string> playerIds = new Dictionary<string, string>();

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
                    string path = (string)System.AppDomain.CurrentDomain.BaseDirectory.Split(new [] { "bin" }, StringSplitOptions.None)[0];

                    Console.WriteLine("Attempting to load path " + path);
                    string json = File.ReadAllText(path + "/Creations/" + gameType + "/data.json");
                    Console.WriteLine($"Lobby {lobbyId} created.");
                    lobbies[lobbyId] = new Lobby(json, gameType, new List<string>());
                    LobbyCreated?.Invoke(lobbyId);
                }
            }
        }

        public bool JoinLobby(string lobbyId, string playerName, string connectionId)
        {
            lock (locker)
            {
                if (lobbies.ContainsKey(lobbyId))
                {
                    lobbies[lobbyId].Players.Add(playerName);
                    PlayerJoinedLobby?.Invoke(lobbyId, playerName);
                    playerIds[connectionId] = playerName;
                    MoveCursor(playerName, 0, 0, 0);
                    MovePlayer(playerName, 0, 0, 0, 0, 0, 0);

                    Console.Write($"Players in lobby {lobbyId}: {string.Join(", ", GetPlayersInLobby(lobbyId))}");
                    Console.WriteLine();
                    return true;
                }
                return false; // Lobby not found
            }
        }

        public void LeaveLobby(string lobbyId, string playerName, string connectionId)
        {
            lock (locker)
            {
                if (lobbies.ContainsKey(lobbyId))
                {
                    lobbies[lobbyId].Players.Remove(playerName);
                    playerIds.Remove(connectionId);
                    lobbies[lobbyId].PlayerPos.Remove(playerName);
                    lobbies[lobbyId].Cursors.Remove(playerName);
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
                Console.WriteLine($"All lobbies: {string.Join(", ", ListOfLobbies.Select(x =>  new Tuple<string, string>(x.Item1, x.Item2)))}");
                return ListOfLobbies;
            }
        }



        public dynamic GetGameState(string lobbyId)
        {
            lock (locker)
            {
                string gameState = lobbies.ContainsKey(lobbyId) ? lobbies[lobbyId].GameState : null;
                Console.WriteLine($"Gamestate of lobby {lobbyId}:\n{gameState}");
                return gameState;
            }
        }
        public dynamic GetGameType(string lobbyId)
        {
            lock (locker)
            {
                string gameType = lobbies.ContainsKey(lobbyId) ? lobbies[lobbyId].GameType : null;
                Console.WriteLine($"Game type of lobby {lobbyId}:\n{gameType}");
                return gameType;
            }
        }

        public string GetPlayerLobby(string player)
        {
            Console.WriteLine($"Trying to find player {player}");
            try
            {
                string result = lobbies.FirstOrDefault(l => l.Value.Players.Where(p => p == player).Count() > 0).Key;
                Console.WriteLine($"Found player {player} in lobby {result}");
                return result;
            }
            catch
            {
                Console.WriteLine($"Player {player} wasnt found");
                return null;
            }
        }

        public string GetPlayer(string playerId)
        {
            Console.WriteLine($"Trying to find player name for id {playerId}");
            return playerIds[playerId];
        }

        public void MovePlayer(string player, double x, double y, double z, double rx, double ry, double rz)
        {
            lobbies[GetPlayerLobby(player)].PlayerPos[player] = new Tuple<double, double, double, double, double, double>(x, y, z, rx, ry, rz);
            return;
        }

        public void MoveCursor(string player, double x, double y, double z)
        {
            lobbies[GetPlayerLobby(player)].Cursors[player] = new Tuple<double, double, double>(x, y, z);
            return;
        }

        public Dictionary<string, Tuple<double, double, double, double, double, double>> GetPlayersPos(string lobby)
        {
            //getting list of all player pos
            return lobbies[lobby].PlayerPos;
        }

        public Dictionary<string, Tuple<double, double, double>> GetCursors(string lobby)
        {

            Console.WriteLine("Getting cursors");
            Console.WriteLine(lobbies[lobby].Cursors);
            //getting list of all player pos
            return lobbies[lobby].Cursors;
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

            public Dictionary<string, Tuple<double, double, double, double, double, double>> PlayerPos { get; set; }

            public Dictionary<string, Tuple<double, double, double>> Cursors { get; set; }
            public Lobby(dynamic gameState, string gameType, List<string> players)
            {
                GameState = gameState;
                GameType = gameType;
                Players = players;
                PlayerPos = new Dictionary<string, Tuple<double, double, double, double, double, double>>();
                Cursors = new Dictionary<string, Tuple<double, double, double>>();
                foreach (string player in players)
                {
                    PlayerPos.Add(player, new Tuple<double, double, double, double, double, double>(0,0,0,0,0,0));
                    Cursors.Add(player, new Tuple<double, double, double>(0, 0, 0));
                }
            }

            public Lobby(Lobby other)
            {
                GameState = other.GameState;
                GameType = other.GameType;
                Players = other.Players;
                PlayerPos = other.PlayerPos;
                Cursors = other.Cursors;
            }
        }
    }
}
