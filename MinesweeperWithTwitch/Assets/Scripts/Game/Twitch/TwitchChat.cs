using System;
using UnityEngine;

[RequireComponent(typeof(TwitchIRC))]
public class TwitchChat : MonoBehaviour {
	private Game game;
	private TwitchIRC IRC;

    // ========================================================================
    // Twitch chat initialization
    private void Start () {
		this.game = this.GetComponentInParent<Game>();
		this.IRC = this.GetComponent<TwitchIRC>();
		this.IRC.messageReceivedEvent.AddListener(this.OnChatMsgReceived);
    }

    // ========================================================================
    // When message is received from IRC-server or our own message.
    private void OnChatMsgReceived(string msg) {
        int msgIndex = msg.IndexOf("PRIVMSG #");
		string message = msg.Substring(msgIndex + IRC.channelName.Length + 11).ToLower();
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
                    } else if(command[0] == "clear" || command[0] == "cl") {
						order = "clear";
					}
					this.game.ChatCommand(user, order, number - 1, letterCode - 97);
                }
            }
        }
    }
}
