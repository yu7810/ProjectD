using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCam : MonoBehaviour
{
    // �R�A��ҡA�Ω�s�x�ߤ@�����
    private static MainCam instance;

    // �b Awake ���ˬd�M�Ыس��
    void Awake()
    {
        // �p�G��ҩ|���s�b�A�]�m�����󬰰ߤ@��ҨëO�d�b����������
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // �p�G�w�g�s�b�@�ӹ�ҡA�P����e����
            Destroy(gameObject);
        }
    }
}
