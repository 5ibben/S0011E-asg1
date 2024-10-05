using UnityEngine;

public class BaseGameEntity : MonoBehaviour
{
    public string name = "";
    //every entity must have a unique identifying number
    private int m_ID;

    //this is the next valid ID. Each time a BaseGameEntity is instantiated
    //this value is updated
    private static int m_iNextValidID =0;

    //this must be called within the constructor to make sure the ID is set
    //correctly. It verifies that the value passed to the method is greater
    //or equal to the next valid ID, before setting the ID and incrementing
    //the next valid ID
    public BaseGameEntity()
    {
    }
    public BaseGameEntity(int id)
    {
        SetID(id);
    }
    private void SetID(int val)
    {
        ////make sure the val is equal to or greater than the next available ID
        //assert((val >= m_iNextValidID) && "<BaseGameEntity::SetID>: invalid ID");

        m_ID = val;

        m_iNextValidID = m_ID + 1;
        Debug.Log("m_iNextValidID is now: " + m_iNextValidID);
    }

    public void SetAutomaticID()
    {
        SetID(m_iNextValidID);
    }

    //virtual ~BaseGameEntity() { }

    //all entities must implement an update function
    public virtual void EntityUpdate() { }

    public int ID()
    {return m_ID;}

    public virtual bool HandleMessage(Telegram msg) { return false; }

};
