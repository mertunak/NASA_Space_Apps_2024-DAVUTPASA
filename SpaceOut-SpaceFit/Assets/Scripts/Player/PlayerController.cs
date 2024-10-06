using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;  // Dictionary kullanmak için

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Mixamo kemik referansları
    public Transform head;
    public Transform leftHand;
    public Transform leftForeArm;
    public Transform rightHand;
    public Transform rightForeArm;
    public Transform leftFoot;
    public Transform leftLeg;
    public Transform rightFoot;
    public Transform rightLeg;

    private Vector3 dhead;
    private Vector3 dleftHand;
    private Vector3 dleftForeArm;
    private Vector3 drightHand;
    private Vector3 drightForeArm;
    private Vector3 dleftFoot;
    private Vector3 dleftLeg;
    private Vector3 drightFoot;
    private Vector3 drightLeg;

    private Dictionary<string, Vector3> jointPositions;  // Gelen JSON verilerini saklamak için

    private float switchTime = 3.7f;  // Her 5 saniyede bir veri değişimi
    private float timer = 0f;
    private int currentDataIndex = 0;  // Şu anda kullanılan JSON datasının indeksi

    void Start()
    {
        animator = GetComponent<Animator>();
        jointPositions = new Dictionary<string, Vector3>();

        dhead = head.position;
        dleftHand = leftHand.position;
        dleftForeArm = leftForeArm.position;
        drightHand = rightHand.position;
        drightForeArm = rightForeArm.position;
        dleftFoot = leftFoot.position;
        dleftLeg = leftLeg.position;
        drightFoot = rightFoot.position;
        drightLeg = rightLeg.position;

        // Astronotun başlangıç pozisyonunu ayarla (sabit pozisyon)
    }

    void Update()
    {
        if (GameManager.gameOver || !GameManager.isGameStarted)
            return;

        // Zamanlayıcıyı güncelle ve 5 saniyede bir veri değiştir
        timer += Time.deltaTime;
        if (timer > switchTime)
        {
            timer = 0f;
            currentDataIndex = (currentDataIndex + 1) % 3;  // 3 tane veri var, her 5 saniyede bir sıradaki veriyi kullan
        }

        // MediaPipe'den gelen JSON verilerini al
        string jsonData = GetMediapipeData(currentDataIndex);  // Sıradaki MediaPipe verisini al
        UpdateJointPositionsFromJson(jsonData);  // JSON'dan kemik pozisyonlarını güncelle

        // Gelen verilere göre karakterin postürünü güncelle
        UpdatePosture();
        CheckForCollision();  // Çarpışma kontrolü
    }

    // MediaPipe'den gelen JSON verilerini parse eden fonksiyon
    private void UpdateJointPositionsFromJson(string jsonData)
    {
        // Gelen JSON'u Dictionary olarak parse et
        jointPositions = JsonConvert.DeserializeObject<Dictionary<string, Vector3>>(jsonData);
    }

    private void CheckForCollision()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, 1f))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                GameManager.gameOver = true;
                animator.SetTrigger("Die");
            }
        }
    }

    // Gelen veriye göre postürü güncelleyen fonksiyon
    private void UpdatePosture()
    {
        if (jointPositions != null)
        {
            // Baş pozisyonunu güncelle
            if (jointPositions.ContainsKey("head"))
            {
                head.position = dhead + jointPositions["head"];  // Sabit pozisyona ekleyerek güncelle
            }

            // Sol el ve ön kol pozisyonunu güncelle
            if (jointPositions.ContainsKey("left_hand") && jointPositions.ContainsKey("left_forearm"))
            {
                leftHand.position = dleftHand + jointPositions["left_hand"];
                leftForeArm.position = dleftForeArm + jointPositions["left_forearm"];
            }

            // Sağ el ve ön kol pozisyonunu güncelle
            if (jointPositions.ContainsKey("right_hand") && jointPositions.ContainsKey("right_forearm"))
            {
                rightHand.position = drightHand + jointPositions["right_hand"];
                rightForeArm.position = drightForeArm + jointPositions["right_forearm"];
            }

            // Sol bacak ve ayak pozisyonunu güncelle
            if (jointPositions.ContainsKey("left_leg") && jointPositions.ContainsKey("left_foot"))
            {
                leftLeg.position = dleftLeg + jointPositions["left_leg"];
                leftFoot.position = dleftFoot + jointPositions["left_foot"];
            }

            // Sağ bacak ve ayak pozisyonunu güncelle
            if (jointPositions.ContainsKey("right_leg") && jointPositions.ContainsKey("right_foot"))
            {
                rightLeg.position = drightLeg + jointPositions["right_leg"];
                rightFoot.position = drightFoot + jointPositions["right_foot"];
            }
        }
    }

    // MediaPipe'den gelen verileri simüle eden bir örnek (gerçek projede MediaPipe API ile entegre olmalı)
    private string GetMediapipeData(int dataIndex)
    {
        float forearm = drightHand.x - drightForeArm.x;
        var negforearm = -forearm;
        // Simülasyon olarak 3 farklı JSON datasını sırayla döndürüyoruz
        if (dataIndex == 0)
        {
            return @"
            {
                'head': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'left_hand': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'left_forearm': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'right_hand': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'right_forearm': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'left_leg': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'left_foot': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'right_leg': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'right_foot': {'x': 0.0, 'y': 0.0, 'z': 0.0}
            }";
        }
        else if (dataIndex == 1)
        {
            return @"
            {
                'head': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'left_hand': {'x': 0.4, 'y': 1, 'z': 0.0},
                'left_forearm': {'x': 0.2, 'y': 0.5, 'z': 0.0},
                'right_hand': {'x': -0.4, 'y': 1, 'z': 0.0},
                'right_forearm': {'x': -0.2, 'y': 0.5, 'z': 0.0},
                'left_leg': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'left_foot': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'right_leg': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'right_foot': {'x': 0.0, 'y': 0.0, 'z': 0.0}
            }";
        }
        else
        {
            return @"
            {
                'head': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'left_hand': {'x': 0.4, 'y': -0.5, 'z': 0.0},
                'left_forearm': {'x': 0.2, 'y': -0.25, 'z': 0.0},
                'right_hand': {'x': -0.4, 'y': -0.5, 'z': 0.0},
                'right_forearm': {'x': -0.2, 'y': -0.25, 'z': 0.0},
                'left_leg': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'left_foot': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'right_leg': {'x': 0.0, 'y': 0.0, 'z': 0.0},
                'right_foot': {'x': 0.0, 'y': 0.0, 'z': 0.0}
            }";
        }
    }
}
