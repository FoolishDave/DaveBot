using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DaveBot.Modules.Colors;
using DaveBot.Modules.General;
using DaveBot.Modules.Voice;
using DaveBot.Services;

namespace DaveBot
{
    public class DaveBot
    {
        private DiscordClient _client;
        private const string name = "FuckBoiBot";
        private const string url = "";
        private ulong focusedChannel;
        private Window window;

        public DaveBot(string token, Window window)
        {
            this.window = window;
            Start(token);
        }

        public void setFocusedChannel(ulong channelId)
        {
            focusedChannel = channelId;
            window.memberList.Items.Clear();
            Server activeServer = _client.GetServer(focusedChannel);
            foreach (User u in activeServer.Users)
            {
                window.memberList.Items.Add(u.Name);
                if (u.Status == UserStatus.Offline)
                {
                    window.memberList.Items[window.memberList.Items.Count-1].ForeColor = System.Drawing.Color.LightGray;
                }
            }
        }

        public string idToName(ulong channelId)
        {
            return _client.GetServer(channelId).Name;
        }

        public async 
        Task
dc()
        {
            if (_client != null && _client.State == ConnectionState.Connected)
            {
                await _client.Disconnect();
            }
        }

        private void Start(string token)
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
                x.ExecuteHandler = OnCommandExecuted;
                x.ErrorHandler = OnCommandError;
                x.PrefixChar = '~';
            }).UsingModules().UsingAudio(x =>
            {
                x.Mode = AudioMode.Both;
                x.EnableMultiserver = false;
                x.EnableEncryption = false;
                x.Bitrate = AudioServiceConfig.MaxBitrate;
                x.BufferLength = 10000;
            }).UsingPermissionLevels(PermissionResolver);

            _client.AddService<SettingsService>();
            _client.AddService<HttpService>();

            _client.AddModule<VoiceModule>("Voice", ModuleFilter.None);
            _client.AddModule<GeneralModule>("General", ModuleFilter.None);
            _client.AddModule<ColorModule>("Color", ModuleFilter.None);
            

            _client.ExecuteAndWait(async () =>
            {
                bool connecting = true;
                while (connecting)
                {
                    try
                    {
                        await _client.Connect(token);
                        _client.SetGame("Discord.Net");
                        Program.connected();
                        connecting = false;

                        

                        window.giveBot(this);

                        if (window.serverIdList.Items.Count > 0)
                        {
                            setFocusedChannel((ulong)window.serverIdList.SelectedItem);
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        _client.Log.Error($"Login Failed", ex);
                        await Program.disconnect("Login Failed: " + ex.Message);
                        connecting = false;
                        break;
                    }
                }
            });

           

            
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
                //_client.ReplyError(e, msg);
                _client.Log.Error("Command", msg);
            }
        }
        private void OnCommandExecuted(object sender, CommandEventArgs e)
        {
            _client.Log.Info("Command", $"{e.Command.Text} ({e.User.Name})");
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

