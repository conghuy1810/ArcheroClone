using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TestTask.Core;
using TestTask.Attribute;

namespace TestTask.Core
{
    public class VampireSpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private List<GameObject> enemyPrefabs;

        [Header("Spawn Settings")]
        [SerializeField] private float baseSpawnInterval = 1.5f;
        [SerializeField] private int maxEnemies = 50;
        [SerializeField] private float minSpawnRadius = 14f;
        [SerializeField] private float maxSpawnRadius = 22f;
        [SerializeField] private bool spawnOutsideCameraOnly = true;
        [SerializeField] private float viewportMargin = 0.1f; // Khoảng lề an toàn bên ngoài viewport (0.1 = 10% màn hình)

        [Header("Difficulty Progression")]
        [SerializeField] private float difficultyIncreaseInterval = 30f; // Tăng độ khó mỗi 30 giây
        [SerializeField] private float spawnIntervalReduction = 0.1f;    // Giảm thời gian chờ spawn đi 10%
        [SerializeField] private float minSpawnInterval = 0.3f;
        [SerializeField] private int maxEnemiesIncrease = 5;

        private Transform player;
        private float spawnTimer;
        private float currentSpawnInterval;
        private float difficultyTimer;
        private int currentMaxEnemies;
        private int elapsedIntervals = 0;

        private void Start()
        {
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                player = playerGo.transform;
            }
            else
            {
                Debug.LogError("VampireSpawner: Không tìm thấy GameObject của Người chơi với tag 'Player'!");
            }

            currentSpawnInterval = baseSpawnInterval;
            currentMaxEnemies = maxEnemies;
            spawnTimer = 0f;
            difficultyTimer = 0f;
        }

        private void Update()
        {
            if (player == null || GameHandler.instance.isPause) return;

            // Xử lý việc sinh quái vật (spawn)
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= currentSpawnInterval)
            {
                spawnTimer = 0f;
                // Đếm số quái vật đang hoạt động để tránh sinh quá số lượng tối đa cho phép
                if (GetActiveEnemyCount() < currentMaxEnemies)
                {
                    SpawnEnemyAroundPlayer();
                }
            }

            // Xử lý tăng độ khó tiến trình game
            difficultyTimer += Time.deltaTime;
            if (difficultyTimer >= difficultyIncreaseInterval)
            {
                difficultyTimer = 0f;
                IncreaseDifficulty();
            }
        }

        private int GetActiveEnemyCount()
        {
            // Trả về số lượng quái vật hiện tại đang có mặt trong màn chơi
            return FindObjectsByType<TestTask.Movement.Enemy>(FindObjectsInactive.Exclude).Length;
        }

        private void SpawnEnemyAroundPlayer()
        {
            if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;

            // Lấy ngẫu nhiên một prefab quái vật
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            if (prefab == null) return;

            Vector3 spawnPosition = Vector3.zero;
            bool validPositionFound = false;
            int maxAttempts = 30;
            Camera mainCam = Camera.main;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Tính toán một điểm ngẫu nhiên trên đường tròn xung quanh người chơi
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float distance = Random.Range(minSpawnRadius, maxSpawnRadius);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                spawnPosition = player.position + offset;
                spawnPosition.y = 0.5f; // Giữ nguyên độ cao chuẩn của quái vật

                if (spawnOutsideCameraOnly && mainCam != null)
                {
                    Vector3 viewportPos = mainCam.WorldToViewportPoint(spawnPosition);
                    // Kiểm tra xem vị trí có nằm trong vùng nhìn thấy của Camera (cộng thêm khoảng lề) không
                    bool isInsideCamera = (viewportPos.x >= -viewportMargin && viewportPos.x <= 1f + viewportMargin) &&
                                          (viewportPos.y >= -viewportMargin && viewportPos.y <= 1f + viewportMargin) &&
                                          (viewportPos.z > 0);

                    if (!isInsideCamera)
                    {
                        validPositionFound = true;
                        break;
                    }
                }
                else
                {
                    validPositionFound = true;
                    break;
                }
            }

            // Fallback nếu không tìm thấy vị trí nằm ngoài camera sau maxAttempts lần thử
            if (!validPositionFound)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float distance = maxSpawnRadius + 10f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                spawnPosition = player.position + offset;
                spawnPosition.y = 0.5f;
            }

            // Tạo quái vật mới
            GameObject enemyGo = Instantiate(prefab, spawnPosition, prefab.transform.rotation);
            var attr = enemyGo.GetComponent<Attributes>();
            if (attr != null)
            {
                float healthMultiplier = 1f + (elapsedIntervals * 0.3f);
                // Lấy máu gốc từ prefab trước khi nhân
                var prefabAttr = prefab.GetComponent<Attributes>();
                float originalMhp = prefabAttr != null ? prefabAttr.MaxHP : 10f; 
        
                // Thiết lập máu mới đã nhân hệ số cho quái vừa sinh ra
                attr.SetMaxHP(originalMhp * healthMultiplier);
            }
            // Đưa quái vật vào danh sách quản lý của GameHandler
            if (GameHandler.instance != null)
            {
                GameHandler.instance.AddEnemy(enemyGo.transform);
            }
        }

        private void IncreaseDifficulty()
        {
            elapsedIntervals++;
            currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval * (1f - spawnIntervalReduction));
            currentMaxEnemies += maxEnemiesIncrease;
            Debug.Log($"[Vampire Survivors] Tăng độ khó! Đợt: {elapsedIntervals} | Số lượng quái tối đa: {currentMaxEnemies} | Khoảng thời gian spawn: {currentSpawnInterval:F2}s");
        }
    }
}
