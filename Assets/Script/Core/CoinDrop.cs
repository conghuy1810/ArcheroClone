using ArcheroClone.pool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace TestTask.Core
{
    public class CoinDrop : MonoBehaviour
    {
        public static event Action<CoinDrop> OnCoinDrop;
        
        private Transform player;
        private bool isCollected = false;
        private float pickupRadius = 2.5f;

        private void Awake()
        {
            isCollected = false;
            if (OnCoinDrop != null)
                OnCoinDrop(this);
        }

        private void Start()
        {
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
            {
                player = playerGo.transform;
            }
        }

        private void Update()
        {
            if (player == null || isCollected) return;

            // Trong chế độ Vampire Survivors hoặc chế độ thường, người chơi có thể nhặt xu bằng cách đi lại gần
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= pickupRadius)
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (isCollected) return;
            isCollected = true;

            // Di chuyển đồng xu về phía người chơi bằng DOTween rồi trả về Pool và tăng điểm kinh nghiệm (XP)
            transform.DOMove(player.position, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (GameHandler.instance != null)
                {
                    GameHandler.instance.IncreaseXP(1);
                }
                RemoveCoin();
            });
        }

        internal void Remove()
        {
            if (isCollected) return;
            isCollected = true;
            Invoke(nameof(RemoveCoin), .55f);
        }

        private void RemoveCoin()
        {
            GameHandler.instance.CoinPool.Return(this);
        }
    }
}
