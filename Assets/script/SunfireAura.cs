using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// One independent toggle/timer per equipped skill slot.
public sealed class SunfireAura : MonoBehaviour
{
    private int fieldId;
    private float elapsed;
    private Quaternion worldRotation;
    private PlayerAttack attack;
    [SerializeField] private Transform visual;
    private readonly List<GameObject> targets = new List<GameObject>();
    private readonly HashSet<Enemy> seen = new HashSet<Enemy>();
    private readonly HashSet<GameObject> seenProjectiles = new HashSet<GameObject>();

    public void Initialize(int slot, GameObject effect = null)
    {
        fieldId = slot;
        worldRotation = transform.rotation;
        attack = gameObject.AddComponent<PlayerAttack>();
        attack.fidleid = slot;
        if (visual == null && effect != null)
        {
            visual = Instantiate(effect, transform).transform;
            visual.localPosition = Vector3.up * 0.05f;
        }
        if (visual != null)
        {
            foreach (var particles in visual.GetComponentsInChildren<ParticleSystem>())
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = particles.main;
                main.loop = true;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                particles.Play();
            }
        }
        SceneManager.activeSceneChanged += SceneChanged;
        UpdateVisual();
    }

    private void SceneChanged(Scene previous, Scene next) => Close();
    private void LateUpdate()
    {
        // Follow the player's position through parenting, while keeping the aura's world orientation.
        transform.rotation = worldRotation;
    }

    private void OnDisable() => SceneManager.activeSceneChanged -= SceneChanged;
    private void Close()
    {
        if (Skill.Instance != null) Skill.Instance.StopSunfire(fieldId);
        else Destroy(gameObject);
    }

    private void UpdateVisual()
    {
        if (visual != null)
            visual.localScale = Vector3.one * ValueData.Instance.SkillField[fieldId].Size;
    }

    private void Update()
    {
        var data = ValueData.Instance;
        if (data == null || data.Player == null || !data.Player.activeInHierarchy
            || data.HP <= 0f || data.SkillField[fieldId].ID != ValueData.SunfireSkillId)
        {
            Close();
            return;
        }
        // Do not keep an unaffordable aura active until its next damage interval.
        if (!data.CanStartSkillResource(fieldId))
        {
            Close();
            return;
        }
        UpdateVisual();
        elapsed += Time.deltaTime;
        var stats = data.GetSkillLevelStats(ValueData.SunfireSkillId, data.SkillField[fieldId].Level);
        float interval = Mathf.Max(0.001f, stats?.SunfireInterval ?? 0.5f);
        while (elapsed >= interval && gameObject.activeSelf)
        {
            elapsed -= interval;
            StartCoroutine(PulseCopies());
        }
    }

    private IEnumerator PulseCopies()
    {
        var data = ValueData.Instance;
        // Match weapon 6's three casts and spacing, including Bell's hit cooldown.
        int copies = data.isHaveweaponid(fieldId, 6) ? 3 : 1;
        for (int copy = 0; copy < copies && gameObject.activeSelf; copy++)
        {
            if (copy > 0) yield return new WaitForSeconds(0.2f);
            if (!gameObject.activeSelf || data.HP <= 0f
                || data.SkillField[fieldId].ID != ValueData.SunfireSkillId)
            {
                Close();
                yield break;
            }
            float cost = Mathf.Max(0f, data.SkillField[fieldId].Cost);
            if (!data.TryPaySkillCost(cost, fieldId))
            {
                Close();
                yield break;
            }
            CollectTargets(data.SkillField[fieldId].Size);
            float damage = data.CalculateSkillDamage(fieldId);
            float multiplier = data.ConsumeSunbreakerRage(fieldId);
            attack.Pulse(fieldId, damage, multiplier, targets);
            if (data.HP <= 0f || data.SkillField[fieldId].ID != ValueData.SunfireSkillId
                || !data.CanStartSkillResource(fieldId))
            {
                Close();
                yield break;
            }
        }
    }

    private void CollectTargets(float radius)
    {
        targets.Clear();
        seen.Clear();
        seenProjectiles.Clear();
        Vector3 center = transform.position;
        foreach (var collider in Physics.OverlapCapsule(center + Vector3.down * 2f,
            center + Vector3.up * 2f, radius, ~0, QueryTriggerInteraction.Collide))
        {
            var projectile = collider.GetComponent<EnemyAttack>();
            if (ValueData.Instance.isHaveweaponid(fieldId, 27) && projectile != null
                && projectile.CompareTag("EnemyAttack") && projectile.enemyType == EnemyType.Ranged)
            {
                Vector3 distance = collider.ClosestPoint(center) - center;
                distance.y = 0f;
                if (distance.sqrMagnitude <= radius * radius && seenProjectiles.Add(projectile.gameObject))
                    targets.Add(projectile.gameObject);
                continue;
            }
            var enemy = collider.GetComponentInParent<Enemy>();
            if (enemy == null || !enemy.isActiveAndEnabled || !enemy.canBeHit
                || (enemy.Hp <= 0f && !enemy.CompareTag("Bell"))
                || enemy.transform.IsChildOf(ValueData.Instance.Player.transform)
                || !(enemy.CompareTag("Enemy") || enemy.CompareTag("Barrel") || enemy.CompareTag("Bell")))
                continue;
            Vector3 offset = collider.ClosestPoint(center) - center;
            offset.y = 0f;
            if (offset.sqrMagnitude <= radius * radius && seen.Add(enemy))
                targets.Add(enemy.gameObject);
        }
    }
}
