using System.Collections;
using System.Collections.Generic;
using Alteracia.Web;
using UnityEngine;

public class WebRequestRuntimeTests : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        string test = "Get(\"https://httpbin.org/get\")";
        using (var req = await Alteracia.Web.Requests.Get("https://httpbin.org/get"))
        {
            if (req.Success()) Debug.Log(test + " SUCCESS\n" + req.downloadHandler.text);
            else Debug.LogError(test + " FAILS\n" + $"{req.error}: {req.downloadHandler.text}");
        }
        test = "Get(\"https://httpbin.org/basic-auth/:user/:passwd\", new []{\"Authorization\", \"Basic c2Q6c3Nz\"})";
        using (var req = await Alteracia.Web.Requests.Get("https://httpbin.org/basic-auth/:user/:passwd", new []{"Authorization", "Basic c2Q6c3Nz"}))
        {
            if (req.responseCode == 401) Debug.Log(test + " SUCCESS\n" + req.downloadHandler.text);
            else Debug.LogError(test + " FAILS\n" + $"{req.error}: {req.downloadHandler.text}");
            req.downloadHandler.text.FormatJsonText(typeof(JsonArray<string>));
        }

        test = "Post(\"https://httpbin.org/get\")";
        using (var req = await Alteracia.Web.Requests.Post("https://httpbin.org/post"))
        {
            if (req.Success()) Debug.Log(test + " SUCCESS\n" + req.downloadHandler.text);
            else Debug.LogError(test + " FAILS\n" + $"{req.error}: {req.downloadHandler.text}");
        }
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
