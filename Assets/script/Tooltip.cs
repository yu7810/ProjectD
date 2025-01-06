using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    RectTransform tooltipPanel;  // �ѦҴ��ܵ����� RectTransform
    public Vector2 offset = new Vector2(10, -10);  // �����������q
    private Coroutine Follow;

    void Start()
    {
        // ���ô��ܵ���
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

            // �N�ƹ��y���ഫ���@�ɮy��
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                UICtrl.Instance.canvas.transform as RectTransform,
                Input.mousePosition,
                UICtrl.Instance.canvas.worldCamera,
                out mousePosition);

            // �]�w������m
            tooltipPanel.localPosition = mousePosition + offset;

            // �ھڷƹ���m�վ���������I
            Vector2 panelSize = tooltipPanel.sizeDelta;
            Vector2 canvasSize = (UICtrl.Instance.canvas.transform as RectTransform).sizeDelta;

            Vector2 pivot = new Vector2(0f, 1f);

            // ����
            if (mousePosition.x > 0)
                pivot.x = 1f;
            else if (mousePosition.x <= -canvasSize.x / 2)
                pivot.x = 0f;

            // �W��
            if (mousePosition.y >= canvasSize.y / 2)
                pivot.y = 1f;
            else if (mousePosition.y < 0)
                pivot.y = 0f;

            tooltipPanel.pivot = pivot;
        }
    }
}

