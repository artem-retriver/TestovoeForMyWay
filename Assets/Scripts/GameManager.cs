using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

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

    private const string SettingsFileName = "Settings.json";
    private const string GreetingFileName = "Greeting.json";
    private const string AssetBundleName = "Sprite";

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
        yield return StartCoroutine(LoadLocalJson(SettingsFileName, (json) =>
        {
            SettingsData settings = JsonUtility.FromJson<SettingsData>(json);
            _counter = LoadCounter(settings.startingNumber, isNeedNewSettings);
            UpdateCounterText();
        }));

        yield return StartCoroutine(LoadLocalJson(GreetingFileName, (json) =>
        {
            GreetingData greeting = JsonUtility.FromJson<GreetingData>(json);
            greetingText.text = greeting.message;
        }));

        yield return StartCoroutine(LoadAssetBundle());
        
        yield return new WaitForSeconds(3f);
    }

    private static IEnumerator LoadLocalJson(string fileName, System.Action<string> callback)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            callback(json);
        }
        else
        {
            Debug.LogError($"Failed to load JSON file: {filePath}");
        }
        yield return null;
    }

    private IEnumerator LoadAssetBundle()
    {
        string bundlePath = Path.Combine(Application.streamingAssetsPath, AssetBundleName);
        if (File.Exists(bundlePath))
        {
            if (_loadedBundle != null)
            {
                _loadedBundle.Unload(true);
            }
            
            _loadedBundle = AssetBundle.LoadFromFile(bundlePath);
            
            if (_loadedBundle != null)
            {
                Sprite sprite = _loadedBundle.LoadAsset<Sprite>("sprite");
                buttonImage.sprite = sprite;
            }
            else
            {
                Debug.LogError("Failed to load Asset Bundle.");
            }
        }
        else
        {
            Debug.LogError("Asset Bundle file not found.");
        }
        yield return null;
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