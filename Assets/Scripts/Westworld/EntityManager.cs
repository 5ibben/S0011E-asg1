using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class EntityManager
{
    Dictionary<int, BaseGameEntity> m_EntityMap = new Dictionary<int, BaseGameEntity>();

    //to facilitate quick lookup the entities are stored in a std::map, in which
    //pointers to entities are cross referenced by their identifying number

    EntityManager() { }

    //copy ctor and assignment should be private
    //EntityManager(EntityManager);
    //EntityManager& operator=(const EntityManager&);

    //this class is a singleton
    private static EntityManager instance = null;
    public static EntityManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EntityManager();
            }
            return instance;
        }
    }

    //this method stores a pointer to the entity in the std::vector
    //m_Entities at the index position indicated by the entity's ID
    //(makes for faster access)
    public void RegisterEntity(BaseGameEntity NewEntity)
    {
        Debug.Log("adding new entity: ");
        Debug.Log("ID : " + NewEntity.ID());
        Debug.Log("Entity : " + NewEntity.name);
        m_EntityMap.Add(NewEntity.ID(), NewEntity);
    }

    //returns a pointer to the entity with the ID given as a parameter
    public BaseGameEntity GetEntityFromID(int id)
    {
        //find the entity
        m_EntityMap.TryGetValue(id, out BaseGameEntity bge);

        //assert that the entity is a member of the map
        //assert((ent != m_EntityMap.end()) && "<EntityManager::GetEntityFromID>: invalid ID");

        return bge;
    }

    //this method removes the entity from the list
    public void RemoveEntity(BaseGameEntity pEntity)
    {
        m_EntityMap.Remove(pEntity.ID());
    }
};
