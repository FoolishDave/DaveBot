using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.Commands.Permissions.Visibility;
using Discord.Commands.Permissions.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DaveBot.Modules.Colors
{
    internal class ColorModule : IModule
    {
        private class ColorDefinition
        {
            public string Id;
            public string Name;
            public Color Color;
            public ColorDefinition(string name, Color color)
            {
                Name = name;
                Id = name.ToLowerInvariant();
                Color = color;
            }
        }
        private readonly List<ColorDefinition> _colors;
        private readonly Dictionary<string, ColorDefinition> _colorMap;
        private ModuleManager _manager;
        private DiscordClient _client;

        public ColorModule()
        {
            _colors = new List<ColorDefinition>()
            {
                new ColorDefinition("Blue", Color.Blue),
                new ColorDefinition("Teal", Color.Teal),
                new ColorDefinition("Gold", Color.Gold),
                new ColorDefinition("Green", Color.Green),
                new ColorDefinition("Purple", Color.Purple),
                new ColorDefinition("Orange", Color.Orange),
                new ColorDefinition("Magenta", Color.Magenta),
                new ColorDefinition("Red", Color.Red),
                new ColorDefinition("DarkBlue", Color.DarkBlue),
                new ColorDefinition("DarkTeal", Color.DarkTeal),
                new ColorDefinition("DarkGold", Color.DarkGold),
                new ColorDefinition("DarkGreen", Color.DarkGreen),
                new ColorDefinition("DarkMagenta", Color.DarkMagenta),
                new ColorDefinition("DarkOrange", Color.DarkOrange),
                new ColorDefinition("DarkPurple", Color.DarkPurple),
                new ColorDefinition("DarkRed", Color.DarkRed),
            };
            _colorMap = _colors.ToDictionary(x => x.Id);
        }

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("colors", group =>
             {
                 group.CreateCommand("list")
                        .Description("Displays all of the pretty colors available.")
                        .Do(async e =>
                        {
                            string text = $"{Format.Bold("Available Colors:")}\n" + string.Join(", ", _colors.Select(x => '`' + x.Name + '`'));
                            await e.Channel.SendMessage(text);
                        });
                 group.CreateCommand("set")
                        .Parameter("user")
                        .Parameter("color")
                        .MinPermissions((int) PermissionLevel.ChannelAdmin)
                        .Description("Sets a users name to a custom color.")
                        .Do(e =>
                        {
                            User user = e.Server.FindUsers(e.Args[0]).FirstOrDefault();
                            if (user == null && !e.Args[0].ToLowerInvariant().Equals("all"))
                            {
                                return e.Channel.SendMessage("Could not find user.");
                            }
                            else
                            {
                                if (e.Args[0].ToLowerInvariant().Equals("all"))
                                {
                                    foreach (User u in e.Server.Users)
                                    {
                                        SetColor(e, u, e.Args[1]);
                                    }
                                    return null;
                                }
                                else
                                { 
                                    return SetColor(e, user, e.Args[1]);
                                }
                            }
                        });

                 group.CreateCommand("create")
                        .Parameter("name")
                        .Parameter("r")
                        .Parameter("g")
                        .Parameter("b")
                        .Description("Allows for customizable colors.")
                        .Do(e =>
                        {
                            
                                Color custom = new Color(float.Parse(e.Args[1])/255f, float.Parse(e.Args[2])/255f, float.Parse(e.Args[3])/255f);

                                ColorDefinition color = new ColorDefinition(e.Args[0], custom);

                                _colors.Add(color);
                            _colorMap.Add(e.Args[0].ToLowerInvariant(), color);
                            
                        });

                 group.CreateCommand("clear")
                         .Parameter("user")
                         .Description("Clears user colors.")
                         .MinPermissions((int)PermissionLevel.ServerModerator)
                         .Do(async e =>
                         {
                             var otherRoles = GetOtherRoles(e.User);
                             await e.User.Edit(roles: otherRoles);
                             await e.Channel.SendMessage("Color cleared.");
                         });
             });
        }

        private IEnumerable<Role> GetOtherRoles(User user)
            => user.Roles.Where(x => !_colorMap.ContainsKey(x.Name.ToLowerInvariant()));

        private async Task SetColor(CommandEventArgs e, User user, string colorName)
        {
            ColorDefinition color;
            if (!_colorMap.TryGetValue(colorName.ToLowerInvariant(), out color))
            {
                await e.Channel.SendMessage("Unknown color");
                return;
            }
            if (!e.Server.CurrentUser.ServerPermissions.ManageRoles)
            {
                await e.Channel.SendMessage("Insufficient Permission");
                return;
            }
            Role role = e.Server.Roles.Where(x => x.Name == color.Name).FirstOrDefault();
            if (role == null)
            {
                role = await e.Server.CreateRole(color.Name);
                await role.Edit(permissions: ServerPermissions.None, color: color.Color);
            }
            var otherRoles = GetOtherRoles(user);
            await user.Edit(roles: otherRoles.Concat(new Role[] { role }));
            await e.Channel.SendMessage("Set users color.");
        }
    }

    
}
