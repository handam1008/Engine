using UnityEngine;

namespace RYU.Script.Charcter.Notepad
{
    [CreateAssetMenu(fileName = "NotepadAttackSO", menuName = "SO/Charcter/Notepad", order = 0)]
    public class NotepadAttackSO : AbstractAttackSO
    {
        [SerializeField] private NotepadBullet bulletPrefab;
        [SerializeField] private ParticleSystem notepadEndParticles;
        public override void Attacking(AbstractUnit unit)
        {
            BulletSet(unit, 1, 0.5f);
            BulletSet(unit, 0.5f, 0.4f);
            BulletSet(unit, 0, 0.3f);
            BulletSet(unit, -0.5f, 0.4f);
            BulletSet(unit, -1, 0.5f);
            ParticleSystem ps = Instantiate(notepadEndParticles);
            ps.Play();
        }

        private void BulletSet(AbstractUnit unit, float height, float launchDelay)
        {
            NotepadBullet bullet = Instantiate(bulletPrefab, unit.transform.position,  Quaternion.identity);
            bullet.Init(unit.UnitSo.attackDmg);
            bullet.Shoot(unit.target, bullet.TextSpeed, height, launchDelay);
        }
    }
}