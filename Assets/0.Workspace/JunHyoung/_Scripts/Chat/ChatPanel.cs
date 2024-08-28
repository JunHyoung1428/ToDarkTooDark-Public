using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Chat.Demo;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour, IChatClientListener
{
    [SerializeField] ChatEntry chatEntry;
    [SerializeField] Transform contents;

    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button buttonSend;

    ChatClient chatClient;
    string curChannelName;

    [Space(10), Header("System Message")]
    [SerializeField] string connectMsg = "Connect On Channel...";
    [SerializeField] string enteredMsg = "You Entered Room:";
    [SerializeField] string otherEnteredMsg = "was Entered Room";
    [SerializeField] string otherExitMsg = "was Exit Room";

    /******************************************************
    *                    Unity Events
    ******************************************************/
    #region Unity Events
    void Start()
    {
        PhotonPeer.RegisterType(typeof(ChatData), 100, ChatData.Serialize, ChatData.Deserialize);

        buttonSend.onClick.AddListener(SendMessage);
        inputField.onSubmit.AddListener(SendMessage);
    }

    void OnEnable()
    {
        chatClient = new ChatClient(this);
        chatClient.AuthValues = new AuthenticationValues(PhotonNetwork.LocalPlayer.NickName);
        ChatEntry newChat = Instantiate(chatEntry, contents);
        newChat.SetChat(new ChatData(" ", connectMsg, Color.black, Color.yellow));
        chatClient.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings()); // GetChatSetting는 Demo에 있는 확장 메서드(...)
        curChannelName = PhotonNetwork.CurrentRoom.Name; //채팅 채널을 로비-룸 상태랑 게임중일 때 상태랑 구분지어야 하는지 확인해 볼것.
    }

    void OnDisable()
    {
        if (chatClient == null)
            return;

        if (chatClient.PublicChannels.TryGetValue(curChannelName, out ChatChannel chatChannel))
        {

            chatChannel.ClearMessages();
        }
        chatClient.Disconnect();
    }

    void Update()
    {
        if(!inputField.isFocused && Input.GetKey(KeyCode.Return))
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
            inputField.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        if (chatClient == null)
            return;
        chatClient.Service();
    }

    #endregion

    /******************************************************
    *                    Methods
    ******************************************************/

    #region Methods
    private void SendMessage() // 버튼 클릭 시
    {
        if (string.IsNullOrEmpty(inputField.text))
            return;


        //chatClient.PublishMessage(curChannelName, inputField.text);
        chatClient.PublishMessage(curChannelName, new ChatData(PhotonNetwork.LocalPlayer.NickName, inputField.text));
        //Player의 color 커스텀 프로퍼티 구현할 것.
        inputField.text = "";
        inputField.ActivateInputField();
    }

    new void SendMessage(string message) // InputField 에서 Enter 시
    {
        if (string.IsNullOrEmpty(message))
            return;

        chatClient.PublishMessage(curChannelName, new ChatData(PhotonNetwork.LocalPlayer.NickName, inputField.text));
        inputField.text = "";
        inputField.ActivateInputField();
    }
    #endregion
    /******************************************************
    *              IChatClientListener Callbacks
    ******************************************************/

    #region Callbacks
    void IChatClientListener.DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(message);
    }

    void IChatClientListener.OnChatStateChange(ChatState state)
    {
        Debug.Log($"[ChatState : {state}]");
    }

    void IChatClientListener.OnConnected()
    {
        // PublishSubscribers = true 가 아니면 OnuserSubscribe 가 콜백되지 않음
        chatClient.Subscribe(curChannelName, 0, 0, //-1 이면 이전 모든 메세지 수신, 0이면 수신안함, 1보다 크면 그만큼만 수신
            new ChannelCreationOptions() { PublishSubscribers = true, MaxSubscribers = PhotonNetwork.CurrentRoom.MaxPlayers });
    }

    void IChatClientListener.OnDisconnected()
    {
        Debug.Log("Chat Disconnected");
    }

    void IChatClientListener.OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        //chatClient.Service(); 가 이거 호출함
        if (channelName.Equals(curChannelName))
        {
            for (int i = 0; i < senders.Length; i++)
            {
                if (senders[i] == null)
                    return;
                ChatEntry newChat = Instantiate(chatEntry, contents);
                //newChat.Set(senders[i], (string)messages[i]);
                ChatData chatData = (ChatData) messages[i];
                Debug.Log($"{chatData.name}, {chatData.message}");
                newChat.SetChat(chatData);
            }
        }
    }

    void IChatClientListener.OnPrivateMessage(string sender, object message, string channelName)
    {
        //개인 메세지 수신
        throw new System.NotImplementedException();
    }

    void IChatClientListener.OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    void IChatClientListener.OnSubscribed(string[] channels, bool[] results)
    {
        //내가 채널 입장시
        ChatEntry newChat = Instantiate(chatEntry, contents);
        newChat.SetChat(new ChatData(" ", $"{enteredMsg} : {curChannelName}", Color.black, Color.green));
    }

    void IChatClientListener.OnUnsubscribed(string[] channels)
    {
        //내가 채널에서 퇴장할 시
        //ChatChannel.ClearMessages();
    }

    void IChatClientListener.OnUserSubscribed(string channel, string user)
    {
        ChatEntry newChat = Instantiate(chatEntry, contents);
        //유저가 채널에 입장 할 시
        newChat.SetChat(new ChatData(" ", $"{user} {otherEnteredMsg}", Color.black, Color.blue));
    }

    void IChatClientListener.OnUserUnsubscribed(string channel, string user)
    {
        // 유저가 채널 퇴장시
        ChatEntry newChat = Instantiate(chatEntry, contents);
        newChat.SetChat(new ChatData(" ", $"{user} {otherExitMsg}", Color.black, Color.red));
    }
    #endregion
}
