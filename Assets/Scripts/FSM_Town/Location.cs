using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{
    //if location is a home
    public int foodInFridge = 5;
    //if location is a workplace
    public int workStart = 7;
    public int workEnd = 4;
    //character stat modifiers for an hours work.
    public int money = 5;
    public int food = 5;
    public int drink = 5;
    public int social = 5;
    public int sleep = 5;
    //how many characters are at this location at the moment
    public int attendees = 0;
    //this is only used to determine the position of the character on screen.
    public int corpses = 0;
}
