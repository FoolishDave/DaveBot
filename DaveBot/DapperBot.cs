using DaveBot.Modules.Colors;
using DaveBot.Modules.General;
using DaveBot.Modules.Voice;
using DaveBot.Services;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System;
using System.Text;

namespace DaveBot
{
    class DapperBot
    {
        public static DiscordClient _client;
        private const string name = "Dapper Bot";
        private const string url = "http://davidgrougan.com/dapper.html";
        private static long lastDenial = 0;

        public DapperBot()
        {
            Start();
        }

        private void Start()
        {
            GlobalSettings.Load();

            _client = new DiscordClient(x =>
            {
                x.AppName = name;
                x.AppUrl = url;
                x.MessageCacheSize = 0;
                x.UsePermissionsCache = true;
                x.EnablePreUpdateEvents = true;
                x.LogLevel = LogSeverity.Debug;
                x.LogHandler = OnLogMessage;
            }).UsingCommands(x =>
            {
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
                x.CustomPrefixHandler = PrefixHandler;
                x.ExecuteHandler = OnCommandExecuted;
                x.ErrorHandler = OnCommandError;
                x.PrefixChar = '~';
            }).UsingModules().UsingAudio(x =>
            {
                x.Mode = AudioMode.Both;
                x.EnableEncryption = false;
                x.Bitrate = AudioServiceConfig.MaxBitrate;
                x.BufferLength = 10000;
            }).UsingPermissionLevels(PermissionResolver);

            _client.AddService<SettingsService>();
            _client.AddService<HttpService>();

            _client.Log.Debug("Installing Voice Module", null);
            _client.AddModule<VoiceModule>("Voice", ModuleFilter.None);
            _client.Log.Debug("Installing General Module", null);
            _client.AddModule<GeneralModule>("General", ModuleFilter.None);
            _client.Log.Debug("Installing Color Module", null);
            _client.AddModule<ColorModule>("Color", ModuleFilter.None);



            _client.ExecuteAndWait(async () =>
            {
                try
                {
                    await _client.Connect(GlobalSettings.DiscordSettings.Token);
                    _client.SetGame("Botting");
                }
                catch (Exception ex)
                {
                    _client.Log.Error($"Loggin failed.", ex);
                }
            });
        }

        private int PrefixHandler(Message arg)
        {
            if (arg.Text.StartsWith("FUCKBOI"))
            {
                return 8;
            }

            return -1;
        }

        private void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.Message;
            if (msg == null) //No exception - show a generic message
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        msg = "Unknown error.";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command.";
                        break;
                    case CommandErrorType.BadArgCount:
                        msg = "You provided the incorrect number of arguments for this command.";
                        break;
                    case CommandErrorType.InvalidInput:
                        msg = "Unable to parse your command, please check your input.";
                        break;
                    case CommandErrorType.UnknownCommand:
                        msg = "Unknown command.";
                        break;
                }
            }
            if (msg != null)
            {
                e.Channel.SendMessage("Command error: " + msg);
                _client.Log.Error("Command", msg);
            }
        }

        private void OnCommandExecuted(object sender, CommandEventArgs e)
        {
            _client.Log.Info("Command", $"{e.Command.Text} {string.Join(" ",e.Args)} ({e.User.Name})");
        }

        private void OnLogMessage(object sender, LogMessageEventArgs e)
        {
            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }

        private int PermissionResolver(User user, Channel channel)
        {
            if (user.Name.ToLowerInvariant().Contains("jtklaus"))
            {
                if (!System.IO.File.Exists(@"..\data\JordanTimeout.txt"))
                    System.IO.File.WriteAllText(@"..\data\JordanTimeout.txt", "0");

                string timeout = System.IO.File.ReadAllText(@"..\data\JordanTimeout.txt");
                var curTime = System.DateTime.Now.Ticks / TimeSpan.TicksPerMinute;
                long t;
                long timeDif = 0;
                if (long.TryParse(timeout, out t))
                {
                    timeDif = curTime - t;
                }

                if (timeDif < 30)
                {
                    if (curTime - lastDenial > 1)
                    {
                        channel.SendMessage(":x: You have used up all of your account commands. Please try again in " + (30 - timeDif) + " minutes. :x:" + "\n" +
                            "If you would like to purchase more commands, visit http://davidgrougan.com/BotTransactions.html");
                        lastDenial = curTime;
                    }
                    else
                    {
                        Console.WriteLine("Jordan tried to spam");
                    }
                    return (int)PermissionLevel.Jordan;
                }
                else
                {
                    System.IO.File.WriteAllText(@"..\data\JordanTimeout.txt", curTime.ToString());
                }
            }

            if (user.Id == GlobalSettings.Users.DevId)
                return (int)PermissionLevel.BotOwner;
            if (user.Server != null)
            {
                if (user == channel.Server.Owner)
                    return (int)PermissionLevel.ServerOwner;

                var serverPerms = user.ServerPermissions;
                if (serverPerms.ManageRoles)
                    return (int)PermissionLevel.ServerAdmin;
                if (serverPerms.ManageMessages && serverPerms.KickMembers && serverPerms.BanMembers)
                    return (int)PermissionLevel.ServerModerator;

                var channelPerms = user.GetPermissions(channel);
                if (channelPerms.ManagePermissions)
                    return (int)PermissionLevel.ChannelAdmin;
                if (channelPerms.ManageMessages)
                    return (int)PermissionLevel.ChannelModerator;
            }
            return (int)PermissionLevel.User;
        }
    }
}
