using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alteracia.Web
{
    public interface IWebSocketHandler
    {
        public enum CloseCode
        {
            /* Copy of NativeWebSocket.WebSocketCloseCode */
            NotSet = 0,
            Normal = 1000,
            Away = 1001,
            ProtocolError = 1002,
            UnsupportedData = 1003,
            Undefined = 1004,
            NoStatus = 1005,
            Abnormal = 1006,
            InvalidData = 1007,
            PolicyViolation = 1008,
            TooBig = 1009,
            MandatoryExtension = 1010,
            ServerError = 1011,
            TlsHandshakeFailure = 1015
        }
        
        public class Events
        {
            public Action OnOpen;
            public Action<byte[]> OnMessage;
            public Action<CloseCode> OnClose;
            public Action<string> OnError;
        }

        public Task Connect(string url, Dictionary<string, string> headers = null, IWebSocketHandler.Events events = null);
        public Task Send(byte[] bytes);
        public Task SendText(string message);
        public Task Close();
    }
}