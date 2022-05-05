using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject softScreen,mainMenuScreen, playScreen, statsScreen, settingsScreen, rulesScreen, creditsScreen;
    [SerializeField] private TextMeshProUGUI winText, loseText;
    [SerializeField] private Button aiVsAiButton;
    [SerializeField] private TMP_Dropdown changeResolution;

    private const string WINS = "Wins", LOSS = "Losses";
    
    Resolution[] resolutions; // Stores all possible resolutions that the current monitor you are playing on supports!

    private void Awake()
    {
        softScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
        playScreen.SetActive(false);
        statsScreen.SetActive(false);
        rulesScreen.SetActive(false);
        creditsScreen.SetActive(false);
        settingsScreen.SetActive(false);

        // Will comment the following out once ai vs ai mode is complete
        aiVsAiButton.interactable = false;

        // Text = saved win score
        SetStatText();

        // And wherever wins / losses are handled (probably Game Manager), update the value
    }

    private void Start()
    {
        resolutions = Screen.resolutions;
        changeResolution.ClearOptions(); // clean up for new screen

        List<string> options = new List<string>();

       int currentResIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "hz";
            
            options.Add(option);

           if (resolutions[i].width == Screen.width)
            {
                if (resolutions[i].height == Screen.height)
                {
                    currentResIndex = i;
                    Debug.Log(i);
                    Debug.Log(resolutions[i].width + " x " + resolutions[i].height);
                }
            }
        }

        changeResolution.AddOptions(options);
        changeResolution.value = currentResIndex;
        changeResolution.RefreshShownValue();

    }


    private void SetStatText()
    {
        if (PlayerPrefs.HasKey(WINS))
        {
            winText.text = PlayerPrefs.GetString(WINS);
        }
        else
        {
            winText.text = "000";
        }

        if (PlayerPrefs.HasKey(LOSS))
        {
            loseText.text = PlayerPrefs.GetString(LOSS);
        }
        else
        {
            loseText.text = "000";
        }
    }

    // Update is called once per frame
    void Update()
    { 
        if (Input.anyKey && softScreen.activeSelf)
        {
            softScreen.SetActive(false);
            mainMenuScreen.SetActive(true);
        }
        if (statsScreen.activeSelf)
        {
            SetStatText();
        }
    }
    public void UI_PlayMode()
    {
        playScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
        SFXController.PlaySoundMenuButton();
    }

    public void UI_HumanMode()
    {
        Debug.Log("Human vs AI Play Mode");
        // Start game in Human Mode
        GameManager.Instance.UpdateGameState(GameState.PlayerTurn);
        SceneManager.LoadScene(1);
    }

    public void UI_AIMode()
    {
        Debug.Log("AI vs AI Play Mode");
        // Start game in AI Mode
    }

    public void UI_Back(GameObject currentScreen)
    {
        mainMenuScreen.SetActive(true);
        currentScreen.SetActive(false);
        SFXController.PlaySoundMenuButton();
    }

    public void UI_StatsMode()
    {
        statsScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
        SFXController.PlaySoundMenuButton();
    }

    public void UI_ResetScore()
    {
        Debug.Log("Reset Player Score");
        // Reset Player Score

        // Reset wins
        // Reset losses
        winText.text = "000";
        loseText.text = "000";

        PlayerPrefs.SetString(WINS, "000");
        PlayerPrefs.SetString(LOSS, "000");
        SFXController.PlaySoundMenuButton();
    }

    public void UI_SettingsMode()
    {
        settingsScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
        SFXController.PlaySoundMenuButton();
    }

    public void UI_AgressiveAI()
    {
        // Modify AI script values here
        Debug.Log("AI set to attack more often.");
        SFXController.PlaySoundMenuButton();
    }

    public void UI_DefensiveAI()
    {
        // Modify AI script values here
        Debug.Log("AI set to protect itself.");
        SFXController.PlaySoundMenuButton();
    }

    public void UI_BalancedAI()
    {
        // Modify AI script values here
        Debug.Log("AI set to default profile.");
        SFXController.PlaySoundMenuButton();
    }

    public void UI_Animations()
    {
        //Piece.animationsEnabled edit somehow
        SFXController.PlaySoundMenuButton();
    }

    public void UI_CameraSpin()
    {
        //edit here
        SFXController.PlaySoundMenuButton();
    }
    
    public void UI_SetResolution(int resIndex)
    {
        Debug.Log("change res to " + resIndex);
        Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void UI_RulesMode()
    {
        rulesScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
        SFXController.PlaySoundMenuButton();
    }
    
    public void UI_CreditsMode()
    {
        creditsScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
        SFXController.PlaySoundMenuButton();
    }

    public void UI_Exit()
    {
        Debug.Log("Quitting Application");
        Application.Quit();
    }
}
