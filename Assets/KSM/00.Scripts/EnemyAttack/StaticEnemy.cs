using System;
using System.Collections.Generic;
using UnityEngine;

public class StaticEnemy : MonoBehaviour
{
    [SerializeField]private LayerMask whatIsEntity;
    [SerializeField]private float range;
    [SerializeField] private int maxChainCount;
    private bool isHit;
    [SerializeField] private LineRenderer lineRenderer;
    private Rigidbody2D _rb;
    private SpriteRenderer _reder;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _reder = GetComponent<SpriteRenderer>();
    }


    public Collider2D[] Detect(Vector2 targetPos)
    {
        return Physics2D.OverlapCircleAll(targetPos, range, whatIsEntity);
    }

    private void Update()
    {
        
    }

    public void TargetPosUpdate(Collider2D[] targets)
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((whatIsEntity.value & (1 << other.gameObject.layer)) == 0) return; 
            if (isHit) return;
            isHit = true;
            Vector2 targetPos = other.transform.position;
            List<Collider2D> targets = new List<Collider2D>(Detect(targetPos));
            targets.Sort((a,b)=> Vector2.Distance(targetPos, a.transform.position)
                .CompareTo(Vector2.Distance(targetPos, b.transform.position)));
            
            int count = 0;
            
            _rb.linearVelocityX = 0;
            lineRenderer.positionCount = 0;
            _reder.color = new Color(0, 0, 0, 0);
            foreach (var c in targets)
            {
                if (count >= maxChainCount) break;
                if (c.TryGetComponent<IDamageAble>(out IDamageAble damage))
                {
                    damage.Damage(3);
                    Debug.Log(c.name+" 타격");
                }
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(count, c.transform.position);
                
                count++;
            }
            Destroy(gameObject,0.2f);

        
    }
}
