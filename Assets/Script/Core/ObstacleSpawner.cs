using System.Collections.Generic;
using UnityEngine;

namespace TestTask.Core
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Danh sách mẫu vật cản (Prefabs)")]
        [SerializeField] private List<GameObject> obstaclePrefabs;

        [Header("Cấu hình số lượng")]
        [SerializeField] private int totalObstacles = 150; // Tổng số vật cản muốn tạo trên map

        [Header("Giới hạn khu vực sinh")]
        [SerializeField] private float spawnAreaSize = 280f; // Nhỏ hơn kích thước Ground (300f) một chút để tránh rìa map

        [Header("Khu vực an toàn cho người chơi")]
        [SerializeField] private float safeRadiusAroundPlayer = 10f; // Không sinh vật cản quá gần người chơi lúc bắt đầu

        [Header("Cấu hình tỉ lệ ngẫu nhiên (Scale)")]
        [SerializeField] private float minScale = 0.8f;
        [SerializeField] private float maxScale = 1.2f;

        [Header("Khoảng cách tối thiểu giữa các vật cản")]
        [SerializeField] private float minDistanceBetweenObstacles = 8f; // Tránh các vật cản sinh quá sát nhau

        private Transform player;

        private void Start()
        {
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                player = playerGo.transform;
            }

            SpawnObstacles();
        }

        private void SpawnObstacles()
        {
            if (obstaclePrefabs == null || obstaclePrefabs.Count == 0) return;

            int spawnedCount = 0;
            int maxAttempts = totalObstacles * 5; // Giới hạn số lần thử để tránh treo game (vòng lặp vô tận)
            int attempts = 0;

            while (spawnedCount < totalObstacles && attempts < maxAttempts)
            {
                attempts++;

                // 1. Tạo vị trí ngẫu nhiên trên map
                float halfSize = spawnAreaSize / 2f;
                float randomX = Random.Range(-halfSize, halfSize);
                float randomZ = Random.Range(-halfSize, halfSize);
                Vector3 spawnPos = new Vector3(randomX, 0f, randomZ); // Đặt độ cao sát mặt đất (0.0f) để tránh bay lơ lửng

                // 2. Kiểm tra khoảng cách với người chơi
                if (player != null && Vector3.Distance(spawnPos, player.position) < safeRadiusAroundPlayer)
                {
                    continue; // Quá gần người chơi, bỏ qua vị trí này
                }

                // 3. Kiểm tra va chạm để tránh các vật cản đè lên nhau
                // Kiểm tra xem xung quanh bán kính cấu hình đã có vật cản nào chưa (bỏ qua mặt đất)
                Collider[] colliders = Physics.OverlapSphere(spawnPos, minDistanceBetweenObstacles);
                bool hasOverlap = false;
                foreach (var col in colliders)
                {
                    if (col.gameObject.name.Contains("Ground"))
                        continue; // Bỏ qua mặt đất
                    
                    hasOverlap = true;
                    break;
                }
                if (hasOverlap)
                {
                    continue; // Đã có vật thể khác ở đây, thử lại vị trí khác
                }

                // 4. Chọn ngẫu nhiên 1 mẫu vật cản từ danh sách
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
                if (prefab != null)
                {
                    // Tạo vật cản với góc xoay Y ngẫu nhiên (cho tự nhiên hơn)
                    Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    GameObject obstacle = Instantiate(prefab, spawnPos, randomRotation);
                    
                    // Thiết lập tỉ lệ (Scale) ngẫu nhiên
                    float randomScale = Random.Range(minScale, maxScale);
                    obstacle.transform.localScale = prefab.transform.localScale * randomScale;

                    // Đảm bảo có BoxCollider thích hợp dựa trên MeshRenderer bounds để người chơi và quái vật không đi xuyên qua
                    MeshRenderer meshRen = obstacle.GetComponentInChildren<MeshRenderer>();
                    if (meshRen != null)
                    {
                        // Xóa MeshCollider cũ nếu có để tránh xung đột va chạm
                        MeshCollider oldMC = obstacle.GetComponentInChildren<MeshCollider>();
                        if (oldMC != null)
                        {
                            Destroy(oldMC);
                        }

                        // Thêm BoxCollider chuẩn xác và đồng bộ hóa kích thước theo Local Bounds của Mesh
                        BoxCollider boxCol = obstacle.AddComponent<BoxCollider>();
                        boxCol.center = meshRen.localBounds.center;
                        boxCol.size = meshRen.localBounds.size;
                    }
                    else
                    {
                        // Nếu không tìm thấy MeshRenderer, gán BoxCollider mặc định
                        if (obstacle.GetComponent<Collider>() == null)
                        {
                            obstacle.AddComponent<BoxCollider>();
                        }
                    }
                    
                    // Gán làm con của Spawner để tránh làm rối bảng Hierarchy
                    obstacle.transform.SetParent(this.transform);
                    spawnedCount++;
                }
            }

            Debug.Log($"[Obstacle Spawner] Đã tạo thành công {spawnedCount} vật cản cố định!");
        }
    }
}