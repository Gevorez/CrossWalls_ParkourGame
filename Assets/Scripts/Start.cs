using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Start : MonoBehaviour
{   

    public TextMeshProUGUI timeText;
    private bool startTime = false;
    private float time;
    
    void FixedUpdate()
    {
        Debug.Log(startTime);
        if(startTime)
        {
            time += Time.deltaTime;
            timeText.text = "Time: " + Mathf.Round(time *100f)/100f;
        }else
        {
            timeText.text = "";
        }

         if(Input.GetKey(KeyCode.R))
         {
             startTime = false;
         }
    }

    private void OnTriggerEnter(Collider other) {
        time = 0f;
        startTime = true;
    }
}
