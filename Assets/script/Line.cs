using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Line : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public RectTransform line;

    public void UpdateLine()
    {
        // �p����s�����Z���M����
        Vector3 startPos = button1.transform.position;
        Vector3 endPos = button2.transform.position;
        Vector3 direction = endPos - startPos;

        // �]�m�u����m�����s���������I
        line.position = (startPos + endPos) / 2;

        float buttonWidth = button1.GetComponent<RectTransform>().sizeDelta.x * button1.GetComponent<RectTransform>().lossyScale.x;
        float lineLength = direction.magnitude - buttonWidth;

        // �]�m�u��������
        line.sizeDelta = new Vector2(lineLength, line.sizeDelta.y);

        // �p�⨤�רñ���u��
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        line.rotation = Quaternion.Euler(0, 0, angle);
    }

}
