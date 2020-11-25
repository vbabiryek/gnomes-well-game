﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Contains a UnityEvent that can be used 
//to reset the state of this object.
public class Resettable : MonoBehaviour {

    //In the editor, connect this event to methods that should 
    //run when the game resets.
    public UnityEvent onReset;

    //Called by the GameChanger when the game
    //resets.
    public void Reset(){
        //Kicks off the event, which calls all of the
        //connected methods.
        onReset.Invoke();
    }
}