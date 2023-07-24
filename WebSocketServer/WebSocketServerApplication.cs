using System.Net;
using System.Net.WebSockets;

namespace WebSocketServer
{
    public static class WebSocketServerApplication
    {
        public static void StartServer(string[] applicationArguments)
        {
            Console.Clear();
            Console.WriteLine("\nStarting Server...\n");

            WebApplicationBuilder webApplicationbuilder = WebApplication.CreateBuilder(applicationArguments);
            WebApplication webApplication = BuildServerApplication(webApplicationbuilder);

            if (webApplication == null)
            {
                Console.Beep();
                Console.Beep();
                Console.Beep();
                Console.Error.WriteLine($"ServerError: Application couldn't be build. Aborting...");
                System.Environment.Exit(-1);
            }
            webApplication.Run(); 
        }

        private static WebApplication BuildServerApplication(WebApplicationBuilder webApplicationbuilder)
        {
            WebApplication webApplication = webApplicationbuilder.Build();

            webApplication.UseRouting();

            webApplication.UseWebSockets();

            webApplication.Run(async (context) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    WebSocketConnection webSocketConnection = new WebSocketConnection(webSocket);
                    await webSocketConnection.Disconnect(webSocket);

                    // Clean-up
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, System.Threading.CancellationToken.None);
                    Console.WriteLine("WebSocket disconnected.");
                    webSocket.Dispose();
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // StatusCode 400
                }
            });

            return webApplication;
        }
    }
}
