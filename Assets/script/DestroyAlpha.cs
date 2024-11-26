using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DestroyAlpha : MonoBehaviour
{
    TextMeshPro textmeshpro;
    public float fadeDuration = 1.0f;   // �H�X����ɶ�
    private float elapsedTime = 0f;

    void Start()
    {
        textmeshpro = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        // ����H�X�ĪG
        elapsedTime += Time.deltaTime;
        if (elapsedTime < fadeDuration)
        {
            textmeshpro.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
        }
        else
        {
            Destroy(gameObject); // �����H�X��P������
        }
    }
}
