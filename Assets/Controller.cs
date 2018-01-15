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
public struct UserData
{
    public string id;
    public int current_life;
    public int max_life;
    public string character;
};
public struct AttackData
{
    public string from;
    public string to;
    public int damage;
};


public class Controller : MonoBehaviour
{
    public string serverURL = "http://192.168.1.36:4444";

    public InputField uiInput = null;
    public Button uiSend = null;
    public Text uiChatLog = null;

    protected Socket socket = null;
    protected List<string> chatLog = new List<string>();
    
    public List<UserData> users = new List<UserData>();

    public static UserData myUser;
    public static UserData opponent;

    private Text userTxt;
    private Text opponentTxt;

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
    public float speed = 0.1F;
    void Update()
    {
        //update the myUser.current_life in the text view
        userTxt = GameObject.Find("LifeTxt").GetComponent<Text>();
        userTxt.text = Controller.myUser.current_life.ToString();

        //update opponent.current_life
        opponentTxt = GameObject.Find("OpponentLifeTxt").GetComponent<Text>();
        opponentTxt.text = Controller.opponent.current_life.ToString();

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            // Move object across XY plane
            transform.Translate(-touchDeltaPosition.x * speed, -touchDeltaPosition.y * speed, 0);
        }
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
                Controller.myUser.id = receivedUser.id;
                Controller.myUser.current_life = receivedUser.current_life;
                Controller.myUser.max_life = receivedUser.max_life;
                Debug.Log(Controller.myUser);
            });
            socket.On("users_update", (data) => {
                string str = data.ToString();
                List<UserData> updateUsers = JsonConvert.DeserializeObject < List<UserData>>(str);
                Debug.Log("AAAAA");
                Debug.Log(str);
                Debug.Log(updateUsers);
                users = updateUsers;
                foreach (UserData user in users)
                {
                    if(user.id == Controller.myUser.id)
                    {
                        Controller.myUser = user;
                    } else
                    {
                        Controller.opponent = user;
                    }
                }
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
    public void Attack(string id)
    {
        Debug.Log("attack:");
        Debug.Log(id);
        Debug.Log(Controller.myUser.id);
        AttackData attack;
        attack.from = Controller.myUser.id;

        attack.to = Controller.opponent.id;
        attack.damage = 5;
        
        string attackStr = JsonConvert.SerializeObject(attack);
        Debug.Log(attackStr);
        socket.Emit("attack", attackStr);
    }
}