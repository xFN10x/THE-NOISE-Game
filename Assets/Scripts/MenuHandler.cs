using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    public Canvas MainCanvas;
    public AudioSource MainSource;
    public int MusicStartMessure = 0;

    public List<Texture> Logos;

    private void Start()
    {
        MainSource.time = MusicStartMessure * 2;
        MainSource.Play();
        StartCoroutine(StartingLogos());
    }

    IEnumerator StartingLogos()
    {
        if (MusicStartMessure == 0)
            yield return new WaitForSecondsRealtime(4);
        foreach (var tex in Logos)
        {
            GameObject UIElement = new(Logos.IndexOf(tex).ToString());
            RawImage img = UIElement.AddComponent<RawImage>();
            img.texture = tex;
            img.SetNativeSize();
            img.color = new Color(255, 255, 255);

            UIElement.transform.parent = MainCanvas.gameObject.transform;
            UIElement.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);

            img.DOColor(new Color(255, 255, 255, 0), 3).WaitForCompletion();
            yield return new WaitForSecondsRealtime(4);
        }

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
        MainCanvas.enabled = false;
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
    }
}
