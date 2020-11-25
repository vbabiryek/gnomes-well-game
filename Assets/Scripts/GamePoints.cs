using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePoints
{
    public int points;

    public GamePoints(){

    }

    public GamePoints(int points){
        this.points = points;
    }

    public int getPoints(){
        return points;
    }

}
