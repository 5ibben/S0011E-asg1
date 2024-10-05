using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<T>
{
    //virtual ~State() { }

    //this will execute when the state is entered
    public virtual void Enter(T entity) { }

    //this is the state's normal update function
    public virtual void Execute(T entity) { }

    //this will execute when the state is exited
    public virtual void Exit(T entity) { }

    //message dispatcher
    public virtual bool OnMessage(T entity,  Telegram tgm) { return false; }
};
