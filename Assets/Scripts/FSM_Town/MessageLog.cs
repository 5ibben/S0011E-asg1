using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MessageLog : MonoBehaviour
{
    public TextMeshProUGUI[] txtFields;

    public void LogMessage(string message, Color color)
    {
        for (int i = 0; i < txtFields.Length-1; i++)
        {
            txtFields[i].text = txtFields[i + 1].text;
            txtFields[i].color = txtFields[i + 1].color;
        }
        txtFields[txtFields.Length - 1].text = message;
        txtFields[txtFields.Length - 1].color = color;
    }
}
