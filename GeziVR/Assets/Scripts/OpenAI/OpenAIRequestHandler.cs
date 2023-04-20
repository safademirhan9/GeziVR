using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System.Text.Json;

namespace OpenAI_handler
{
    public class OpenAIRequestHandler : MonoBehaviour
    {
        public void generate()
        {
            StartCoroutine(generateImages("https://gezivr.onrender.com/generate_images"));
        }

        IEnumerator generateImages(string uri)
        {
            var webRequest = UnityWebRequest.Get(uri);
            webRequest.certificateHandler = null;

            yield return webRequest.SendWebRequest();
            MyDictionary dict = JsonUtility.FromJson<MyDictionary>(webRequest.downloadHandler.text);
            using (var client = new WebClient())
            {
                // kac tane kullanilacaksa eklenip silinebilir, 4 te denendi yavas calisiyor
                client.DownloadFile(dict.img1, Application.dataPath + "/GeneratedImages/img1.png");
                client.DownloadFile(dict.img2, Application.dataPath + "/GeneratedImages/img2.png");
                client.DownloadFile(dict.img3, Application.dataPath + "/GeneratedImages/img3.png");
                client.DownloadFile(dict.img4, Application.dataPath + "/GeneratedImages/img4.png");
                client.DownloadFile(dict.img4, Application.dataPath + "/GeneratedImages/img5.png");
                client.DownloadFile(dict.img4, Application.dataPath + "/GeneratedImages/img6.png");
                client.DownloadFile(dict.img4, Application.dataPath + "/GeneratedImages/img7.png");
                client.DownloadFile(dict.img4, Application.dataPath + "/GeneratedImages/img8.png");
            }
        }
    }
    [System.Serializable]
    public class MyDictionary
    {
        public string img1;
        public string img2;
        public string img3;
        public string img4;
        public string img5;
        public string img6;
        public string img7;
        public string img8;
    }
}