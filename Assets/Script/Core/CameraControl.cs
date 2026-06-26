using UnityEngine;

namespace TestTask.Core
{
    public class CameraControl : MonoBehaviour
    {
        [Header("Gắn Component của người chơi vào đây.")]
        [SerializeField] Transform player;

        [Header("Khoảng cách (Chiều cao) của camera so với vị trí người chơi.")]
        [SerializeField] float offsetZ;

        private float offsetX;
        private bool isOffsetInitialized = false;

        private void Start()
        {
            if (player != null)
            {
                offsetX = transform.position.x - player.position.x;
                isOffsetInitialized = true;
            }
        }

        private void LateUpdate()
        {
            if (player == null) return;

            float targetX = isOffsetInitialized ? player.position.x + offsetX : transform.position.x;
            transform.position = new Vector3(targetX, transform.position.y, player.position.z + offsetZ);
        }
    }
}

