
using DG.Tweening;
using TestTask.Attribute;
using TestTask.Core;
using UnityEngine;


namespace TestTask.Movement
{

    public abstract class Enemy : Attributes
    {     
        [Header("Base Enemy Settings")]
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float attackRange = 15f;
        [SerializeField] protected bool requireVisibleToAttack = true;

        float stationaryTime;
        protected Transform player;
        protected Collider enemyCollider;

        protected new void Awake()
        {
            base.Awake();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            stationaryTime = Random.Range(1, 10f);
            enemyCollider = GetComponent<Collider>();
        }
        
        protected override void IsDeath(float hp)
        {
           
            transform.DOKill(false);
            if (hp <= 0)
            {
                GameHandler.instance.RemoveEnemy(this.transform);
                //Instantiate the coin/gems/diamond to the position after enemy death
                InstantiateSomeCoins();
                //gameObject.SetActive(false);
                Destroy(this.gameObject);
            }
        }

        private void InstantiateSomeCoins()

        {
            for (int i = 0; i < Random.Range(2, 5); i++)
            {
                //Randomly instantiate the coins near player
                CoinDrop coin = GameHandler.instance.CoinPool.Request();
                Instantiate(coin.gameObject, transform.position + transform.forward * Random.Range(-1, 1f)
                + transform.right * Random.Range(-1, 1f), coin.transform.rotation);
            }            
        }

        private void Update()
        {
            if (player == null || GameHandler.instance.isPause) return;

            transform.LookAt(player.position);
            
            // Di chuyển quái về phía người chơi
            MoveTowardsPlayer();

            if (stationaryTime < 0)
            {
                if (CanAttack())
                {
                    EnemyAttack();
                    stationaryTime = Random.Range(3, 6);
                }
            }
            else
            {
                stationaryTime -= Time.deltaTime;
            }
        }

        protected virtual void MoveTowardsPlayer()
        {
            if (DOTween.IsTweening(transform)) return;

            // Nếu là quái đứng yên (hoặc quái cận chiến khi đã áp sát), kiểm tra xem đã vào tầm bắn chưa
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                if (StopMovingInAttackRange()) return;
            }

            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

            ResolveCollisions();
        }

        protected void ResolveCollisions()
        {
            if (enemyCollider == null) return;

            float checkRadius = 1.5f;
            if (enemyCollider is BoxCollider box)
            {
                checkRadius = Mathf.Max(box.size.x, box.size.y, box.size.z) * 1.5f;
            }
            else if (enemyCollider is SphereCollider sphere)
            {
                checkRadius = sphere.radius * 1.5f;
            }

            Collider[] overlaps = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask("Default"));
            foreach (var col in overlaps)
            {
                if (col == enemyCollider) continue;
                if (col.gameObject.name.Contains("Ground")) continue; // Bỏ qua mặt đất
                if (col.CompareTag("Player")) continue; // Bỏ qua người chơi

                if (Physics.ComputePenetration(enemyCollider, transform.position, transform.rotation, col, col.transform.position, col.transform.rotation, out Vector3 dir, out float dist))
                {
                    transform.position += dir * dist;
                }
            }
        }

        protected virtual bool StopMovingInAttackRange()
        {
            return false; // Mặc định quái thường vẫn tiếp tục di chuyển áp sát player
        }

        protected virtual bool CanAttack()
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > attackRange) return false;

            if (requireVisibleToAttack)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);
                    bool isVisible = (viewportPos.x >= 0f && viewportPos.x <= 1f) &&
                                     (viewportPos.y >= 0f && viewportPos.y <= 1f) &&
                                     (viewportPos.z > 0);
                    if (!isVisible) return false;
                }
            }

            return true;
        }

        public abstract void EnemyAttack();
    }
}
