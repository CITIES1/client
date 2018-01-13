using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;

public class ChatData
{
    public string id;
    public string msg;
};
public class UserData
{
    public string id;
    public int current_life;
    public int max_life;
    public string character;
};

public class Controller : MonoBehaviour
{
    public string serverURL = "http://192.168.1.34:4444";

    public InputField uiInput = null;
    public Button uiSend = null;
    public Text uiChatLog = null;

    protected Socket socket = null;
    protected List<string> chatLog = new List<string>();
    
    public List<UserData> users = new List<UserData>();
    public UserData myUser;
    
    void Destroy()
    {
        DoClose();
    }

    // Use this for initialization
    void Start()
    {

        Debug.Log("Start()");
        DoOpen();
        
    }

    // Update is called once per frame
    void Update()
    {
        /*lock (chatLog)
        {
            if (chatLog.Count > 0)
            {
                string str = uiChatLog.text;
                foreach (var s in chatLog)
                {
                    str = str + "\n" + s;
                }
                uiChatLog.text = str;
                chatLog.Clear();
            }
        }*/
        Debug.Log(users[0].id);
    }

    void DoOpen()
    {
        Debug.Log("DoOpen");
        if (socket == null)
        {
            socket = IO.Socket(serverURL);
            Debug.Log(socket);
            socket.On(Socket.EVENT_CONNECT, () => {
                Debug.Log("connected!");
                /*lock (chatLog)
                {
                    // Access to Unity UI is not allowed in a background thread, so let's put into a shared variable
                    chatLog.Add("Socket.IO connected.");
                }*/
            });
            socket.On("user", (data) => {
                string str = data.ToString();
                UserData receivedUser = JsonConvert.DeserializeObject<UserData>(str);
                Debug.Log("AAAAA");
                Debug.Log(str);
                Debug.Log(receivedUser);
                myUser = receivedUser;
            });
            socket.On("users_update", (data) => {
                string str = data.ToString();
                List<UserData> updateUsers = JsonConvert.DeserializeObject < List<UserData>>(str);
                Debug.Log("AAAAA");
                Debug.Log(str);
                Debug.Log(updateUsers);
                users = updateUsers;
            });
            socket.On("chat", (data) => {
                string str = data.ToString();

                ChatData chat = JsonConvert.DeserializeObject<ChatData>(str);
                string strChatLog = "user#" + chat.id + ": " + chat.msg;

                // Access to Unity UI is not allowed in a background thread, so let's put into a shared variable
                lock (chatLog)
                {
                    chatLog.Add(strChatLog);
                }
            });
        }
    }

    void DoClose()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket = null;
        }
    }

    void SendChat(string str)
    {
        if (socket != null)
        {
            socket.Emit("chat", str);
        }
    }
}