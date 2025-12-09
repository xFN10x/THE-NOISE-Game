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

    private InputAction PointAction;

    private void Start()
    {
        MakeActiveAfterLogos.gameObject.SetActive(false);
        MakeActiveAfterLogos.interactable = false;
        BGCoveringUIElement.gameObject.SetActive(true);
        PointAction = GetComponent<PlayerInput>().actions.FindAction("Point");

        MainSource.time = MusicStartMessure * 2;
        StartCoroutine(StartingLogos());
    }

    private void Update()
    {
        Vector2 mousePos = PointAction.ReadValue<Vector2>();
        MainCamera.gameObject.transform.position = new Vector3(
            ((mousePos.x - (Screen.width / 2)) / Screen.width) / 10,
            ((mousePos.y - (Screen.height / 2)) / Screen.height) / 10 + 1.42f,
            0);
    }

    IEnumerator StartCutscene()
    {
        MainSource.time = 200;
        MakeActiveAfterLogos.DOFade(0, 3f);
        yield return new WaitForSeconds(4);
        BGCoveringUIElement.color = Color.black;
        DontDestroyOnLoad(MainSource.gameObject);
        yield return new WaitForSeconds(4);
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
    }

    IEnumerator StartingLogos()
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

            UIElement.transform.parent = MainCanvas.gameObject.transform;
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
}
