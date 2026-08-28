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

    public float easyTime = 8f;
    public float mediumTime = 5f;
    public float hardTime = 3f;

    private float answerTime;
    private float currentTime;
    private bool timerRunning = false;

    [Header("Difficulty")]
    public GameObject difficultyPanel;

    [Header("Scenario Data")]
    public Scenario[] scenarios;
    private int currentScenario = 0;

    [Header("Score")]
    public int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("Movement")]
    public Transform player;
    public Transform ball;
    public Transform hoopTarget;
    public Transform ballHoldPoint;
    public Transform passTarget;
    public Transform receivePoint;

    public Transform defender;
    public Transform defenderShootTarget;
    public Transform defenderDriveTarget;

    public Transform driveAroundTarget;

    public float moveSpeed = 5f;
    public float shootSpeed = 8f;
    public float passSpeed = 10f;

    private bool isDriving = false;
    private bool isDrivingAround = false;
    private bool isShooting = false;
    private bool isPassing = false;
    private bool teammateShooting = false;
    private bool actionInProgress = false;

    private bool isDefenderClosing = false;
    private Transform defenderTarget;

    private Vector3 playerStartPosition;
    private Vector3 ballStartPosition;
    private Vector3 defenderStartPosition;


    void Start()
    {
        playerStartPosition = player.position;
        ballStartPosition = ball.position;
        defenderStartPosition = defender.position;

        difficultyPanel.SetActive(true);

        questionText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
    }

    // DIFFICULTY BUTTONS

    public void StartEasy()
    {
        answerTime = easyTime;
        StartGame();
    }

    public void StartMedium()
    {
        answerTime = mediumTime;
        StartGame();
    }

    public void StartHard()
    {
        answerTime = hardTime;
        StartGame();
    }

    void StartGame()
    {
        difficultyPanel.SetActive(false);

        questionText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);

        currentScenario = 0;

        score = 0;

        if (scoreText != null)
        {
            scoreText.text = "Score: 0";
        }

        LoadScenario();
    }

    public void ResetPositions()
    {
        player.position = playerStartPosition;
        ball.position = ballHoldPoint.position;
        defender.position = defenderStartPosition;

        isDefenderClosing = false;
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


        // DRIVE MOVEMENT
        if (isDrivingAround)
        {
            player.position = Vector2.MoveTowards(
                player.position,
                driveAroundTarget.position,
                moveSpeed * Time.deltaTime
            );

            // Ball follows player's hand
            ball.position = ballHoldPoint.position;

            // Once player reaches the target around defender
            if (Vector2.Distance(
                player.position,
                driveAroundTarget.position
            ) < 0.1f)
            {
                player.position = driveAroundTarget.position;

                isDrivingAround = false;
                isDriving = true;
            }
        }

        if (isDriving)
        {
            player.position = Vector2.MoveTowards(
                player.position,
                hoopTarget.position,
                moveSpeed * Time.deltaTime
            );

            // Ball follows player's hand
            ball.position = ballHoldPoint.position;

            if (Vector2.Distance(
                player.position,
                hoopTarget.position
            ) < 0.1f)
            {
                player.position = hoopTarget.position;

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

        // DEFENDER CLOSEOUT
        if (isDefenderClosing)
        {
            defender.position = Vector2.MoveTowards(
                defender.position,
                defenderTarget.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector2.Distance(
                defender.position,
                defenderTarget.position
            ) < 0.1f)
            {
                isDefenderClosing = false;
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

        // Player goes around defender first
        isDrivingAround = true;

        // Defender over-commits
        defenderTarget = defenderDriveTarget;
        isDefenderClosing = true;
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

        // Ball shoots
        isShooting = true;

        ball.position = ballHoldPoint.position;

        // Defender gives the shooter space
        defenderTarget = defenderShootTarget;
        isDefenderClosing = true;
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

        if (isCorrectDecision)
        {
            score += 1;

            if (scoreText != null)
            {
                scoreText.text = "Score: " + score;
            }
        }
        else
        {
            feedbackText.text = "Wrong decision!";
            timerRunning = false;
            actionInProgress = true;

            isDriving = false;
            isShooting = false;
            isPassing = false;
            teammateShooting = false;

            StartCoroutine(ResetAfterDelay(3f));
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
