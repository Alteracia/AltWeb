using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
#if !UNITY_WEBGL
#endif
using UnityEngine.Networking;

namespace Alteracia.Web
{
    [ DebuggerNonUserCode ]
    public readonly struct UnityWebRequestAwaiter : INotifyCompletion
    {
        private readonly UnityWebRequestAsyncOperation _asyncOperation;

        public bool IsCompleted => _asyncOperation.isDone;

        public UnityWebRequestAwaiter( UnityWebRequestAsyncOperation asyncOperation ) => _asyncOperation = asyncOperation;

        public void OnCompleted( Action continuation ) => _asyncOperation.completed += _ => continuation();

        public UnityWebRequest GetResult() => _asyncOperation.webRequest;
    }
    
    public static class Requests
    {
        // TODO Protect from same requests: static Dictionary<string, Task<UnityWebRequest>>
        
        /// <summary>
        /// Is request succeed?
        /// </summary>
        /// <param name="request"></param>
        /// <returns>true if success</returns>
        public static bool Success(this UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
        return request.isDone && request.result == UnityWebRequest.Result.Success;
#else
        return request.isDone && !request.isNetworkError && !request.isHttpError;
#endif
        }
        
        /// <summary>
        /// Get from server using UnitWebRequest
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="header">Headers add to request</param>
        /// <returns>UnitWebRequest</returns>
        public static async Task<UnityWebRequest> Get(string uri, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(uri);

            return await request.SendWebRequest(header);
        }
        
        /// <summary>
        /// Post from server using UnitWebRequest without message
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="header">Headers add to request</param>
        /// <returns>UnitWebRequest</returns>
        public static async Task<UnityWebRequest> Post(string uri, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Post(uri, "");

            return await request.SendWebRequest(header);
        }

        /// <summary>
        /// Get from server using UnitWebRequest with string message
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="message">String message</param>
        /// <param name="header">Headers add to request</param>
        /// <returns>UnitWebRequest</returns>
        public static async Task<UnityWebRequest> Post(string uri, string message, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Post(uri, message);

            return await request.SendWebRequest(header);
        }

        /// <summary>
        /// Get from server using UnitWebRequest with WWWform message
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="message">WWWform message</param>
        /// <param name="header">Headers add to request</param>
        /// <returns>UnitWebRequest</returns>
        public static async Task<UnityWebRequest> Post(string uri, WWWForm message, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Post(uri, message);

            return await request.SendWebRequest(header);
        }

        /// <summary>
        /// Post to server using UnitWebRequest with multipart array
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="message">Multipart array</param>
        /// <param name="headers">Headers add to request</param>
        /// <returns>UnitWebRequest</returns>
        public static async Task<UnityWebRequest> Post(string uri, List<IMultipartFormSection> message, string[] headers = null)
        {
            UnityWebRequest request = UnityWebRequest.Post(uri, message);
            
            return await request.SendWebRequest(headers);
        }
        
        public static async Task<UnityWebRequest> PostJson(string uri, string json, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Put(uri, json);
            request.method = "POST";
            
            return await request.SendWebRequest(header);
        }
        
        public static async Task<UnityWebRequest> Put(string uri, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Put(uri, "");
            
            return await request.SendWebRequest(header);
        }
        
        public static async Task<UnityWebRequest> Put(string uri, string message, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Put(uri, message);
            
            return await request.SendWebRequest(header);
        }
        
        public static async Task<UnityWebRequest> Put(string uri, byte[] data, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Put(uri, data);
            
            return await request.SendWebRequest(header);
        }
        
        public static async Task<UnityWebRequest> Delete(string uri, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Delete(uri);

            return await request.SendWebRequest(header);
        }
        
        public static async Task<UnityWebRequest> Image(string uri, bool nonReadable = true, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri, nonReadable);

            return await request.SendWebRequest(header);
        }
        
        private static async Task<UnityWebRequest> SendWebRequest(this UnityWebRequest request, string[] header)
        {
            if (request == null) return null;
            
#if ALT_LOADING_LOG || UNITY_EDITOR
            int log = Log.Start($"curl -X POST \"{request.uri}\"" + (header != null ? $"-H \"{header[0]}: {header[1]}\" " : " "));
#endif
            if (header != null)
            {
                for (int i = 0; i < header.Length / 2; i += 2)
                {
                    request.SetRequestHeader(header[i], header[i + 1]);
                }
            }

            // Send the request and wait for a response
            await request.SendWebRequest();
            
#if ALT_LOADING_LOG || UNITY_EDITOR
            if (!request.Success())
                Log.Finish(log, $"{request.error}: {request.downloadHandler?.text}");
            else
                Log.Finish(log,
                    $"SUCCESS: data - {request.downloadHandler?.data.Length}, text - {request.downloadHandler?.text.Length}");
#endif
            return request;
        }
        
        private static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
        }
    }
}
