using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateGlobal : State<Character>
{
    CharacterStateGlobal() { }

    //this is a singleton
    private static CharacterStateGlobal instance = null;
    public static CharacterStateGlobal Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateGlobal();
            }
            return instance;
        }
    }

    public override void Execute(Character character)
    {
        if (character.GetFSM().isInState(CharacterStateDead.Instance))
            return;

        //if (100 < character.GetFSM().getHistorySize())
        //{
        //    character.GetFSM().ChangeState(CharacterStateDead.Instance);
        //}

        //adjust vitals
        character.money = Mathf.Max(character.money + character.location.money, 0);
        character.drink = Mathf.Min(character.drink + character.location.drink, (int)character.maxVitals);
        character.food = Mathf.Min(character.food + character.location.food, (int)character.maxVitals);
        character.sleep = Mathf.Min(character.sleep + character.location.sleep, (int)character.maxVitals);
        //social can always be decreased, but requires company to be increased
        if (0 <= character.location.social && 1 < character.location.attendees)
        {
            character.social = Mathf.Min(character.social + character.location.social, (int)character.maxVitals);
        }
        else
        {
            character.social = Mathf.Min(character.social - 1, (int)character.maxVitals);
        }
        //Debug.Log("vitals: money: " + character.money + " drink: " + character.drink + " food: " + character.food + " sleep: " + character.sleep + " social: " + character.social);

        //check vitals
        if (!character.invurnerable)
        {
            if (character.social <= 0 || character.sleep <= 0 || character.food <= 0 || character.drink <= 0)
                character.GetFSM().ChangeState(CharacterStateDead.Instance);
        }

        //check notebook for meeting
        if (character.currentMeeting == null)
        {
            character.currentMeeting = character.notebook.CheckCalendar();
            if (character.currentMeeting != null)
            {
                character.meetingEndTime = FSM_Clock.Instance.DaysToMinutes(character.currentMeeting.day) + FSM_Clock.Instance.HoursToMinutes(character.currentMeeting.time) + FSM_Clock.Instance.HoursToMinutes(character.currentMeeting.duration);
                character.onSchedule = true;
            }
        }
        else
        {
            //at meeting location
            if (character.location == character.currentMeeting.location && !character.GetFSM().isInState(CharacterStateTravel.Instance))
            {
                //Debug.Log(character.name + " I am at meeting the meeting location: " + character.meeting.location + " for the meeting at " + character.meeting.time + "attendees: "+ character.meeting.attendees);
                character.GetFSM().ChangeState(character.currentMeeting.state);
                character.GetFSM().stateLock = true;
            }
            //travel to meeting location
            else if (!character.GetFSM().isInState(CharacterStateTravel.Instance))// || character.travelDestination != character.meeting.location)
            {
                //Debug.Log(character.name + " I am traveling to the " + character.meeting.location + " for the meeting at " + character.meeting.time + " statelock is "+ character.GetFSM().stateLock + " statelocktime is " + character.stateLockTime);
                character.travelDestination = character.currentMeeting.location;
                character.GetFSM().ChangeState(CharacterStateTravel.Instance);
            }
            //meeting is over
            if (character.meetingEndTime <= FSM_Clock.Instance.DaysToMinutes(FSM_Clock.Instance.ElapsedDays()) + FSM_Clock.Instance.HoursToMinutes(FSM_Clock.Instance.TimeOfDayHours()))
            {
                //Debug.Log(character.name + " meeting in the " + character.meeting.location + " at " + character.meeting.time + " is over");
                character.notebookController.DisplayPage();
                character.GetFSM().stateLock = false;
                character.onSchedule = false;
                character.meetingEndTime = int.MaxValue;
                character.currentMeeting = null;
                character.travelDestination = character.home;
                
                //change to prority state
                State<Character>[] priorityStates = character.GetPriorityState();
                int priorityIndex = 0;
                //if the character feel a need for more socializing
                if (priorityStates[priorityIndex] == CharacterStateLonely.Instance && character.notebook.CheckCalendar(-1, FSM_Clock.Instance.TimeOfDayHours() + 1) != null)
                {
                    priorityIndex = 1;
                }
                character.GetFSM().ChangeState(priorityStates[priorityIndex]);
                if (character.notebookController.character == character)
                {
                    character.notebookController.DisplayPage(character.currentDay);
                }
            }
        }
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        switch (msg.Msg)
        {
            case (int)messages.Schedule_meeting:
                {
                    Meeting meeting = msg.meeting;
                    Location loc = meeting.location;
                    //request declined
                    string senderName = EntityManager.Instance.GetEntityFromID(msg.Sender).name;
                    if (!character.notebook.TryAddDate(meeting) || character.GetFSM().isInState(CharacterStateDead.Instance))
                    {
                        character.messageLog.LogMessage("[" + character.name + "] I decline " + senderName + "'s invite to the " + meeting.location.gameObject.name +" on day " + meeting.day + " at " + meeting.time, character.color);
                        MessageDispatcher.Instance.DispatchMessage(0, character.ID(), msg.Sender, (int)messages.Decline_meeting, msg.meeting);
                    }
                    //request accepted
                    else
                    {
                        character.messageLog.LogMessage("[" + character.name + "] I accept " + senderName + "'s invite to the " + meeting.location.gameObject.name + " on day " + meeting.day + " at " + meeting.time, character.color);
                    }
                    return true;
                }
            case (int)messages.Decline_meeting:
                {
                    Meeting meeting = msg.meeting;
                    string senderName = EntityManager.Instance.GetEntityFromID(msg.Sender).name;
                    int attendees = character.notebook.DecreaseAttendees(meeting);
                    //character.messageLog.LogMessage("[" + character.name + "] " + senderName + " declined the invite to " + meeting.location.gameObject.name + " on day " + meeting.day + " at " + meeting.time + " attendees is now: "+ attendees, character.color);
                    if (attendees < 2)
                    {
                        character.notebook.TryRemoveDate(meeting);
                    }
                    return true;
                }

        }//end switch
        return false;
    }
}

