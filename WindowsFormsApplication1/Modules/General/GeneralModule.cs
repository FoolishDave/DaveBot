using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using Discord;
using Discord.Commands.Permissions.Levels;
using System.Windows.Forms;
using System.Diagnostics;
using Discord.API.Client;
using System;
using System.Linq;

namespace DaveBot.Modules.General
{
    internal class GeneralModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            _client.MessageReceived += async (s, e) =>
            {
                if (e.Message.User.Name.Equals("dittyofterror"))
                {
                    if (e.Message.Text.StartsWith("*"))
                    {
                        await e.Message.Channel.SendMessage("Stop correcting people.");
                    }
                    else
                    {
                    }
                }
            };

            _client.UserIsTyping += async (s, e) =>
            {
                //if (e.User.Name.Equals("dittyofterror") || e.User.Name.Equals("FoolishDave"))
               // {
                //    await e.User.Server.FindChannels("pls_no").FirstOrDefault().SendMessage("Bitch I see you typing.");
                //}
            };

            _manager.CreateCommands("FUCKBOI", group =>
            {
                group.PublicOnly();
                group.CreateCommand("DIE")
                        .Description("Secret Command.")
                        .MinPermissions((int)PermissionLevel.BotOwner)
                        .Do(e =>
                        {
                            Application.Exit();
                            Process.GetCurrentProcess().Kill();
                        });
                group.CreateCommand("Name")
                        .Description("Sets the name of the bot.")
                        .MinPermissions((int)PermissionLevel.BotOwner)
                        .Parameter("New Name")
                        .Do(e =>
                        {
                            _client.CurrentUser.Edit("avidday97", e.Args[0]);
                        });
                group.CreateCommand("Playing")
                        .Description("Sets what the bot is playing.")
                        .MinPermissions((int)PermissionLevel.ServerOwner)
                        .Parameter("Game",Discord.Commands.ParameterType.Multiple)
                        .Do(e =>
                        {
                            string game = (string.Join(" ", e.Args));
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
                    .Description("Cleans messages.")
                    .MinPermissions((int)PermissionLevel.ServerAdmin)
                    .Parameter("Type")
                    .Parameter("Message/User", Discord.Commands.ParameterType.Multiple)
                    .Do(async e =>
                    {
                        if (e.GetArg("Type").Equals("M"))
                        {
                            string[] removeArray = new string[e.Args.Length - 1];
                            Array.Copy(e.Args, 1, removeArray, 0,e.Args.Length - 1);
                            string remove = String.Join(" ", removeArray);

                            Console.WriteLine("Removing " + remove);

                            Discord.Message[] messages = await e.Channel.DownloadMessages();

                            foreach(Discord.Message m in messages)
                            {
                                Console.WriteLine("Message text is: " + m.Text);
                                if (m.Text.ToLowerInvariant().Contains(remove.ToLowerInvariant()))
                                {
                                    Console.WriteLine("Message has remove text.");
                                    await m.Delete();
                                }
                            }
                        }
                    });

            });


        }
    }
}
