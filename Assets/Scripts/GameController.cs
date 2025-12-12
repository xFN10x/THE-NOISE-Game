using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum InteractAction
    {
        ReadyToPrepareAction,
        BedAction,
        DoorAction,
        Cutscene
    }
    public static GameController CurrentGameController;

    public PlayerController Player;

    public Camera CutsceneCamera;
    public Image FadePanel;
    public TextMeshProUGUI DialougeText;
    public TextMeshProUGUI PrepareText;

    public GameObject OutsideTP;
    //private bool isOutside = false;
    private bool isPrepared = false;
    public Canvas MainCanvas;
    public Canvas OtherCanvas;
    public RawImage Haunted;
    public TextMeshProUGUI HauntedText;
    public AudioClip HauntedTalkSound;
    public AudioSource OutdoorAmbientSource;
    public AudioSource MainSource;

    public RawImage AngryHauntedBG;
    public Texture AngryHauntedImage;
    public AudioClip AngryHauntedJumpscare;

    public AudioSource Cutscene1Sound;
    public SpriteRenderer Cutscene1Sprite;
    public AudioSource ShotgunSound;

    public void DoInteract(InteractAction action)
    {
        switch (action)
        {
            case InteractAction.ReadyToPrepareAction:
                StartCoroutine(GetReadyToPrepare());
                break;
            case InteractAction.BedAction:
                StartCoroutine(ShowText("You are too scared to go back to sleep."));
                break;
            case InteractAction.DoorAction:
                StartCoroutine(GoOutside());
                break;
            case InteractAction.Cutscene:
                PlayerController plr = GameController.CurrentGameController.Player;
                plr.ControlsEnabled = false;
                StartCoroutine(Cutscene1());
                break;
            default:
                StartCoroutine(ShowText("Nothing happens."));
                break;
        }
    }
    IEnumerator Cutscene1()
    {
        yield return new WaitForSeconds(2);
        PlayerController plr = GameController.CurrentGameController.Player;
        plr.Camera.DOFieldOfView(50, 1f);
        yield return plr.transform.DORotate(new Vector3(0, 270, 0), 5f).WaitForCompletion();
        yield return new WaitForSeconds(2);
        StartCoroutine(ShowText("\"Whew, nothing there...\""));
        Cutscene1Sprite.enabled = true;
        yield return new WaitForSeconds(1);
        yield return plr.transform.DORotate(new Vector3(0, 130, 0), 5f).SetEase(Ease.Linear).WaitForCompletion();
        plr.transform.LookAt(Cutscene1Sound.transform);
        plr.Camera.transform.LookAt(Cutscene1Sound.transform);
        plr.Camera.transform.localEulerAngles = new Vector3(plr.Camera.transform.localEulerAngles.x + -20, 0, 0);
        plr.transform.localEulerAngles = new Vector3(0, plr.transform.localEulerAngles.y, 0);
        Cutscene1Sound.Play();
        if (isPrepared)
        {
            yield return new WaitForSeconds(1.1f);
            OutdoorAmbientSource.Stop();
            ShotgunSound.Play();
            yield return new WaitForSeconds(0.1f);
            FadePanel.color = Color.black;
            Cutscene1Sprite.enabled = false;
            Cutscene1Sound.Stop();
            yield return new WaitForSeconds(3);
            //FadePanel.DOFade(0f, 4f);
            //OutdoorAmbientSource.Play();
            plr.Camera.DOFieldOfView(70, 1f);
            plr.ControlsEnabled = true;
            yield return new WaitForSeconds(1);
            StartCoroutine(ShowText("To be continued."));
            yield return new WaitForSeconds(5);
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else   
            Application.Quit();
#endif
            /*
            StartCoroutine(ShowText("\"Fuck it, I need to leave.\""));
            yield return new WaitForSeconds(3);
            StartCoroutine(ShowText("\"I don't think it's safe to get my car... but it's the only way out...\""));
            yield return new WaitForSeconds(3);*/
        }
        else
        {
            yield return new WaitForSeconds(1.1f);
            FadePanel.color = Color.black;
            Cutscene1Sprite.enabled = false;
            OtherCanvas.enabled = true;
            MainCanvas.enabled = false;
            Cutscene1Sound.Stop();
            OutdoorAmbientSource.Stop();
            yield return new WaitForSeconds(5);
            yield return new WaitForSeconds(3);
            Haunted.enabled = true;
            Haunted.color = new Color(0, 0, 0, 0);
            yield return Haunted.DOColor(Color.white, 3f).WaitForCompletion();
            yield return new WaitForSeconds(3);
            StartCoroutine(ShowHauntedText("SO,"));
            yield return new WaitForSeconds(3);
            StartCoroutine(ShowHauntedText("YOU WERE LET OFF EASY."));
            yield return new WaitForSeconds(4);
            StartCoroutine(ShowHauntedText("SADLY,"));
            yield return new WaitForSeconds(4);
            StartCoroutine(ShowHauntedText("THE EXPERIMENTS COULDN'T BE DONE."));
            yield return new WaitForSeconds(10);
            StartCoroutine(ShowHauntedText("EXPERIMENT 101" + DateTime.UnixEpoch.Millisecond + " CONCLUDED: "));
            yield return new WaitForSeconds(10);
            MainSource.PlayOneShot(AngryHauntedJumpscare);
            AngryHauntedBG.enabled = true;
            Haunted.texture = AngryHauntedImage;
            Haunted.rectTransform.DOSizeDelta(new Vector2(15000, 15000), 1f).SetEase(Ease.Linear);
            yield return new WaitForSeconds(AngryHauntedJumpscare.length);
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else   
            Application.Quit();
#endif
        }
    }

    private void Awake()
    {
        CurrentGameController = this;
    }
    void Start()
    {
        AngryHauntedBG.enabled = false;
        MainCanvas.enabled = true;
        OtherCanvas.enabled = false;
        Haunted.enabled = false;
        HauntedText.text = "";
        Cutscene1Sprite.enabled = false;
        Player.Camera.enabled = false;
        StartCoroutine(StartingCutscene());
    }
    IEnumerator GoOutside()
    {
        StartCoroutine(ShowText("Leaving the bedroom, and going outside."));
        Player.ControlsEnabled = false;
        yield return FadePanel.DOFade(1f, 3f).WaitForCompletion();
        Player.transform.position = OutsideTP.transform.position;
        //isOutside = true;
        Player.ControlsEnabled = true;
        yield return FadePanel.DOFade(0f, 3f).WaitForCompletion();
    }

    IEnumerator GetReadyToPrepare()
    {
        yield return new WaitForSeconds(2);
        StartCoroutine(ShowText("It's not safe out here. Prepare yourself."));
        yield return new WaitForSeconds(1);
        PrepareText.enabled = true;
        Player.GetComponent<PlayerInput>().actions.FindAction("Interact 2").performed += GetGun;
    }

    private void GetGun(InputAction.CallbackContext obj)
    {
        isPrepared = true;
        PrepareText.enabled = false;
        Player.GetComponent<PlayerInput>().actions.FindAction("Interact 2").performed -= GetGun;
        Player.Gun.transform.DOLocalMove(new Vector3(0.57099998f, -0.736999989f, 0.800999999f), 3f);
        Player.Gun.transform.DOLocalRotate(new Vector3(0, 270, 0), 3f);
        //Player.Gun.transform.DOScale(new Vector3(2.00509167f, 1.99226785f, 3.5453999f), 3f);
    }

    IEnumerator ShowHauntedText(string text)
    {
        HauntedText.text = "";
        for (int i = 0; i < (text.Length + 1); i++)
        {
            yield return new WaitForEndOfFrame();
            HauntedText.text = text[..i];
            MainSource.PlayOneShot(HauntedTalkSound);
        }
        yield return new WaitForSeconds(3);
        if (HauntedText.text.Equals("text"))
        {
            for (float i = 1f; i > 0f; i -= 0.01f)
            {
                yield return new WaitForEndOfFrame();
                HauntedText.alpha = i;
            }
        }
    }

    IEnumerator ShowText(string text)
    {
        DialougeText.text = "";
        DialougeText.color = Color.white;
        for (int i = 0; i < (text.Length + 1); i++)
        {
            yield return new WaitForEndOfFrame();
            DialougeText.text = text[..i];
        }
        yield return new WaitForSeconds(3);
        if (DialougeText.text.Equals("text"))
            for (float i = 1f; i > 0f; i -= 0.01f)
            {
                yield return new WaitForEndOfFrame();
                DialougeText.alpha = i;
            }
    }
    IEnumerator StartingCutscene()
    {
        yield return new WaitForSeconds(2);
        yield return FadePanel.DOFade(0f, 3f).WaitForCompletion();
        yield return new WaitForSeconds(1);
        StartCoroutine(ShowText("You've heard a noise outside..."));
        yield return new WaitForSeconds(5);
        StartCoroutine(ShowText("This one sounds different from the others..."));
        yield return new WaitForSeconds(3);
        StartCoroutine(ShowText("Go find the source of the noise... Make sure it's not a threat."));
        CutsceneCamera.transform.DORotate(Player.Camera.transform.eulerAngles, 2f);
        yield return CutsceneCamera.transform.DOMove(Player.Camera.transform.position, 2f).WaitForCompletion();
        Player.ControlsEnabled = true;
        Player.Camera.enabled = true;
        CutsceneCamera.enabled = false;
    }
}
