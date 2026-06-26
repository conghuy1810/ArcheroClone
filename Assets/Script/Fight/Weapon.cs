using TestTask.Attribute;
using TestTask.Core;
using UnityEngine;

namespace TestTask.Fight
{
    public abstract class Weapon : MonoBehaviour
    {
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected WeponInfromation weponInfromation;

        //Todo: May be do with layers
        [Header("Tag of the gameobject that this weapon is aimed at.")]

        [SerializeField] protected GameHandler.Tags enemyTag;


        public virtual void OnTriggerEnter(Collider other)
        {
            if (ShouldHandleCollision(other, out Attributes attr))
            {
                ResetRigidBody();
                if (attr != null)
                {
                    OnCollision(attr);
                }
            }
        }

        protected bool ShouldHandleCollision(Collider other, out Attributes attr)
        {
            attr = null;

            // 1. Tránh va chạm với chính người bắn (vd: đạn của người chơi không va chạm với người chơi)
            string shooterTag = (enemyTag == GameHandler.Tags.Enemy) ? "Player" : "Enemy";
            if (other.CompareTag(shooterTag))
            {
                return false;
            }

            // 2. Nếu va chạm với mục tiêu đối địch chính xác (kẻ địch hoặc người chơi) - xử lý trước để tránh bị nhận nhầm là vũ khí
            if (other.CompareTag(enemyTag.ToString()))
            {
                attr = other.GetComponent<Attributes>();
                if (attr == null)
                {
                    attr = other.GetComponentInParent<Attributes>();
                }
                return true;
            }

            // 3. Tránh đạn va chạm với các loại đạn/vũ khí khác hoặc các vật thể không mong muốn
            if (other.gameObject.name.Contains("Arrow") || other.gameObject.name.Contains("Weapon") || other.GetComponent<Weapon>() != null)
            {
                return false;
            }

            // 4. Nếu va chạm với địa hình hoặc chướng ngại vật (vật cản)
            if (other.gameObject.name.Contains("Ground") || other.gameObject.name.Contains("Fence") || other.gameObject.name.Contains("Obstacle"))
            {
                // Chỉ xử lý va chạm khi chướng ngại vật có collider thực sự (không phải trigger vô hại)
                if (!other.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract void OnCollision(Attributes attribute);
       
        public abstract void ResetRigidBody();
    }
}