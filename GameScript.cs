using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using Random = UnityEngine.Random;
using System.Text;
using TMPro;

public class GameScript : MonoBehaviour
{
    public FirstPersonController controller;
    public Image foreground;
    public int phoneMask;
    public GameObject cursor, phase4Phones, phase5Phones, phase5Lights, floatingLights;
    public Light playerLight;
    public Transform player, phase1Phones, phase2Phones, tempPhase3Phones,
        phase3Phones, shaker, handReceiver, currReceiver, pauseMenu, phase2Lights;
    public AudioSource pickUp, hangUp, ring2, spotlight, bassDrop, rain, roar, powerDown;
    public AudioSource[] music;
    public PhoneScript phonePrefab, godPhone;
    //this is the trigger for when we want to activate phase 3 which is where random phones start spawning
    bool holdingPhone, finalPhone, zooming,
        waiting, hungUp, holdingAxis, exiting, pickedUp;
    public bool startPhase5, paused, startPhase2, startPhase3, startPhase4, phase5;
    Camera cam;
    public Quaternion playerRotStart, camRotStart;
    Vector3 receiverPos, playerEndStart;
    Quaternion receiverRot;
    AudioSource receiverNoise;
    float lightIntensity, threshold, axis, joystickTriggersThreshold, hangUpSpeed, phase1MusicVolume, phase3MusicVolume,
        shakeDuration, shakeDistance, grabDistance, shakeSpeed;
    public int numPhase3Phones, phonesPickedUp, menuChoice;
    Color textSelectionColor;
    GameObject house2;
    bool hangingUp, disability;
    string[] Xbox360Joysticks, XboxOneJoysticks, PS3Joysticks, PS4Joysticks;
    string joystick;
    void Awake()
    {
        shakeDistance = 0.1f;
        Cursor.visible = false;
        phoneMask = 8;
        phonesPickedUp = 0;
        startPhase2 = false;
        startPhase3 = false;
        startPhase4 = false;
        cam = Camera.main;
        numPhase3Phones = 0;
        holdingPhone = false;
        hungUp = true;
        playerRotStart = player.transform.rotation;
        camRotStart = player.transform.GetChild(0).GetChild(0).localRotation;
        paused = false;
        StartCoroutine(IntroDelay());
        lightIntensity = playerLight.intensity;
        finalPhone = false;
        zooming = false;
        threshold = 0.5f;
        menuChoice = 1;
        axis = 0.0f;
        holdingAxis = false;
        exiting = true;
        textSelectionColor = pauseMenu.GetChild(1).GetComponent<TextMeshProUGUI>().color;
        pickedUp = false;
        hangingUp = false;
        shakeDuration = 3.6f;
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
        joystickTriggersThreshold = 0.3f;
        hangUpSpeed = 1.0f;
        phase1MusicVolume = music[0].volume;
        phase3MusicVolume = music[9].volume;
        playerEndStart = new Vector3(-85.7f, 1.351256f, -3f);
        disability = DisabilityScript.disability;
        if (disability) pauseMenu.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Extreme Visuals: OFF";
        else pauseMenu.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Extreme Visuals: ON";
        if (Screen.fullScreen) pauseMenu.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Fullscreen: On";
        else pauseMenu.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Fullscreen: Off";
        phase5 = false;
        grabDistance = 1.8f;
        shakeSpeed = 0.02f;
    }
    void OnApplicationFocus(bool hasFocus)
    {
        if (exiting) return;
        if (!paused)
        {
            paused = true;
            Pause();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (exiting) return;
        JoystickCheck();
        if (CancelButtonDown())
        {
            paused = !paused;
            Pause();
        }
        if (paused)
        {
            axis = CrossPlatformInputManager.GetAxisRaw("Vertical");
            if (holdingAxis && axis == 0) holdingAxis = false;
            else if(!holdingAxis && (axis > threshold || axis < -threshold))
            {
                holdingAxis = true;
                pauseMenu.GetChild(menuChoice).GetComponent<TextMeshProUGUI>().color = Color.white;
                if(axis > threshold)
                {
                    if (menuChoice == 1) menuChoice = 5;
                    else --menuChoice;
                }
                else if(axis < -threshold)
                {
                    if (menuChoice == 5) menuChoice = 1;
                    else ++menuChoice;
                }
                pauseMenu.GetChild(menuChoice).GetComponent<TextMeshProUGUI>().color = textSelectionColor;
            }
            if(CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                switch (menuChoice)
                {
                    case 1:
                        paused = false;
                        controller.enabled = true;
                        pauseMenu.gameObject.SetActive(false);
                        PauseSounds();
                        Time.timeScale = 1;
                        break;
                    case 2:
                        Screen.fullScreen = !Screen.fullScreen;
                        if (Screen.fullScreen)
                            pauseMenu.GetChild(menuChoice).GetComponent<TextMeshProUGUI>().text = "Fullscreen: Off";
                        else
                            pauseMenu.GetChild(menuChoice).GetComponent<TextMeshProUGUI>().text = "Fullscreen: On";
                        break;
                    case 3:
                        disability = !disability;
                        if (disability) pauseMenu.GetChild(menuChoice).GetComponent<TextMeshProUGUI>().text = "Extreme Visuals: OFF";
                        else pauseMenu.GetChild(menuChoice).GetComponent<TextMeshProUGUI>().text = "Extreme Visuals: ON";
                        break;
                    case 4:
                        ExitToMenu();
                        break;
                    case 5:
#if (!UNITY_EDITOR && !UNITY_WEBGL)
                        Application.Quit();
#endif
                        break;
                }
            }
            return;
        }
        if (!holdingPhone && controller.enabled && !hangingUp) CheckForPhoneInteraction();
        if (phase2Phones != null && phase2Phones.gameObject.activeSelf && roar != null && !roar.isPlaying)
            StartCoroutine(ScreenShake());
    }
    void Pause()
    {
        if (paused) Time.timeScale = 0;
        else Time.timeScale = 1;
        if (!paused && menuChoice != 1)
        {
            pauseMenu.GetChild(menuChoice).GetComponent<TextMeshProUGUI>().color = Color.white;
            pauseMenu.GetChild(1).GetComponent<TextMeshProUGUI>().color = textSelectionColor;
            menuChoice = 1;
        }
        controller.enabled = !controller.enabled;
        pauseMenu.gameObject.SetActive(paused);
        PauseSounds();
    }
    void ExitToMenu()
    {
        exiting = true;
        StartCoroutine(ForegroundFadeOut());
    }
    IEnumerator IntroDelay()
    {
        StartCoroutine(Wait(4.0f));
        while (waiting) yield return null;
        StartCoroutine(ForegroundFadeIn());
    }
    IEnumerator ForegroundFadeIn()
    {
        float a = foreground.color.a;
        while (a > 0.0f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            a -= 0.005f;
            foreground.color = new Color(0, 0, 0, a);
            StartCoroutine(Wait(0.005f));
            while (waiting) yield return null;
        }
        foreground.gameObject.SetActive(false);
        foreground.color = new Color(0, 0, 0, 0.5f);
        controller.enabled = true;
        exiting = false;
        controller.isIntro = false;
    }
    IEnumerator ForegroundFadeOut()
    {
        float a = foreground.color.a;
        foreground.gameObject.SetActive(true);
        while (a < 1.0f)
        {
            a += 0.005f;
            foreground.color = new Color(0, 0, 0, a);
            yield return null;
        }
        SceneManager.LoadScene("Menu");
    }
    void CheckForPhoneInteraction()
    {
        RaycastHit hit;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        bool didHit = Physics.Raycast(ray, out hit, grabDistance);
        if (!didHit || hit.collider.gameObject.layer != phoneMask)
        {
            cursor.SetActive(false);
            return;
        }
        cursor.SetActive(true);
        if (CrossPlatformInputManager.GetButtonDown("Submit") 
            || (IsPS3Joystick() && CrossPlatformInputManager.GetAxis("SubmitPS3Triggers") >= joystickTriggersThreshold)
            || (IsPS4Joystick() && CrossPlatformInputManager.GetAxis("SubmitPS4Triggers") >= joystickTriggersThreshold)
            || (IsXbox360Joystick() && CrossPlatformInputManager.GetAxis("SubmitXbox360Triggers") >= joystickTriggersThreshold)
            || (IsXboxOneJoystick() && CrossPlatformInputManager.GetAxis("SubmitXboxOneTriggers") >= joystickTriggersThreshold))
        {
            cursor.SetActive(false);
            StartCoroutine(HandlePhoneInteraction(hit.collider.gameObject));
        }
    }
    IEnumerator HandlePhoneInteraction(GameObject phone)
    {
        currReceiver = phone.transform.GetChild(0);
        receiverNoise = currReceiver.GetComponent<AudioSource>();
        receiverPos = currReceiver.position;
        currReceiver.localRotation = new Quaternion(0, 0, 0, currReceiver.localRotation.w);
        receiverRot = currReceiver.rotation;
        PhoneScript phoneTing = phone.GetComponent<PhoneScript>();
        int ringType = -1;
        StringBuilder fileName = new StringBuilder("Sounds/");
        if (phoneTing != null)
        {
            if (phase1Phones != null)
            {
                if (phonesPickedUp > 1) StartCoroutine(LowerPhase1Music());
                switch (phonesPickedUp)
                {
                    case 0:
                        fileName.Append("dial tone");
                        break;
                    case 1:
                        fileName.Append("off hook");
                        break;
                    case 2:
                        fileName.Append("beep");
                        break;
                    case 3:
                        fileName.Append("interference");
                        break;
                    case 4:
                        fileName.Append("orchestra");
                        break;
                    case 5:
                        fileName.Append("office");
                        break;
                    case 6:
                        fileName.Append("chair");
                        break;
                    case 7:
                        fileName.Append("beach");
                        break;
                    case 8:
                        fileName.Append("wahwah");
                        break;
                    case 9:
                        fileName.Append("this is");
                        break;
                }
            }
            else if (phase3Phones != null && phase3Phones.gameObject.activeSelf)
            {
                ringType = phoneTing.ringType;
                StartCoroutine(LowerPhase3Music());
                StartCoroutine(LowerRing2());
                if (String.Compare(phone.name, "God Phone") != 0 && ringType == 0)
                {
                    StringBuilder temp = new StringBuilder("correct ");
                    temp.Append((numPhase3Phones + 1).ToString());
                    fileName.Append(temp.ToString());
                }
                else fileName.Append("wrong");
            }
            else if (startPhase2) fileName.Append("this is");
            else if (startPhase3) fileName.Append("glitch");
            else if (startPhase4) fileName.Append("swarm");
            else if (startPhase5) fileName.Append("good bye");
        }
        StartCoroutine(HoldPhone(phone, fileName.ToString()));
        while (holdingPhone) yield return null;
        if (phase1Phones != null) StartCoroutine(UnlowerPhase1Music());
        else if (phase3Phones != null && phase3Phones.gameObject.activeSelf)
        {
            StartCoroutine(UnlowerPhase3Music());
            StartCoroutine(UnlowerRing2());
        }
        if (!phoneTing.enabled || !pickedUp) yield break;
        phoneTing.enabled = false;
        if (startPhase2)
        {
            StartCoroutine(Phase2());
            startPhase2 = false;
        }
        else if (startPhase3)
        {
            startPhase3 = false;
            StartCoroutine(Phase3());
        }
        else if(phase3Phones != null && phase3Phones.gameObject.activeSelf)
        {
            if (ringType == 0 && String.Compare(phone.name, "God Phone") != 0)
            {
                phoneTing.ringType = -1;
                if (++numPhase3Phones < 5) yield break;
                StartCoroutine(FadeOutPhase3Music());
                foreach (Transform child in phase3Phones)
                {
                    child.GetComponent<PhoneScript>().enabled = false;
                    BoxCollider bc = child.GetComponent<BoxCollider>();
                    if (bc != null) bc.enabled = false;
                }
                StartCoroutine(FadeLight(false));
                ring2.Stop();
                powerDown.Play();
                StartCoroutine(Wait(25.0f));
                while (waiting) yield return null;
                StartCoroutine(FadeLight(true));
                playerLight.range = 3.31f;
                startPhase4 = true;
                Destroy(phase3Phones.gameObject);
                ActivateGodPhone();
            }
            else
            {
                numPhase3Phones = 0;
                foreach (Transform child in phase3Phones)
                {
                    PhoneScript ps = child.GetComponent<PhoneScript>();
                    if (!ps.enabled)
                    {
                        foreach(Transform child2 in phase3Phones)
                        {
                            if (!child2.GetComponent<PhoneScript>().enabled) continue;
                            ps.timer = child2.GetComponent<PhoneScript>().timer;
                            break;
                        }
                        ps.enabled = true;
                        int newRingType = -1;
                        while ((newRingType = Random.Range(0,2)) == ringType) continue;
                        ps.ringType = newRingType;
                    }
                }
            }
        }
        else if(startPhase4)
        {
            phase4Phones.SetActive(true);
            startPhase4 = false;
        }
        else if (phase1Phones != null)
        {
            if (phonesPickedUp > 1 && phonesPickedUp < 8) music[phonesPickedUp - 1].Play();
            switch (phonesPickedUp++)
            {
                case 0:
                    StartCoroutine(Wait(45.0f));
                    break;
                case 1:
                    StartCoroutine(Wait(2.0f));
                    break;
                case 2:
                    StartCoroutine(Wait(0.5f));
                    controller.m_WalkSpeed = 2.7f;
                    controller.m_HeadBob.VerticalBobRange = 0.01f;
                    controller.m_HeadBob.HorizontalBobRange = 0.01f;
                    hangUpSpeed = 1.6f;
                    break;
                case 3:
                    controller.m_WalkSpeed = 3.0f;
                    controller.m_HeadBob.VerticalBobRange = 0.07f;
                    controller.m_HeadBob.HorizontalBobRange = 0.07f;
                    hangUpSpeed = 1.7f;
                    break;
                case 4:
                    controller.m_HeadBob.VerticalBobRange = 0.09f;
                    controller.m_HeadBob.HorizontalBobRange = 0.09f;
                    hangUpSpeed = 2.0f;
                    break;
                case 5:
                    controller.m_HeadBob.VerticalBobRange = 0.13f;
                    controller.m_HeadBob.HorizontalBobRange = 0.13f;
                    hangUpSpeed = 2.5f;
                    break;
                case 7:
                    controller.m_WalkSpeed = 4.0f;
                    hangUpSpeed = 3.0f;
                    break;
                case 8:
                    for (int i = 0; i < 7; ++i) music[i].Stop();
                    startPhase2 = true;
                    controller.m_WalkSpeed = 1.4f;
                    controller.m_HeadBob.VerticalBobRange = 0.07f;
                    controller.m_HeadBob.HorizontalBobRange = 0.07f;
                    hangUpSpeed = 1.6f;
                    playerLight.intensity = 0.0f;
                    powerDown.Play();
					Destroy(phase1Phones.gameObject);
                    StartCoroutine(Wait(25.0f));
                    while (waiting) yield return null;
                    StartCoroutine(DimLight(false));
                    ActivateGodPhone();
                    music[7].Play();
                    yield break;
            }
            while (waiting) yield return null;
            if (phonesPickedUp == 2) music[0].Play();
            SpawnPhone(phase1Phones);
        }
    }
    IEnumerator Phase2()
    {
        music[7].Stop();
        controller.m_HeadBob.VerticalBobRange = 0.16f;
        controller.m_HeadBob.HorizontalBobRange = 0.16f;
        playerLight.intensity = lightIntensity;
        music[8].Play();
        StartCoroutine(Phase2StrobeLights());
        StartCoroutine(HangUpPhone());
        controller.enabled = true;
        phase2Phones.gameObject.SetActive(true);
        SpawnPhone(phase2Phones);
        controller.m_WalkSpeed = 5.0f;
        StartCoroutine(Wait(25.0f));
        while (waiting) yield return null;
        SpawnPhone(phase2Phones);
        StartCoroutine(Wait(17.0f));
        while (waiting) yield return null;
        SpawnPhone(phase2Phones);
        StartCoroutine(Wait(11.0f));
        while (waiting) yield return null;
        SpawnPhone(phase2Phones);
        StartCoroutine(Wait(4.0f));
        while (waiting) yield return null;
        SpawnPhone(phase2Phones);
        StartCoroutine(Wait(1.0f));
        while (waiting) yield return null;
        SpawnPhone(phase2Phones);
        StartCoroutine(Wait(.5f));
        while (waiting) yield return null;
        SpawnPhone(phase2Phones);
        foreach(Transform child in phase2Phones)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            if (child.gameObject.activeSelf) continue;
            Instantiate(phonePrefab, child.position,child.rotation,phase2Phones);
            Destroy(child.gameObject);
            yield return new WaitForSeconds(1);
        }
        StartCoroutine(Wait(25.5f));
        while (waiting) yield return null;
        roar.Stop();
        controller.m_WalkSpeed = 2.5f;
        controller.m_HeadBob.VerticalBobRange = 0.07f;
        controller.m_HeadBob.HorizontalBobRange = 0.07f;
        playerLight.gameObject.SetActive(false);
        powerDown.Play();
        Destroy(phase2Phones.gameObject);
        StartCoroutine(Wait(35.0f));
        while (waiting) yield return null;
        StartCoroutine(FadeLight(true));
        ActivateGodPhone();
        startPhase3 = true;
    }
    IEnumerator Phase3()
    {
        playerLight.gameObject.SetActive(false);
        powerDown.Play();
        int i = 0;
        ring2.Play();
        foreach(Transform child in tempPhase3Phones)
        {
            PhoneScript phone = Instantiate(phonePrefab, child.position, child.rotation, phase3Phones);
            Destroy(phone.GetComponent<Rigidbody>());
            Destroy(phone.GetComponent<AudioSource>());
            phone.name = child.name;
            phone.ringType = i++;
            if (i > 2) i = 0;
            if (child.localPosition.y > 2.9f) Destroy(phone.GetComponent<BoxCollider>());
        }
        Destroy(tempPhase3Phones.gameObject);
        StartCoroutine(Wait(ring2.clip.length));
        while (waiting) yield return null;
        phase3Phones.gameObject.SetActive(true);
        music[9].Play();
        playerLight.gameObject.SetActive(true);
        playerLight.range = 9.89f;
        playerLight.intensity = 2.01f;
        spotlight.Play();
        ring2.Play();
    }
    IEnumerator Phase5()
    {
        phase5 = true;
        holdingPhone = false;
        startPhase5 = false;
        StartCoroutine(FinalPhone());
        while (finalPhone) yield return null;
        bassDrop.Play();
        StartCoroutine(SlowZoom());
        while(bassDrop.isPlaying)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            yield return null;
        }
        cam.backgroundColor = new Color(159.0f / 255.0f, 96 / 255.0f, 190.0f / 255.0f,255f);
        house2 = GameObject.Find("House 2");
        while (zooming) yield return null;
        float spacer = 250;
        int bound = 4;
        for(int i = -bound; i < bound; ++i)
        {
            for(int j = -bound; j < bound; ++j)
            {
                for (int k = -bound; k < bound; ++k)
                {
                    PhoneScript ps = Instantiate(phonePrefab, new Vector3(i * spacer, j * spacer, k * spacer), Quaternion.identity, 
                        phase5Phones.transform);
                    Destroy(ps.GetComponent<BoxCollider>());
                    ps.GetComponent<AudioSource>().volume = 0.0f;
                    Destroy(ps.GetComponent<Rigidbody>());
                    ps.transform.localScale = new Vector3(500,500,500);
                }
            }
        }
        music[10].Play();
        phase5Lights.SetActive(false);
        floatingLights.SetActive(true);
        player.position = playerEndStart;
        controller.m_MouseLook.m_CameraTargetRot = new Quaternion(-0.1f,0.0f,0.0f,1.0f);
        controller.m_MouseLook.m_CharacterTargetRot = new Quaternion(0.0f, -179.9f, 0.0f, -4.4f);
        phase5Phones.SetActive(true);
        controller.enabled = true;
        StopCoroutine(SlowZoom());
        cam.fieldOfView = 60.0f;
        StartCoroutine(FloatPlayer());
        house2.SetActive(false);
        godPhone.gameObject.SetActive(false);
        StartCoroutine(Wait(13.0f));
        while (waiting) yield return null;
        foreground.gameObject.SetActive(true);
        StartCoroutine(Wait(7.0f));
        while (waiting) yield return null;
        SceneManager.LoadScene("Menu");
    }
    IEnumerator Wait(float seconds)
    {
        waiting = true;
        yield return new WaitForSeconds(seconds);
        waiting = false;
    }
    PhoneScript SpawnPhone(Transform originalList)
    {
        List<Transform> list = new List<Transform>();
        foreach (Transform child in originalList)
        {
            if (!child.gameObject.activeSelf && Vector3.Distance(player.position, child.position) > 6.0f)
                list.Add(child);
        }
        Transform val;
        if (list.Count == 0)
        {
            while (true)
            {
                val = originalList.GetChild(Random.Range(0, originalList.childCount - 1));
                if (!val.gameObject.activeSelf) break;
            }
        }
        else val = list[Random.Range(0, list.Count - 1)];
        Destroy(val.gameObject);
        PhoneScript val2 = Instantiate(phonePrefab, val.position, val.rotation, originalList);
        return val2;
    }
    IEnumerator ScreenShake()
    {
        if (disability) yield break;
        Vector3 localPos = shaker.localPosition;
        float elapsed = 0.0f, distSave = shakeDistance;
        int i = 0;
        roar.Play();
        while(elapsed < shakeDuration)
        {
			if (paused) yield return new WaitForSeconds(0.1f);
            if (disability) break;
            switch(i++)
            {
                case 0:
                    shaker.localPosition = new Vector3(shaker.localPosition.x + shakeDistance, shaker.localPosition.y - (shakeDistance / 2), 
                        shaker.localPosition.z);
                    break;
                case 1:
                    shaker.localPosition = new Vector3(shaker.localPosition.x - shakeDistance, shaker.localPosition.y + (shakeDistance / 2),
                        shaker.localPosition.z);
                    break;
            }
            if (i == 2) i = 0;
            elapsed += shakeSpeed;
            yield return new WaitForSeconds(shakeSpeed);
        }
        shaker.localPosition = localPos;
        shakeDistance = distSave;
    }
    IEnumerator HoldPhone(GameObject phone, string fileName)
    {
        controller.enabled = false;
        holdingPhone = true;
        pickedUp = false;
        hungUp = true;
        float lerp = 0.0f, lastY = currReceiver.position.y;
        AudioClip src = null;
        while (true)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            float val = -CrossPlatformInputManager.GetAxis("Mouse Y") / 80.0f;
            if (val == 0) val = -CrossPlatformInputManager.GetAxis("Vertical") / 40.0f;
            if (((val > 0 && lerp < 1.0f) || (val < 0 && lerp > 0.0f)) && !((startPhase5 && val < 0))) lerp += val;
            currReceiver.position = Vector3.Lerp(receiverPos, handReceiver.position, lerp);
            currReceiver.rotation = Quaternion.Lerp(receiverRot, handReceiver.rotation, lerp);
            if (hungUp && lerp > 0.01f)
            {
                if(startPhase5) StartCoroutine(FadeOutRain());
                pickedUp = true;
                hungUp = false;
                pickUp.Play();
                AudioSource src2 = phone.GetComponent<AudioSource>();
                if (src2 != null || startPhase5 || (phase3Phones != null && phase3Phones.gameObject.activeSelf))
                {
                    if(!startPhase5) Destroy(src2);
                    src = Resources.Load(fileName) as AudioClip;
                    if (String.Compare(phone.name, "God Phone") == 0)
                    {
                        phone.GetComponent<PhoneScript>().timer = 0.0f;
                        phone.GetComponent<PhoneScript>().ringing = false;
                    }
                }
                else src = Resources.Load("Sounds/off hook 2") as AudioClip;
                receiverNoise.clip = src;
                receiverNoise.Play();
            }
            else if(!hungUp && lerp <= 0.0f)
            {
                hungUp = true;
                hangUp.Play();
                receiverNoise.Stop();
                if(startPhase2)
                {
                    holdingPhone = false;
                    break;
                }
            }
            if(!hungUp && !receiverNoise.isPlaying)
            {
                if(startPhase2)
                {
                    holdingPhone = false;
                    break;
                }
                else if(startPhase5)
                {
                    StartCoroutine(Phase5());
                    break;
                }
                src = Resources.Load("Sounds/off hook 2") as AudioClip;
                receiverNoise.clip = src;
                receiverNoise.Play();
            }
            if (!CrossPlatformInputManager.GetButton("Submit") && CrossPlatformInputManager.GetAxis("SubmitXbox360Triggers") == 0
                && CrossPlatformInputManager.GetAxis("SubmitPS3Triggers") == 0 && CrossPlatformInputManager.GetAxis("SubmitPS4Triggers")
                == 0 && CrossPlatformInputManager.GetAxis("SubmitXboxOneTriggers") == 0)
            {
                if (!startPhase5 && lerp > 0.0f)
                {
                    StartCoroutine(HangUpPhone());
                    break;
                }
                if (lerp <= 0.0f)
                {
                    controller.enabled = true;
                    holdingPhone = false;
                    currReceiver.localRotation = new Quaternion(0, 0, 0, currReceiver.localRotation.w);
                    break;
                }
                if (startPhase5)
                {
                    while (receiverNoise.isPlaying)
                    {
                        if (paused) yield return new WaitForSeconds(0.1f);
                        val = -CrossPlatformInputManager.GetAxis("Mouse Y") / 80.0f;
                        if (val == 0) val = -CrossPlatformInputManager.GetAxis("Vertical") / 80.0f;
                        if (((val > 0 && lerp < 1.0f) || (val < 0 && lerp > 0.0f)) && !((startPhase5 && val < 0))) lerp += val;
                        currReceiver.position = Vector3.Lerp(receiverPos, handReceiver.position, lerp);
                        currReceiver.rotation = Quaternion.Lerp(receiverRot, handReceiver.rotation, lerp);
                        yield return null;
                    }
                    StartCoroutine(Phase5());
                    break;
                }
            }
            lastY = currReceiver.position.y;
            yield return null;
        }
    }
    IEnumerator HangUpPhone()
    {
        float time = 0.0f;
        Vector3 receiverStartPos = currReceiver.position;
        Quaternion receiverStartRot = currReceiver.rotation;
        float dist = Vector3.Distance(receiverStartPos, receiverPos);
        hangingUp = true;
        if (phase3Phones != null && phase3Phones.gameObject.activeSelf)
        {
            StartCoroutine(UnlowerPhase3Music());
            StartCoroutine(UnlowerRing2());
        }
        while (true)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            time += (Time.deltaTime / dist) * hangUpSpeed;
            currReceiver.position = Vector3.Lerp(receiverStartPos, receiverPos, time);
            currReceiver.rotation = Quaternion.Lerp(receiverStartRot, receiverRot, time);
            if (time >= 1.0f) break;
            yield return null;
        }
        hangingUp = false;
        currReceiver.localRotation = new Quaternion(0, 0, 0, currReceiver.localRotation.w);
        receiverNoise.Stop();
        hangUp.Play();
        controller.enabled = true;
        holdingPhone = false;
        hungUp = true;
    }
    void ActivateGodPhone()
    {
        godPhone.enabled = true;
        godPhone.ringing = true;
        godPhone.timer = 0.0f;
        AudioSource src = godPhone.gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.priority = 128;
        src.volume = 0.181f;
        src.pitch = 1;
        src.panStereo = 0;
        src.spatialBlend = 1.0f;
        src.reverbZoneMix = 0.264f;
        src.dopplerLevel = 0.0f;
        src.spread = 82;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.minDistance = 0.83f;
        src.maxDistance = 100057.9f;
        src.clip = Resources.Load("Sounds/ring 1") as AudioClip;
        godPhone.src = src;
        src.Play();
    }
    void PauseSounds()
    {
        AudioSource[] src = GameObject.FindObjectsOfType<AudioSource>();
        for(int i = 0; i < src.Length; ++i)
        {
            if (paused) src[i].Pause();
            else src[i].UnPause();
        }
    }
    IEnumerator FadeLight(bool fadeIn)
    {
        if (fadeIn)
        {
            playerLight.gameObject.SetActive(true);
            for (float f = 0; f < lightIntensity; f += 0.01f)
            {
                playerLight.intensity = f;
                yield return null;
            }
            playerLight.intensity = lightIntensity;
        }
        else
        {
            for(float f = playerLight.intensity; f > 0; f -= 0.01f)
            {
                playerLight.intensity = f;
                yield return null;
            }
            playerLight.gameObject.SetActive(false);
        }
    }
    IEnumerator FinalPhone()
    {
        Transform thing = player.GetChild(0).GetChild(0).GetChild(1);
        finalPhone = true;
        float time = 0.0f;
        Vector3 startPos = currReceiver.position;
        Quaternion startRot = currReceiver.rotation;
        Vector3 endPos = thing.position;
        Quaternion endRot = thing.rotation;
        while(time < 1.0f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            time += Time.deltaTime;
            currReceiver.position = Vector3.Lerp(startPos, endPos, time);
            currReceiver.rotation = Quaternion.Lerp(startRot, endRot, time);
            yield return null;
        }
        finalPhone = false;
    }
    IEnumerator FloatPlayer()
    {
        player.GetComponent<CharacterController>().enabled = false;
        controller.doMove = false;
        while (true)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            player.position = new Vector3(player.position.x,
                player.position.y, player.position.z - 0.16f);
            yield return null;
        }
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
            if (temp[i].Length == 0 || String.Compare(temp[i]," ") == 0) continue;
            joystick = temp[i];
            return;
        }
        joystick = null;
    }
    bool CancelButtonDown()
    {
        if (CrossPlatformInputManager.GetButtonDown("CancelKey") 
            || (IsPS4Joystick() && CrossPlatformInputManager.GetButtonDown("CancelPS4"))
            || (IsXbox360Joystick() && CrossPlatformInputManager.GetButtonDown("CancelXbox360"))
            || (IsXboxOneJoystick() && CrossPlatformInputManager.GetButtonDown("CancelXboxOne"))
            || (IsPS3Joystick() && CrossPlatformInputManager.GetButtonDown("CancelPS3"))) return true;
        return false;
    }
    IEnumerator FadeOutRain()
    {
        float volume = rain.volume;
        while(volume > 0.0f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            volume -= 0.003f;
            rain.volume = volume;
            yield return new WaitForSeconds(0.02f);
        }
    }
    IEnumerator FadeOutPhase3Music()
    {
        while(music[9].volume > 0.0f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            music[9].volume -= 0.003f;
            yield return null;
        }
        music[9].Stop();
    }
    IEnumerator LowerPhase1Music()
    {
        float volume = phase1MusicVolume;
        while (volume > 0.1f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            volume -= 0.003f;
            for (int i = 0; i < 7; ++i) music[i].volume = volume;
            yield return null;
        }
        for (int i = 0; i < 7; ++i) music[i].volume = 0.1f;
    }
    IEnumerator UnlowerPhase1Music()
    {
        float volume = music[0].volume;
        while (volume < phase1MusicVolume)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            volume += 0.003f;
            for (int i = 0; i < 7; ++i) music[i].volume = volume;
            yield return null;
        }
        for (int i = 0; i < 7; ++i) music[i].volume = phase1MusicVolume;
    }
    IEnumerator LowerPhase3Music()
    {
        while(music[9].volume > 0.001f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            music[9].volume -= 0.006f;
            yield return null;
        }
        music[9].volume = 0.01f;
    }
    IEnumerator UnlowerPhase3Music()
    {
        while(music[9].volume < phase3MusicVolume)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            music[9].volume += 0.006f;
            yield return null;
        }
        music[9].volume = phase3MusicVolume;
    }
    IEnumerator LowerRing2()
    {
        while(ring2.volume > 0.01f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            ring2.volume -= 0.006f;
            yield return null;
        }
        ring2.volume = 0.01f;
    }
    IEnumerator UnlowerRing2()
    {
        while(ring2.volume < 0.06f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            ring2.volume += 0.006f;
            yield return null;
        }
        ring2.volume = 0.06f;
    }
    IEnumerator DimLight(bool fadeOut)
    {
        if (fadeOut)
        {
            for (float f = playerLight.intensity; f > 2.5f; f -= .01f)
            {
                if (paused) yield return new WaitForSeconds(0.1f);
                playerLight.intensity = f;
                yield return null;
            }
            playerLight.intensity = 2.5f;
            yield break;
        }
        for(float f = playerLight.intensity; f < lightIntensity; f += .01f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            playerLight.intensity = f;
            yield return null;
        }
    }
    IEnumerator SlowZoom()
    {
        float camIter = 0.003f;
        Vector3 camSave = cam.transform.position;
        for (float i = cam.fieldOfView; i > 0; i -= 0.7f)
        {
            if (paused) yield return new WaitForSeconds(0.1f);
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y + camIter,
                cam.transform.position.z);
            cam.fieldOfView = i;
            yield return new WaitForSeconds(.01f);
        }
        cam.transform.position = camSave;
    }
    List<bool> isStrobingList;
    IEnumerator Phase2StrobeLights()
    {
        float timer = 0.0f;
        if (disability)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (timer >= 90f) yield break;
        phase2Lights.gameObject.SetActive(true);
        List<float> delayList = new List<float>();
        List<float> delay2List = new List<float>();
        isStrobingList = new List<bool>();
        int numLights = phase2Lights.childCount;
        for(int i = 0; i < numLights; ++i) delayList.Add(Random.Range(1.0f, 2.7f));
        for (int i = 0; i < numLights; ++i) delay2List.Add(0);
        for (int i = 0; i < numLights; ++i) isStrobingList.Add(false);
        float intensity = 1.5f;
        while (timer < 90f)
        {
			if (paused) yield return new WaitForSeconds(0.01f);
            if (disability)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            float timerIncrement = Time.deltaTime;
            timer += timerIncrement;
            for(int i = 0; i < numLights; ++i)
            {
                delay2List[i] += timerIncrement;
                if (!isStrobingList[i] && delay2List[i] >= delayList[i])
                {
                    if (timer >= 69.8f)
                    {
                        intensity = 0.08f;
                        phase2Lights.GetChild(i).GetComponent<Light>().color =
                            new Color(Random.Range(55, 255), Random.Range(0, 137), Random.Range(0, 255));
                        if(delayList[i] > 0) delayList[i] -= 0.05f;
                        if (delayList[i] < 0) delayList[i] = 0;
                    }
                    delay2List[i] = 0;
                    StartCoroutine(StrobeLight(phase2Lights.GetChild(i).GetComponent<Light>(), i, intensity));
                }
            }
            yield return null;
        }
        intensity = 1.5f;
        while (intensity > 0.0f)
        {
            if (paused) yield return new WaitForSeconds(0.01f);
            if (disability) break;
            intensity -= 0.005f;
            foreach(Transform child in phase2Lights)
            {
                child.GetComponent<Light>().intensity = intensity;
            }
            yield return null;
        }
        phase2Lights.gameObject.SetActive(false);
    }
    IEnumerator StrobeLight(Light light, int i, float intensity)
    {
        isStrobingList[i] = true;
        for(float f = 0.0f; f < intensity; f += 0.02f)
        {
            if (paused) yield return new WaitForSeconds(0.01f);
            if (disability) break;
            light.intensity = f;
            yield return null;
        }
        for(float f = intensity; f > 0.0f; f -= 0.02f)
        {
            if (paused) yield return new WaitForSeconds(0.01f);
            if (disability) break;
            light.intensity = f;
            yield return null;
        }
        light.intensity = 0.0f;
        isStrobingList[i] = false;
    }
}
