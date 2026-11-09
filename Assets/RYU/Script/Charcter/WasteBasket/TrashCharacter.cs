using UnityEngine;

namespace RYU.Script.Charcter.WasteBasket
{
    public class TrashCharacter : AbstractUnit
    {
        private TrashAttackSO _trashAttackSo;
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
        }
        
        private void OnDrawGizmos()
        {
            _trashAttackSo = Attack as TrashAttackSO;
            Vector3 center = transform.position + new Vector3(1, 0, 0);

            if (_trashAttackSo != null)
            {
                Vector3 boxsize = new Vector3(_trashAttackSo.boxSize.x, _trashAttackSo.boxSize.y, 0);
            
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(center, boxsize);
            }
        }
    }
}