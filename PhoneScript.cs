using UnityEngine;
using System;

public class PhoneScript : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider bc;
    public AudioSource src;
    public static float ringLength;
    public float timer, displaceSave, rotateOffset, displaceOffset;
    public bool ringing;
    public int ringType;
    public Vector3 origPos, origRot;
    public Transform receiver;
    GameScript game;
    private void Awake()
    {
        bc = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        src = GetComponent<AudioSource>();
        timer = 0.0f;
        ringing = true;
        ringLength = 1.988f;
        rotateOffset = 4.0f;
        displaceSave = 0.006f;
        displaceOffset = displaceSave;
        receiver = transform.GetChild(0);
        origPos = receiver.localPosition;
        origRot = receiver.localEulerAngles;
    }
    private void Start()
    {
        game = GameObject.FindGameObjectWithTag("Game").GetComponent<GameScript>();
    }
    private void Update()
    {
        if (game.paused) return;
        if (ringing)
        {
            timer += Time.deltaTime;
            switch (ringType)
            {
                case 0:
                    receiver.localEulerAngles = new Vector3(origRot.x, origRot.y, origRot.z + rotateOffset);
                    rotateOffset *= -1.0f;
                    break;
                case 1:
                    receiver.localPosition = new Vector3(origPos.x, origPos.y + displaceOffset, origPos.z);
                    if (displaceOffset > 0.0f) displaceOffset = 0.0f;
                    else displaceOffset = displaceSave;
                    break;
                case 2:
                    receiver.localPosition = new Vector3(origPos.x + displaceOffset, origPos.y, origPos.z);
                    displaceOffset *= -1.0f;
                    break;
            }
            if (timer >= ringLength)
            {
                timer = 0.0f;
                ringing = false;
                receiver.localEulerAngles = new Vector3(origRot.x, origRot.y, origRot.z);
                receiver.localPosition = new Vector3(origPos.x, origPos.y, origPos.z);
            }
        }
        else if (!ringing)
        {
            if (game.phase3Phones != null && game.phase3Phones.gameObject.activeSelf)
            {
                if (!game.ring2.isPlaying)
                {
                    ringing = true;
                    if(transform.GetSiblingIndex() == (transform.parent.childCount - 1)) game.ring2.Play();
                }
            }
            else
            {
                if (src != null && !src.isPlaying)
                {
                    ringing = true;
                    src.Play();
                }
            }
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (rb != null) Destroy(GetComponent<Rigidbody>());
        if (String.Compare(name, "God Phone") == 0) return;
        if (bc != null && String.Compare(transform.parent.name, "Phase 2 Phones") == 0) Destroy(GetComponent<BoxCollider>());
    }
}