using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour
{

    public GameObject JumpText;
    public GameObject StartText;
    public GameObject SlideText;
    public GameObject DashText;
    public GameObject WallrunText;
    public GameObject EndText;

    void OnTriggerEnter(Collider collider) {
        //JUMPTUTORIAL
        if(collider.gameObject.name == "JumpTutorial")
        {
            JumpText.SetActive(true);
        }

        //STARTTUTORIAL
        if (collider.gameObject.name == "StartTutorial")
        {
            StartText.SetActive(true);
        } 

        //SLIDETUTORIAL
        if (collider.gameObject.name == "SlideTutorial")
        {
            SlideText.SetActive(true);
        }

        //DASHTUTORIAL
        if (collider.gameObject.name == "DashTutorial")
        {
            DashText.SetActive(true);
        }

        //WALLRUN
        if (collider.gameObject.name == "WallRunTutorial")
        {
            WallrunText.SetActive(true);
        }

        //END
        if (collider.gameObject.name == "EndTutorial")
        {
            EndText.SetActive(true);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        //JUMPTUTORIAL
        if (collider.gameObject.name == "JumpTutorial")
        {
            JumpText.SetActive(false);
        }

        //STARTTUTORIAL
        if (collider.gameObject.name == "StartTutorial")
        {
            StartText.SetActive(false);
        }

        //SLIDETUTORIAL
        if (collider.gameObject.name == "SlideTutorial")
        {
            SlideText.SetActive(false);
        }

        //DASHTUTORIAL
        if (collider.gameObject.name == "DashTutorial")
        {
            DashText.SetActive(false);
        }

        //WALLRUN
        if (collider.gameObject.name == "WallRunTutorial")
        {
            WallrunText.SetActive(false);
        }

        //END
        if (collider.gameObject.name == "EndTutorial")
        {
            EndText.SetActive(false);
        }
    }
}
