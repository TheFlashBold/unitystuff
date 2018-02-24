using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Kit_PhotonPlayerExtensions
{
    public static int GetPlayerScore(this PhotonPlayer p)
    {
        int score = 0;

        //Check for kills
        if (p.CustomProperties["kills"] != null)
        {
            //Add kills to score
            score += (int)p.CustomProperties["kills"];
        }

        return score;
    }
}
