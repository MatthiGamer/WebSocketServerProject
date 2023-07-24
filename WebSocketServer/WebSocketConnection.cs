using Microsoft.AspNetCore.Connections;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketServer
{
    public class WebSocketConnection
    {
        private byte[] receiveBuffer = new byte[4096];
        private WebSocketReceiveResult? webSocketReceiveResult;
        private bool hasLostConnection = false;

        public WebSocketConnection(WebSocket webSocket)
        {

            if (webSocket == null)
                return;

            HandleConnection(webSocket);
        }

        public async Task Disconnect(WebSocket webSocket)
        {
            // Keep the web socket open until the client disconnects or the connection is lost
            while (!webSocket.CloseStatus.HasValue && !hasLostConnection && webSocket.State == WebSocketState.Open) { }
        }

        private async void HandleConnection(WebSocket webSocket)
        {
            Console.WriteLine("Websocket connected.");

            try
            {
                // May throw an error, when the client disconnects unforeseen

                // Get connect confirmation
                webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                PrintClientMessage(Encoding.UTF8.GetString(receiveBuffer[..webSocketReceiveResult.Count]));
            }
            catch
            {
                WebSocketError();
                return;
            }

            await SendInitialData(webSocket);

            // As long as the connection wasn't closed by the client
            while (!webSocket.CloseStatus.HasValue && !hasLostConnection)
            {
                try
                {
                    webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                }
                catch (ConnectionAbortedException exception)
                {
                    Console.WriteLine($"Server is stopping.\nConnection aborted.\n{exception.Message}");
                }

                if (webSocketReceiveResult == null)
                    break;

                await SendUpdate(webSocket);
            }
        }

        private async Task SendInitialData(WebSocket webSocket)
        {
            Console.WriteLine("Sending initial data...");
            await WebSocketServerDataHandler.CheckForNewData();
            BlogPost[] blogPosts = WebSocketServerDataHandler.GetAllBlogPosts();
            await SendMessage(webSocket, blogPosts);
        }

        private async Task SendUpdate(WebSocket webSocket)
        {
            // Check if modified data is available
            if (!await WebSocketServerDataHandler.CheckForNewData())
                return;

            Console.WriteLine("Sending data...");
            BlogPost[] blogPosts = WebSocketServerDataHandler.GetModifiedPosts();
            await SendMessage(webSocket, blogPosts);

            // wait for a few seconds before checking the data again
            Thread.Sleep(5_000);
        }

        private async Task SendMessage(WebSocket webSocket, BlogPost[] blogPosts)
        {
            foreach (BlogPost post in blogPosts)
            {
                string jsonString = GetJSONString(post.ID, post.Title, post.Content);
                byte[] messageToSend = Encoding.UTF8.GetBytes(jsonString);

                try
                {
                    // Send data
                    await webSocket.SendAsync(new ArraySegment<byte>(messageToSend, 0, messageToSend.Length), webSocketReceiveResult!.MessageType, webSocketReceiveResult.EndOfMessage, CancellationToken.None);

                    // Receive answer
                    webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                    if (Encoding.UTF8.GetString(receiveBuffer[..webSocketReceiveResult.Count]) != string.Empty)
                        PrintClientMessage(Encoding.UTF8.GetString(receiveBuffer[..webSocketReceiveResult.Count]));

                }
                catch
                {
                    WebSocketError();
                    return;
                }
            }
        }

        public static string GetJSONString(int id, string title, string content)
        {
            // {
            //     "id": id,
            //     "title": title,
            //     "content": content
            // }

            string jsonString =
            @"{
                    ""id"": """ + id.ToString() + @""",
                    ""title"": """ + title + @""",
                    ""content"": """ + content + @"""
            }";

            return jsonString;
        }

        private void PrintClientMessage(string message) => Console.WriteLine($"Client message: {message}");

        private void WebSocketError()
        {
            hasLostConnection = true;
        }
    }
}
