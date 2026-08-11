using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Scenario
{
    public string question;
    public string correctAnswer;
}

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI feedbackText;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public float answerTime = 5f;

    private float currentTime;
    private bool timerRunning = false;

    [Header("Scenario Data")]
    public Scenario[] scenarios;
    private int currentScenario = 0;

    [Header("Movement")]
    public Transform player;
    public Transform ball;
    public Transform hoopTarget;
    public Transform ballHoldPoint;
    public Transform passTarget;
    public Transform receivePoint;

    public float moveSpeed = 5f;
    public float shootSpeed = 8f;
    public float passSpeed = 10f;

    private bool isDriving = false;
    private bool isShooting = false;
    private bool isPassing = false;
    private bool teammateShooting = false;
    private bool actionInProgress = false;

    private Vector3 playerStartPosition;
    private Vector3 ballStartPosition;

    
    
    void Start()
    {
        playerStartPosition = player.position;
        ballStartPosition = ball.position;

        LoadScenario();
    }

    public void ResetPositions()
    {
        player.position = playerStartPosition;
        ball.position = ballHoldPoint.position;
    }

    void Update()
    {

        // TIMER
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;

            timerText.text = Mathf.Ceil(currentTime).ToString();

            if (currentTime <= 0)
            {
                timerRunning = false;

                feedbackText.text = "Time's up!";

                actionInProgress = true;

                StartCoroutine(TimeOut());
            }
        }


        // DRIVE movement
        if (isDriving)
        {
            player.position = Vector2.MoveTowards(
                player.position,
                hoopTarget.position,
                moveSpeed * Time.deltaTime
            );

            // Ball follows player
            ball.position = ballHoldPoint.position;

            // Stop when close to hoop
            if (Vector2.Distance(player.position, hoopTarget.position) < 0.1f)
            {
                isDriving = false;
                feedbackText.text = "Nice finish at the rim!";

                StartCoroutine(ResetAfterDelay(3f));
            }
        }

        if (isShooting)
        {
            ball.position = Vector2.MoveTowards(
                ball.position,
                hoopTarget.position,
                shootSpeed * Time.deltaTime
            );

            if (Vector2.Distance(ball.position, hoopTarget.position) < 0.1f)
            {
                isShooting = false;
                feedbackText.text = "SCORE! Nice shot!";

                StartCoroutine(ResetAfterDelay(3f));
            }
        }   

        if (isPassing)
        {
            ball.position = Vector2.MoveTowards(
                ball.position,
                receivePoint.position,
                passSpeed * Time.deltaTime
            );

            if (Vector2.Distance(ball.position, receivePoint.position) < 0.1f)
            {
                isPassing = false;
                teammateShooting = true;

                feedbackText.text = "Teammate shoots!";
            }
        }

        if (teammateShooting)
        {
            ball.position = Vector2.MoveTowards(
                ball.position,
                hoopTarget.position,
                shootSpeed * Time.deltaTime
            );

            if (Vector2.Distance(ball.position, hoopTarget.position) < 0.1f)
            {
                teammateShooting = false;
                feedbackText.text = "Great pass! SCORE!";

                StartCoroutine(ResetAfterDelay(3f));
            }
        }
    }

    void LoadScenario()
    {
        questionText.text = scenarios[currentScenario].question;
        feedbackText.text = "";

        currentTime = answerTime;
        timerRunning = true;

        timerText.text = currentTime.ToString("F0");
    }

    public void MakeDecision(string choice)
    {
        if (choice == scenarios[currentScenario].correctAnswer)
        {
            feedbackText.text = "Correct!";
        }
        else
        {
            feedbackText.text = "Incorrect!";
        }
    }

    public void NextScenario()
    {
        if (actionInProgress) return;

        actionInProgress = true;

        feedbackText.text = "Loading next scenario...";

        StartCoroutine(NextScenarioDelay());
    }

    IEnumerator NextScenarioDelay()
    {
        yield return new WaitForSeconds(3f);

        currentScenario++;

        if (currentScenario < scenarios.Length)
        {
            LoadScenario();
        }
        else
        {
            questionText.text = "Game Complete!";
            feedbackText.text = "Great job!";

            timerRunning = false;
            timerText.text = "";
        }

        ResetPositions();

        actionInProgress = false;
    }

    // 🏀 DRIVE BUTTON
    public void ChooseDrive()
    {
        if (actionInProgress) return;
        timerRunning = false;

        CheckAnswer("Drive");

        if (!isCorrectDecision) return;

        actionInProgress = true;
        feedbackText.text = "Driving to the basket!";
        isDriving = true;
    }

    // 🏀 SHOOT BUTTON 
    public void ChooseShoot()
    {
        if (actionInProgress) return;
        timerRunning = false;

        CheckAnswer("Shoot");

        if (!isCorrectDecision) return;

        actionInProgress = true;
        feedbackText.text = "Shot taken!";
        isShooting = true;

        ball.position = ballHoldPoint.position;
    }

    // 🏀 PASS BUTTON 
    public void ChoosePass()
    {
        if (actionInProgress) return;
        timerRunning = false;

        CheckAnswer("Pass");

        if (!isCorrectDecision) return;

        actionInProgress = true;
        feedbackText.text = "Passing the ball!";
        isPassing = true;

        ball.position = ballHoldPoint.position;
    }


    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ResetPositions();

        currentScenario++;

        if (currentScenario < scenarios.Length)
        {
            LoadScenario();
        }
        else
        {
            questionText.text = "Game Complete!";
            feedbackText.text = "Great job!";
            timerRunning = false;
            timerText.text = "";
            yield break;
        }

        actionInProgress = false;
    }

    IEnumerator TimeOut()
    {
        yield return new WaitForSeconds(3f);

        ResetPositions();

        currentScenario++;

        if (currentScenario < scenarios.Length)
        {
            LoadScenario();
        }
        else
        {
            questionText.text = "Game Complete!";
            feedbackText.text = "Great job!";
            timerRunning = false;
            timerText.text = "";
            yield break;
        }

        actionInProgress = false;
    }

    bool isCorrectDecision = false;

    void CheckAnswer(string chosenAction)
    {
        isCorrectDecision = (chosenAction == scenarios[currentScenario].correctAnswer);

        if (!isCorrectDecision)
        {
            timerRunning = false;
            actionInProgress = true;

            feedbackText.text = "Wrong decision!";

            isDriving = false;
            isShooting = false;
            isPassing = false;
            teammateShooting = false;

            StartCoroutine(ResetAfterDelay(3f));
        }
    }
}
