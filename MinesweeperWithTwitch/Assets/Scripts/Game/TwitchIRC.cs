using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.IO;

public class TwitchIRC : MonoBehaviour {
	public string oauth;
	public string nickName;
	public string channelName;
	public class MsgEvent : UnityEngine.Events.UnityEvent<string> { }
	public MsgEvent messageReceivedEvent = new MsgEvent();

    // Scenes
    public Game game { set; get; }
    public Victory victory { set; get; }

    private string buffer = string.Empty;
    private string server = "irc.twitch.tv";
    private int port = 6667;
    private bool stopThreads = false;
	private Queue<string> commandQueue = new Queue<string>();
	private List<string> receivedMsgs = new List<string>();
	private System.Threading.Thread inProc, outProc;

    // ========================================================================
    // Keep it active between scenes
    private void Awake() {
        DontDestroyOnLoad(transform.gameObject);
    }

    // ========================================================================
    // Initialization of the IRC connexion to the Twitch chat
    private void StartIRC() {
		System.Net.Sockets.TcpClient sock = new System.Net.Sockets.TcpClient();
		var result = sock.BeginConnect(this.server, this.port, null, null);
		var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));

		if(!success) {
			throw new Exception("Failed to connect to " + this.channelName + " Twitch chat.");
		}

		sock.EndConnect(result);

		var networkStream = sock.GetStream();
		var input = new System.IO.StreamReader(networkStream);
		var output = new System.IO.StreamWriter(networkStream);

		// Send PASS & NICK
		output.WriteLine("PASS " + oauth);
		output.WriteLine("NICK " + nickName.ToLower());
		output.Flush();

		// output proc
		outProc = new System.Threading.Thread(() => IRCOutputProcedure(output));
		outProc.Start();

		// input proc
		inProc = new System.Threading.Thread(() => IRCInputProcedure(input, networkStream));
		inProc.Start();
	}

    // ========================================================================
    private void IRCInputProcedure(System.IO.TextReader input, System.Net.Sockets.NetworkStream networkStream) {
        while (!this.stopThreads) {
            if (!networkStream.DataAvailable) {
                Thread.Sleep(100); // Check every 100ms, prevent high CPU usage
                continue;
            }

            this.buffer = input.ReadLine();

            //was message?
            if (buffer.Contains("PRIVMSG #")) {
                lock (this.receivedMsgs) {
                    this.receivedMsgs.Add(this.buffer);
                }
            }

            //Send pong reply to any ping messages
            if (this.buffer.StartsWith("PING ")) {
                SendCommand(this.buffer.Replace("PING", "PONG"));
            }

            //After server sends 001 command, we can join a channel
            if (this.buffer.Split(' ')[1] == "001") {
                SendCommand("JOIN #" + this.channelName);
            }
        }
	}

    // ========================================================================
    private void IRCOutputProcedure(System.IO.TextWriter output) {
		System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        bool firstTime = true;
        while (!this.stopThreads) {
            // Do we have any commands to send?
            if (this.commandQueue.Count > 0 && (stopWatch.ElapsedMilliseconds > 1750 || firstTime)) {
                // https://github.com/justintv/Twitch-API/blob/master/IRC.md#command--message-limit 

                if(firstTime) {
                    firstTime = false;
                }

                // Send message
                output.WriteLine(this.commandQueue.Peek());
				output.Flush();

                // Remove message from queue
                this.commandQueue.Dequeue();

				// Restart stopwatch
				stopWatch.Reset();
				stopWatch.Start();
			} else {
                Thread.Sleep(900);
            }
		}
	}

    // ========================================================================
    public void SendCommand(string cmd) {
		commandQueue.Enqueue(cmd);
	}

    // ========================================================================
    public void SendMsg(string msg) {
		commandQueue.Enqueue("PRIVMSG #" + channelName + " :" + msg);
	}

    // ========================================================================
    void OnEnable() {
		stopThreads = false;
		StartIRC();
	}

    // ========================================================================
    void OnDisable() {
		stopThreads = true;
	}

    // ========================================================================
    void OnDestroy() {
		stopThreads = true;
	}

    // ========================================================================
    void Update() {
        lock (this.receivedMsgs) {
            if (this.receivedMsgs.Count > 0) {
                for (int i = 0; i < this.receivedMsgs.Count; i++) {
                    this.ChatMsgReceived(this.receivedMsgs[i]);
                }
                this.receivedMsgs.Clear();
            }
        }
    }

    // ========================================================================
    // When message is received from IRC-server or our own message.
    private void ChatMsgReceived(string msg) {
        // Temporary, for now there isn't any commands on the victory scene
        if(this.game == null) {
            return;
        }

        int msgIndex = msg.IndexOf("PRIVMSG #");
        string message = msg.Substring(msgIndex + this.channelName.Length + 11).ToLower();
        string user = msg.Substring(1, msg.IndexOf('!') - 1);
        this.ParseMessage(user, message);
    }

    // ========================================================================
    // Parse the user message
    private void ParseMessage(string user, string message) {
        string[] command = message.Split(' ');
        if (command.Length == 2) {
            if (command[1].Length == 2 || command[1].Length == 3) {
                // Parsing the coordinates
                int numberLength = command[1].Length == 2 ? 1 : 2;
                int letterCode = (int)(command[1][0]);
                int number;
                bool parsed = Int32.TryParse(command[1].Substring(1, numberLength), out number);

                // Parsing the command
                if (letterCode >= 97 && letterCode <= 122 && parsed && number >= 1) {
                    string order = string.Empty;
                    if (command[0] == "check" || command[0] == "c") {
                        order = "check";
                    } else if (command[0] == "flag" || command[0] == "f") {
                        order = "flag";
                    } else if (command[0] == "unflag" || command[0] == "uf") {
                        order = "unflag";
                    } else if (command[0] == "clear" || command[0] == "cl") {
                        order = "clear";
                    }
                    this.game.ChatCommand(user, order, number - 1, letterCode - 97);
                }
            }
        }
    }
}
