using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EntitySelection : MonoBehaviour
{
    GameObject selectedObject;
    public NotebookController notebookController;

    public GameObject characterStats;
    public GameObject locationStats;
    public GameObject graveStats;

    public TextMeshProUGUI name;
    public TextMeshProUGUI money;
    public Slider foodSlider;
    public Slider drinkSlider;
    public Slider sleepSlider;
    public Slider socialSlider;

    public Button invulnerabilityButton;

    public Image portrait;


    private void Start()
    {
        //selectObject(FindObjectOfType<Character>().gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D targetObject = Physics2D.OverlapPoint(mousePosition);
            if (targetObject)
            {
                SelectObject(targetObject.gameObject);
            }
        }
    }

    int charIndex = 0;
    public void nextCharacter()
    {
        if (++charIndex >= FSM_MainLoop.Instance.characters.Length)
            charIndex = 0;
        SelectObject(FSM_MainLoop.Instance.characters[charIndex].gameObject);
    }

    public void ToggleCharacterInvulnerability()
    {
        Character character = selectedObject.GetComponent<Character>();
        if (character)
        {
            if (character.invurnerable)
            {
                //invulnerabilityButton.colors = ColorBlock.defaultColorBlock;
                invulnerabilityButton.image.color = Color.white;
                character.invurnerable = false;
            }
            else
            {
                invulnerabilityButton.image.color = Color.green;
                character.invurnerable = true;
            }
        }
    }

    public void SelectObject(GameObject target)
    {
        selectedObject = target;
        //Debug.Log("selected object: " + target.name);
        Character character = target.GetComponent<Character>();
        Location location = target.GetComponent<Location>();
        if (character)
        {
            name.text = character.name;
            if (character.GetFSM().isInState(CharacterStateDead.Instance))
            {
                characterStats.SetActive(false);
                locationStats.SetActive(false);
                graveStats.SetActive(true);
            }
            else
            {
                characterStats.SetActive(true);
                locationStats.SetActive(false);
                graveStats.SetActive(false);

                TextMeshProUGUI[] textfields = characterStats.GetComponentsInChildren<TextMeshProUGUI>();
                textfields[1].text = character.workplace.name.ToString();
            }
            notebookController.displayNotebook();
            notebookController.character = character;
            notebookController.DisplayPage(FSM_Clock.Instance.ElapsedDays());
            if (character.invurnerable)
            {
                invulnerabilityButton.image.color = Color.green;
            }
            else
            {
                invulnerabilityButton.image.color = Color.white;
            }
        }
        else
        {
            notebookController.hideNotebook();
            if (location) 
            {
                name.text = location.name;
                characterStats.SetActive(false);
                locationStats.SetActive(true);
                graveStats.SetActive(false);
            }
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (selectedObject)
        {
            Location location = selectedObject.GetComponent<Location>();
            Character character = selectedObject.GetComponent<Character>();
            if (character)
            {
                if (character.GetFSM().isInState(CharacterStateDead.Instance))
                {
                    TextMeshProUGUI[] textFields = graveStats.GetComponentsInChildren<TextMeshProUGUI>();
                    textFields[0].text = character.dayOfDeath.ToString();
                    textFields[1].text = character.stateOnDeath;
                    textFields[2].text = character.causeOfDeath;
                }
                else
                {
                    foodSlider.value = character.food;
                    drinkSlider.value = character.drink;
                    sleepSlider.value = character.sleep;
                    socialSlider.value = character.social;
                    money.text = ((int)character.money).ToString();
                    characterStats.GetComponentsInChildren<TextMeshProUGUI>(true)[3].text = character.GetFSM().GetNameOfCurrentState();
                }
            }
            else if(location)
            {
                TextMeshProUGUI[] textFields = locationStats.GetComponentsInChildren<TextMeshProUGUI>();
                if (0 < location.money)
                    textFields[0].text = "+" + location.money;
                else
                    textFields[0].text = location.money.ToString();

                if (0 < location.food)
                    textFields[1].text = "+" + location.food;
                else
                    textFields[1].text = location.food.ToString();

                if (0 < location.drink)
                    textFields[2].text = "+" + location.drink;
                else
                    textFields[2].text = location.drink.ToString();

                if (0 < location.sleep)
                    textFields[3].text = "+" + location.sleep;
                else
                    textFields[3].text = location.sleep.ToString();

                if (0 < location.social)
                    textFields[4].text = "+" + location.social;
                else
                    textFields[4].text = location.social.ToString();

                textFields[5].text = location.attendees.ToString();
            }


            portrait.sprite = selectedObject.GetComponent<SpriteRenderer>().sprite;
            portrait.color = selectedObject.GetComponent<SpriteRenderer>().color;
        }
    }

    public void setFood()
    {
        selectedObject.GetComponent<Character>().food = (int)foodSlider.value;
    }

    public void setDrink()
    {
        selectedObject.GetComponent<Character>().drink = (int)drinkSlider.value;
    }

    public void setSleep()
    {
        selectedObject.GetComponent<Character>().sleep = (int)sleepSlider.value;
    }

    public void setSocial()
    {
        selectedObject.GetComponent<Character>().social = (int)socialSlider.value;
    }

    public void incrementMoney()
    {
        selectedObject.GetComponent<Character>().money+=1000;
        money.text = selectedObject.GetComponent<Character>().money.ToString();
    }

    public void decrementMoney()
    {
        selectedObject.GetComponent<Character>().money-=1000;
        money.text = selectedObject.GetComponent<Character>().money.ToString();
    }
}