public class CharacterStateWork : State<Character>
{
    CharacterStateWork() { }

    //this is a singleton
    private static CharacterStateWork instance = null;
    public static CharacterStateWork Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateWork();
            }
            return instance;
        }
    }
    public override void Enter(Character character)
    {
        //Debug.Log(character.name + " Time to go to work");
    }

    int hoursWorked = 0;
    public override void Execute(Character character)
    {
        Location work = character.workplace;
        if (character.location == character.workplace)
        {
            character.money += work.money;
            hoursWorked++;
        }
        else
        {
            character.travelDestination = character.workplace;
            character.GetFSM().ChangeState(CharacterStateTravel.Instance);
        }

        if (work.workEnd - work.workStart <= hoursWorked)
        {
            character.GetFSM().ChangeState(character.GetPriorityState()[0]);
        }

    }

    public override void Exit(Character character)
    {
        //Debug.Log(character.name + " Aaah! Workday's finally over!");
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateTravel : State<Character>
{
    CharacterStateTravel() { }

    //this is a singleton
    private static CharacterStateTravel instance = null;
    public static CharacterStateTravel Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateTravel();
            }
            return instance;
        }
    }

    public override void Enter(Character character)
    {
        if (character.location != character.travelDestination)
        {
            character.location.attendees = Mathf.Max(0, character.location.attendees - 1);
            character.travelRatio = 0;

            Vector3 travelVec = character.travelDestination.transform.position - character.transform.position;
            character.travelDistance = travelVec.magnitude;
            character.travelTicks = (int)(character.travelDistance / (character.movementSpeed));
            //we use the characters position as start rather than the location as they can differ.
            character.travelStart = character.transform.position;
        }
        //Debug.Log(character.name + " is starting traveling to " + character.travelDestination);
    }

    public override void Execute(Character character)
    {
        if (character.location != character.travelDestination)
        {
            if (character.travelTicks-- == 0)
            {
                character.GetFSM().RevertToPreviousState();
            }
        }
        else
        {
            character.GetFSM().RevertToPreviousState();
        }
    }

    public override void Exit(Character character)
    {
        if (character.location != character.travelDestination)
        {
            //Debug.Log(character.name + " has reached" + character.travelDestination);
            //has arrived
            character.location = character.travelDestination;
            character.travelRatio = 0;
            character.transform.position = character.travelDestination.transform.position + new Vector3(0.2f,0,0)*(character.location.attendees + character.location.corpses);
            character.location.attendees++;
            //we set the travel startposition at exit to avoid snapping when nested travel states revert to the previous travel state.
            character.travelStart = character.transform.position;
        }
        //Debug.Log(character.name + " I have arrived at "+ );
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateDrink : State<Character>
{
    CharacterStateDrink() { }

    //this is a singleton
    private static CharacterStateDrink instance = null;
    public static CharacterStateDrink Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateDrink();
            }
            return instance;
        }
    }

    public override void Enter(Character character)
    {
        //Debug.Log(character.name + " I'm thirsty, gonna have a drink!");
        character.stateLockTime = character.drinkDuration;
    }

    public override void Execute(Character character)
    {
        character.SetDialogueText(" Gulp!");
        //drink
        if (character.drink < character.maxVitals && 0 < character.stateLockTime--)
        {
            character.drink += character.drinkPerMinute;
        }
        //change state
        else
        {
            character.GetFSM().ChangeState(character.GetPriorityState()[0]);
        }
    }

    public override void Exit(Character character)
    {
        character.SetDialogueText(" Aaaah!");
        character.stateLockTime = 0;
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateEat : State<Character>
{
    CharacterStateEat() { }

    //this is a singleton
    private static CharacterStateEat instance = null;
    public static CharacterStateEat Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateEat();
            }
            return instance;
        }
    }

    public override void Enter(Character character)
    {
        //Debug.Log(character.name + "CharacterStateEat.Enter()");
        //consume one food item
        if (0 < character.home.foodInFridge)
        {
            character.home.foodInFridge--;
            character.stateLockTime = character.foodDuration;
        }
        //go to store if out of food
        else
        {
            character.SetDialogueText("Gotta go to the store!");
            if (50 < character.money)
            {
                character.GetFSM().ChangeState(CharacterStateShop.Instance);
            }
            //if the character has no money for food
            else
            {

            }
        }
    }

    public override void Execute(Character character)
    {
        //travel home
        if (character.location != character.home)
        {
            character.travelDestination = character.home;
            character.GetFSM().ChangeState(CharacterStateTravel.Instance);
        }
        //eat
        else if (character.food < character.maxVitals && 0 < character.stateLockTime--)
        {
            character.food += character.foodPerMinute;
            character.SetDialogueText("nom nom!");
        }
        //change state
        else
        {
            character.GetFSM().ChangeState(character.GetPriorityState()[0]);
        }
    }

    public override void Exit(Character character)
    {
        character.SetDialogueText("Burp!");
        character.stateLockTime = 0;
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateShop : State<Character>
{
    //this is a singleton
    private static CharacterStateShop instance = null;
    public static CharacterStateShop Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateShop();
            }
            return instance;
        }
    }

    enum StoreItems
    {
        Milk,
        Juice,
        Fruit,
        Vegetables,
        Meat,
        Pasta,
        Bread,

        GardenGnomes,
        Batteries,
        ToiletPaper,
        CleaningStuff
    };

    public override void Enter(Character character)
    {
        //Debug.Log(character.name + " Time to do some shopping!");
    }

    public override void Execute(Character character)
    {
        if (50 <= character.money)
        {
            //travel to store
            if (character.location != character.store)
            {
                character.travelDestination = character.store;
                character.GetFSM().ChangeState(CharacterStateTravel.Instance);
            }
            //shop
            else
            {
                if (character.foodItemsInShoppingBag + character.otherItemsInShoppingBag <= 10)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int item = Random.Range(0, 12);
                        if (item < 6)
                            character.foodItemsInShoppingBag++;
                        else
                            character.otherItemsInShoppingBag++;
                        character.SetDialogueText(" bought some " + ((StoreItems)item).ToString());

                        //Debug.Log(character.name + " Bought some " + ((StoreItems)item).ToString());
                        character.money-= 50;
                    }
                }
                else
                {
                    character.GetFSM().ChangeState(CharacterStateRestockFridge.Instance);
                }
            }
        }
        else
        {
            State<Character>[] prioStates = character.GetPriorityState();
            int index = 0;
            if (prioStates[index] == CharacterStateEat.Instance)
                index++;
            character.GetFSM().ChangeState(character.GetPriorityState()[index]);
        }
    }

    public override void Exit(Character character)
    {
        //Debug.Log(character.name + " I'm done eating. Time to resume what I was doing");
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateRestockFridge : State<Character>
{
    //singleton
    private static CharacterStateRestockFridge instance = null;
    public static CharacterStateRestockFridge Instance
    {
        get
        {
            if (instance == null)
                instance = new CharacterStateRestockFridge();
            return instance;
        }
    }

    public override void Enter(Character character)
    {
        //travel home
        if (character.location != character.home)
        {
            character.SetDialogueText("Gotta get home with my shopping!");
            //Debug.Log(character.name + " Gotta get home with my shopping!");
            character.travelDestination = character.home;
            character.GetFSM().ChangeState(CharacterStateTravel.Instance);
        }
    }

    public override void Execute(Character character)
    {
        //restock fridge
        character.SetDialogueText("Restocked Fridge!");
        character.home.foodInFridge += character.foodItemsInShoppingBag;
        character.foodItemsInShoppingBag = 0;
        character.otherItemsInShoppingBag = 0;
        character.GetFSM().ChangeState(character.GetPriorityState()[0]);

    }

    public override void Exit(Character character)
    {
        //Debug.Log(character.name + " I'm done eating. Time to resume what I was doing");
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateSleep : State<Character>
{
    //singleton
    private static CharacterStateSleep instance = null;
    public static CharacterStateSleep Instance
    {
        get
        {
            if (instance == null)
                instance = new CharacterStateSleep();
            return instance;
        }
    }

    public override void Enter(Character character)
    {
        //Debug.Log(character.name + " I'm tired, think I'll go to sleep");
        character.stateLockTime = character.sleepDuration;
    }

    public override void Execute(Character character)
    {
        //travel home
        if (character.location != character.home)
        {
            character.SetDialogueText("I can't sleep here, gotta go home!");
            character.travelDestination = character.home;
            character.GetFSM().ChangeState(CharacterStateTravel.Instance);
        }
        //sleep
        else if(character.sleep < character.maxVitals && 0 < character.stateLockTime--)
        {
            character.SetDialogueText(" zzzZZZzz");
            character.sleep += character.sleepPerMinute;
        }
        //change state
        else
            character.GetFSM().ChangeState(character.GetPriorityState()[0]);
    }

    public override void Exit(Character character)
    {
        character.stateLockTime = 0;
        //character.setDialogueText(" What a wierd dream! I dreamt that I was in some sort of simulation, and had no will of my own.");
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateHangOut : State<Character>
{
    //singleton
    private static CharacterStateHangOut instance = null;
    public static CharacterStateHangOut Instance
    {
        get
        {
            if (instance == null)
                instance = new CharacterStateHangOut();
            return instance;
        }
    }

    public override void Enter(Character character)
    {
    }

    public override void Execute(Character character)
    {
        if (character.currentMeeting == null)
        {
            character.GetFSM().ChangeState(character.GetPriorityState()[0]);
        }
    }

    public override void Exit(Character character)
    {
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateLonely : State<Character>
{
    //singleton
    private static CharacterStateLonely instance = null;
    public static CharacterStateLonely Instance
    {
        get
        {
            if (instance == null)
                instance = new CharacterStateLonely();
            return instance;
        }
    }

    public override void Enter(Character character)
    {
        //Debug.Log(character.name + " Gonna go see my friends!");
    }

    public override void Execute(Character character)
    {
        FSM_MainLoop mainLoop = FSM_MainLoop.Instance;
        FSM_Clock clock = FSM_Clock.Instance;


        //SCHEDULE A MEETING AT THE RESTAURANT OR PARK IN AN HOUR.
        //All possible recievers
        List<int> recievers = new List<int>();
        foreach (var item in mainLoop.characters)
        {
            if (item.ID() != character.ID())
                recievers.Add(item.ID());
        }
        //randomly remove some
        for (int j = 0; j < Random.Range(0, mainLoop.characters.Length - 1); j++)
        {
            recievers.RemoveAt(Random.Range(0, recievers.Count));
        }
        int attendees = 1 + recievers.Count;
        Meeting myMeeting = new Meeting(mainLoop.locations[Random.Range(0,2)], clock.ElapsedDays(), clock.TimeOfDayHours()+1, Random.Range(1, 3), attendees, CharacterStateHangOut.Instance);
        if (character.social < character.maxVitals - 60 && character.notebook.TryAddDate(myMeeting))
        {
            if (character.notebookController.character == character && character.notebookController.day == clock.ElapsedDays())
            {
                character.notebookController.DisplayPage(clock.ElapsedDays());
            }
            //send invite to the lucky ones
            foreach (var reciever in recievers)
            {
                character.SendMeetingRequest(myMeeting, reciever);
            }
        }

        //IF DESPERATE, I CAN ALWAYS FIND PEOPLE TO CHAT WITH AT THE STORE
        if (character.social < character.socialThresholdCritical * character.maxVitals)
        {
            character.GetFSM().ChangeState(CharacterStateShop.Instance);
        }
        else
        {
            //CHANGE TO A PRIO STATE OTHER THAN THIS
            int prioStateIndex = 0;
            State<Character> prioState = character.GetPriorityState()[prioStateIndex];
            while (prioState == instance)
            {
                prioState = character.GetPriorityState()[++prioStateIndex];
            }
            character.GetFSM().ChangeState(prioState);

        }
    }

    public override void Exit(Character character)
    {
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

public class CharacterStateDead : State<Character>
{
    CharacterStateDead() { }

    //this is a singleton
    private static CharacterStateDead instance = null;
    public static CharacterStateDead Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateDead();
            }
            return instance;
        }
    }

    public override void Enter(Character character)
    {
        //automatic pause on death
        FSM_MainLoop.Instance.PauseTime(true);

        string causeOfDeath = "mysterious cirumstances";
        if (character.food <= 0)
            causeOfDeath = "Starvation";
        else if (character.drink <= 0)
            causeOfDeath = "Thirst";
        else if (character.social <= 0)
            causeOfDeath = "Loneliness";
        else if (character.sleep <= 0)
            causeOfDeath = "Exhaustion";

        //if (100 < character.GetFSM().getHistorySize())
        //{
        //    causeOfDeath = "Brain Rot";
        //}

        character.location.attendees--;
        character.transform.position = character.location.transform.position + new Vector3(0.2f, 0, 0) * character.location.corpses;
        character.location.corpses++;
        character.dayOfDeath = FSM_Clock.Instance.ElapsedDays();
        character.causeOfDeath = causeOfDeath;
        character.stateOnDeath = character.GetFSM().GetNameOfCurrentState();
        character.GetComponent<SpriteRenderer>().sprite = character.graveStone;
        character.GetComponent<SpriteRenderer>().sortingOrder = 5;
    }

    public override void Execute(Character character)
    {
    }

    public override void Exit(Character character)
    {
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        return false;
    }
}

