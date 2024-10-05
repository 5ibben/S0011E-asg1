using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Telegram
{
    //the entity that sent this telegram
    public int Sender;

    //the entity that is to receive this telegram
    public int Receiver;

    //the message itself. These are all enumerated in the file
    //"MessageTypes.h"
    public int Msg;

    //messages can be dispatched immediately or delayed for a specified amount
    //of time. If a delay is necessary this field is stamped with the time 
    //the message should be dispatched.
    public double DispatchTime;

    //any additional information that may accompany the message
    public Meeting meeting;

    public Telegram() 
    {
        DispatchTime = -1;
        Sender = -1;
        Receiver = -1;
        Msg = -1;
    }

    public Telegram(double time, int sender, int receiver, int msg, Meeting info = null)
    {
        DispatchTime = time;
        Sender = sender;
        Receiver = receiver;
        Msg = msg;
        meeting = info;
    }

    const double SmallestDelay = 0.25;

    public static bool operator ==(Telegram a, Telegram b)
    {
        return (Mathf.Abs((float)(a.DispatchTime - b.DispatchTime)) < SmallestDelay) &&
        (a.Sender == b.Sender) &&
        (a.Receiver == b.Receiver) &&
        (a.Msg == b.Msg);
    }

    public static bool operator !=(Telegram a, Telegram b)
    {
        return !(a==b);
    }

    public override bool Equals(object other)
    {
        return this == (Telegram)other;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public static bool operator >(Telegram a, Telegram b)
    {
        return a.DispatchTime > b.DispatchTime;
    }

    public static bool operator <(Telegram a, Telegram b)
    {
        return a.DispatchTime < b.DispatchTime;
    }


}
