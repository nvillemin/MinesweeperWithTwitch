using UnityEngine;
using System.Collections.Generic;
using System;

public class TwitchIRC : MonoBehaviour {
	public string oauth;
	public string nickName;
	public string channelName;
	private string server = "irc.twitch.tv";
	private int port = 6667;

	public class MsgEvent : UnityEngine.Events.UnityEvent<string> { }
	public MsgEvent messageReceivedEvent = new MsgEvent();

	private string buffer = string.Empty;
	private bool stopThreads = false;
	private Queue<string> commandQueue = new Queue<string>();
	private List<string> receivedMsgs = new List<string>();
	private System.Threading.Thread inProc, outProc;

    // Initialization of the IRC connexion to the Twitch chat
	private void StartIRC() {
		System.Net.Sockets.TcpClient sock = new System.Net.Sockets.TcpClient();
		var result = sock.BeginConnect(this.server, this.port, null, null);
		var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

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

	private void IRCInputProcedure(System.IO.TextReader input, System.Net.Sockets.NetworkStream networkStream) {
		while(!this.stopThreads) {
			if(!networkStream.DataAvailable)
				continue;

            this.buffer = input.ReadLine();

			//was message?
			if(buffer.Contains("PRIVMSG #")) {
				lock (this.receivedMsgs) {
                    this.receivedMsgs.Add(this.buffer);
				}
			}

			//Send pong reply to any ping messages
			if(this.buffer.StartsWith("PING ")) {
				SendCommand(this.buffer.Replace("PING", "PONG"));
			}

			//After server sends 001 command, we can join a channel
			if(this.buffer.Split(' ')[1] == "001") {
				SendCommand("JOIN #" + this.channelName);
			}
		}
	}

	private void IRCOutputProcedure(System.IO.TextWriter output) {
		System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
		stopWatch.Start();
		while(!this.stopThreads) {
			lock (this.commandQueue) {
				if(this.commandQueue.Count > 0) //do we have any commands to send?
				{
					// https://github.com/justintv/Twitch-API/blob/master/IRC.md#command--message-limit 
					//have enough time passed since we last sent a message/command?
					if(stopWatch.ElapsedMilliseconds > 1750) {
						// send msg.
						output.WriteLine(this.commandQueue.Peek());
						output.Flush();

                        // remove msg from queue.
                        this.commandQueue.Dequeue();

						// restart stopwatch.
						stopWatch.Reset();
						stopWatch.Start();
					}
				}
			}
		}
	}

	public void SendCommand(string cmd) {
		lock (commandQueue) {
			commandQueue.Enqueue(cmd);
		}
	}

	public void SendMsg(string msg) {
		lock (commandQueue) {
			commandQueue.Enqueue("PRIVMSG #" + channelName + " :" + msg);
		}
	}

	void OnEnable() {
		stopThreads = false;
		StartIRC();
	}

	void OnDisable() {
		stopThreads = true;
	}

	void OnDestroy() {
		stopThreads = true;
	}

	void Update() {
        lock (receivedMsgs) {
            if (receivedMsgs.Count > 0) {
                for (int i = 0; i < receivedMsgs.Count; i++) {
                    messageReceivedEvent.Invoke(receivedMsgs[i]);
                }
                receivedMsgs.Clear();
            }
        }
    }
}
