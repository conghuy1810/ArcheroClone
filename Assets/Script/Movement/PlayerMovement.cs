using System.Collections;
using System.Collections.Generic;
using TestTask.Core;
using UnityEngine;
using TestTask.Attribute;
namespace TestTask.Movement
{
    public class PlayerMovement : Movement
    {
        [SerializeField] PlayerInformation playerInformation;

        [Header("Vampire Survivors Settings")]
        [SerializeField] private bool autoDetectVampireMode = true;
        [SerializeField] private bool autoAttackWhileMoving = false;

        bool setAttack = false;
        float speed;
        private new void Awake() {
            base.Awake();
           speed = playerInformation.speed; 
        }

        private void OnDestroy() {
            playerInformation.speed = speed;
        }
        private void Update()
        {
            if(GameHandler.instance.isPause) return;

            bool isVampireMode = autoDetectVampireMode ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "VampireSurvivors" : autoAttackWhileMoving;

            if (isVampireMode)
            {
                if (Input.anyKey)
                {
                    Move();
                }

                if (!attack.IsAttacking && GameHandler.instance.EnemyExits())
                {
                    attack.SetCanAttack(enemy);
                }
            }
            else
            {
                if (Input.anyKey)
                    Move();
                else if (!setAttack)
                {
                    setAttack = true;
                    if (GameHandler.instance.EnemyExits())
                        attack.SetCanAttack(enemy);
                }
            }
        }
        ///<summary>
        ///Control the player with the use of arrow key.
        ///</summary>
        void Move()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            //Determine if the player is pressing the WASD or ARROW key
            if (horizontal == 0f && vertical == 0f)
            {
                return;
            }
            setAttack = false;
            CollisionCheck();

            bool isVampireMode = autoDetectVampireMode ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "VampireSurvivors" : autoAttackWhileMoving;
            if (!isVampireMode)
            {
                behaviour.ChangeBehaviour(this);
            }

            float perFrameTime = Time.deltaTime;
            Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
            //Move player by speed value every franme
            transform.position = transform.position + moveDirection * perFrameTime * playerInformation.speed;
            //Look towards the move direction
            transform.rotation = LookAtDirection(animationMesh.rotation, moveDirection);
        }
    }

}
