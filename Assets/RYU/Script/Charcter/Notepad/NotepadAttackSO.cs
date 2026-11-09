using System;
using Cysharp.Threading.Tasks;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;

namespace RYU.Script.Charcter.Notepad
{
    [CreateAssetMenu(fileName = "NotepadAttackSO", menuName = "SO/Attack/Notepad", order = 0)]
    public class NotepadAttackSO : AbstractAttackSO
    {
        [SerializeField] private NotepadBullet bulletPrefab;
        [SerializeField] private ParticleSystem notepadEndParticles;
        [SerializeField] private float delay;
        [SerializeField] private SoundClipSO soundClip;
        public override void Attacking(AbstractUnit unit)
        {
            ServiceLocator.Get<IAudioService>().PlaySfx(soundClip);
            BulletSet(unit, 1, 0.5f);
            BulletSet(unit, 0.5f, 0.4f);
            BulletSet(unit, 0, 0.3f);
            BulletSet(unit, -0.5f, 0.4f);
            BulletSet(unit, -1, 0.5f);
            _ = WaitForParticles(delay, unit);
            
        }

        private void BulletSet(AbstractUnit unit, float height, float launchDelay)
        {
            var poolable = PoolManager.PoolManager.Instance.Pop(bulletPrefab.name);
            var pp = poolable.GetGameObject().GetComponent<NotepadBullet>();
            pp.Init(unit.UnitSo.attackDmg);
            pp.Shoot(unit.target, pp.TextSpeed, height, launchDelay);
            poolable.ResetItem();
            
            /*NotepadBullet bullet = Instantiate(bulletPrefab, unit.transform.position,  Quaternion.identity);
            bullet.Init(unit.UnitSo.attackDmg);
            bullet.Shoot(unit.target, bullet.TextSpeed, height, launchDelay);*/
        }

        async UniTaskVoid WaitForParticles(float time, AbstractUnit unit)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(time));
            if (unit == null || unit.target == null) return;

            ParticleSystem ps = Instantiate(notepadEndParticles,unit.target.transform.position, Quaternion.identity);
            ps.Play();
        }
    }
}