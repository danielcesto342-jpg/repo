using ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChildrenOfTheGrave.ChildrenOfTheGraveServer.Chatbox
{
    /// <summary>
    /// Manages chat commands for the game server.
    /// </summary>
    public class ChatCommandManager
    {
        public string CommandStarterCharacter = "!";

        private readonly SortedDictionary<string, ChatCommandBase> _chatCommandsDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatCommandManager"/> class.
        /// </summary>
        /// <param name="cheatsEnabled">Indicates if cheat commands should be loaded.</param>
        public ChatCommandManager(bool cheatsEnabled)
        {
            _chatCommandsDictionary = cheatsEnabled
                ? GetAllChatCommandHandlers(new[] { ServerLibAssemblyDefiningType.Assembly })
                : new SortedDictionary<string, ChatCommandBase>();
            if (!cheatsEnabled)
            {
                var suicide = new SuicideCommand(this);
                AddCommand(suicide);
            }
        }

        /// <summary>
        /// Retrieves all chat command handlers from the specified assemblies.
        /// </summary>
        /// <param name="loadFromArray">Array of assemblies to load command handlers from.</param>
        /// <returns>A sorted dictionary of chat commands.</returns>
        internal SortedDictionary<string, ChatCommandBase> GetAllChatCommandHandlers(Assembly[] loadFromArray)
        {
            var commandsOutput = new SortedDictionary<string, ChatCommandBase>();
            var args = new object[] { this };

            foreach (var loadFrom in loadFromArray)
            {
                var commands = loadFrom.GetTypes()
                    .Where(t => t.BaseType == typeof(ChatCommandBase))
                    .Select(t => Activator.CreateInstance(t, args) as ChatCommandBase);

                foreach (var command in commands)
                {
                    if (command != null)
                    {
                        commandsOutput[command.Command] = command;
                    }
                }
            }

            return commandsOutput;
        }

        /// <summary>
        /// Adds a new chat command to the manager.
        /// </summary>
        /// <param name="command">The command to add.</param>
        /// <returns>True if the command was added successfully; otherwise, false.</returns>
        public bool AddCommand(ChatCommandBase command)
        {
            if (_chatCommandsDictionary.ContainsKey(command.Command))
            {
                return false;
            }

            _chatCommandsDictionary[command.Command] = command;
            return true;
        }

        /// <summary>
        /// Removes a chat command from the manager.
        /// </summary>
        /// <param name="command">The command to remove.</param>
        /// <returns>True if the command was removed successfully; otherwise, false.</returns>
        public bool RemoveCommand(ChatCommandBase command)
        {
            return RemoveCommand(command.Command);
        }

        /// <summary>
        /// Removes a chat command by its command string.
        /// </summary>
        /// <param name="commandString">The command string to remove.</param>
        /// <returns>True if the command was removed successfully; otherwise, false.</returns>
        public bool RemoveCommand(string commandString)
        {
            return _chatCommandsDictionary.Remove(commandString);
        }

        /// <summary>
        /// Retrieves all chat commands.
        /// </summary>
        /// <returns>A list of all chat commands.</returns>
        public List<ChatCommandBase> GetCommands()
        {
            return _chatCommandsDictionary.Values.ToList();
        }

        /// <summary>
        /// Retrieves all chat command strings.
        /// </summary>
        /// <returns>A list of all chat command strings.</returns>
        public List<string> GetCommandsStrings()
        {
            return _chatCommandsDictionary.Keys.ToList();
        }

        /// <summary>
        /// Retrieves a chat command by its command string.
        /// </summary>
        /// <param name="commandString">The command string to retrieve.</param>
        /// <returns>The chat command associated with the specified string, or null if not found.</returns>
        public ChatCommandBase? GetCommand(string commandString)
        {
            _chatCommandsDictionary.TryGetValue(commandString, out var command);
            return command;
        }

        /// <summary>
        /// Updates all chat commands.
        /// </summary>
        internal void Update()
        {
            foreach (var command in _chatCommandsDictionary.Values)
            {
                command.Update();
            }
        }
    }
}