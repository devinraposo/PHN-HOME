using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;
public class Phase4Script : MonoBehaviour
{
    public PhoneScript phonePrefab;
    public AudioSource rain;
    Vector3 start, rot;
    public Transform destination, gimbals, house, house2;
    float timer, radius, rotationSpeed;
    int k;
    public GameObject player, godPhone, mainLight, phase5Lights;
    GameScript game;
    public Image foreground;
    public FirstPersonController controller;
    private void Awake()
    {
        timer = 0.0f;
        radius = 3f;
        k = 0;
        rotationSpeed = 200.0f;
    }
    private void Start()
    {
        game = GameObject.FindGameObjectWithTag("Game").GetComponent<GameScript>();
    }
    // Update is called once per frame
    void Update()
    {
        if (game.paused) return;
        //TODO: need a strong light that will hit the wall you're staring at behind phone 1 to tell you visually that there's something
        //coming behind you
        float time = Time.deltaTime;
        timer += time;
        transform.position = Vector3.Lerp(start, destination.position, timer / 19.0f);
        switch(k++)
        {
            case 0:
                rot = new Vector3(0, 1, 0);
                break;
            case 1:
                rot = new Vector3(0, 1, -1);
                break;
            case 2:
                rot = new Vector3(1, 0, 1);
                break;
            case 3:
                rot = new Vector3(-1, 1, 0);
                break;
            case 4:
                rot = new Vector3(-1, 0, 0);
                break;
        }
        for(int i = 0; i < gimbals.childCount; ++i) gimbals.GetChild(i).Rotate(rot * (rotationSpeed * time));
    }
    private void OnEnable()
    {
        start = transform.position;
        for (int i = 1; i <= 350; ++i)
        {
            float randX = Random.Range(0, radius);
            float randY = Random.Range(0, radius);
            float randZ = Random.Range(0, radius);
            if (i % 2 == 0)
            {
                randX *= -1;
                randY *= -1;
                randZ *= -1;
            }
            Vector3 vec = new Vector3(randX, randY, randZ);
            randX = Random.Range(0.0f, 359.99f);
            randY = Random.Range(0.0f, 359.99f);
            randZ = Random.Range(0.0f, 359.99f);
            Vector3 vec2 = new Vector3(randX, randY, randZ);
            Transform trans = transform.GetChild(0).GetChild(i % 5);
            PhoneScript phone = Instantiate(phonePrefab, trans, false);
            phone.transform.localPosition = vec;
            phone.transform.localRotation = Quaternion.Euler(vec2);
            Destroy(phone.GetComponent<BoxCollider>());
            Destroy(phone.GetComponent<Rigidbody>());
            int j = 0;
            if (i % 2 == 0) j = 1;
            else if (i % 3 == 0) j = 2;
            phone.ringType = j;
            float ting = Random.Range(0.0f, PhoneScript.ringLength);
            phone.timer = ting;
            phone.src.time = ting;
        }
        k = 0;
    }
    void OnTriggerEnter(Collider other)
    {
        foreground.color = new Color(0, 0, 0, 1);
        foreground.gameObject.SetActive(true);
        Destroy(house.gameObject);
        Transform daHouse = Instantiate(house2, new Vector3(0, 0, 86), house.rotation);
        daHouse.localScale = house.localScale;
        daHouse.name = "House 2";
        godPhone.transform.position = new Vector3(godPhone.transform.position.x, godPhone.transform.position.y,
            godPhone.transform.position.z + 60.47f);
        godPhone.GetComponent<PhoneScript>().enabled = false;
        godPhone.GetComponent<BoxCollider>().enabled = true;
        controller.enabled = false;
        game.startPhase5 = true;
        Destroy(player.transform.GetChild(1).gameObject);
        Destroy(mainLight);
        player.transform.position = new Vector3(15.73f, 1.205f, 94.595f);
        player.GetComponent<FirstPersonController>().m_MouseLook.m_CameraTargetRot
            = game.camRotStart;
        player.GetComponent<FirstPersonController>().m_MouseLook.m_CharacterTargetRot
            = game.playerRotStart;
        StartCoroutine(JumpCut());
    }
    IEnumerator JumpCut()
    {
        yield return new WaitForSeconds(0.2f);
        player.GetComponent<AudioSource>().volume = 0.009f;
        foreground.gameObject.SetActive(false);
        phase5Lights.SetActive(true);
        controller.enabled = true;
        rain.Play();
        Destroy(gameObject);
    }
}
