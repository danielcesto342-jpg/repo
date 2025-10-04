using System;
using System.Collections.Generic;
using static PacketVersioning.PktVersioning;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class PacketCommand : ChatCommandBase
    {
        public override string Command => "packet";
        public override string Syntax => $"{Command} XX XX XX...";

        public PacketCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            try
            {
                var s = arguments.Split(' ');
                if (s.Length < 2)
                {
                    ChatManager.SyntaxError();
                    ShowSyntax();
                    return;
                }
                List<byte> _bytes = new();
                _bytes.Add(Convert.ToByte(s[1], 16));

                for (var i = 2; i < s.Length; i++)
                {
                    if (s[i].Equals("netid"))
                    {
                        _bytes.Add(Convert.ToByte(Game.PlayerManager.GetPeerInfo(userId).Champion.NetId));
                    }
                    else
                    {
                        _bytes.Add(Convert.ToByte(s[i], 16));
                    }
                }

                DebugPacketNotify(userId, _bytes.ToArray());
            }
            catch
            {
                // ignored
            }
        }
    }
}
