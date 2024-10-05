using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.UI;
using TMPro;

public class Character : BaseGameEntity
{
    public Color color;

    //vital data
    public float maxVitals = 1440;

    public int money =100;
    public int food = 1440;
    public int drink = 1440;
    public int sleep = 1440;
    public int social = 1440;

    //replenish data
    public float foodThreshold = 0.9f;
    public float drinkThreshold = 0.9f;
    public float sleepThreshold = 0.8f;
    public float socialThreshold = 0.1f;

    public float foodThresholdCritical = 0.05f;
    public float drinkThresholdCritical = 0.05f;
    public float sleepThresholdCritical = 0.05f;
    public float socialThresholdCritical = 0.15f;

    public int foodDuration = 15;
    public int drinkDuration = 3;
    public int sleepDuration = 30;
    public int socialDuration = 30;

    public int foodPerMinute = 20;
    public int drinkPerMinute = 50;
    public int sleepPerMinute = 8;

    //state data
    public int stateLockTime = 0;
    public bool invurnerable = false;
    public bool onSchedule = false;
    public Meeting currentMeeting = null;
    public int meetingEndTime = int.MaxValue;

    //inventory
    public int foodItemsInShoppingBag = 0;
    public int otherItemsInShoppingBag = 0;

    //locations
    public Location workplace;
    public Location home;
    public Location store;
    public Location location;
    public Location[] knownLocations;
    public Location travelDestination;

    //travel data
    public Vector3 travelStart;
    public float travelDistance;
    public int travelTicks = 0;
    public float movementSpeed = 1.0f;
    public float travelRatio = 0;
    public int stepCount = 0;

    //other
    int dialogueTextTimer = 0;
    //public bool dead = false;
    public int dayOfDeath = 0;
    public string causeOfDeath = "";
    public string stateOnDeath = "";
    public int currentDay = 1;
    public Sprite graveStone;

    //message log
    public MessageLog messageLog;
    //dialogue box
    public TextMeshProUGUI dialogueText;
    //Notebook
    public Notebook notebook = new();
    //Notebook controller
    public NotebookController notebookController;
    //state machine
    StateMachine<Character> m_pStateMachine;
   
    public StateMachine<Character> GetFSM(){return m_pStateMachine;}

    private void Update()
    {
        //beautiful travel is done in the update loop
        if (travelDestination != location)
        {
            //by lerping we conviniently avoid overshoot
            travelRatio = Mathf.Min(1.0f, travelRatio + Time.deltaTime * movementSpeed * FSM_Clock.Instance.TimeScale()/travelDistance);
            transform.position = Vector3.Lerp(travelStart, travelDestination.transform.position, travelRatio);
        }
    }

    public void EntityStart()
    {
        SetAutomaticID();
        m_pStateMachine = new StateMachine<Character>(this);
        m_pStateMachine.SetGlobalState(CharacterStateGlobal.Instance);
        m_pStateMachine.SetCurrentState(CharacterStateSleep.Instance);

        //find some buddies to add to our notebook.
        foreach (var character in FindObjectsOfType<Character>())
        {
            if (character != this)
            {
                notebook.contacts.Add(character);
            }
        }

        travelDestination = home;
        currentDay = FSM_Clock.Instance.ElapsedDays();
    }


    public override void EntityUpdate()
    {
        m_pStateMachine.UpdateFSM();

        if (currentDay < FSM_Clock.Instance.ElapsedDays())
            DailyUpdate();

        if (dialogueTextTimer-- <= 0)
        {
            dialogueText.enabled = false;
        }
    }

    void DailyUpdate()
    {
        currentDay = FSM_Clock.Instance.ElapsedDays();

        if (currentDay % 7 == 0)
            WeeklyUpdate();
    }

    public void WeeklyUpdate()
    {
        //WEEKLY PLANNING
        for (int i = 0; i < 7; i++)
        {
            //WORKDAY
            if ((currentDay + i) % 7 != 0 && (currentDay + i + 1) % 7 != 0)
            {
                notebook.TryAddDate(currentDay + i, workplace.workStart, workplace.workEnd - workplace.workStart, workplace, CharacterStateWork.Instance);
            }
            //WEEKEND
            else
            {
                //SETUP A MEETING AT RESTAURANT OR PARK
                FSM_MainLoop mainLoop = FSM_MainLoop.Instance;
                //pick out some buddies
                List<int> recievers = new List<int>();
                foreach (var item in mainLoop.characters)
                {
                    if (item.ID() != ID())
                        recievers.Add(item.ID());
                }
                for (int j = 0; j < Random.Range(0, mainLoop.characters.Length - 1); j++)
                {
                    recievers.RemoveAt(Random.Range(0, recievers.Count));
                }
                int attendees = 1 + recievers.Count;
                Meeting myMeeting = new Meeting(mainLoop.locations[Random.Range(0, 2)], currentDay + i, Random.Range(17, 20), Random.Range(1, 3), attendees, CharacterStateHangOut.Instance);
                if (notebook.TryAddDate(myMeeting))
                {
                    //send invite to the lucky ones
                    foreach (var reciever in recievers)
                    {
                        SendMeetingRequest(myMeeting, reciever);
                    }
                }
            }
        }
    }

    public void SendMeetingRequest(Meeting meeting, int receiverID)
    {
        string recieverName = EntityManager.Instance.GetEntityFromID(receiverID).name;
        
        messageLog.LogMessage("["+ name + "] I Invite " + recieverName + " to the "+ meeting.location.gameObject.name + " on day "+ meeting.day + " at "+ meeting.time, color);
        
        MessageDispatcher.Instance.DispatchMessage(0, ID(), receiverID, (int)messages.Schedule_meeting, meeting);
    }

    public override bool HandleMessage(Telegram msg)
    {
        return m_pStateMachine.HandleMessage(msg);
    }

    public void SetDialogueText(string text)
    {
        dialogueTextTimer = 3;
        dialogueText.text = text;
        dialogueText.enabled = true;
    }

    //public void kill(string cod)
    //{
    //    dead = true;
    //    location.attendees--;
    //    transform.position = location.transform.position + new Vector3(0.2f, 0, 0) * location.corpses;
    //    location.corpses++;
    //    dayOfDeath = FSM_Clock.Instance.ElapsedDays();
    //    causeOfDeath = cod;
    //    stateOnDeath = GetFSM().GetNameOfCurrentState();
    //    GetComponent<SpriteRenderer>().sprite = graveStone;
    //    GetComponent<SpriteRenderer>().sortingOrder = 5;
    //    GetFSM().ChangeState(CharacterStateDead.Instance);
    //    //automatic pause on death
    //    FSM_MainLoop.Instance.PauseTime(true);
    //}

    public State<Character>[] GetPriorityState()
    {
        SortedList<int,State<Character>> priorityStates = new();

        void tryAdd(int key, State<Character> state)
        {
            int i = 0;
            while (!priorityStates.TryAdd(key + i, state))
                i++;
        }

        if (food < maxVitals * foodThreshold)
            tryAdd((int)food, CharacterStateEat.Instance);
        if (drink < maxVitals * drinkThreshold)
            tryAdd((int)drink, CharacterStateDrink.Instance);
        if (sleep < maxVitals * sleepThreshold)
            tryAdd((int)sleep, CharacterStateSleep.Instance);
        if (social < maxVitals * socialThreshold)
            tryAdd((int)social, CharacterStateLonely.Instance);
        tryAdd(1500, CharacterStateHangOut.Instance);

        State<Character>[] states = new State<Character>[priorityStates.Count];
        priorityStates.Values.CopyTo(states, 0);

        return states;
    }
}
