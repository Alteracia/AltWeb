using System;
using System.Collections;
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
        /*
        /// <summary>
        /// Get from server using UnitWebRequest
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="callback">On complete callback</param>
        /// <param name="header">Headers add to request</param>
        /// <returns></returns>
        public static IEnumerator Get(string uri, Action<UnityWebRequest> callback, string[] header = null)
        {
#if ALT_LOADING_LOG || UNITY_EDITOR
             int log = Log.Start($"curl -X GET \"{uri}\"" + (header != null ? $"-H \"{header[0]} {header[1]}\" " : " "));
#endif
            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                if (header != null) request.SetRequestHeader(header[0], header[1]);
                
                // Send the request and wait for a response
                yield return request.SendWebRequest();
#if ALT_LOADING_LOG || UNITY_EDITOR
                if (request.isNetworkError || request.isHttpError)
                    Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
                else
                    Log.Finish(log, $"SUCCESS: data - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
                callback(request);
            }
        }
        
        /// <summary>
        /// Load Image from server using UnityWebRequestTexture
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="callback">On complete callback</param>
        /// <param name="header">Headers add to request</param>
        /// <returns></returns>
        public static IEnumerator Image(string uri, Action<UnityWebRequest> callback, string[] header = null)
        {
#if ALT_LOADING_LOG || UNITY_EDITOR
            int log = Log.Start($"curl -X GET \"{uri}\"" + (header != null ? $"-H \"{header[0]}: {header[1]}\" " : " "));
#endif
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri))
            {
                if (header != null) request.SetRequestHeader(header[0], header[1]);
                yield return request.SendWebRequest();
#if ALT_LOADING_LOG || UNITY_EDITOR
                if (request.isNetworkError || request.isHttpError)
                    Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
                else
                    Log.Finish(log, $"SUCCESS: image - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
                callback(request);
            }
        }
        */

        public static async Task<UnityWebRequest> Image(string uri, string[] header = null)
        {
#if ALT_LOADING_LOG || UNITY_EDITOR
            int log = Log.Start($"curl -X GET \"{uri}\"" + (header != null ? $"-H \"{header[0]}: {header[1]}\" " : " "));
#endif
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri, true);

            if (header != null) request.SetRequestHeader(header[0], header[1]);
            await request.SendWebRequest();
#if ALT_LOADING_LOG || UNITY_EDITOR
            if (request.isNetworkError || request.isHttpError)
                Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
            else
                Log.Finish(log, $"SUCCESS: image - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
            return request;
        }
/*
        /// <summary>
        /// Post to server using UnitWebRequest
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="message">Post message</param>
        /// <param name="callback">On complete callback</param>
        /// <param name="header">Headers add to request</param>
        /// <returns></returns>
        public static IEnumerator Post(string uri, WWWForm message, Action<UnityWebRequest> callback, string[] header = null)
        {
#if ALT_LOADING_LOG || UNITY_EDITOR
            int log = Log.Start($"curl -X POST \"{uri}\"" + (header != null ? 
                $"-H \"{header[0]}: {header[1]}\" " : " ") + $"-d \"{message}\"");
#endif

            using (UnityWebRequest request = UnityWebRequest.Post(uri, message))
            {
                if (header != null) request.SetRequestHeader(header[0], header[1]);
                // Send the request and wait for a response
                yield return request.SendWebRequest();
#if ALT_LOADING_LOG || UNITY_EDITOR
                if (request.isNetworkError || request.isHttpError)
                    Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
                else
                    Log.Finish(log, $"SUCCESS: data - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
                callback(request);
            }
        }
        */
        /*
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
        */
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
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
            if (request.isNetworkError || request.isHttpError)
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
        
        public static async Task<UnityWebRequest> PostJson(string uri, string json, string[] header = null)
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
            if (request.isNetworkError || request.isHttpError)
                Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
            else
                Log.Finish(log, $"SUCCESS: data - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
            return request;
        }

        /*
        /// <summary>
        /// Post to server using UnitWebRequest
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="data">Post data</param>
        /// <param name="fileName">Post file name</param>
        /// <param name="callback">On complete callback</param>
        /// <param name="headers">Headers add to request</param>
        /// <returns></returns>
        public static IEnumerator PostMultipartFormData(string uri, byte[] data, string fileName, Action<UnityWebRequest> callback, string[] headers = null)
        {
#if ALT_LOADING_LOG || UNITY_EDITOR
            int log = Log.Start($"curl -X POST \"{uri}\"" + (headers != null ?
             $"-H \"{headers[0]}: {headers[1]}\" " : " ") + $"-d \"screenshot.png\"");
#endif
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("data", data, fileName, "multipart/form-data") // contentType: image/png
            };

            using (UnityWebRequest request = UnityWebRequest.Post(uri, formData))
            {
                if (headers != null) request.SetRequestHeader(headers[0], headers[1]);
                yield return request.SendWebRequest();
#if ALT_LOADING_LOG || UNITY_EDITOR
                if (request.isNetworkError || request.isHttpError)
                    Log.Finish(log, $"{request.error}: {request.downloadHandler.text}");
                else
                    Log.Finish(log, $"SUCCESS: data - {request.downloadHandler.data.Length}, text - {request.downloadHandler.text.Length}");
#endif
                callback(request);
            }
        }
        
        /// <summary>
        /// Get redirected url from server using UnitWebRequest
        /// </summary>
        /// <param name="uri">Full uri of request</param>
        /// <param name="callback">On complete callback</param>
        /// <param name="header">Headers add to request</param>
        /// <returns></returns>
        public static IEnumerator GetRedirectionUrl(string uri, Action<string> callback, string[] header = null)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                
#if UNITY_WEBGL
                request.downloadHandler.Dispose(); // don't load 
#else
                // Does not work in WebGL -> get url from request
                request.redirectLimit = 0;
#endif
                
                // Add header
                if (header != null) request.SetRequestHeader(header[0], header[1]);
                
                // Send the request and wait for a response
                yield return request.SendWebRequest();
#if UNITY_WEBGL
                callback(request.url);
#else
                callback(request.GetResponseHeader("location"));
#endif
            }
        }

        */
    }
}
