using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets;
using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FmCndoeServer.Controllers;

[ApiController]
[Route("[controller]")]
public class TelnetController : ControllerBase
{
    private static ConcurrentDictionary<string, TcpClient> _telnetClients = new ConcurrentDictionary<string, TcpClient>();
    private readonly String _serverAddr = "localhost"; //"localhost";
    [HttpGet("connect")]
    public async Task<IActionResult> Connect([FromQuery] string port, [FromQuery] string? server)
    {
        if (string.IsNullOrEmpty(port))
        {
            return BadRequest("Port parameter is required.");
        }
        int portNumber;
        if (!int.TryParse(port, out portNumber))
        {
            return BadRequest("Invalid port number.");
        }
        var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();
        var client = new TcpClient();
        _telnetClients.TryAdd(connectionId, client);
        try
        {
            if (!string.IsNullOrEmpty(server))
            {
                await client.ConnectAsync(server, portNumber);
            }
            else
                await client.ConnectAsync(_serverAddr, portNumber);

            _ = HandleWebSocketToTelnet(socket, client, connectionId);
            _ = HandleTelnetToWebSocket(socket, client, connectionId);
            _ = WaitForSocketClose(socket, client, connectionId);
            // 避免 Connect 方法结束后关闭连接
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error connecting to Telnet server: {ex.Message}");
        }
        return new EmptyResult();
    }

    private async Task HandleWebSocketToTelnet(WebSocket webSocket, TcpClient telnetClient, string connectionId)
    {
        var buffer = new byte[1024];
        var stream = telnetClient.GetStream();
        try
        {
            while (webSocket.State == WebSocketState.Open && telnetClient.Connected)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var bytes = Encoding.UTF8.GetBytes(message);
                    if (message.StartsWith("{\"event\":\"heartbeat\"}"))
                        continue;
                    //Console.WriteLine(message);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }
            }
        }
        catch (Exception exp){
            Console.WriteLine(exp.Message);
        }
    
        // 关闭连接时清理资源
        CleanupConnection(connectionId);
    }
    private async Task WaitForSocketClose(WebSocket socket, TcpClient telnetClient, string connectionId)
    {
        var buffer = new byte[1024];
        while (socket.State == WebSocketState.Open)
        {
            await Task.Delay(100);
        }
        // 当 WebSocket 关闭时，关闭相应的 TcpClient 连接
        CleanupConnection(connectionId);
            
    }
    private async Task HandleTelnetToWebSocket(WebSocket webSocket, TcpClient telnetClient, string connectionId)
    {
        var buffer = new byte[1024];
        var stream = telnetClient.GetStream();
        try
        {
            while (webSocket.State == WebSocketState.Open && telnetClient.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    if (buffer[0] == 255 && buffer[1] == 251)
                    {
                        var stringto = "\n";//发送一个\n，让telnet进入输入不需要前端用户手动输入
                        var b = Encoding.UTF8.GetBytes(stringto);
                        await stream.WriteAsync(b, 0, b.Length);
                        continue;
                    }
                    var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        // WebSocket 已关闭，不再发送数据
                        break;
                    }
                }
            }
        } catch (Exception exp){
            Console.WriteLine(exp.ToString());
        }
        // 关闭连接时清理资源
        CleanupConnection(connectionId);
    }

    private void CleanupConnection(string connectionId)
    {
        if (_telnetClients.TryRemove(connectionId, out var client))
        {
            client.Close();
        }
    }
}
