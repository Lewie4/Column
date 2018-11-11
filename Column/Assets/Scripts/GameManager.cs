using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public class RowLayout
    {
        public GameObject leftPillar;
        public GameObject rightPillar;
        public int rowNumber;
    }

    public class DespawnManagement
    {
        public float timeToDespawn;
        public RowLayout row;

        public DespawnManagement(float time, RowLayout currentRow)
        {
            timeToDespawn = time;
            row = currentRow;
        }
    }

    [Header("In Level")]
    [SerializeField] private TextMeshProUGUI m_scoreText;
    [SerializeField] private int m_maxActivePassedRows;

    [Header("Not in Level")]
    [SerializeField] private List<GameSettings> m_gameSettings;

    //Settings
    private LevelSettings m_levelSettings;
    private PlayerSettings m_playerSettings;

    //Pillar
    private List<RowLayout> m_rows;
    private int m_spawnedRowCount;
    private float m_totalChance;
    private List<DespawnManagement> m_rowsToDespawn;

    //Player
    private Transform m_player;
    private int m_currentRow;
    private int m_activeCurrentRow;
    private bool m_isLeft;
    private Vector3 m_startPos;
    private Vector3 m_destinationPos;
    private bool m_isAlive;
    private bool m_isJumping;
    private float m_jumpProgress;
    private bool m_isDying;
    private float m_deathProgress;

    //Camera
    private Transform m_camera;

    //Score
    private int m_score;

    #region GameManager
    private void Start()
    {
        bool requiredComponents = true;

        //Camera setup
        m_camera = Camera.main.transform;
        requiredComponents = requiredComponents && m_camera != null;

        //Score text setup
        requiredComponents = requiredComponents && m_scoreText != null;

        if (requiredComponents)
        {
            SetupGame();
        }
        else
        {
            Debug.LogError("Required components aren't set up properly!");
        }
    }

    private void SetupGame()
    {
        m_levelSettings = m_gameSettings[0].levelSettings;
        m_playerSettings = m_gameSettings[0].playerSettings;

        m_rowsToDespawn = new List<DespawnManagement>();

        //Switch to game UI
        //Spawn pillars
        SetupPillars();

        //Spawn player
        SetupPlayer();
    }

    private void Update()
    {
        if (m_isJumping)
        {
            if (m_jumpProgress <= 1)
            {
                m_player.position = Vector3.Lerp(m_startPos, m_destinationPos, m_jumpProgress);
                m_player.position = new Vector3(m_player.position.x, m_player.position.y + m_playerSettings.jumpCurve.Evaluate(m_jumpProgress), m_player.position.z);
                m_jumpProgress += Time.deltaTime / m_playerSettings.jumpTime;
            }
            else
            {
                m_player.position = m_destinationPos;
                m_isJumping = false;
                m_jumpProgress = 0;
                m_rowsToDespawn.Add(new DespawnManagement(m_levelSettings.timeToDespawn, m_rows[m_activeCurrentRow]));

                if (!m_isAlive)
                {
                    Death();
                }
            }

        }

        if (m_isAlive)
        {
            if (m_rowsToDespawn != null && m_rowsToDespawn.Count > 0)
            {
                List<DespawnManagement> despawnsToRemove = new List<DespawnManagement>();
                for (int i = 0; i < m_rowsToDespawn.Count; i++)
                {
                    m_rowsToDespawn[i].timeToDespawn -= Time.deltaTime;
                    if (m_rowsToDespawn[i].timeToDespawn <= 0 || (m_rowsToDespawn.Count - i > m_maxActivePassedRows))
                    {
                        RemoveRow(m_rowsToDespawn[i].row);
                        despawnsToRemove.Add(m_rowsToDespawn[i]);
                    }
                }

                foreach (var despawner in despawnsToRemove)
                {
                    m_rowsToDespawn.Remove(despawner);
                }
            }
        }

        if (m_isDying)
        {
            if (m_deathProgress <= 1)
            {
                m_player.position = Vector3.Lerp(m_startPos, m_destinationPos, m_deathProgress);
                m_player.position = new Vector3(m_player.position.x, m_player.position.y + m_playerSettings.jumpCurve.Evaluate(m_jumpProgress), m_player.position.z);
                m_deathProgress += Time.deltaTime / m_playerSettings.jumpTime;
            }
            else
            {
                Destroy(m_player.gameObject);
                m_isDying = false;
            }
        }
    }
    private void UpdateScore(int score, bool shouldOverride = false)
    {
        if (shouldOverride)
        {
            m_score = score;
        }
        else
        {
            m_score += score;
        }
        m_scoreText.text = m_score.ToString();
    }
    #endregion

    #region Pillars
    private void SetupPillars()
    {
        m_rows = new List<RowLayout>();
        m_spawnedRowCount = 0;

        m_totalChance = m_levelSettings.spawnChance.x + m_levelSettings.spawnChance.y + m_levelSettings.spawnChance.z;

        for (int i = 0; i < m_levelSettings.visibleRows && (i < m_levelSettings.totalRows || m_levelSettings.totalRows < 0); i++)
        {
            SetupRow(i);
        }
    }

    private void SetupRow(int rowCount)
    {
        var chosenValue = Random.Range(0, m_totalChance);

        m_rows.Add(new RowLayout());

        if (m_totalChance - m_levelSettings.spawnChance.x < chosenValue)
        {
            SpawnPillar(true, false, rowCount);
        }
        else if (m_totalChance - m_levelSettings.spawnChance.x - m_levelSettings.spawnChance.y < chosenValue)
        {
            SpawnPillar(false, true, rowCount);
        }
        else
        {
            SpawnPillar(true, true, rowCount);
        }

        m_spawnedRowCount++;
    }

    private void SpawnPillar(bool spawnLeft, bool spawnRight, int rowCount)
    {
        if (spawnLeft)
        {
            var leftPillar = Instantiate(m_levelSettings.pillar, new Vector3(-m_levelSettings.positionOffset.x, m_levelSettings.positionOffset.y * m_spawnedRowCount,
                m_levelSettings.positionOffset.z * m_spawnedRowCount), m_levelSettings.pillar.transform.rotation, transform);
            m_rows[rowCount].leftPillar = leftPillar;
        }

        if (spawnRight)
        {
            var rightPillar = Instantiate(m_levelSettings.pillar, new Vector3(m_levelSettings.positionOffset.x, m_levelSettings.positionOffset.y * m_spawnedRowCount,
                m_levelSettings.positionOffset.z * m_spawnedRowCount), m_levelSettings.pillar.transform.rotation, transform);
            m_rows[rowCount].rightPillar = rightPillar;
        }

        m_rows[rowCount].rowNumber = m_spawnedRowCount;
    }

    private void RemoveRow(RowLayout row)
    {
        if (m_currentRow == row.rowNumber)
        {
            m_isAlive = false;
            Death();
        }

        if (row.leftPillar != null)
        {
            Destroy(row.leftPillar);
        }
        if (row.rightPillar != null)
        {
            Destroy(row.rightPillar);
        }

        m_rows.Remove(row);

        m_activeCurrentRow--;

        //Spawn in new row if needed
        if (m_levelSettings.totalRows >= m_spawnedRowCount)
        {
            AddRow();
        }
    }

    private void AddRow()
    {
        SetupRow(m_rows.Count);
    }
    #endregion

    #region Player
    private void SetupPlayer()
    {
        m_isAlive = true;
        m_isJumping = false;
        m_jumpProgress = 0;
        m_currentRow = 0;
        m_activeCurrentRow = 0;
        m_isDying = false;
        m_deathProgress = 0;

        UpdateScore(0, true);

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        m_isLeft = Random.Range(0, 2) == 0;

        m_player = Instantiate(m_playerSettings.player, transform).transform;

        float xPos = m_isLeft ? -m_levelSettings.positionOffset.x : m_levelSettings.positionOffset.x;
        float yPos = m_player.GetComponent<MeshFilter>().mesh.bounds.size.y;
        float zPos = -m_levelSettings.positionOffset.z;

        m_player.position = new Vector3(xPos, yPos, zPos);

        m_camera.parent = m_player;
    }

    public void StartJump(bool switchSides)
    {
        if (!m_isJumping && m_isAlive)
        {
            m_isJumping = true;
            m_startPos = m_player.position;

            m_isLeft = switchSides ? !m_isLeft : m_isLeft;
            m_isAlive = CheckForPillar();

            m_currentRow++;
            m_activeCurrentRow++;

            UpdateScore(1);

            float xPos = m_isLeft ? -m_levelSettings.positionOffset.x : m_levelSettings.positionOffset.x;
            float yPos = m_player.GetComponent<MeshFilter>().mesh.bounds.size.y + (m_currentRow * m_levelSettings.positionOffset.y);
            float zPos = m_currentRow * m_levelSettings.positionOffset.z;

            m_destinationPos = new Vector3(xPos, yPos, zPos);
        }
    }

    private bool CheckForPillar()
    {
        var nextRow = m_rows[m_activeCurrentRow + 1];
        var destinationPillar = m_isLeft ? nextRow.leftPillar : nextRow.rightPillar;
        return destinationPillar != null;
    }

    private void Death()
    {
        Debug.Log("YOU DEAD!");

        m_camera.parent = null;

        m_startPos = m_player.position;
        m_destinationPos = new Vector3(m_startPos.x, m_startPos.y - 6, m_startPos.z);
        m_isDying = true;
    }
    #endregion
}
