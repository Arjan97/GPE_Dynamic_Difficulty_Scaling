using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using TMPro;

[Serializable]
public class UsernameListWrapper
{
    public List<string> usernames;
}

public class UsernameManager : MonoBehaviour
{
    public static UsernameManager Instance { get; private set; }

    // Key used for storing the global usernames list.
    private const string GlobalUsernamesKey = "globalUsernames";

    // Maximum allowed length for a username.
    [SerializeField] private int maxUsernameLength = 10;

    // Holds the current text from the input field.
    private string typed_Username;

    // Regex to validate that the username consists of letters and numbers only.
    private Regex validUsernameRegex = new Regex("^[A-Za-z0-9]+$");

    // Name of the next scene to load after a successful registration.
    [SerializeField] private string nextSceneName = "MainGame";


    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private GameObject registerButton;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject changeNameButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        string savedUsername = PlayerPrefs.GetString("username", "");

        if (!string.IsNullOrEmpty(savedUsername))
        {
            // Disable username input and show play/change buttons
            usernameInputField.text = savedUsername;
            usernameInputField.interactable = false;
            registerButton.gameObject.SetActive(false);

            playButton.gameObject.SetActive(true);
            changeNameButton.gameObject.SetActive(true);
        }
        else
        {
            // First time player
            usernameInputField.text = "";
            usernameInputField.interactable = true;
            registerButton.gameObject.SetActive(true);

            playButton.gameObject.SetActive(false);
            changeNameButton.gameObject.SetActive(false);
        }
    }
    public void EnableUsernameEdit()
    {
        usernameInputField.interactable = true;
        registerButton.gameObject.SetActive(true);

        playButton.gameObject.SetActive(false);
        changeNameButton.gameObject.SetActive(false);
    }
    public void GoGamble()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Called from your TMP_InputField's OnValueChanged event.
    /// Validates the input and stores it.
    /// </summary>
    public void OnUsernameValueChanged(string newUsername)
    {
        newUsername = newUsername.Trim();

        if (newUsername.Length > maxUsernameLength)
        {
            Debug.Log("Username is too long. Maximum length is " + maxUsernameLength + " characters.");
            return;
        }

        if (!string.IsNullOrEmpty(newUsername) && !validUsernameRegex.IsMatch(newUsername))
        {
            Debug.Log("Username must contain only letters and numbers.");
            return;
        }

        // Save the in-progress username.
        typed_Username = newUsername;
        Debug.Log("Username is valid so far: " + typed_Username);
    }

    /// <summary>
    /// Called from the InputField's OnSubmit (or OnEndEdit) event.
    /// Validates the final username, checks uniqueness, and if valid, registers it.
    /// </summary>
    public async void TryRegisterUsername(string unusedParam)
    {
        // Use the stored username from OnValueChanged.
        string username = typed_Username.Trim();

        if (username.Length == 0 || username.Length > maxUsernameLength)
        {
            Debug.LogWarning($"Username must be between 1 and {maxUsernameLength} characters.");
            return;
        }

        if (!validUsernameRegex.IsMatch(username))
        {
            Debug.LogWarning("Username must contain only letters and numbers.");
            return;
        }

        // Load the current global usernames list from Cloud Save.
        List<string> usernames = await LoadUsernamesAsync();
        bool exists = usernames.Exists(s => s.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            Debug.LogWarning("The username is already taken. Please choose another one.");
            return;
        }

        // Add and save the updated global usernames list.
        usernames.Add(username);
        await SaveUsernamesAsync(usernames);
        Debug.Log("Username registered successfully: " + username);

        // Save the username locally for later retrieval.
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.Save();
        string playerName = username.Trim();
        await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);

        // Switch to the next scene.
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Loads the global usernames list from Cloud Save.
    /// Returns an empty list if none exists.
    /// </summary>
    public async Task<List<string>> LoadUsernamesAsync()
    {
        List<string> usernames = new List<string>();

        try
        {
            HashSet<string> keys = new HashSet<string> { GlobalUsernamesKey };
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (data.TryGetValue(GlobalUsernamesKey, out var item))
            {
                string csv = item.Value.ToString();
                usernames = new List<string>(csv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading usernames (CSV): " + e.Message);
        }

        return usernames;
    }
    /// <summary>
    /// Saves the global usernames list to Cloud Save.
    /// </summary>
    public async Task SaveUsernamesAsync(List<string> usernames)
    {
        string csv = string.Join(",", usernames);

        Dictionary<string, object> dataToSave = new Dictionary<string, object>
    {
        { GlobalUsernamesKey, csv }
    };

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log("Usernames saved as CSV to cloud.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving usernames: " + e.Message);
        }
    }
}
