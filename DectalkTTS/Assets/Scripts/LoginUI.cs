using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Examples.Chat
{
    public class LoginUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_InputField usernameInput;
        public Button hostButton;
        public Button clientButton;
        public Text errorText;

        public static LoginUI instance;

        void Awake()
        {
            instance = this;
        }

        // Called by UI element UsernameInput.OnValueChanged
        public void ToggleButtons(string username)
        {
            hostButton.interactable = !string.IsNullOrWhiteSpace(username);
            clientButton.interactable = !string.IsNullOrWhiteSpace(username);
        }

        public void Website(string url)
        {
            Application.OpenURL("http://"+url);
        }
    }
}
