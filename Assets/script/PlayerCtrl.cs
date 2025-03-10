﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;


public class PlayerCtrl : MonoBehaviour
{
    public Rigidbody m_Rigidbody;
    Animator m_Animator;
    bool Move;
    LayerMask mask = (1 << 7);//角色旋轉用
    public bool canMove;
    public bool canAttack;
    public bool isReload; // 正在reload魔力(天賦28)
    public GameObject UpgradeSystem;
    public ValueData valuedata;
    public Skill skill;
    GameObject ontriggerTarget; // 放當前可互動(E)的物體
    public GameObject openedTarget; // 已經互動的NPC，用來開關對應UI
    RaycastHit floorhit;
    public Vector3 playerToMouse;//滑鼠指到的座標
    private List<GameObject> interactableObjects = new List<GameObject>(); // 追蹤範圍內的目標

    //角色漂浮效果
    private float amplitude = 0.15f;  // 漂浮的幅度
    private float frequency = 0.8f;   // 漂浮的頻率
    private Vector3 startPosition; // 初始位置
    public GameObject Character;

    // 靜態實例，用於存儲唯一的實例
    private static PlayerCtrl instance;
    public static PlayerCtrl Instance
    {
        get => instance;
    }

    // 在 Awake 中檢查和創建單例
    void Awake()
    {
        // 如果實例尚未存在，設置此物件為唯一實例並保留在場景切換中
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果已經存在一個實例，銷毀當前物件
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        Application.targetFrameRate = 60;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        canMove = true;
        canAttack = true;
        isReload = false;
        valuedata.canBehurt = true;
        
        //初始武器
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(0);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(1);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(2);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(3);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(4);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(5);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(6);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(7);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(8);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(9);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(10);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(11);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(12);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(13);
        UICtrl.Instance.ChangeWeapon_ID = 0;
        UICtrl.Instance.DoChangeWeapon(14);
        
        //初始技能
        UICtrl.Instance.ChangeSkill_ID[1] = 0;//初始技能等級
        UICtrl.Instance.ChangeSkill_ID[0] = 0;
        UICtrl.Instance.DoChangeSkill(0);
        UICtrl.Instance.ChangeSkill_ID[0] = 0;
        UICtrl.Instance.DoChangeSkill(1);
        UICtrl.Instance.ChangeSkill_ID[0] = 0;
        UICtrl.Instance.DoChangeSkill(2);
        startPosition = Character.transform.localPosition;
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            //基本移動
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                Move = true;
                m_Animator.SetBool("Move", Move);
                Vector3 m_Input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
                Vector3 _move = m_Input * valuedata.MoveSpeed * Time.fixedDeltaTime * 3f;
                m_Rigidbody.MovePosition(m_Rigidbody.position + _move);
            }
            else
            {
                Move = false;
                m_Animator.SetBool("Move", Move);
            }
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D))
            {
                m_Rigidbody.velocity = Vector3.zero;
            }
            //3D角色轉向
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(camRay, out floorhit, 30f, mask))
            {
                playerToMouse = floorhit.point - transform.position;
                playerToMouse.y = 0;
                Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
                m_Rigidbody.MoveRotation(newRotation);
            }
        }

        //漂浮
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        Character.transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);

    }

    private void Update()
    {
        if (UICtrl.Instance.IsPointerOverUI(out GameObject uiElement) || Time.timeScale == 0)
            return;
        //滑鼠L
        if (Input.GetKey(KeyCode.Mouse0))
        {
            UseSkill(0);
        }
        //滑鼠R
        if (Input.GetKey(KeyCode.Mouse1))
        {
            UseSkill(1);
        }
        //空白鍵
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseSkill(2);
        }
        //互動鍵E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!ontriggerTarget || !canMove)
                return;
            if (openedTarget != null && openedTarget == ontriggerTarget)
            {
                ontriggerTarget.GetComponent<Npc>().doNpc(false);
                openedTarget = null;
            }
            else if (ontriggerTarget.tag == "NPC")
            {
                openedTarget = ontriggerTarget;
                ontriggerTarget.GetComponent<Npc>().doNpc(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "NPC")
        {
            if (other.TryGetComponent<Npc>(out Npc _npc))
            {
                interactableObjects.Add(other.gameObject);
                if (ontriggerTarget == null)
                {
                    UpdateCurrentTarget();
                }
            }
        }
        else if(other.tag == "Money")
        {
            Destroy(other.transform.parent.gameObject);
            ValueData.Instance.GetMoney(1);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "NPC") // 所有可互動(E)對象tag都會是NPC
        {
            if(UICtrl.Instance.nowSkillstore && UICtrl.Instance.nowSkillstore.gameObject == other.gameObject)
                other.GetComponent<Npc>().doNpc(false);
            if(UICtrl.Instance.nowWeaponstore && UICtrl.Instance.nowWeaponstore.gameObject == other.gameObject)
                other.GetComponent<Npc>().doNpc(false);
            if (other.TryGetComponent<Npc>(out Npc npc))
            {
                if (interactableObjects.Contains(other.gameObject))
                {
                    interactableObjects.Remove(other.gameObject);
                    if (ontriggerTarget == other.gameObject)
                    {
                        ontriggerTarget = null;

                        // 關閉名稱UI
                        if (npc.NameUI)
                        {
                            npc.NameUI.gameObject.SetActive(false);
                        }
                        UpdateCurrentTarget();
                    }
                }
            }
            if (openedTarget == other.gameObject)
                openedTarget = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!valuedata.canBehurt)
            return;
        if (other.tag == "EnemyAttack")
        {
            EnemyAttack enemyattack = other.transform.GetComponent<EnemyAttack>();
            Enemy enemy = enemyattack.enemy;
            ValueData.Instance.GetHp(-enemyattack.dmg, true);
            if (enemyattack.enemyType == EnemyType.Ranged)
            {
                Destroy(other.gameObject);
            }
            else if(enemyattack.enemyType == EnemyType.Tank)
            {
                enemy.StopAction();
            }
        }
    }

    public void UseSkill(int Field)
    {
        // 裝備18 暴擊時施放 不能主動使用技能
        if(ValueData.Instance.isHaveweaponid(Field, 18))
        {
            return;
        }
        // 冷卻及魔力足夠才能使用
        if (valuedata.SkillField[Field].nowCD <= 0 && valuedata.AP >= valuedata.SkillField[Field].Cost && canAttack && !isReload)
        {
            if (valuedata.SkillField[Field].ID <= 0)
                return;
            if (valuedata.PassiveSkills[21])
            {
                int rng = UnityEngine.Random.Range(0, 2);
                if(rng == 1)
                    valuedata.GetAp(-valuedata.SkillField[Field].Cost);
            }
            else
                valuedata.GetAp(-valuedata.SkillField[Field].Cost);
            valuedata.SkillField[Field].nowCD = valuedata.SkillField[Field].maxCD;
            UICtrl.Instance.UpdateSkillCD();
            StartCoroutine(UICtrl.Instance.SkillCD(Field));
            skill.UseSkill(valuedata.SkillField[Field].ID, Field);
        }
        else if (valuedata.AP < valuedata.SkillField[Field].Cost)
        {
            if (valuedata.PassiveSkills[14]) // 天賦14
            {
                valuedata.Reload(true);
            }
        }
    }

    //受傷無敵幀
    public IEnumerator BehurtTimer(bool isvignetteColor)
    {
        valuedata.canBehurt = false;
        if(UICtrl.Instance.vignette != null)
            UICtrl.Instance.vignette.color.value = UICtrl.Instance.vignetteBehurtColor;
        //Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", new Color(0.6f, 0, 0));
        //yield return new WaitForSeconds(0.3f);
        //Mesh.transform.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        yield return new WaitForSecondsRealtime(0.4f);
        if (UICtrl.Instance.vignette != null)
            UICtrl.Instance.vignette.color.value = UICtrl.Instance.vignetteColor;
        yield return new WaitForSecondsRealtime(0.2f);
        valuedata.canBehurt = true;
    }

    private void UpdateCurrentTarget()
    {
        // 移除已經消失的物件
        interactableObjects.RemoveAll(obj => obj == null);

        if (interactableObjects.Count > 0)
        {
            // 設定最接近的目標作為當前目標
            ontriggerTarget = FindClosestTarget();

            // 開啟名稱UI
            if (ontriggerTarget.TryGetComponent<Npc>(out Npc _npc))
            {
                if (_npc.NameUI)
                {
                    _npc.NameUI.gameObject.SetActive(true);
                    _npc.NameUI.transform.parent.rotation = Camera.main.transform.rotation;
                    _npc.showName();
                }
            }
        }
    }
    private GameObject FindClosestTarget()
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;
        foreach (var obj in interactableObjects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = obj;
            }
        }
        return closest;
    }

}
