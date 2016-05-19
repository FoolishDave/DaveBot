using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using Discord;
using Discord.Commands.Permissions.Levels;
using System.Windows.Forms;
using System.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace DaveBot.Modules.General
{
    internal class GeneralModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private List<string[]> quotes = new List<string[]>();

        public static IEnumerable<Role> GetOtherRoles(User user)
            => user.Roles.Where(x => x.Name.ToLowerInvariant().Contains("admin"));

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            LoadExternal();

            _manager.CreateCommands("", group =>
           {
               group.PublicOnly();
               group.CreateCommand("Quote")
                    .Description("Quote people.")
                    .MinPermissions((int)PermissionLevel.User)
                    .Parameter("Person")
                    .Parameter("Quote", Discord.Commands.ParameterType.Multiple)
                    .Do(e =>
                    {
                        string[] quoteArray = new string[e.Args.Count() - 1];
                        Array.ConstrainedCopy(e.Args, 1, quoteArray, 0, e.Args.Count() - 1);
                        string quote = String.Join(" ", quoteArray);

                        quotes.Add(new string[] { e.Args[0], quote });
                        if (!System.IO.File.Exists(@"..\data\quotes.txt"))
                            System.IO.File.WriteAllText(@"..\data\quotes.txt", e.GetArg("Person") + "|" + quote + "\n");
                        else
                            System.IO.File.AppendAllText(@"..\data\quotes.txt", e.GetArg("Person") + "|" + quote + "\n");
                    });
               group.CreateCommand("GetQuote")
                    .Description("Get a random quote.")
                    .MinPermissions((int)PermissionLevel.User)
                    .Do(async e =>
                    {
                        Random ran = new Random();
                        string[] quote = quotes[ran.Next(0, quotes.Count)];
                        await e.Channel.SendMessage("\"" + quote[1] + "\"" + "\n" + "**" + quote[0] + "**");
                    });
               group.CreateCommand("Name")
                       .Description("Sets the name of the bot.")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Parameter("New Name", Discord.Commands.ParameterType.Multiple)
                       .Do(e =>
                       {
                           _client.CurrentUser.Edit(username: String.Join(" ", e.Args));
                       });
               group.CreateCommand("Playing")
                       .Description("Sets what the bot is playing.")
                       .MinPermissions((int)PermissionLevel.ServerOwner)
                       .Parameter("Game", Discord.Commands.ParameterType.Multiple)
                       .Do(e =>
                       {
                           string game = (string.Join(" ", e.Args));
                           _client.Log.Debug("Setting game as: " + game, null);
                           _client.SetGame(game);
                       });
               group.CreateCommand("Echo")
                   .Description("Echos message.")
                   .MinPermissions((int)PermissionLevel.ServerAdmin)
                   .Parameter("Message", Discord.Commands.ParameterType.Multiple)
                   .Do(async e =>
                   {
                       string messageText = String.Join(" ", e.Args);
                       await e.Message.Delete();
                       await e.Channel.SendMessage(messageText);
                   });
               group.CreateCommand("Clean")
                   .Description("Cleans messages containing specified text.")
                   .MinPermissions((int)PermissionLevel.ServerAdmin)
                   .Parameter("Message", Discord.Commands.ParameterType.Multiple)
                   .Do(async e =>
                   {
                       string[] removeArray = e.Args;
                       string remove = String.Join(" ", removeArray);

                       Discord.Message[] messages = await e.Channel.DownloadMessages();

                       foreach (Discord.Message m in messages)
                       {
                           if (m.Text.ToLowerInvariant().Contains(remove.ToLowerInvariant()))
                           {
                               await m.Delete();
                           }
                       }
                   });
               group.CreateCommand("Purge")
                      .Description("Clears messages from one user.")
                      .MinPermissions((int)PermissionLevel.ServerAdmin)
                      .Parameter("User",Discord.Commands.ParameterType.Multiple)
                      .Do(async e =>
                      {
                          string[] userArr = e.Args;
                          string nameUser = String.Join(" ", userArr);

                          Discord.Message[] messages = await e.Channel.DownloadMessages();

                          foreach (Discord.Message m in messages)
                          {
                              if (m.User.Name.Equals(nameUser))
                              {
                                  await m.Delete();
                              }
                          }
                      });
           });
        }

        private void LoadExternal()
        {
            if (System.IO.File.Exists(@"..\data\quotes.txt"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(@"..\data\quotes.txt"))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            string[] quote = line.Split('|');
                            quotes.Add(quote);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
        }
    }
}
