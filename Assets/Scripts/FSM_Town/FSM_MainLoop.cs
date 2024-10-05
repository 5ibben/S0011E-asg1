using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FSM_Clock : MonoBehaviour
{
    FSM_Clock() { }

    //this is a singleton
    private static FSM_Clock instance = null;
    public static FSM_Clock Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject().AddComponent<FSM_Clock>();
            }
            return instance;
        }
    }

    private void Update()
    {
        if (is_paused)
            return;

        accumulator += Time.deltaTime * timeScale;
    }

    //public void UpdateClock()
    //{
    //    if (is_paused)
    //        return;

    //    accumulator += Time.deltaTime;
    //}

    public int RequestAccumulatedTime()
    {
        int accumulatedUpdates = Mathf.FloorToInt(accumulator);
        accumulator -= accumulatedUpdates;

        return accumulatedUpdates;
    }

    public void UpdateDayTime()
    {
        //progress daytime
        minutes ++;
        hours += minutes / 60;
        days += hours / 24;
        minutes = minutes % 60;
        hours = hours % 24;
    }

    public void PauseClock(bool pause)
    {
        is_paused = pause;
    }
    public void SetTimeScale(int val = 1)
    {
        if (0 < val)
        {
            timeScale = val;
        }
    }

    public bool IsPaused()
    {
        return Instance.is_paused;
    }
    public int TimeScale()
    {
        return timeScale;
    }
    public int ElapsedDays()
    {
        return days;
    }
    public int TimeOfDayHours(int minutes)
    {
        float minute = minutes % 1440;
        return Mathf.FloorToInt(minute / 60);
    }
    public int TimeOfDayMinutes(int minutes)
    {
        float minutesRemainig = minutes % 1440;

        return Mathf.FloorToInt(minutesRemainig % 60);
    }
    public int TimeOfDayHours()
    {
        return hours;
    }
    public int TimeOfDayMinutes()
    {
        return minutes;
    }
    public int MinutesToHours(int minutes)
    {
        return minutes / 60;
    }
    public int MinutesToDays(int minutes)
    {
        return minutes / 1440;
    }
    public int HoursToMinutes(int hours)
    {
        return hours * 60;
    }
    public int DaysToMinutes(int days)
    {
        return days * 1440;
    }

    int minutes = 0;
    int hours = 0;
    int days = 1;
    //'time scale' or 'ticks per second'
    int timeScale = 1;
    bool is_paused = false;
    float accumulator = 0;
}

public class FSM_MainLoop : MonoBehaviour
{
    FSM_MainLoop() { }

    //references
    public EntitySelection entitySelection;
    public Location[] locations;
    public Character[] characters;
    //UI
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI timeScaleText;
    public Button timeScalePlus;
    public Button timeScaleMinus;
    public Image pauseButtonImage;
    public TextMeshProUGUI pauseButtonText;

    //debug stuff
    public GameObject spinner;
    float spinner_angle = 0;
    public TextMeshProUGUI debugText;

    //this is a singleton
    private static FSM_MainLoop instance = null;
    public static FSM_MainLoop Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject().AddComponent<FSM_MainLoop>();
            }
            return instance;
        }
    }

    private void Start()
    {
        Initialize();
    }

    void Update()
    {
        spinner_angle -= Time.deltaTime * 100;
        spinner.transform.eulerAngles = new Vector3(0, 0, spinner_angle);

        //We need to assign this to a variable since the accumulated time is set to zero upon request.
        //Having it directly in the for loop will not work as it will be evaluated every iteration.
        int updates = FSM_Clock.Instance.RequestAccumulatedTime();
        for (int i = 0; i < updates; i++)
        {
            //character entity update
            foreach (var character in characters)
            {
                character.EntityUpdate();
            }
            //dispatch any delayed messages
            MessageDispatcher.Instance.DispatchDelayedMessages();
            //day time update
            FSM_Clock.Instance.UpdateDayTime();
        }

        UpdateUI();
    }

    void Initialize()
    {
        instance = this;
        foreach (var character in characters)
        {
            character.EntityStart();
            EntityManager.Instance.RegisterEntity(character);
        }

        foreach (var character in characters)
        {
            character.WeeklyUpdate();
        }

        entitySelection.SelectObject(characters[0].gameObject);

        entitySelection.SelectObject(FindObjectOfType<Character>().gameObject);
    }
    void UpdateUI()
    {
        UpdateClockText();
        UpdateScaleText();
        entitySelection.UpdateUI();
    }

    //--------UI STUFF-------
    void UpdateScaleText()
    {
        float timeScale = FSM_Clock.Instance.TimeScale();
        string scaleString = "";
        if (timeScale < 1)
        {
            scaleString = "0." + (int)(timeScale * 10);
        }
        else
        {
            scaleString = ((int)timeScale).ToString();
        }

        timeScaleText.text = scaleString + "X";
    }
    
    public void PauseTime(bool pause)
    {
        if (pause)
        {
            FSM_Clock.Instance.PauseClock(true);
            pauseButtonImage.color = Color.red;
            pauseButtonText.text = "PLAY";
        }
        else
        {
            FSM_Clock.Instance.PauseClock(false);
            pauseButtonImage.color = Color.green;
            pauseButtonText.text = "PAUSE";
        }

    }

    public void PauseToggleTime()
    {
        PauseTime(!FSM_Clock.Instance.IsPaused());
    }

    public void IncreaseTimeScale(int value = 1)
    {
        FSM_Clock.Instance.SetTimeScale(FSM_Clock.Instance.TimeScale() + value);
    }

    void UpdateClockText()
    {
        string hourStr = "";
        string minuteStr = "";

        if (FSM_Clock.Instance.TimeOfDayMinutes() < 10)
            minuteStr = "0";
        if (FSM_Clock.Instance.TimeOfDayHours() < 10)
            hourStr = "0";

        minuteStr += ((int)FSM_Clock.Instance.TimeOfDayMinutes()).ToString();
        hourStr += ((int)FSM_Clock.Instance.TimeOfDayHours()).ToString();

        dayText.text = "Day: " + FSM_Clock.Instance.ElapsedDays().ToString();
        timeText.text = hourStr + " : " + minuteStr;
    }
}