using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    RectTransform tooltipPanel;  // 參考提示視窗的 RectTransform
    public Vector2 offset = new Vector2(10, -10);  // 視窗的偏移量
    private Coroutine Follow;

    void Start()
    {
        // 隱藏提示視窗
        //tooltipPanel.gameObject.SetActive(false);
        tooltipPanel = this.gameObject.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if(Follow == null)
            Follow = StartCoroutine(FollowMouse());
    }

    private void OnDisable()
    {
        if (Follow != null)
        {
            StopCoroutine(Follow);
            Follow = null;
        }
    }

    IEnumerator FollowMouse()
    {
        while(true)
        {
            yield return null;

            // 將滑鼠座標轉換為世界座標
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                UICtrl.Instance.canvas.transform as RectTransform,
                Input.mousePosition,
                UICtrl.Instance.canvas.worldCamera,
                out mousePosition);

            // 設定視窗位置
            tooltipPanel.localPosition = mousePosition + offset;

            // 根據滑鼠位置調整視窗中心點
            Vector2 panelSize = tooltipPanel.sizeDelta;
            Vector2 canvasSize = (UICtrl.Instance.canvas.transform as RectTransform).sizeDelta;

            Vector2 pivot = new Vector2(0f, 1f);

            // 左側
            if (mousePosition.x > 0)
                pivot.x = 1f;
            else if (mousePosition.x <= -canvasSize.x / 2)
                pivot.x = 0f;

            // 上側
            if (mousePosition.y >= canvasSize.y / 2)
                pivot.y = 1f;
            else if (mousePosition.y < 0)
                pivot.y = 0f;

            tooltipPanel.pivot = pivot;
        }
    }
}

