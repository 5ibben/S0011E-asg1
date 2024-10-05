using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotebookController : MonoBehaviour
{
    public TextMeshProUGUI[] meetings;
    public Character character = null;

    public void setNotebook(Character character)
    {
        this.character = character;
    }

    public int day = 0;
    public void nextDay()
    {
        DisplayPage(++day);
    }
    public void previousDay()
    {
        if (0 < day)
            DisplayPage(--day);
    }

    public void displayNotebook()
    {
        lerpDir = 1;
    }

    public void hideNotebook()
    {
        lerpDir = -1;
    }
    public void DisplayPage(int day = -1)
    {
        if (day == -1)
            day = this.day;
        this.day = day;
        meetings[0].text = "Day " + day.ToString();
        if (!character.notebook.calendar.ContainsKey(day))
        {
            CalendarDay page = new CalendarDay();
            character.notebook.calendar.Add(day, page);
        }
        if ((day + 1) % 7 != 0 && day % 7 != 0)
        {
            meetings[0].color = Color.grey;
        }
        else
        {
            meetings[0].color = Color.red;
        }
        int i = 1;

        for (int j = 1; j < meetings.Length; j++)
        {
            meetings[j].text = "";
            meetings[j].GetComponentsInChildren<TextMeshProUGUI>()[1].text = "";
        }

        foreach (var date in character.notebook.calendar.GetValueOrDefault(day).schedule)
        {
            if (i < meetings.Length)
            {
                if (date.Value.time + date.Value.duration <= FSM_Clock.Instance.TimeOfDayHours() && day == FSM_Clock.Instance.ElapsedDays() || day < FSM_Clock.Instance.ElapsedDays())
                {
                    meetings[i].gameObject.GetComponentsInChildren<TextMeshProUGUI>()[1].fontStyle = FontStyles.Strikethrough;
                    meetings[i].fontStyle = FontStyles.Strikethrough;
                }
                else
                {
                    meetings[i].gameObject.GetComponentsInChildren<TextMeshProUGUI>()[1].fontStyle = FontStyles.Normal;
                    meetings[i].fontStyle = FontStyles.Normal;
                }
                string location = date.Value.location.ToString();
                meetings[i].gameObject.GetComponentsInChildren<TextMeshProUGUI>()[1].text = "" + location.Substring(0, location.Length - 11);
                meetings[i++].text = "" + (date.Key ).ToString() + ":00";
            }
        }
    }

    float lerpVal =0;
    int lerpDir = 1;
    private void Update()
    {
        lerpVal = Mathf.Clamp(lerpVal + Time.deltaTime * lerpDir * 3, 0, 1);
        if (lerpVal == 1)
            GetComponentInChildren<Canvas>(true).enabled = true;
        else
            GetComponentInChildren<Canvas>(true).enabled = false;
        transform.position = Vector2.Lerp(new Vector2(3.5f, -2.4f), new Vector2(3.5f, -1.5f), lerpVal);
    }
}
