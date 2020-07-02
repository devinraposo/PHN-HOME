using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;
using TMPro;
using System;
public class MenuScript : MonoBehaviour
{
    int choice;
    bool holdingAxis, mainMenu, focus, waiting;
    float axis, threshold, creditsStart;
    Color highlightColor;
    public Image foreground;
    public GameObject logo, labels, disabilityMessage, disabilityFollowup;
    public TextMeshProUGUI credits;
    string[] Xbox360Joysticks, XboxOneJoysticks, PS3Joysticks, PS4Joysticks;
    string joystick;
    void Awake()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        choice = 0;
        holdingAxis = false;
        mainMenu = false;
        axis = 0.0f;
        threshold = 0.5f;
        Xbox360Joysticks = new[] {
                "Controller (Afterglow Gamepad for Xbox 360)",
                "Controller (Batarang wired controller (XBOX))",
                "Controller (Gamepad for Xbox 360)",
                "Controller (Infinity Controller 360)",
                "Controller (Mad Catz FPS Pro GamePad)",
                "Controller (MadCatz Call of Duty GamePad)",
                "Controller (MadCatz GamePad)",
                "Controller (MLG GamePad for Xbox 360)",
                "Controller (Razer Sabertooth Elite)",
                "Controller (Rock Candy Gamepad for Xbox 360)",
                "Controller (Xbox 360 For Windows)",
                "Controller (Xbox 360 Wireless Receiver for Windows)",
                "XBOX 360 For Windows (Controller)",
                "Controller (XEOX Gamepad)",
                "",
                "Microsoft Wireless 360 Controller",
                "Mad Catz, Inc. Mad Catz FPS Pro GamePad",
                "\\u00A9Microsoft Corporation Controller",
                "\\u00A9Microsoft Corporation Xbox Original Wired Controller",
                "Microsoft X - Box 360 pad",
                "Generic X-Box pad",
                "Controller (XBOX 360 For Windows)"
            };
        XboxOneJoysticks = new[] {
            "Microsoft Xbox One Wired Controller",
            "Controller (XBOX One For Windows)"
        };
        PS3Joysticks = new[] {
            "MotioninJoy Virtual Game Controller",
                "Sony PLAYSTATION(R)3 Controller",
                "SHENGHIC 2009/0708ZXW-V1Inc. PLAYSTATION(R)3Conteroller"
        };
        PS4Joysticks = new[] {
            "Sony Computer Entertainment Wireless Controller",
                "Unknown Wireless Controller",
                "Wireless Controller"
        };
        creditsStart = credits.rectTransform.anchoredPosition.y;
        highlightColor = labels.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        focus = true;
        waiting = false;
        Screen.SetResolution(1366, 768, true);
        if (Screen.fullScreen) labels.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Fullscreen: On";
        else labels.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Fullscreen: Off";
        StartCoroutine(ForegroundFadeIn());
    }
    void Update()
    {
        if (!focus) return;
        JoystickCheck();
        if (disabilityFollowup.activeSelf) return;
        if (disabilityMessage.activeSelf)
        {
            axis = CrossPlatformInputManager.GetAxisRaw("Horizontal");
            if (holdingAxis && axis == 0) holdingAxis = false;
            else if (!holdingAxis && (axis > threshold || axis < -threshold))
            {
                holdingAxis = true;
                disabilityMessage.transform.GetChild(choice).GetComponent<TextMeshProUGUI>().color = Color.white;
                if (axis > threshold)
                {
                    if (choice == 0) ++choice;
                    else choice = 0;
                }
                else if (axis < -threshold)
                {
                    if (choice == 1) choice = 0;
                    else choice = 1;
                }
                disabilityMessage.transform.GetChild(choice).GetComponent<TextMeshProUGUI>().color = highlightColor;
            }
            if (Input.GetButtonDown("Submit")) StartCoroutine(DisabilityFollowup());
            return;
        }
        if (!mainMenu) return;
        axis = CrossPlatformInputManager.GetAxisRaw("Vertical");
        if (holdingAxis && axis == 0) holdingAxis = false;
        else if (!holdingAxis && (axis > threshold || axis < -threshold))
        {
            holdingAxis = true;
            labels.transform.GetChild(choice).GetComponent<TextMeshProUGUI>().color = Color.white;
            if (axis > threshold)
            {
                if (choice == 0) choice = 3;
                else --choice;
            }
            else if (axis < -threshold)
            {
                if (choice == 3) choice = 0;
                else ++choice;
            }
            labels.transform.GetChild(choice).GetComponent<TextMeshProUGUI>().color = highlightColor;
        }
        if (CrossPlatformInputManager.GetButtonDown("Submit")) Submit();
        if (Input.GetButtonDown("CancelKey")
                || (IsPS4Joystick() && CrossPlatformInputManager.GetButtonDown("CancelPS4"))
                || (IsXbox360Joystick() && CrossPlatformInputManager.GetButtonDown("CancelXbox360"))
                || (IsXboxOneJoystick() && CrossPlatformInputManager.GetButtonDown("CancelXboxOne"))
                || (IsPS3Joystick() && CrossPlatformInputManager.GetButtonDown("CancelPS3")))
        {
#if (!UNITY_EDITOR && !UNITY_WEBGL)
                Application.Quit();
#endif
        }
    }
    void Submit()
    {
        switch (choice)
        {
            case 0:
                StartCoroutine(ForegroundFadeOut());
                mainMenu = false;
                break;
            case 1:
                Screen.fullScreen = !Screen.fullScreen;
                if (Screen.fullScreen)
                    labels.transform.GetChild(choice).GetComponent<TextMeshProUGUI>().text = "Fullscreen: Off";
                else
                    labels.transform.GetChild(choice).GetComponent<TextMeshProUGUI>().text = "Fullscreen: On";
                break;
            case 2:
                StartCoroutine(Credits());
                break;
            case 3:
#if (!UNITY_EDITOR && !UNITY_WEBGL)
                Application.Quit();
#endif
                break;
        }
    }
    IEnumerator ForegroundFadeIn()
    {
        float a = 1.0f;
        while (a > 0.0f)
        {
            if (!focus) yield return new WaitForSeconds(0.1f);
            a -= 0.01f;
            foreground.color = new Color(0, 0, 0, a);
            yield return null;
        }
        mainMenu = true;
    }
    IEnumerator ForegroundFadeOut()
    {
        float a = 0.0f;
        while (a < 1.0f)
        {
            if (!focus) yield return new WaitForSeconds(0.1f);
            a += 0.005f;
            foreground.color = new Color(0, 0, 0, a);
            yield return null;
        }
        yield return new WaitForSeconds(3);
        disabilityMessage.SetActive(true);
    }
    IEnumerator Credits()
    {
        labels.SetActive(false);
        mainMenu = false;
        float y = credits.rectTransform.anchoredPosition.y;
        bool canExit = false;
        while (y < 3921f)
        {
            if (!focus) yield return new WaitForSeconds(0.1f);
            if (!canExit && (Input.GetButtonUp("Submit")
                || (IsPS4Joystick() && CrossPlatformInputManager.GetButtonUp("CancelPS4"))
                || (IsXbox360Joystick() && CrossPlatformInputManager.GetButtonUp("CancelXbox360"))
                || (IsXboxOneJoystick() && CrossPlatformInputManager.GetButtonUp("CancelXboxOne"))
                || (IsPS3Joystick() && CrossPlatformInputManager.GetButtonUp("CancelPS3"))))
            {
                canExit = true;
                continue;
            }
            if (canExit && (Input.GetButtonDown("CancelKey")
                || (IsPS4Joystick() && CrossPlatformInputManager.GetButtonDown("CancelPS4"))
                || (IsXbox360Joystick() && CrossPlatformInputManager.GetButtonDown("CancelXbox360"))
                || (IsXboxOneJoystick() && CrossPlatformInputManager.GetButtonDown("CancelXboxOne"))
                || (IsPS3Joystick() && CrossPlatformInputManager.GetButtonDown("CancelPS3"))
                || Input.GetButtonDown("Submit")))
            {
                credits.rectTransform.anchoredPosition = new Vector2(credits.rectTransform.anchoredPosition.x, creditsStart);
                labels.SetActive(true);
                break;
            }
            y += 4.0f;
            credits.rectTransform.anchoredPosition = new Vector2(credits.rectTransform.anchoredPosition.x, y);
            yield return null;
        }
        mainMenu = true;
        credits.rectTransform.anchoredPosition = new Vector2(credits.rectTransform.anchoredPosition.x, creditsStart);
        labels.SetActive(true);
    }
    IEnumerator DisabilityFollowup()
    {
        disabilityMessage.SetActive(false);
        if (choice == 0) DisabilityScript.disability = false;
        else DisabilityScript.disability = true;
        disabilityFollowup.SetActive(true);
        StartCoroutine(Wait(6));
        while (waiting) yield return null;
        disabilityFollowup.SetActive(false);
        StartCoroutine(Wait(5));
        while (waiting) yield return null;
        logo.SetActive(true);
        StartCoroutine(Wait(5));
        while (waiting) yield return null;
        logo.SetActive(false);
        SceneManager.LoadScene("Game");
    }
    bool IsXbox360Joystick()
    {
        if (joystick == null) return false;
        for (int j = 0; j < Xbox360Joysticks.Length; ++j)
        {
            if (String.Compare(joystick, Xbox360Joysticks[j]) == 0) return true;
        }
        return false;
    }
    bool IsXboxOneJoystick()
    {
        if (joystick == null) return false;
        for (int j = 0; j < XboxOneJoysticks.Length; ++j)
        {
            if (String.Compare(joystick, XboxOneJoysticks[j]) == 0) return true;
        }
        return false;
    }
    bool IsPS3Joystick()
    {
        if (joystick == null) return false;
        for (int j = 0; j < PS3Joysticks.Length; ++j)
        {
            if (String.Compare(joystick, PS3Joysticks[j]) == 0) return true;
        }
        return false;
    }
    bool IsPS4Joystick()
    {
        if (joystick == null) return false;
        for (int j = 0; j < PS4Joysticks.Length; ++j)
        {
            if (String.Compare(joystick, PS4Joysticks[j]) == 0) return true;
        }
        return false;
    }
    void JoystickCheck()
    {
        string[] temp = Input.GetJoystickNames();
        for (int i = 0; i < temp.Length; ++i)
        {
            if (temp[i].Length == 0 || String.Compare(temp[i], " ") == 0) continue;
            joystick = temp[i];
            return;
        }
        joystick = null;
    }
    IEnumerator Wait(float seconds)
    {
        waiting = true;
        yield return new WaitForSeconds(seconds);
        waiting = false;
    }
    void OnApplicationFocus(bool hasFocus)
    {
        focus = hasFocus;
        if(!focus) Time.timeScale = 0.0f;
        else Time.timeScale = 1.0f;
    }
}