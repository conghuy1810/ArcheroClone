using TestTask.Attribute;
using UnityEngine;

namespace TestTask.Fight
{

    public class Projectile : Weapon
    {
        [SerializeField] float force;
         //Time to destroy after the gameobject collides
        [SerializeField] float time;
        private void Start()
        {
            rb.linearVelocity = transform.forward * force;
        }

        public override void OnTriggerEnter(Collider other) {
            if (ShouldHandleCollision(other, out Attributes attr))
            {
                ResetRigidBody();
                if (attr != null)
                {
                    OnCollision(attr);
                }
                Destroy(gameObject, time);
            }
        }
        ///<summary>
        ///Reset the rigidbody velocity to zero.
        ///</summary>
        public override void ResetRigidBody()
        {       
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
        }

        public override void OnCollision(Attributes attribute)
        {
              attribute.TakeDamage(weponInfromation.damage);
            Instantiate(weponInfromation.AfterEffectOnHit, transform.position, transform.rotation);
        }
    }
}
