using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public float tileLength = 60f;
    public float moveSpeed = 10f;  // Platformların hareket hızı
    public int numberOfTiles = 5;
    public Transform playerTransform;
    private List<GameObject> activeTiles = new List<GameObject>();
    private int tileIndex = 0;  // Başlangıç olarak 0. index

    void Start()
    {
        // İlk platformları oluştur
        for (int i = 0; i < numberOfTiles; i++)
        {
            SpawnTile(tileIndex);  // Sırasıyla platformları yarat
            tileIndex = (tileIndex + 1) % tilePrefabs.Length;  // Index'i döngüsel olarak artır
        }
    }

    void Update()
    {
        if (!GameManager.isGameStarted || GameManager.gameOver)  // Oyun başlamadıysa veya bittiğindeyse platform hareket etmesin
            return;

        MoveTiles();

        // İlk platform karakterin çok gerisindeyse sil ve yeni bir platform yarat
        if (activeTiles[0].transform.position.z < playerTransform.position.z - tileLength)
        {
            DeleteTile();
            SpawnTile(tileIndex);  // Sıradaki platformu yarat
            tileIndex = (tileIndex + 1) % tilePrefabs.Length;  // Index'i döngüsel olarak artır
        }
    }

    private void MoveTiles()
    {
        // Platformları karaktere doğru hareket ettir
        foreach (var tile in activeTiles)
        {
            tile.transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        }
    }

    public void SpawnTile(int tileIndex)
    {
        // Yeni platform yarat
        GameObject go = Instantiate(tilePrefabs[tileIndex], new Vector3(0, 0, tileLength * activeTiles.Count), Quaternion.identity);
        activeTiles.Add(go);
    }

    private void DeleteTile()
    {
        // Eski platformu sil
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}
