using System;
using UnityEngine;
using UnityEngine.UI;

namespace MarsFPSKit
{
    /// <summary>
    /// Displays a simple, COD4 style victory screen
    /// </summary>
    public class Kit_SimpleVictoryScreen : Kit_VictoryScreenUI
    {
        /// <summary>
        /// Reference to the main behaviour
        /// </summary>
        public Kit_IngameMain main;
        /// <summary>
        /// Root object of the UI elements
        /// </summary>
        public GameObject root;

        [Header("Player Win")]
        /// <summary>
        /// It uses different styles for player / team. This is the root of the player object
        /// </summary>
        public GameObject pwRoot;
        /// <summary>
        /// This displays whether we won or lost
        /// </summary>
        public Text pwVictoryLoose;
        /// <summary>
        /// This displays the name of the winning player
        /// </summary>
        public Text pwName;

        [Header("Team")]
        /// <summary>
        /// It uses different styles for player / team. This is the root of the team object
        /// </summary>
        public GameObject teamWinRoot;
        /// <summary>
        /// This displays whether we won or lost
        /// </summary>
        public Text teamWinVictoryLoose;
        /// <summary>
        /// Root object of points for teams
        /// </summary>
        public GameObject teamWinScore;
        /// <summary>
        /// Displays the score for team one
        /// </summary>
        public Text teamWinScoreTeamOne;
        /// <summary>
        /// Displays the score for team two
        /// </summary>
        public Text teamWinScoreTeamTwo;

        [Header("Draw")]
        /// <summary>
        /// Draw uses a display of its own. This is its root
        /// </summary>
        public GameObject drawRoot;
        /// <summary>
        /// Root object of points for teams
        /// </summary>
        public GameObject drawScore;
        /// <summary>
        /// Displays the score for team one
        /// </summary>
        public Text drawScoreTeamOne;
        /// <summary>
        /// Displays the score for team two
        /// </summary>
        public Text drawScoreTeamTwo;

        public override void CloseUI()
        {
            //Disable root
            root.SetActive(false);
        }

        public override void DisplayPlayerWinner(PhotonPlayer winner)
        {
            //Reset roots
            drawRoot.SetActive(false);
            pwRoot.SetActive(false);
            drawScore.SetActive(false);
            teamWinRoot.SetActive(false);
            teamWinScore.SetActive(false);

            //Check if we won
            if (winner == PhotonNetwork.player)
            {
                //We won
                //Display victory
                pwVictoryLoose.text = "Victory!";
                //Display the name
                pwName.text = winner.NickName;
            }
            else
            {
                //We lost
                //Display loose
                pwVictoryLoose.text = "Defeat!";
                //Display the name
                pwName.text = winner.NickName;
            }

            //Activate player root
            pwRoot.SetActive(true);
            //Enable root
            root.SetActive(true);
        }

        public override void DisplayBotWinner(Kit_Bot winner)
        {
            //Reset roots
            drawRoot.SetActive(false);
            pwRoot.SetActive(false);
            drawScore.SetActive(false);
            teamWinRoot.SetActive(false);
            teamWinScore.SetActive(false);

            //We lost
            //Display loose
            pwVictoryLoose.text = "Defeat!";
            //Display the name
            pwName.text = winner.name;

            //Activate player root
            pwRoot.SetActive(true);
            //Enable root
            root.SetActive(true);
        }

        public override void DisplayTeamWinner(int winner)
        {
            //Reset roots
            drawRoot.SetActive(false);
            pwRoot.SetActive(false);
            drawScore.SetActive(false);
            teamWinRoot.SetActive(false);
            teamWinScore.SetActive(false);

            if (winner == 0)
            {
                //Check if we won
                if (main.assignedTeamID == 0)
                {
                    teamWinVictoryLoose.text = "Victory!";
                }
                else
                {
                    teamWinVictoryLoose.text = "Defeat!";
                }
                //Activate root
                teamWinRoot.SetActive(true);
            }
            else if (winner == 1)
            {
                //Check if we won
                if (main.assignedTeamID == 1)
                {
                    teamWinVictoryLoose.text = "Victory!";
                }
                else
                {
                    teamWinVictoryLoose.text = "Defeat!";
                }
                //Activate root
                teamWinRoot.SetActive(true);
            }
            else
            {
                //Draw
                //Activate draw root
                drawRoot.SetActive(true);
            }

            //Enable root
            root.SetActive(true);
        }

        public override void DisplayTeamWinnerWithScores(int winner, int teamOneScore, int teamTwoScore)
        {
            //Reset roots
            drawRoot.SetActive(false);
            pwRoot.SetActive(false);
            drawScore.SetActive(false);
            teamWinRoot.SetActive(false);
            teamWinScore.SetActive(false);

            if (winner == 0)
            {
                //Check if we won
                if (main.assignedTeamID == 0)
                {
                    teamWinVictoryLoose.text = "Victory!";
                }
                else
                {
                    teamWinVictoryLoose.text = "Defeat!";
                }

                //Set points
                teamWinScoreTeamOne.text = teamOneScore.ToString();
                teamWinScoreTeamTwo.text = teamTwoScore.ToString();

                //Enable
                teamWinScore.SetActive(true);

                //Activate root
                teamWinRoot.SetActive(true);
            }
            else if (winner == 1)
            {
                //Check if we won
                if (main.assignedTeamID == 1)
                {
                    teamWinVictoryLoose.text = "Victory!";
                }
                else
                {
                    teamWinVictoryLoose.text = "Defeat!";
                }

                //Set points
                teamWinScoreTeamOne.text = teamOneScore.ToString();
                teamWinScoreTeamTwo.text = teamTwoScore.ToString();

                //Enable
                teamWinScore.SetActive(true);

                //Activate root
                teamWinRoot.SetActive(true);
            }
            else
            {
                //Draw
                //Activate draw root
                drawRoot.SetActive(true);

                //Set points
                drawScoreTeamOne.text = teamOneScore.ToString();
                drawScoreTeamTwo.text = teamTwoScore.ToString();

                //Enable
                drawScore.SetActive(true);
            }

            //Enable root
            root.SetActive(true);
        }
    }
}
