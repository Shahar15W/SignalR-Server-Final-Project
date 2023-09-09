using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

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

            if (_serverManager.JoinLobby(lobbyId, playerName))
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
            _serverManager.LeaveLobby(lobbyId, playerName);
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
