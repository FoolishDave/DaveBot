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
using System.IO;

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
        private static Dictionary<string, ColorDefinition> _statColorMap;
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

            LoadColors();

            _colorMap = _colors.ToDictionary(x => x.Id);
            _statColorMap = _colorMap;
        }

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("colors", group =>
             {
                 group.CreateCommand("list")
                        .Description("Displays all of the pretty colors available.")
                        .MinPermissions((int)PermissionLevel.User)
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
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(e =>
                        {
                                Color custom = new Color(float.Parse(e.Args[1])/255f, float.Parse(e.Args[2])/255f, float.Parse(e.Args[3])/255f);

                                ColorDefinition color = new ColorDefinition(e.Args[0], custom);

                            string[] saveColor = { e.GetArg("name"), e.Args[1], e.Args[2], e.Args[3] };
                            if (!System.IO.File.Exists(@"..\data\CustomColors.txt"))
                                File.Create(@"..\data\CustomColors.txt");

                            System.IO.File.AppendAllLines(@"..\data\CustomColors.txt",saveColor);


                                _colors.Add(color);
                                _colorMap.Add(e.Args[0], color);  
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

        public static IEnumerable<Role> GetOtherRoles(User user)
            => user.Roles.Where(x => !_statColorMap.ContainsKey(x.Name.ToLowerInvariant()));


        public static void AddColor(string name, float r, float g, float b)
        {
            Color custom = new Color(r / 255f, g / 255f, b / 255f);

            ColorDefinition color = new ColorDefinition(name, custom);

            string[] saveColor = { "\n"+name, ""+r, ""+g, ""+b };
            if (!System.IO.File.Exists(@"..\data\CustomColors.txt"))
                File.Create(@"..\data\CustomColors.txt");

            System.IO.File.AppendAllLines(@"..\data\CustomColors.txt", saveColor);

            ColorDefinition customDef = new ColorDefinition(name,custom);
            if (!_statColorMap.Keys.Contains(name.ToLowerInvariant())) 
                _statColorMap.Add(name.ToLowerInvariant(), customDef);
        }

        public static Color GetColor(User user)
        {
            Role colorRole = user.Roles.Where(x => _statColorMap.ContainsKey(x.Name.ToLowerInvariant())).FirstOrDefault();
            if (colorRole != null)
            {
                string colorName = colorRole.Name.ToLowerInvariant();
                ColorDefinition colorDef;
                _statColorMap.TryGetValue(colorName, out colorDef);
                return colorDef.Color;
            }
            return Color.DarkGrey;
        }

        public static async Task SetColor(ulong c, User user, string colorName)
        {
            await SetColor(DapperBot._client.GetServer(c).TextChannels.FirstOrDefault(), DapperBot._client.GetServer(c), user, colorName);
        }

        private async Task SetColor(CommandEventArgs e, User user, string colorName)
        {
            await SetColor(e.Channel, e.Server, user, colorName);
        }

        public static async Task SetColor(Channel c, Server s, User user, string colorName)
        {
            ColorDefinition color;
            if (!_statColorMap.TryGetValue(colorName.ToLowerInvariant(), out color))
            {
                await c.SendMessage("Unknown color");
                return;
            }
            if (!s.CurrentUser.ServerPermissions.ManageRoles)
            {
                await c.SendMessage("Insufficient Permission");
                return;
            }
            Role role = s.Roles.Where(x => x.Name == color.Name).FirstOrDefault();
            if (role == null)
            {
                role = await s.CreateRole(color.Name);
                await role.Edit(permissions: ServerPermissions.None, color: color.Color);
            }
            var otherRoles = GetOtherRoles(user);
            await user.Edit(roles: otherRoles.Concat(new Role[] { role }));
            await c.SendMessage("Set users color.");
        }

        private void LoadColors()
        {
            if (System.IO.File.Exists(@"..\data\CustomColors.txt"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(@"..\data\CustomColors.txt"))
                    {
                        while (!sr.EndOfStream)
                        {
                            string colorName = sr.ReadLine();
                            string r = sr.ReadLine();
                            string g = sr.ReadLine();
                            string b = sr.ReadLine();

                            float fR = 0;
                            float fG = 0;
                            float fB = 0;
                            float.TryParse(r, out fR);
                            float.TryParse(g, out fG);
                            float.TryParse(b, out fB);

                            Color customFromFile = new Color(fR / 255f, fG / 255f, fB / 255f);
                            if (!_colors.Any(x => x.Name.Equals(colorName)))
                                _colors.Add(new ColorDefinition(colorName, customFromFile));
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
