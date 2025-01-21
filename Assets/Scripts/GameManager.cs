using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class SettingsData
    {
        public int startingNumber;
    }

    [System.Serializable]
    public class GreetingData
    {
        public string message;
    }
    
    [Header("Text:")]
    public Text greetingText;
    public Text counterText;
    
    [Space(10)]
    [Header("Button:")]
    public Button incrementButton;
    public Button refreshButton;
    
    [Space(10)]
    [Header("Image:")]
    public Image buttonImage;

    private const string SettingsUrl = "https://raw.githubusercontent.com/artem-retriver/TestovoeForMyWay/refs/heads/main/Assets/StreamingAssets/Settings.json";
    private const string GreetingUrl = "https://raw.githubusercontent.com/artem-retriver/TestovoeForMyWay/refs/heads/main/Assets/StreamingAssets/Greeting.json";
    private const string AssetBundleUrl = "https://raw.githubusercontent.com/artem-retriver/TestovoeForMyWay/refs/heads/main/Assets/StreamingAssets/sprite";

    private int _counter;
    private string _saveFilePath;
    private AssetBundle _loadedBundle;

    private void Start()
    {
        _saveFilePath = Path.Combine(Application.streamingAssetsPath, "SaveData.json");
        StartCoroutine(LoadContent());
        
        incrementButton.onClick.AddListener(IncrementCounter);
        refreshButton.onClick.AddListener(() => StartCoroutine(RefreshContent()));
    }

    private IEnumerator LoadContent(bool isNeedNewSettings = false)
    {
        yield return StartCoroutine(LoadRemoteJson(SettingsUrl, (json) =>
        {
            SettingsData settings = JsonUtility.FromJson<SettingsData>(json);
            _counter = LoadCounter(settings.startingNumber, isNeedNewSettings);
            UpdateCounterText();
        }));

        yield return StartCoroutine(LoadRemoteJson(GreetingUrl, (json) =>
        {
            GreetingData greeting = JsonUtility.FromJson<GreetingData>(json);
            greetingText.text = greeting.message;
        }));

        yield return StartCoroutine(LoadAssetBundle());
        
        yield return new WaitForSeconds(3f);
    }

    private static IEnumerator LoadRemoteJson(string url, System.Action<string> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        try
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Failed to load JSON from {url}: {request.error}");
            }
        }
        finally
        {
            request?.Dispose();
        }
    }

    private IEnumerator LoadAssetBundle()
    {
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(AssetBundleUrl);
        
        try
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (_loadedBundle != null)
                {
                    _loadedBundle.Unload(true);
                }
                
                _loadedBundle = DownloadHandlerAssetBundle.GetContent(request);

                if (_loadedBundle != null)
                {
                    Sprite sprite = _loadedBundle.LoadAsset<Sprite>("sprite");
                    buttonImage.sprite = sprite;
                }
                else
                {
                    Debug.LogError("Asset Bundle file not found.");
                }
            }
            else
            {
                Debug.LogError("Failed to load Asset Bundle: " + request.error);
            }
        }
        finally
        {
            request?.Dispose();
        }
    }

    private void IncrementCounter()
    {
        _counter++;
        UpdateCounterText();
        SaveCounter();
    }

    private void UpdateCounterText()
    {
        counterText.text = _counter.ToString();
    }

    private void SaveCounter()
    {
        File.WriteAllText(_saveFilePath, _counter.ToString());
    }

    private int LoadCounter(int defaultValue, bool isNeedNewSettings)
    {
        if (File.Exists(_saveFilePath) && isNeedNewSettings == false)
        {
            return int.Parse(File.ReadAllText(_saveFilePath));
        }
        return defaultValue;
    }

    private IEnumerator RefreshContent()
    {
        yield return LoadContent(true);
    }

    private void OnApplicationQuit()
    {
        SaveCounter();
    }
}