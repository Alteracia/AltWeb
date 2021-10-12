using System;
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
    public readonly struct AsyncOperationAwaiter : INotifyCompletion
    {
        private readonly AsyncOperation _asyncOperation;
        public bool IsCompleted => _asyncOperation.isDone;

        public AsyncOperationAwaiter( AsyncOperation asyncOperation ) => _asyncOperation = asyncOperation;

        public void OnCompleted( Action continuation ) => _asyncOperation.completed += _ => continuation();

        public void GetResult() { }
    }

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
        public static bool Success(this UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
        return request.isDone && request.result == UnityWebRequest.Result.Success;
#else
        return request.isDone && !request.isNetworkError && !request.isHttpError;
#endif
        }
        
        private static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
        }
        
        public static async Task<UnityWebRequest> Image(string uri, string[] header = null)
        {
#if ALT_LOADING_LOG || UNITY_EDITOR
            int log = Log.Start($"curl -X GET \"{uri}\"" + (header != null ? $"-H \"{header[0]}: {header[1]}\" " : " "));
#endif
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri, true);

            if (header != null) request.SetRequestHeader(header[0], header[1]);
            
            await request.SendWebRequest();
            
#if ALT_LOADING_LOG || UNITY_EDITOR
            if (!request.Success())
                Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
            else
                Log.Finish(log, $"SUCCESS: image - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
            return request;
        }

        
        // TODO Protect from same requests
        //static Dictionary<string, Task<UnityWebRequest>>

        public static async Task<UnityWebRequest> Post(string uri, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Post(uri, "");

            return await SendWebRequest(uri, header, request);
        }
        
        public static async Task<UnityWebRequest> Get(string uri, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(uri);

            return await SendWebRequest(uri, header, request);
        }
        
        private static async Task<UnityWebRequest> SendWebRequest(string uri, string[] header, UnityWebRequest request)
        {
#if ALT_LOADING_LOG || UNITY_EDITOR
            int log = Log.Start($"curl -X POST \"{uri}\"" + (header != null ? $"-H \"{header[0]}: {header[1]}\" " : " "));
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
                Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
            else
                Log.Finish(log,
                    $"SUCCESS: data - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
            return request;
        }
        
        public static async Task<UnityWebRequest> Post(string uri, string message, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Post(uri, message);

            return await SendWebRequest(request);
        }

        public static async Task<UnityWebRequest> Post(string uri, WWWForm message, string[] header = null)
        {
            UnityWebRequest request = UnityWebRequest.Post(uri, message);

            return await SendWebRequest(request);
        }
        
        private static async Task<UnityWebRequest> SendWebRequest(UnityWebRequest request)
        {
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Delay(100);
            }

            return request;
        }
        
        public static async Task<UnityWebRequest> Put(string uri, string json, string[] header = null)
        {
#if ALT_LOADING_LOG || UNITY_EDITOR
            int log = Log.Start($"curl -X POST \"{uri}\"" + (header != null ? 
                $"-H \"{header[0]}: {header[1]}\" " : " ") + $"-d \"{json}\"");
#endif

            UnityWebRequest request = UnityWebRequest.Put(uri, json);
            if (header != null) request.SetRequestHeader(header[0], header[1]);
            request.method = "POST";
            await request.SendWebRequest();
#if ALT_LOADING_LOG || UNITY_EDITOR
            if (!request.Success())
                Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
            else
                Log.Finish(log, $"SUCCESS: data - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
            return request;
        }
    }
}
