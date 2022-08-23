using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Input = UnityEngine.Input;

// Complete Script by Collin Gale
// Networking by Mirror, FizzySteamWorks

namespace Mirror.Examples.Chat
{
    public class Manager : NetworkBehaviour
    {
        [Header("UI Elements")]
        //public TMP_InputField usernameInput;
        public Button hostButton;
        public Button clientButton;
        //public Text errorText;
        public TMP_InputField chatMessage;
        public TMP_Text chatHistory;
        public Scrollbar scrollbar;
        
        public static Manager instance;
        public bool readSent; // Reads your own messages when sent
        public bool censor; // Removes incoming words that appear on blacklist
        public bool beep; // Replaces blacklisted words with a beep
        private string dtPath;
        private string[] blacklist;
        private string voice = "";
        private string volume = "[:volume set 100] ";
        private string rate = "[:rate 200] ";
        private string tone;
        public TMP_Text volumeNum;
        public GameObject beepBlocker;
        public GameObject videoPanel;
        private Slider videoTime;
        private YoutubePlayer.YoutubePlayer video;
        private Vector3 videoPosition = new Vector3(0, 0, 0);
        [SyncVar] private bool videoPlaying;
        private string[] videoHistory;
        //[SyncVar] private int videoNumber = 0;
        [Header("Diagnostic - Do Not Edit")]
        public string localPlayerName;
        Dictionary<NetworkConnectionToClient, string> connNames = new Dictionary<NetworkConnectionToClient, string>();
        
        void Awake()
        {
            instance = this;
            dtPath = Application.dataPath + "/StreamingAssets";
            blacklist = System.IO.File.ReadAllLines(dtPath + "/blacklist.txt");
            video = videoPanel.transform.GetChild(0).GetComponent<YoutubePlayer.YoutubePlayer>();
            videoTime = videoPanel.transform.GetChild(1).GetComponent<Slider>();
        }
        
        public void ToggleButtons(string username)
        {
            hostButton.interactable = !string.IsNullOrWhiteSpace(username);
            clientButton.interactable = !string.IsNullOrWhiteSpace(username);
        }

        void Update()
        {
            if (videoPlaying)
            {
                videoTime.value = (float)video.VideoPlayer.time;
                if (video.VideoPlayer.time >= video.VideoPlayer.length - 0.1f)
                {
                    videoPlaying = false;
                    videoPanel.transform.position = new Vector3(0,1000,0);
                }
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdSend(string message, NetworkConnectionToClient sender = null)
        {
            if (!connNames.ContainsKey(sender))
                connNames.Add(sender, sender.identity.GetComponent<Player>().playerName);

            if (!string.IsNullOrWhiteSpace(message))
                RpcReceive(connNames[sender], message.Trim());
        }

        [ClientRpc]
        public void RpcReceive(string playerName, string message)
        {
            if (censor)
            {
                foreach (string bad in blacklist)
                {
                    if (message.Contains(bad))
                    {
                        if (beep) message = message.Replace(bad, "[:tone 500,500]");
                        else message = message.Replace(bad, "");
                    }
                }
            }
            //if (message.Contains("youtube.com")) message = message.Replace("www.youtube.com", "unity-youtube-dl-server.herokuapp.com");
            if (message.StartsWith("http")) PlayVideo(message);
                else if (playerName != localPlayerName || readSent) Speak(message);
            string prettyMessage = playerName == localPlayerName ?
                $"<color=red>{playerName}:</color> {message}" :
                $"<color=blue>{playerName}:</color> {message}";
            AppendMessage(prettyMessage);
        }
        
        public void OnEndEdit(string input)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetButtonDown("Submit"))
                SendMessage();
        }
        
        public void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(chatMessage.text))
            {
                CmdSend(chatMessage.text.Trim()); //voice + rate + volume + 
                chatMessage.text = string.Empty;
                chatMessage.ActivateInputField();
            }
        }

        internal void AppendMessage(string message)
        {
            StartCoroutine(AppendAndScroll(message));
        }

        IEnumerator AppendAndScroll(string message)
        {
            chatHistory.text += message + "\n";
            yield return null;
            yield return null;
            scrollbar.value = 0;
        }
        
        public void Website(string url)
        {
            Application.OpenURL(url); //"http://"+
        }
        
        #region TTS
        public void Speak(string input)
        {
            print("SAYING " + input);
            var info = new ProcessStartInfo();
            info.FileName = dtPath + "/say.exe";
            info.WorkingDirectory = dtPath;
            info.Arguments = input;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            var process = Process.Start(info);
            //process.WaitForExit();
            process.Close();
        }

        public void SetVolume(Slider num)
        {
            volumeNum.text = num.value + "%";
            volume = "[:vol set " + num.value + "]";
            Speak(volume + "Bah");
        }
        
        public void SetRate(TMP_InputField num)
        {
            rate = "[:rate " + num.text + "]";
            Speak(rate + "Bah");
        }
        
        public void Read(bool value)
        {
            readSent = value;
        }
        
        public void Censor(bool value)
        {
            beepBlocker.SetActive(!value);
            censor = value;
        }
        
        public void Beep(bool value)
        {
            beep = value;
        }
        
        public void SetVoice(TMP_Dropdown selected)
        {
            switch (selected.value)
            {
                case 0: // Paul
                    voice = "";
                    Speak(voice + "Paul");
                    break;
                case 1: // Betty
                    voice = "[:nb] ";
                    Speak(voice + "Betty");
                    break;
                case 2: // Harry
                    voice = "[:nh] ";
                    Speak(voice + "Harry");
                    break;
                case 3: // Dennis
                    voice = "[:nd] ";
                    Speak(voice + "Dennis");
                    break;
                case 4: // Frank
                    voice = "[:nf] ";
                    Speak(voice + "Frank");
                    break;
                case 5: // Ursula
                    voice = "[:nu] ";
                    Speak(voice + "Ursula");
                    break;
                case 6: // Rita
                    voice = "[:nr] ";
                    Speak(voice + "Rita");
                    break;
                case 7: // Wendy
                    voice = "[:nw] ";
                    Speak(voice + "Wendy");
                    break;
                case 8: // Kit
                    voice = "[:nk] ";
                    Speak(voice + "Kit");
                    break;
                case 9: // Half-Life
                    voice = "[:dv ap 10] ";
                    Speak(voice + "Half-Life");
                    break;
                case 10: // Helium
                    voice = "[:dv hs 1] ";
                    Speak(voice + "Helium");
                    break;
                case 11: // Monotone
                    voice = "[:dv pr 1] ";
                    Speak(voice + "Monotone");
                    break;
            }
        }
        #endregion
        
        #region Videos
        public void PlayVideo(string url)
        {
            videoPanel.transform.position = videoPosition;
            video.youtubeUrl = url;
            video.PlayVideoAsync();
            videoPlaying = true;
            //videoHistory[videoNumber] = url;  //https://www.youtube.com/watch?v=CJv32cBiyEg
            //videoNumber++;
        }
        
        [Command(requiresAuthority = false)]
        public void PausePlay()
        {
            if (video.VideoPlayer.isPlaying) video.VideoPlayer.Pause();
            else video.VideoPlayer.Play();
        }
        #endregion
    }
}
