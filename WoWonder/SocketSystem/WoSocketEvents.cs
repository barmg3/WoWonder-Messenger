using System;
using SocketIOClient;
using WoWonder.Helpers.Model;

namespace WoWonder.SocketSystem
{
    public class WoSocketEvents
    {
        public void InitEvents(SocketIO client)
        {
            try
            {
                //All events of sockets connestcion are here
                client.OnDisconnected += Socket_OnDisconnected;
                client.OnError += Socket_OnError;
                client.OnReconnectFailed += Socket_OnReconnectFailed; 
                client.OnReconnectAttempt += Socket_OnReconnecting;
                client.OnPong += Socket_OnPong;
                client.OnPing += Client_OnPing;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private void Client_OnPing(object sender, EventArgs e)
        {
            Console.WriteLine("Socket_OnPing " + e);

            if (!WoSocketHandler.IsJoined)
                UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
        }

        private static void Socket_OnPong(object sender, TimeSpan e)
        {
            Console.WriteLine("Socket_OnPong " + e);

            if (!WoSocketHandler.IsJoined)
                UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
        }

        private static void Socket_OnReconnecting(object sender, int e)
        {
            Console.WriteLine("Socket_OnReconnecting " + e);
            UserDetails.Socket?.ReconnectSocket();
        }

        private void Socket_OnReconnectFailed(object sender, EventArgs e)
        {
            Console.WriteLine("Socket_OnReconnectFailed " + e);
            UserDetails.Socket?.ReconnectSocket();
        }

        private static void Socket_OnError(object sender, string e)
        {
            Console.WriteLine("Socket_OnError " + e);

            if (!WoSocketHandler.IsJoined)
                UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
        }

        private static void Socket_OnDisconnected(object sender, string e)
        {
            Console.WriteLine("Socket_OnDisconnected" + e);
        }
    }
}