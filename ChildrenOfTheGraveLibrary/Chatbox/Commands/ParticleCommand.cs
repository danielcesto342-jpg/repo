using ChildrenOfTheGrave.ChildrenOfTheGraveServer.API;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands
{
    public class ParticleCommand : ChatCommandBase
    {
        public override string Command => "particle";
        public override string Syntax => $"{Command} <particle_name.troy>";

        public ParticleCommand(ChatCommandManager chatCommandManager) : base(chatCommandManager)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatManager.SyntaxError();
                ShowSyntax();
            }
            else
            {
                var ch = Game.PlayerManager.GetPeerInfo(userId).Champion;
                ApiFunctionManager.AddParticleTarget(ch, split[1], ch);
            }
        }
    }
}
