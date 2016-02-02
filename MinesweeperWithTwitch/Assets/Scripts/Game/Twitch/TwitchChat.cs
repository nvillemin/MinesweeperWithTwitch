using System;
using UnityEngine;

[RequireComponent(typeof(TwitchIRC))]
public class TwitchChat : MonoBehaviour {
	private Game game;
	private TwitchIRC IRC;

	// Twitch chat initialization
	private void Start () {
		this.game = this.GetComponentInParent<Game>();
		this.IRC = this.GetComponent<TwitchIRC>();
		this.IRC.messageReceivedEvent.AddListener(this.OnChatMsgReceived);
    }

    // When message is received from IRC-server or our own message.
    private void OnChatMsgReceived(string msg) {
        int msgIndex = msg.IndexOf("PRIVMSG #");
		string message = msg.Substring(msgIndex + IRC.channelName.Length + 11);
		string user = msg.Substring(1, msg.IndexOf('!') - 1);
        this.ParseMessage(user, message);
    }

    // Parse the user message
    private void ParseMessage(string user, string message) {
        int messageLength = message.Length;
        if (message.StartsWith("check ") && messageLength == 8 || messageLength == 9) {
            int numberLength = messageLength == 8 ? 1 : 2;
            int letterCode = (int)(char.ToLower(message[6]));
            int number;
            bool parsed = Int32.TryParse(message.Substring(7, numberLength), out number);

            if (letterCode >= 97 && letterCode <= 122 && parsed && number >= 1) {
                this.game.ChatCommand(user, "check", number - 1, letterCode - 97);
            }
        }
        // Change this to a function
        if (message.StartsWith("flag ") && messageLength == 7 || messageLength == 8) {
            int numberLength = messageLength == 7 ? 1 : 2;
            int letterCode = (int)(char.ToLower(message[5]));
            int number;
            bool parsed = Int32.TryParse(message.Substring(6, numberLength), out number);

            if (letterCode >= 97 && letterCode <= 122 && parsed && number >= 1) {
                this.game.ChatCommand(user, "flag", number - 1, letterCode - 97);
            }
        }
    }
}
