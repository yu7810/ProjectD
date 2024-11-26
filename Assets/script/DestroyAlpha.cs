using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DestroyAlpha : MonoBehaviour
{
    TextMeshPro textmeshpro;
    public float fadeDuration = 1.0f;   // 淡出持續時間
    private float elapsedTime = 0f;

    void Start()
    {
        textmeshpro = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        // 控制淡出效果
        elapsedTime += Time.deltaTime;
        if (elapsedTime < fadeDuration)
        {
            textmeshpro.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
        }
        else
        {
            Destroy(gameObject); // 當完全淡出後銷毀物件
        }
    }
}
