using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//to make code easier to read
//const double SEND_MSG_IMMEDIATELY = 0.0f;
//const int NO_ADDITIONAL_INFO = 0;

//to make life easier...
//#define Dispatch MessageDispatcher::Instance()

enum messages
{
    Schedule_meeting,
    Decline_meeting
}

class MessageDispatcher
{
    //messages
    public enum message_type
    {
        Msg_HiHoneyImHome,
        Msg_StewReady,
    };

    //a std::set is used as the container for the delayed messages
    //because of the benefit of automatic sorting and avoidance
    //of duplicates. Messages are sorted by their dispatch time.
    SortedSet<Telegram> PriorityQ = new SortedSet<Telegram>();

    //this method is utilized by DispatchMessage or DispatchDelayedMessages.
    //This method calls the message handling member function of the receiving
    //entity, pReceiver, with the newly created telegram
    void Discharge(BaseGameEntity pReceiver, Telegram msg) 
    {
        if (!pReceiver.HandleMessage(msg))
        {
            //telegram could not be handled
            Debug.Log("Message not handled");
        }
    }

    //---------------------------- DispatchMessage ---------------------------
    //
    //  given a message, a receiver, a sender and any time delay , this function
    //  routes the message to the correct agent (if no delay) or stores
    //  in the message queue to be dispatched at the correct time
    //------------------------------------------------------------------------
    MessageDispatcher() 
    {
        
    }

    //copy ctor and assignment should be private
    //MessageDispatcher(const MessageDispatcher&);
    //MessageDispatcher& operator=(const MessageDispatcher&);


    //this class is a singleton
    private static MessageDispatcher instance = null;
    public static MessageDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MessageDispatcher();
            }
            return instance;
        }
    }

    //send a message to another agent. Receiving agent is referenced by ID.
    public void DispatchMessage(double delay,int sender,int receiver,int msg, Meeting meeting = null)
    {
        //get pointers to the sender and receiver
        BaseGameEntity pSender = EntityManager.Instance.GetEntityFromID(sender);
        BaseGameEntity pReceiver = EntityManager.Instance.GetEntityFromID(receiver);

        //make sure the receiver is valid
        if (pReceiver == null)
        {
            Debug.Log("Warning! No Receiver with ID of " + receiver + " found");
            return;
        }

        //create the telegram
        Telegram telegram = new Telegram(0, sender, receiver, msg, meeting);

        //if there is no delay, route telegram immediately                       
        if (delay <= 0.0f)
        {
            //Debug.Log("Instant telegram dispatched at time: " + Time.time + " by " + pSender.GetNameOfEntity(pSender.ID()) + " for " + pReceiver.GetNameOfEntity(pReceiver.ID()) + ". Msg is " + MsgToStr(msg));

            //send the telegram to the recipient
            Discharge(pReceiver, telegram);
        }

        //else calculate the time when the telegram should be dispatched
        else
        {
            double CurrentTime = Time.time;

            telegram.DispatchTime = CurrentTime + delay;

            //and put it in the queue
            PriorityQ.Add(telegram);

            //Debug.Log("Delayed telegram from " + pSender.GetNameOfEntity(pSender.ID()) + " recorded at time " + Time.time + " for " + pReceiver.GetNameOfEntity(pReceiver.ID()) + ". Msg is " + MsgToStr(msg));
        }

    }

    //send out any delayed messages. This method is called each time through   
    //the main game loop.
    public void DispatchDelayedMessages() 
    {
        //get current time
        double CurrentTime = Time.time;

        //now peek at the queue to see if any telegrams need dispatching.
        //remove all telegrams from the front of the queue that have gone
        //past their sell by date
        while (PriorityQ.Count != 0 && (PriorityQ.Min.DispatchTime < CurrentTime) && (PriorityQ.Min.DispatchTime > 0))
        {
            //read the telegram from the front of the queue
            Telegram telegram = PriorityQ.Min;

            //find the recipient
            BaseGameEntity pReceiver = EntityManager.Instance.GetEntityFromID(telegram.Receiver);

            //Debug.Log("\nQueued telegram ready for dispatch: Sent to " + pReceiver.GetNameOfEntity(pReceiver.ID()) + ". Msg is " + MsgToStr(telegram.Msg));

            //send the telegram to the recipient
            Discharge(pReceiver, telegram);

            //remove it from the queue
            PriorityQ.Remove(PriorityQ.Min);
        }
    }

};
