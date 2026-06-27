using UnityEngine;
using TestTask.Core;

namespace TestTask.Fight
{
    public class DragonPet : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float stopDistance = 2f;
        [SerializeField] private float hoverHeight = 1.2f;

        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 15f;
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private GameObject fireballPrefab;

        private Transform player;
        private Animator animator;
        private Transform headBone;
        private float attackTimer;

        private void Start()
        {
            // Find player
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                player = playerGo.transform;
            }

            animator = GetComponent<Animator>();
            if (animator != null)
            {
                // Ensure dragon pet is in flying state to play fly animation
                animator.SetBool("IsFlying", true);
            }

            // Find head bone for spawning fireball
            headBone = FindDeepChild(transform, "Head");
            
            // If fireballPrefab is not set, try to load it from Default
            if (fireballPrefab == null)
            {
#if UNITY_EDITOR
                fireballPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Weapon/DragonFireball.prefab");
#endif
            }
        }

        private void Update()
        {
            if (player == null)
            {
                GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
                if (playerGo != null)
                {
                    player = playerGo.transform;
                }
                return;
            }

            if (GameHandler.instance != null && GameHandler.instance.isPause) return;

            // Follow player logic
            FollowPlayer();

            // Attack logic
            HandleAttack();
        }

        private void FollowPlayer()
        {
            // Hover position slightly behind and to the side of the player
            Vector3 targetOffset = -player.forward * stopDistance + player.right * 1.5f;
            Vector3 targetPos = player.position + targetOffset;
            targetPos.y = player.position.y + hoverHeight;

            // Move smoothly to target position
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

            // Rotate in movement direction if moving
            Vector3 moveDir = targetPos - transform.position;
            moveDir.y = 0;
            if (moveDir.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
            }
        }

        private void HandleAttack()
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                if (GameHandler.instance != null && GameHandler.instance.EnemyExits())
                {
                    Transform enemy = GameHandler.instance.FindClosetEnemy(transform);
                    if (enemy != null && Vector3.Distance(transform.position, enemy.position) <= attackRange)
                    {
                        // Rotate to look at enemy instantly when attacking
                        Vector3 lookDir = enemy.position - transform.position;
                        lookDir.y = 0;
                        if (lookDir.magnitude > 0.1f)
                        {
                            transform.rotation = Quaternion.LookRotation(lookDir);
                        }

                        // Shoot Fireball
                        ShootFireball(enemy);
                        attackTimer = 0f;
                    }
                }
            }
        }

        private void ShootFireball(Transform enemy)
        {
            if (fireballPrefab == null) return;

            Vector3 spawnPos = transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;
            if (headBone != null)
            {
                spawnPos = headBone.position;
            }

            Vector3 shootDirection = (enemy.position - spawnPos).normalized;
            Quaternion spawnRotation = Quaternion.LookRotation(shootDirection);

            Instantiate(fireballPrefab, spawnPos, spawnRotation);
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
