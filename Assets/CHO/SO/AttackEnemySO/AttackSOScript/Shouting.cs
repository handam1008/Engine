using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shouting", menuName = "SO/AttackShouting")]
public class Shouting : AbstractAttackSO
{
    [SerializeField] private float shoutCooldown = 10f;  
    [SerializeField] private float attackInterval = 1f;  
    [SerializeField] private float stopDuration = 2f;     
    [SerializeField] private string playerTag = "Player";
    
    private class Timer
    {
        public float nextShout;
        public float nextAttack;
    }
    private readonly Dictionary<AbstractUnit, Timer> timers = new();//같은 SO쓰는 중복 유닛 각각 시간 따로 만들어주기 위해
    private readonly Dictionary<AbstractUnit, float> stopUntil = new();

    
    
    public override void Attacking(AbstractUnit unit)
    {
        if (!timers.TryGetValue(unit, out var t))
        {
            t = new Timer
            {
                nextShout = Time.time + shoutCooldown, nextAttack = Time.time//현재시간 형식 타이머 엔진시간에 배운거 
            };              //현재 시간에서 쿨타임을 더해줌, 다음 시간에 현재 시간을 넣어줌
            timers.Add(unit, t);//타이머 딕셔너리에 넣는다(유닛마다 달라야 하니까)
        }

        if (Time.time >= t.nextShout)//쿨 지나면?
        {
            Shout(unit);//실행
            t.nextShout = Time.time + shoutCooldown;//쿨타임 다시 지정
            t.nextAttack = Time.time + attackInterval; //샤우팅한 직후엔 기본공격 안 겹치게
            return;
        }

        if (Time.time >= t.nextAttack)//어택 쿨타임 따로
        {
            BasicAttack(unit);//기본공격
            t.nextAttack = Time.time + attackInterval;//기본공격 쿨 지정
        }
    }
    
    private void BasicAttack(AbstractUnit unit)//기본공격(NearTargetAttack에서 훔침)
    {
        Collider2D target = unit.NearTarget();
        if(target != null && target.TryGetComponent(out HealthSystem healthSystem))
            healthSystem.Damage(unit.UnitSo.attackDmg);
    }
    

    public void Shout(AbstractUnit caster)//함수 한번에 실행 시키는거
    {
        StopPlayer();
        ShoutEffect(caster);
    }

    public void StopPlayer()
    {
        foreach (var u in FindObjectsByType<AbstractUnit>(FindObjectsSortMode.None))//다 멈춰버리는거
        {
            if (!u.CompareTag(playerTag)) continue;

            bool alreadyStopped = stopUntil.ContainsKey(u);
            stopUntil[u] = Time.time + stopDuration; // 멈춘 중에 또 맞으면 시간만 연장

            if (!alreadyStopped)
                u.StartCoroutine(StopRoutine(u));
        }
    }

    private IEnumerator StopRoutine(AbstractUnit u)
    {
        u.enabled = false; // Update가 멈춰서 이동/공격 정지

        while (Time.time < stopUntil[u])
            yield return null;

        stopUntil.Remove(u);
        u.enabled = true;
    }

    public void ShoutEffect(AbstractUnit caster)
    {
        // caster.transform.position 기준으로 이펙트/사운드
    }
}
