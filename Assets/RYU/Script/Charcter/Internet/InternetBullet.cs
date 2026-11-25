using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using RYU.Script.Pool;

namespace RYU.Script.Charcter.Internet
{
    public class InternetBullet : MonoBehaviour, IPoolable
    {
        public string ItemName => "InternetBullet";
        private int _attackDamage;
        private LayerMask _targetMask;
        private Sequence _motion;
        private bool _breaking;
        private bool _startAttack;
        private Animator _animator;
        private CancellationTokenSource _breakCancellation;
        
        
        private readonly int _breakHash = Animator.StringToHash("Break");
        private const float BreakHeight = 0.8f;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Init(int damage, LayerMask targetMask)
        {
            _attackDamage = damage;
            _targetMask = targetMask;
        }

        private void BeginBreak(Vector3 impactPosition)
        {
            if (_breaking) return;
            _breaking = true;
            
            _animator.SetTrigger(_breakHash);

            _breakCancellation = new CancellationTokenSource();
            var token = _breakCancellation.Token;
            
            var cols = Physics2D.OverlapCircleAll(impactPosition, 0.3f, _targetMask);
            foreach (var col in cols)
            {
                if (!col.TryGetComponent(out AbstractUnit health)) continue;
                health.HpCompo.Damage(_attackDamage,health);
            }
            
            ReturnAfterBreakAsync(token).Forget();
        }

        private async UniTask ReturnAfterBreakAsync(CancellationToken token)
        {
            bool cancelled = await UniTask.Delay(650, cancellationToken: token).SuppressCancellationThrow();
            
            if (cancelled || token.IsCancellationRequested) return;

            PoolManager.Instance.Push(this);
        }

        public GameObject GetGameObject() => gameObject;
        

        public void ResetItem()
        {
            _breaking = false;
            if (_motion.isAlive) _motion.Stop();
        }

        private void OnDisable()
        {
            _breakCancellation?.Cancel();
            _breakCancellation?.Dispose();
            _breakCancellation = null;
            _startAttack = false;
            _breaking = false;
            ResetItem();
        } 
            
            
        

        public void Shoot(Transform target)
        {
            Vector3 impactPosition = target.position;
            transform.SetPositionAndRotation(impactPosition + Vector3.up * 1.5f, Quaternion.identity);

            _startAttack = true;
            _motion = Sequence.Create()
                .Chain(Tween.Custom(1.5f, 2f, 0.3f, height =>
                {
                    if (target != null && target.gameObject.activeInHierarchy)
                        impactPosition = target.position;

                    transform.position = impactPosition + Vector3.up * height;
                }, ease: Ease.OutQuad))
                .Chain(Tween.Custom(2f, BreakHeight, 0.25f, height =>
                {
                    transform.position = impactPosition + Vector3.up * height;
                }, ease: Ease.InQuad))
                .OnComplete(() => BeginBreak(impactPosition));
        }

        public void Move(Transform target)
        {
            while (true)
            {
                Vector3 impactPosition = target.position;
                transform.SetPositionAndRotation(impactPosition + Vector3.up * 1.5f, Quaternion.identity);
                if(_startAttack) break;
            }
        }
    }
}
