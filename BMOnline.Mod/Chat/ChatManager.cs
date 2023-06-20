using System;
using System.Collections.Generic;
using BMOnline.Mod.Patches;
using Flash2;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod.Chat
{
    internal class ChatManager
    {
        private const int MAX_MESSAGES = 50;

        class GetCharacterBehaviour : MonoBehaviour
        {
            public GetCharacterBehaviour(IntPtr handle) : base(handle) { }

            public void OnGUI()
            {
                Event e = Event.current;
                OnGUIEvent?.Invoke(this, new GUIEventArgs(e));
            }
        }

        class GUIEventArgs : EventArgs
        {
            public GUIEventArgs(Event guiEvent)
            {
                GuiEvent = guiEvent;
            }

            public Event GuiEvent { get; }
        }
        private static event EventHandler<GUIEventArgs> OnGUIEvent;

        private readonly ModSettings settings;

        private readonly GameObject root;

        private readonly GameObject inputContainer;
        private readonly GameObject input;
        private readonly Text inputText;
        private readonly RectTransform cursor;
        private readonly Text testText;

        private readonly RectTransform messageListMask;
        private readonly Transform messageContainer;
        private readonly List<ChatMessage> messageList = new List<ChatMessage>(MAX_MESSAGES);

        private int cursorPosition = 0;
        private bool isClosing = false;
        private float cursorBlinkTimer = 0;
        private readonly Queue<char> inputQueue = new Queue<char>();
        private readonly SpamTracker backspaceTracker = new SpamTracker(KeyCode.Backspace);
        private readonly SpamTracker deleteTracker = new SpamTracker(KeyCode.Delete);
        private readonly SpamTracker rightTracker = new SpamTracker(KeyCode.RightArrow);
        private readonly SpamTracker leftTracker = new SpamTracker(KeyCode.LeftArrow);

        public ChatManager(ModSettings settings)
        {
            this.settings = settings;

            root = UnityEngine.Object.Instantiate(AssetBundleItems.ChatPrefab, AppSystemUI.Instance.transform.Find("UIList_GUI_Front").transform.Find("c_system_0").Find("safe_area"));
            ClassInjector.RegisterTypeInIl2Cpp<GetCharacterBehaviour>();
            root.AddComponent<GetCharacterBehaviour>();

            inputContainer = root.transform.Find("Background").gameObject;
            input = inputContainer.transform.Find("Input").gameObject;
            inputText = input.GetComponent<Text>();
            cursor = input.transform.Find("Cursor").GetComponent<RectTransform>();
            testText = input.transform.Find("TestText").GetComponent<Text>();

            messageListMask = root.transform.Find("MessageListMask").GetComponent<RectTransform>();
            messageContainer = messageListMask.Find("MessageList");

            OnGUIEvent += (s, eventArgs) =>
            {
                if (!IsOpen) return;
                Event e = eventArgs.GuiEvent;
                if (e.isKey && e.character != 0)
                {
                    inputQueue.Enqueue(e.character);
                }
            };

            settings.OnSettingChanged += (s, e) =>
            {
                if (e.SettingChanged == ModSettings.Setting.EnableChat)
                {
                    root.SetActive(settings.EnableChat);
                    if (!settings.EnableChat)
                        Close();
                }
            };
        }

        public bool IsOpen { get; private set; } = false;

        private ushort maxChatLength = 0;
        public ushort MaxChatLength
        {
            get => maxChatLength;
            set
            {
                maxChatLength = value;
                if (inputText.text.Length > MaxChatLength)
                {
                    inputText.text = inputText.text.Substring(0, MaxChatLength);
                    cursorPosition = Math.Min(cursorPosition, MaxChatLength);
                    RepositionCursor();
                    RepositionMessageList();
                }
            }
        }

        private void UpdateChatMessages() => messageList.ForEach(m => m.Update(IsOpen));

        public void AddChatMessage(string message)
        {
            if (messageList.Count == MAX_MESSAGES)
            {
                messageList[0].Destroy();
                messageList.RemoveAt(0);
            }
            ChatMessage newMessage = new ChatMessage(message, messageContainer);
            messageList.Add(newMessage);
        }

        private List<string> CalculateLines()
        {
            List<string> lines = new List<string>();
            testText.text = string.Empty;
            string currentWord = string.Empty;
            float testTextWidth = 0;
            bool wasPreviousWhitespace = false;
            for (int i = 0; i < inputText.text.Length; i++)
            {
                char c = inputText.text[i];
                testText.text += c;
                if (!char.IsWhiteSpace(c) || !wasPreviousWhitespace)
                    testTextWidth = testText.preferredWidth;
                wasPreviousWhitespace = char.IsWhiteSpace(c);
                if (char.IsWhiteSpace(c))
                    currentWord = string.Empty;
                else
                    currentWord += c;
                //Handle line wrapping
                if (testTextWidth > inputText.rectTransform.sizeDelta.x)
                {
                    string fullLine = testText.text;
                    string line;
                    //If it still overflows, just move the current char over
                    testText.text = currentWord;
                    if (testText.preferredWidth > inputText.rectTransform.sizeDelta.x)
                    {
                        currentWord = c.ToString();
                        testText.text = currentWord;
                        line = fullLine.Substring(0, fullLine.Length - 1);
                    }
                    else
                    {
                        line = fullLine.Substring(0, fullLine.Length - currentWord.Length);
                    }
                    lines.Add(line);
                }
            }
            if (testText.text.Length > 0)
                lines.Add(testText.text);
            return lines;
        }

        private void RepositionCursor()
        {
            List<string> lines = CalculateLines();
            float cursorX = 0;
            float cursorY = 0;
            if (lines.Count > 0)
            {
                int cursorLine = 0;
                int lengthLinesBefore = 0;
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    if (lengthLinesBefore + line.Length <= cursorPosition && i + 1 < lines.Count)
                    {
                        lengthLinesBefore += line.Length;
                        cursorLine++;
                    }
                    else
                        break;
                }
                testText.text = lines[cursorLine].Substring(0, cursorPosition - lengthLinesBefore);
                cursorX = testText.preferredWidth;
                cursorY = (inputText.fontSize + inputText.lineSpacing) * (lines.Count - cursorLine - 1);
            }
            cursor.localPosition = new Vector3(cursorX, cursorY);
        }

        private void RepositionMessageList()
        {
            if (!IsOpen)
            {
                messageListMask.offsetMin = new Vector2(messageListMask.offsetMin.x, 50);
            }
            else
            {
                messageListMask.offsetMin = new Vector2(messageListMask.offsetMin.x, (50 - inputText.fontSize - inputText.lineSpacing) + ((inputText.fontSize + inputText.lineSpacing) * Math.Max(CalculateLines().Count, 1.0f)));
            }
        }

        private void Open()
        {
            IsOpen = true;
            AppInputPatch.PreventKeyboardUpdate = true;
            inputContainer.SetActive(true);
            inputQueue.Clear();
            RepositionMessageList();
        }

        private void Close()
        {
            IsOpen = false;
            isClosing = true;
            inputContainer.SetActive(false);
            inputText.text = string.Empty;
            cursorPosition = 0;
            RepositionCursor();
            RepositionMessageList();
        }

        public string UpdateAndGetSubmittedChat()
        {
            UpdateChatMessages();

            if (isClosing)
            {
                if (!Input.GetKey(KeyCode.Escape) && !Input.GetKey(KeyCode.Return) && !Input.GetKey(KeyCode.KeypadEnter))
                {
                    isClosing = false;
                    AppInputPatch.PreventKeyboardUpdate = false;
                }
                return null;
            }

            if (!settings.EnableChat)
                return null;

            //Open if t pressed
            if (!IsOpen)
            {
                if (Input.GetKeyDown(KeyCode.T))
                    Open();
                else return null;
            }

            //Close and maybe submit
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                string submittedMessage = null;
                if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !string.IsNullOrWhiteSpace(inputText.text))
                    submittedMessage = inputText.text;
                Close();
                return submittedMessage;
            }

            //Cursor blink animation
            cursorBlinkTimer += Time.unscaledDeltaTime;
            if (cursorBlinkTimer >= 0.6f)
            {
                cursor.gameObject.SetActive(!cursor.gameObject.activeSelf);
                cursorBlinkTimer = Mathf.Min(cursorBlinkTimer - 0.6f, 0.6f);
            }

            //Add typed characters
            while (inputQueue.Count > 0)
            {
                char character = inputQueue.Dequeue();
                if (character == '\t' || inputText.text.Length >= MaxChatLength)
                    continue;
                inputText.text = inputText.text.Substring(0, cursorPosition) + character + inputText.text.Substring(cursorPosition, inputText.text.Length - cursorPosition);
                cursorPosition++;
                RepositionCursor();
                RepositionMessageList();
            }

            //Backspace
            if (backspaceTracker.UpdateAndGetState() && cursorPosition > 0)
            {
                inputText.text = inputText.text.Substring(0, cursorPosition - 1) + inputText.text.Substring(cursorPosition, inputText.text.Length - cursorPosition);
                cursorPosition--;
                RepositionCursor();
                RepositionMessageList();
            }

            //Delete
            if (deleteTracker.UpdateAndGetState() && cursorPosition < inputText.text.Length)
            {
                inputText.text = inputText.text.Substring(0, cursorPosition) + inputText.text.Substring(cursorPosition + 1, inputText.text.Length - cursorPosition - 1);
                RepositionCursor();
                RepositionMessageList();
            }

            //Right
            if (rightTracker.UpdateAndGetState() && cursorPosition < inputText.text.Length)
            {
                cursorPosition++;
                RepositionCursor();
            }

            //Left
            if (leftTracker.UpdateAndGetState() && cursorPosition > 0)
            {
                cursorPosition--;
                RepositionCursor();
            }

            //Home
            if (Input.GetKeyDown(KeyCode.Home))
            {
                cursorPosition = 0;
                RepositionCursor();
            }

            //End
            if (Input.GetKeyDown(KeyCode.End))
            {
                cursorPosition = inputText.text.Length;
                RepositionCursor();
            }

            return null;
        }
    }
}
