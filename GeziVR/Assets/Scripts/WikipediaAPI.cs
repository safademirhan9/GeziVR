using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System;
using Newtonsoft.Json;

public class WikipediaAPI : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMPro.TextMeshProUGUI infoText;
    [SerializeField] private Image imageArtist;
    [SerializeField] private PlayerScriptable playerScriptable;

    public static bool isExitedPlane2 = false;
    public static bool isStatusChanged = false;

    private WikiArtArtist artist;
    private string search;

    private WikiArtPainting [] allPaintings; 
    private WikiArtPainting [] window = new WikiArtPainting[8];
    private int index = 0;

    
    private WikiArtArtist[] allArtists;
    private List<WikiArtArtist> recommendedArtists = new List<WikiArtArtist>();

    private void Start()
    {
        GameObject.Find("Player").GetComponent<PlayerMovement2>().enabled = false;
        GameObject.Find("PlayerCam").GetComponent<PlayerCamera2>().enabled = false;
        //bu cursor şeyleri vr'da deneme yaparken olmamali
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 
        StartCoroutine(GetAllArtists("http://www.wikiart.org/en/App/Artist/AlphabetJson?v=new&inPublicDomain={true/false}"));

    }
    
    private void Update()
    {
        if(isStatusChanged)
        {
            CheckPlanes();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var hitPoint = hit.point;
                hitPoint.y = 0;
                var playerPosition = transform.position;
                playerPosition.y = 0;
                var distance = Vector3.Distance(hitPoint, playerPosition);
                if(hit.transform.tag == "ArtInfo")
                {
                    GameObject parent  = hit.transform.parent.gameObject;
                    //Debug.Log(parent.name);
                    //Debug.Log("InfoPedestal");
                }
            }
        }
    }

    public void GetImage(string url, Image image)
    {
        StartCoroutine(setImage(url, image));
    }

    public void DisableCanvas()
    {
        GameObject.Find("Canvas").SetActive(false);
        //bu cursor şeyleri vr'da deneme yaparken olmamali
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
        GameObject.Find("Player").GetComponent<PlayerMovement2>().enabled = true;
        GameObject.Find("PlayerCam").GetComponent<PlayerCamera2>().enabled = true;
    }

    public void Search()
    {
        artist = new WikiArtArtist();
        search = inputField.text;
        foreach (WikiArtArtist artist1 in allArtists)
        {
            if(artist1.url == search)
            {
                Debug.Log(artist1.wikipediaUrl);
                if(artist1.wikipediaUrl != "")
                {
                    StartCoroutine(GetArtistSummary("https://en.wikipedia.org/api/rest_v1/page/summary/" + artist1.wikipediaUrl.Substring(artist1.wikipediaUrl.LastIndexOf('/') + 1)));
                }
                else
                {
                    infoText.text = "No Wikipedia page found";
                }
                StartCoroutine(PutVisitedMuseum("https://gezivr.onrender.com/addVisitedMuseum/" + playerScriptable.token + "/" + artist1.contentId));
                StartCoroutine(GetArtistImage("http://www.wikiart.org/en/" + artist1.url + "?json=2"));
                StartCoroutine(GetPaintings("https://www.wikiart.org/en/App/Painting/PaintingsByArtist?artistUrl=" + artist1.url + "&json=2"));
                break;
            }
        }
    }

    public void GetRecommendedMuseums()
    {
        StartCoroutine(GetRecommendedMuseums("https://gezivr.onrender.com/getRecommendedMuseums/" + playerScriptable.token));
    }

    IEnumerator GetRecommendedMuseums(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Success");
                    var data = webRequest.downloadHandler.text;
                    data = data.Substring(1, data.LastIndexOf(']') - 1);
                    string[] museums = data.Split(',');
                    foreach (string museum in museums)
                    {
                        foreach (WikiArtArtist artist1 in allArtists)
                        {
                            if (artist1.contentId == museum)
                            {
                                recommendedArtists.Add(artist1);
                                break;
                            }
                        }
                    }
                    break;
            }
        }
    }

    IEnumerator PutVisitedMuseum(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Success");
                    break;
            }
        }
    }

    IEnumerator GetArtistSummary(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + (webRequest.downloadHandler.text));
                    var artistIn = JsonUtility.FromJson<WikiArtArtist>(webRequest.downloadHandler.text);
                    infoText.text = artistIn.extract;
                    artist.extract = artistIn.extract;
                    break;
            }
        }
    }

    IEnumerator GetArtistImage(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    artist = JsonUtility.FromJson<WikiArtArtist>(webRequest.downloadHandler.text);
                    GetImage(artist.image, imageArtist);
                    break;
            }
        }
    }

    IEnumerator GetPaintings(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    allPaintings = JsonUtility.FromJson<RootObject>("{\"paintings\":" + webRequest.downloadHandler.text+ "}").paintings;
                    for (int i = 0; i < allPaintings.Length; i++)
                    {
                        try
                        {
                            allPaintings[i].imageBytes = new System.Net.WebClient().DownloadData(allPaintings[i].image);
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        if(i >= allPaintings.Length)
                        {
                            break;
                        }

                        window[i] = allPaintings[i];
                        
                        Texture2D tex = new Texture2D(2, 2);
                        tex.LoadImage(allPaintings[i].imageBytes);
                        GameObject go = GameObject.Find("Frame" + (i + 1));
                        go.transform.GetChild(3).GetComponent<Renderer>().material.mainTexture = tex;
                    }
                    index = 8;
                    break;
            }
        }
    }

    private void CheckPlanes()
    {
        isStatusChanged = false;
        if(isExitedPlane2)
        {
            isExitedPlane2 = false;
            SetNewWindow();
        }
    }

    private void SetNewWindow()
    {
        for (int i = index; i < index+8; i++)
        {
            if(i >= allPaintings.Length)
            {
                break;
            }

            window[i%8] = allPaintings[i];
            
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(allPaintings[i].imageBytes);
            GameObject go = GameObject.Find("Frame" + (i%8 + 1));
            go.transform.GetChild(3).GetComponent<Renderer>().material.mainTexture = tex;
        }
        index += 8;
    }

    IEnumerator setImage(string url, Image image) {
        WWW www = new WWW(url);
        yield return www;

        image.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
    }

    IEnumerator GetAllArtists(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    allArtists = JsonUtility.FromJson<Artists>("{\"artists\":" + webRequest.downloadHandler.text + "}").artists;
                    break;
            }
        }
    }
}