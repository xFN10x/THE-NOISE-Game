using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
        Actions = GetComponent<PlayerInput>().actions;
        CurrentMenu = MakeActiveAfterLogos.gameObject;
        MakeActiveAfterLogos.gameObject.SetActive(false);
        MakeActiveAfterLogos.interactable = false;
        BGCoveringUIElement.gameObject.SetActive(true);
        PointAction = Actions.FindAction("Point");

        Actions.FindAction("SkipCutscene").performed += SkipIntro;

        MainSource.time = MusicStartMessure * 2;
        StartCoroutine(StartingLogos());
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
            0);
    }

    private IEnumerator SwitchMenu(GameObject menu)
    {
        yield return CurrentMenu.GetComponent<RectTransform>().DORotate(new Vector3(0, 90, 0), 0.5f).SetEase(Ease.Linear).WaitForCompletion();
        CurrentMenu.SetActive(false);
        CurrentMenu = menu;
        //now, current menu is the new one
        CurrentMenu.SetActive(true);
        CurrentMenu.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        yield return CurrentMenu.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.Linear).WaitForCompletion();
    }

    private IEnumerator StartCutscene()
    {
        MainSource.time = 200;
        MakeActiveAfterLogos.DOFade(0, 3f);
        yield return new WaitForSeconds(4);
        BGCoveringUIElement.color = Color.black;
        DontDestroyOnLoad(MainSource.gameObject);
        yield return new WaitForSeconds(4);
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
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else   
            Application.Quit();
#endif
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
