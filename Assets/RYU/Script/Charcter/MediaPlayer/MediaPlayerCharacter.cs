using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace RYU.Script.Charcter.MediaPlayer
{
    public class MediaPlayerCharacter : AbstractUnit
    {
        [SerializeField] private Image pauseImage;

        private readonly List<AbstractUnit> _selected = new();

        protected override void Update()
        {
            base.Update();
        }

        public async UniTaskVoid SelectEnemyAsync()
        {
            foreach (var collider in Physics2D.OverlapBoxAll(transform.position, new UnityEngine.Vector2(100, 100), 0f, UnitSo.whatIsDetect))
            {
                if (collider.TryGetComponent(out AbstractUnit enemy))
                {
                    _selected.Add(enemy);
                    enemy.AnimCompo.speed = 0;
                    enemy.MoveCompo.RbCompo.linearVelocity = UnityEngine.Vector2.zero;
                    enemy.MoveCompo.enabled = false;
                    
                }
            }
                if (pauseImage != null)
                    pauseImage.gameObject.SetActive(true);

                await UniTask.Delay(3000);
                AgainMove();
           
        }


        public void AgainMove()
        {
            foreach (var enemy in _selected)
            {
                if (enemy == null) continue;
                enemy.MoveCompo.enabled = true;
                enemy.AnimCompo.speed = 1;
            }
            _selected.Clear();
            if (pauseImage != null)
                pauseImage.gameObject.SetActive(false);
        }

    }
}
