using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAlpha : MonoBehaviour
{
    CanvasGroup canvasGroup;
    public float fadeDuration = 1.0f;   // �H�X����ɶ�
    private float elapsedTime = 0f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        // ����H�X�ĪG
        elapsedTime += Time.deltaTime;
        if (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
        }
        else
        {
            Destroy(gameObject); // �����H�X��P������
        }
    }
}
