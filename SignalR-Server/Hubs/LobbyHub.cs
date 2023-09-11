using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using static SignalR_Server.Hubs.ServerManager;

namespace SignalR_Server.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly ServerManager _serverManager;

        public LobbyHub()
        {
            _serverManager = ServerManager.Instance;
            _serverManager.PlayerJoinedLobby += OnPlayerJoinedLobby;
            _serverManager.PlayerLeftLobby += OnPlayerLeftLobby;
            _serverManager.LobbyCreated += OnLobbyCreated;
        }

        public override Task OnConnected()
        {
            // Send a welcome message to the connected client
            Console.WriteLine($"User {Context.ConnectionId} connection established."); // Display in server console

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // Log or handle the disconnect event
            Console.WriteLine($"User {Context.ConnectionId} has disconnected."); // Display in server console
            string player = _serverManager.GetPlayer(Context.ConnectionId);
            string lobbyId = _serverManager.GetPlayerLobby(player);
            Console.WriteLine($"User {Context.ConnectionId} identified as {player}, leaving {lobbyId}");
            LeaveLobby(lobbyId, player);
            return base.OnDisconnected(stopCalled);
        }

        public void CreateLobby(string lobbyId, string gameType)
        {
            Console.WriteLine($"User {Context.ConnectionId} created lobby {lobbyId} as a {gameType}."); // Display in server console
            _serverManager.CreateLobby(lobbyId, gameType);
            Clients.All.LobbyCreated(lobbyId);
        }

        public void JoinLobby(string lobbyId, string playerName)
        {
            Console.WriteLine($"User {Context.ConnectionId} connected to {lobbyId} as {playerName}."); // Display in server console

            if (_serverManager.JoinLobby(lobbyId, playerName, Context.ConnectionId))
            {
                Groups.Add(Context.ConnectionId, lobbyId);
            }
            else
            {
                Console.WriteLine($"User {Context.ConnectionId} tried to connected to {lobbyId} as {playerName}, server doesnt exist."); // Display in server console
                Clients.Caller.LobbyNotFound();
            }
        }

        public void LeaveLobby(string lobbyId, string playerName)
        {
            _serverManager.LeaveLobby(lobbyId, playerName, Context.ConnectionId);
            Groups.Remove(Context.ConnectionId, lobbyId);
        }

        public void GetPlayersInLobby(string lobbyId)
        {
            var players = _serverManager.GetPlayersInLobby(lobbyId);
            if (players != null)
            {
                Clients.Caller.PlayersInLobby(players);
            }
            else
            {
                Clients.Caller.LobbyNotFound();
            }
        }

        public void GetLobbies()
        {
            Console.WriteLine($"User {Context.ConnectionId} requested list of lobbies."); // Display in server console
            var lobbies = _serverManager.GetLobbies();
            Clients.Caller.ListOfLobbies(lobbies);
        }

        public void GetGameState(string lobbyId)
        {
            Console.WriteLine($"User {Context.ConnectionId} requested gamestate of lobby {lobbyId}."); // Display in server console
            var json = _serverManager.GetGameState(lobbyId);
            Clients.Caller.GameState(json);
        }

        public void GetGameType(string lobbyId)
        {
            Console.WriteLine($"User {Context.ConnectionId} requested game type of lobby {lobbyId}."); // Display in server console
            var type = _serverManager.GetGameType(lobbyId);
            Clients.Caller.GameType(type);
        }

        public void MovePlayer(string player, double x, double y, double z, double rx, double ry, double rz)
        {
            Console.WriteLine($"User {Context.ConnectionId} as {player} moved to {x}, {y}, {z}, looking at direction {rx}, {ry}, {rz} ."); // Display in server console
            _serverManager.MovePlayer(player, x, y, z, rx, ry, rz);
            string lobby = _serverManager.GetPlayerLobby(player);
            Clients.OthersInGroup(lobby).PlayerMoved(player, x, y, z, rx, ry, rz);
        }

        public void MoveCursor(string player, double x, double y, double z)
        {
            Console.WriteLine($"User {Context.ConnectionId} as {player} moved cursor to {x}, {y}, {z}"); // Display in server console
            _serverManager.MoveCursor(player, x, y, z);
            string lobby = _serverManager.GetPlayerLobby(player);
            Clients.OthersInGroup(lobby).CursorMoved(player, x, y, z);
        }

        public void GetPlayersPos(string lobby)
        {
            //getting list of all player pos
            var pos = _serverManager.GetPlayersPos(lobby);
            Clients.Caller.PlayersPos(pos);
        }

        public void GetCursors(string lobby)
        {
            //getting list of all player pos
            var cursors = _serverManager.GetCursors(lobby);
            Clients.Caller.CursorsPos(cursors);
        }

        private void OnPlayerJoinedLobby(string lobbyId, string playerName)
        {
            Clients.Group(lobbyId).PlayerJoined(playerName);
        }

        private void OnPlayerLeftLobby(string lobbyId, string playerName)
        {
            Clients.Group(lobbyId).PlayerLeft(playerName);
        }

        private void OnLobbyCreated(string lobbyId)
        {
            // Handle lobby created event, if needed
        }

        // Implement other hub methods as needed

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serverManager.PlayerJoinedLobby -= OnPlayerJoinedLobby;
                _serverManager.PlayerLeftLobby -= OnPlayerLeftLobby;
                _serverManager.LobbyCreated -= OnLobbyCreated;
            }

            base.Dispose(disposing);
        }
    }
}
