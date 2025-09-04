using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// =============================
//  GameManager.cs (главный файл)
// =============================
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isGameOver = false;
    public bool isGameStarted = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartGame()
    {
        isGameStarted = true;
        isGameOver = false;
        UIManager.Instance.ShowGameplayUI();
    }

    public void GameOver()
    {
        isGameOver = true;
        isGameStarted = false;
        UIManager.Instance.ShowGameOverUI();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

// =============================
//  BirdController.cs
// =============================
public class BirdController : MonoBehaviour
{
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isDead = false;

    public AudioSource audioSource;
    public AudioClip flapSound;
    public AudioClip hitSound;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    private void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        if (!GameManager.Instance.isGameStarted)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameManager.Instance.StartGame();
                rb.gravityScale = 2;
                Flap();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Flap();
            }
        }
    }

    private void Flap()
    {
        rb.velocity = Vector2.up * jumpForce;
        if (audioSource && flapSound)
            audioSource.PlayOneShot(flapSound);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDead)
        {
            isDead = true;
            if (audioSource && hitSound)
                audioSource.PlayOneShot(hitSound);
            GameManager.Instance.GameOver();
        }
    }
}

// =============================
//  PipeController.cs
// =============================
public class PipeController : MonoBehaviour
{
    public float moveSpeed = 2f;

    private void Update()
    {
        if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;

        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
}

// =============================
//  PipeSpawner.cs
// =============================
public class PipeSpawner : MonoBehaviour
{
    public GameObject pipePrefab;
    public float spawnRate = 2f;
    private float timer;

    public float minY = -1f;
    public float maxY = 2f;

    private void Update()
    {
        if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnPipe();
            timer = 0;
        }
    }

    private void SpawnPipe()
    {
        float y = Random.Range(minY, maxY);
        Instantiate(pipePrefab, new Vector3(5, y, 0), Quaternion.identity);
    }
}

// =============================
//  ScoreManager.cs
// =============================
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int score = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddScore()
    {
        score++;
        UIManager.Instance.UpdateScore(score);
    }

    public int GetScore()
    {
        return score;
    }
}

// =============================
//  UIManager.cs
// =============================
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject menuUI;
    public GameObject gameplayUI;
    public GameObject gameOverUI;

    public Text scoreText;
    public Text finalScoreText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ShowMenuUI();
    }

    public void ShowMenuUI()
    {
        menuUI.SetActive(true);
        gameplayUI.SetActive(false);
        gameOverUI.SetActive(false);
    }

    public void ShowGameplayUI()
    {
        menuUI.SetActive(false);
        gameplayUI.SetActive(true);
        gameOverUI.SetActive(false);
        UpdateScore(0);
    }

    public void ShowGameOverUI()
    {
        menuUI.SetActive(false);
        gameplayUI.SetActive(false);
        gameOverUI.SetActive(true);
        finalScoreText.text = "Score: " + ScoreManager.Instance.GetScore();
    }

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}

// =============================
//  ParallaxBackground.cs
// =============================
public class ParallaxBackground : MonoBehaviour
{
    public float speed = 0.5f;
    private Vector3 startPosition;
    private float width;

    private void Start()
    {
        startPosition = transform.position;
        width = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;

        float newPos = Mathf.Repeat(Time.time * -speed, width);
        transform.position = startPosition + Vector3.right * newPos;
    }
}

// =============================
//  TriggerZone.cs (очки)
// =============================
public class TriggerZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BirdController>())
        {
            ScoreManager.Instance.AddScore();
        }
    }
}
