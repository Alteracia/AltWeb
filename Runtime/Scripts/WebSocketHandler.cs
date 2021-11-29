#if ENDEL_NATIVE_WEBSOCKET

using System.Collections.Generic;
using System.Threading.Tasks;
using NativeWebSocket;

namespace Alteracia.Web
{
    public class WebSocketHandler : IWebSocketHandler
    {
        private WebSocket _socket;
        
        public async Task Connect(string url, Dictionary<string, string> headers = null, IWebSocketHandler.Events events = null)
        {
            _socket = new WebSocket(url);
            AssignEvents(events);
            await _socket.Connect();
        }

        public Task Send(byte[] bytes)
        {
            return _socket?.Send(bytes);
        }

        public Task SendText(string message)
        {
            return _socket?.SendText(message);
        }

        public Task Close()
        {
            return _socket?.Close();
        }

        private void AssignEvents(IWebSocketHandler.Events events = null)
        {
            if (_socket == null || events == null) return;

            _socket.OnOpen += () =>
            {
                events.OnOpen?.Invoke();
                DispatchLoop();
            };
            
            _socket.OnMessage += data =>
            {
                events.OnMessage?.Invoke(data);
            };
            
            _socket.OnError += error =>
            {
                _socket.CancelConnection();
                _socket.Close();
                events.OnError?.Invoke(error);
            };
            
            _socket.OnClose += closeCode =>
            {
                _socket.CancelConnection();
                events.OnClose?.Invoke((IWebSocketHandler.CloseCode)closeCode);
            };
        }

        private async void DispatchLoop()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            while (_socket.State == WebSocketState.Open)
            {
                _socket.DispatchMessageQueue();
                await Task.Delay(1000);
            }
#endif
        }
    }
}

#endif // ENDEL_NATIVE_WEBSOCKET