using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class MenuHandler : MonoBehaviour
{
    public Camera MainCamera;
    public Canvas MainCanvas;
    public CanvasGroup MakeActiveAfterLogos;
    public RawImage BGCoveringUIElement;
    public AudioSource MainSource;
    public int MusicStartMessure = 0;

    public List<Texture> Logos;

    private GameObject CurrentMenu;
    private InputAction PointAction;
    private InputActionAsset Actions;

    public TMP_Dropdown ResolutionSelector;
    public TMP_Dropdown RefreshRateSelector;
    public TMP_Dropdown FullscreenSelector;
    public TMP_Dropdown QualitySelector;
    public Slider VolumeSlider;
    public Toggle FPSCounterEnabledToggle;
    public TMP_Dropdown FPSCapSelector;
    public TextMeshProUGUI DebugText = null;

    public FPSCounter FPSCounter;


    public SpriteRenderer CreepyFace;

    public string[] QualityNames;

    private readonly List<string> FPSLimitList = new(new string[] { "30", "60", "90", "120", "144", "240", "360", "Unlimited" });
    private readonly SortedDictionary<string, Resolution> SupportedResolutions = new();
    private readonly SortedDictionary<string, FullScreenMode> FullscreenNames = new();

    public bool CanDebug()
    {
        return Debug.isDebugBuild && DebugText != null;
    }

    private void Start()
    {
        CreepyFace.enabled = false;
        if (DebugText != null)
        {
            if (Debug.isDebugBuild)
            {
                DebugText.gameObject.SetActive(true);
            }
            else
            {
                DebugText.gameObject.SetActive(false);
            }
        }
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            ResolutionSelector.interactable = false;
            ResolutionSelector.interactable = false;
            FullscreenSelector.interactable = false;
        }
        //add fullscreen modes
        FullscreenNames.Add("Borderless Fullscreen", FullScreenMode.FullScreenWindow);
        FullscreenNames.Add("Windowed", FullScreenMode.Windowed);
        if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
        {
            FullscreenNames.Add("Fullscreen", FullScreenMode.ExclusiveFullScreen);
        }
        FPSCounterEnabledToggle.onValueChanged.AddListener(val =>
        {
            FPSCounter.gameObject.SetActive(val);
        });
        VolumeSlider.onValueChanged.AddListener(val =>
        {
            AudioListener.volume = val;
        });

        Actions = GetComponent<PlayerInput>().actions;
        CurrentMenu = MakeActiveAfterLogos.gameObject;
        MakeActiveAfterLogos.gameObject.SetActive(false);
        MakeActiveAfterLogos.interactable = false;
        BGCoveringUIElement.gameObject.SetActive(true);
        PointAction = Actions.FindAction("Point");
        SetupSettings();

        Actions.FindAction("SkipCutscene").performed += SkipIntro;
        MainSource.time = MusicStartMessure * 2;
        StartCoroutine(StartingLogos());

    }

    private string MakeResolutionString(Resolution res)
    {
        return $"{res.width}x{res.height}";
    }

    private string MakeRefreshRateString(Resolution res)
    {
        return $"{res.refreshRateRatio}hz";
    }

    public void ApplySettings()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            //set resolution settings
            string resString = ResolutionSelector.options[ResolutionSelector.value].text;
            int width = SupportedResolutions[resString].width;
            int height = SupportedResolutions[resString].height;
            string hertzText = RefreshRateSelector.options[RefreshRateSelector.value].text;
            string debugText = $"setting resolution to {width}x{height} @ {(uint)int.Parse(hertzText[..(hertzText.IndexOf("h"))])}hz, at {FullscreenNames.Values.ToArray()[FullscreenSelector.value]}";
            if (CanDebug())
                DebugText.SetText(debugText);
            print(debugText);
            Screen.SetResolution(width, height, FullscreenNames.Values.ToArray()[FullscreenSelector.value], new RefreshRate
            {
                numerator = (uint)int.Parse(hertzText[..(hertzText.IndexOf("h"))]),
                denominator = 1u
            });

            PlayerPrefs.SetString("res", resString);
            PlayerPrefs.SetString("rfr", hertzText);
            PlayerPrefs.SetString("fsm", FullscreenNames.Keys.ToArray()[FullscreenSelector.value]);
        }

        QualitySettings.SetQualityLevel(QualitySelector.value);
        PlayerPrefs.SetInt("quality", QualitySelector.value);

        string maxFpsString = FPSCapSelector.options[FPSCapSelector.value].text;
        Application.targetFrameRate = maxFpsString switch
        {
            "Unlimited" => -1,
            _ => int.Parse(maxFpsString),
        };
        PlayerPrefs.SetString("fps", maxFpsString);

        PlayerPrefs.Save();
    }

    public void SetupSettings()
    {
        List<string> resStrings = new();
        List<string> refreshStrings = new();
        foreach (Resolution res in Screen.resolutions)
        {
            if ((res.width / res.height) != (16 / 9))
                continue;

            string propResString = MakeResolutionString(res);
            if (!resStrings.Contains(propResString))
            {
                SupportedResolutions.Add(propResString, res);
                resStrings.Add(propResString);
            }

            string propRRString = MakeRefreshRateString(res);
            if (!refreshStrings.Contains(propRRString))
                refreshStrings.Add(propRRString);

        }

        ResolutionSelector.AddOptions(resStrings);
        RefreshRateSelector.AddOptions(refreshStrings);
        FullscreenSelector.AddOptions(FullscreenNames.Keys.ToList());
        QualitySelector.AddOptions(QualityNames.ToList());
        FPSCapSelector.AddOptions(FPSLimitList);

        if (PlayerPrefs.HasKey("res")) // res is the resolution string. e.g. "1920x1080"
            ResolutionSelector.value = resStrings.IndexOf(PlayerPrefs.GetString("res"));
        else
            ResolutionSelector.value = resStrings.IndexOf(MakeResolutionString(Screen.currentResolution));

        if (PlayerPrefs.HasKey("rfr")) // rfr is the refresh rate string. e.g. "60hz"
            RefreshRateSelector.value = refreshStrings.IndexOf(PlayerPrefs.GetString("rfr"));
        else
            RefreshRateSelector.value = refreshStrings.IndexOf(MakeRefreshRateString(Screen.currentResolution));

        if (PlayerPrefs.HasKey("fsm")) // fsm is the fullscreen name.
            FullscreenSelector.value = FullscreenNames.Keys.ToList().IndexOf(PlayerPrefs.GetString("fsm"));
        else
            FullscreenSelector.value = FullscreenNames.Values.ToList().IndexOf(Screen.fullScreenMode);

        if (PlayerPrefs.HasKey("quality")) // quality is the quality index. e.g. 2 (medium)
            QualitySelector.value = PlayerPrefs.GetInt("quality");
        else
            QualitySelector.value = QualitySettings.GetQualityLevel();

        if (PlayerPrefs.HasKey("fps")) // fps is the fps string. e.g. "60"
            FPSCapSelector.value = FPSLimitList.IndexOf(PlayerPrefs.GetString("fps"));
        else
            FPSCapSelector.value = FPSLimitList.IndexOf("60");

        ApplySettings();
    }

    private void SkipIntro(InputAction.CallbackContext obj)
    {
        MainSource.time = 24;
        if (!MainSource.isPlaying)
            MainSource.Play();
        StopAllCoroutines();
        MakeActiveAfterLogos.alpha = 0;
        MakeActiveAfterLogos.gameObject.SetActive(true);
        MakeActiveAfterLogos.interactable = true;
        BGCoveringUIElement.DOColor(new Color(0, 0, 0, 0), 1);
        MakeActiveAfterLogos.DOFade(1, 2f);
        Actions.FindAction("SkipCutscene").performed -= SkipIntro;
    }

    private void Update()
    {
        Vector2 mousePos = PointAction.ReadValue<Vector2>();
        MainCamera.gameObject.transform.position = new Vector3(
            ((mousePos.x - (Screen.width / 2)) / Screen.width) / 10,
            ((mousePos.y - (Screen.height / 2)) / Screen.height) / 10 + 1.42f,
            -10);
    }

    private IEnumerator SwitchMenu(GameObject menu)
    {
        CurrentMenu.GetComponent<CanvasGroup>().interactable = false;
        yield return CurrentMenu.GetComponent<RectTransform>().DORotate(new Vector3(0, 90, 0), 0.5f).SetEase(Ease.Linear).WaitForCompletion();
        CurrentMenu.SetActive(false);
        CurrentMenu = menu;
        //now, current menu is the new one
        CurrentMenu.SetActive(true);
        CurrentMenu.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        CurrentMenu.GetComponent<CanvasGroup>().interactable = true;
        yield return CurrentMenu.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.Linear).WaitForCompletion();
    }

    private IEnumerator ExitCutscene()
    {
        MainSource.time = 208;
        MakeActiveAfterLogos.DOFade(0, 3f);
        BGCoveringUIElement.DOColor(Color.black, 9);
        yield return new WaitForSeconds(4 + 3.5f);
        CreepyFace.enabled = true;
        yield return new WaitForSeconds(0.5f);
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private IEnumerator StartCutscene()
    {
        MainSource.time = 200;
        MakeActiveAfterLogos.DOFade(0, 3f);
        yield return new WaitForSeconds(3.5f);
        CreepyFace.enabled = true;
        yield return new WaitForSeconds(0.5f);
        BGCoveringUIElement.color = Color.black;
        DontDestroyOnLoad(MainSource.gameObject);
        yield return new WaitForSeconds(4f);
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
    }

    private IEnumerator StartingLogos()
    {
        yield return new WaitForSeconds(2);
        MainSource.Play();
        if (MusicStartMessure == 0)
            yield return new WaitForSeconds(4);

        foreach (var tex in Logos)
        {
            GameObject UIElement = new(Logos.IndexOf(tex).ToString());
            RawImage img = UIElement.AddComponent<RawImage>();
            img.texture = tex;
            img.SetNativeSize();
            img.color = new Color(255, 255, 255);
            img.raycastTarget = false;

            UIElement.transform.SetParent(MainCanvas.transform);
            UIElement.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);

            img.DOColor(new Color(255, 255, 255, 0), 3);
            yield return new WaitForSeconds(4);
            Destroy(UIElement);
        }
        yield return BGCoveringUIElement.DOColor(new Color(0, 0, 0, 0), 1).WaitForCompletion();
        MakeActiveAfterLogos.alpha = 0;
        MakeActiveAfterLogos.gameObject.SetActive(true);
        MakeActiveAfterLogos.interactable = true;
        MakeActiveAfterLogos.DOFade(1, 2f);
    }

    public void Exit()
    {
        MakeActiveAfterLogos.interactable = false;
        StartCoroutine(ExitCutscene());
    }

    public void Play()
    {
        MakeActiveAfterLogos.interactable = false;
        StartCoroutine(StartCutscene());
    }

    public void SwitchMenu(CanvasGroup nextMenu)
    {
        StartCoroutine(SwitchMenu(nextMenu.gameObject));
    }
}
