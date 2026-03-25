using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Menu")]
    public GameObject pauseMenuRoot;
    public Transform xrCamera;

    [Header("Gameplay Scripts To Disable")]
    public VRPlayerInputHandler playerInputHandler;
    public HumanJoystickTranslation humanJoystickTranslation;
    public PlayerWeaponsManager weaponsManager;

    [Header("Menu Placement")]
    public float menuDistance = 1.5f;
    public float heightOffset = -0.1f;

    private bool isPaused = false;

    public Slider bodyOffset;
    public Slider headRotation;

    private const string BodyOffsetPrefKey = "BodyOffsetForMaxSpeed";
    private const string HeadRotPrefKey = "HeadRotationOffset";

    private void Start()
    {
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
    }

    private void Update()
    {
        // Temporary keyboard test in editor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void SetBodyOffsetPrefs()
    {
        PlayerPrefs.SetFloat(BodyOffsetPrefKey, bodyOffset.value);
        PlayerPrefs.Save();
    }

    public void SetHeadRotationPrefs()
    {
        PlayerPrefs.SetFloat(HeadRotPrefKey, headRotation.value);
        PlayerPrefs.Save();
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;

        PositionMenuInFrontOfPlayer();

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);

        SetGameplayScriptsEnabled(false);

        // Optional:
        //Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        SetGameplayScriptsEnabled(true);

        // Optional:
        //Time.timeScale = 1f;
    }

    public void RestartScene()
    {
        // Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu(string sceneName)
    {
        // Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitApplication()
    {
        // Time.timeScale = 1f;
        Application.Quit();
    }

    private void SetGameplayScriptsEnabled(bool enabledState)
    {
        if (playerInputHandler == null) return;

        if (playerInputHandler != null)
            playerInputHandler.enabled = enabledState;

        if (humanJoystickTranslation != null)
            humanJoystickTranslation.enabled = enabledState;

        if (weaponsManager != null)
            weaponsManager.enabled = enabledState;
        //foreach (MonoBehaviour script in playerInputHandler)
        //{

        //}
    }

    private void PositionMenuInFrontOfPlayer()
    {
        if (pauseMenuRoot == null || xrCamera == null) return;

        Vector3 forward = xrCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPosition = xrCamera.position + forward * menuDistance;
        targetPosition.y = xrCamera.position.y + heightOffset;

        pauseMenuRoot.transform.position = targetPosition;

        Vector3 lookDir = pauseMenuRoot.transform.position - xrCamera.position;
        lookDir.y = 0f;

        pauseMenuRoot.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}