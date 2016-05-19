using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using SharpTalk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace DaveBot.Modules.Voice
{

    // <summary> Provides interaction with voice and such. </summary>
    internal class VoiceModule : IModule
    {
        public class AudioInfo
        {
            public ulong id = 0;
            public bool softpause = false;
            public bool skip = false;
            public bool stopped = false;
            public bool playing = false;
            public string songPath = "";
            public Queue<string> QueuedSongs = new Queue<string>();
            public Queue<string> SongNames = new Queue<string>();
            public PBuffer ringBuffer = new PBuffer(2000000);
            public Thread qThread;
            public int volume = 25;
        }

        private ModuleManager _manager;
        private DiscordClient _client;

        static List<AudioInfo> audioInfos = new List<AudioInfo>();

        private FonixTalkEngine tts = new FonixTalkEngine();

        Thread queueThread = null;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            _client.ServerAvailable += (s, e) =>
            {
                _client.Log.Debug("Server added to audioInfos: " + e.Server.Name + " " + e.Server.Id.ToString(), null);
                if (!audioInfos.Any(x => x.id.Equals(e.Server.Id)))
                {
                    AudioInfo aI = new AudioInfo() { id = e.Server.Id, softpause = false, skip = false, stopped = false, playing = false, songPath = "", QueuedSongs = new Queue<string>(), SongNames = new Queue<string>() };
                    audioInfos.Add(aI);
                }
            };

            manager.CreateCommands("", group =>
            {
                group.PublicOnly();

                group.CreateCommand("join")
                        .Description("Summons Dapper Bot to an audio channel.")
                        .Alias("get in here")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();

                            var voiceChannel = e.User.VoiceChannel;
                            await _client.GetService<AudioService>().Join(voiceChannel);
                            if (e.Server.Id.Equals(129649392943628288))
                            {
                                aI.volume = 100;

                                tts.SpeakToWavFile(@"..\sounds\voice.wav", "Sup bitches.");

                                aI.songPath = @"..\sounds\voice.wav";

                                await e.Message.Delete();
                                await e.Channel.SendMessage(":speech_balloon: " + "Sup bitches.");

                                await SendMusic(e.Server.GetAudioClient(), e.Server.GetAudioClient().CancelToken);
                                aI.volume = 20;
                            }
                        });

                group.CreateCommand("volume")
                        .Description("Sets how loud Dapper Bot can be. (Use with no parameters to get volume.)")
                        .MinPermissions((int)PermissionLevel.User)
                        .Parameter("Level",ParameterType.Optional)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();
                            if (e.GetArg("Level").Equals("") || e.GetArg("Level") == null)
                            {
                                string vol = ":loud_sound:Volume: [";
                                for (int i = 0; i < (int)aI.volume / 10; i++)
                                {
                                    vol = vol + ":black_medium_small_square:";
                                }
                                for (int i = 0; i < (int)(100 - aI.volume) / 10; i++)
                                {
                                    vol = vol + ":white_medium_small_square:";
                                }
                                vol = vol + "] " + e.Args[0] + "%";

                                await e.Message.Delete();
                                await e.Channel.SendMessage(vol);
                            }
                            else
                            {
                                aI.volume = int.Parse(e.Args[0]);

                                string vol = ":loud_sound:Volume: [";
                                for (int i = 0; i < (int)aI.volume / 10; i++)
                                {
                                    vol = vol + ":black_medium_small_square:";
                                }
                                for (int i = 0; i < (int)(100 - aI.volume) / 10; i++)
                                {
                                    vol = vol + ":white_medium_small_square:";
                                }
                                vol = vol + "] " + e.Args[0] + "%";

                                await e.Message.Delete();
                                await e.Channel.SendMessage(vol);
                            }
                        });

                group.CreateCommand("play")
                        .Description("Plays a song or resumes after stopping.")
                        .Parameter("url", Discord.Commands.ParameterType.Optional)
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();

                            if (aI.playing)
                            {
                                await AddToQueue(e);
                                RunQueue(e.Server.GetAudioClient());
                            } else if (aI.stopped && aI.QueuedSongs.Count > 0)
                            {
                                aI.stopped = false;
                                aI.playing = true;
                                aI.skip = false;
                                aI.softpause = false;
                                await e.Channel.SendMessage("Resuming song playback.");
                            } else
                            {
                                string url = e.Args[0];
                                string path;

                                await e.Message.Delete();
                                aI.stopped = false;
                                aI.playing = true;
                                aI.skip = false;
                                aI.softpause = false;

                                if (url.StartsWith("http"))
                                {
                                    new Task(async () =>
                                    {
                                        VideoInfo vInfo = DownloadUrlResolver.GetDownloadUrls(url).First();
                                        string song = vInfo.Title;
                                        await e.Channel.SendMessage("**NOW PLAYING:**\n" + "*" + song + "*\n");
                                        _client.SetGame(song);
                                    }).RunSynchronously();

                                    path = ResolveVideoUri(url);

                                } else
                                {
                                    await e.Channel.SendMessage("**NOW PLAYING:**\n" + "*" + url + "*\n");
                                    path = "..\\sounds\\" + url;
                                    path.Replace('/', '\\');
                                }

                                _client.Log.Debug("voice", "Got video at path: " + url);
                                aI.songPath = path;

                                IAudioClient audioClient = e.Server.GetAudioClient();

                                await SendMusic(audioClient, audioClient.CancelToken);
                                aI.playing = false;
                            }
                        });

                group.CreateCommand("pause")
                        .Description("Pauses currently playing song.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();

                            await e.Message.Delete();
                            await e.Channel.SendMessage(":pause_button:Paused song.");

                            aI.softpause = !aI.softpause;
                        });

                group.CreateCommand("resume")
                        .Description("Resumes playback.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();
                            await e.Message.Delete();
                            await e.Channel.SendMessage(":arrow_forward:Resuming playback.");
                            if (aI.softpause)
                                aI.softpause = !aI.softpause;
                        });

                group.CreateCommand("stop")
                        .Description("Ends music playback.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();
                            await e.Message.Delete();
                            await e.Channel.SendMessage(":stop_button:Stopping music playback.");
                            aI.stopped = true;
                            aI.playing = false;
                        });

                group.CreateCommand("skip")
                        .Description("Skips to the next song.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();
                            await e.Message.Delete();
                            await e.Channel.SendMessage(":track_next:Skipping to next song.");
                            aI.skip = true;
                        });

                group.CreateCommand("queue")
                        .Description("Displays the queue.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            await e.Message.Delete();

                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();
                            string queueMes = "";

                            List<string> videoList = aI.QueuedSongs.ToList<string>();
                            List<string> nameList = aI.SongNames.ToList<string>();
                            int i = 1;
                            foreach (string s in videoList)
                            {
                                string addMes = "";
                                addMes += "**Queue #" + i + "**\n";
                                addMes += "    **Title: **" + nameList[i - 1] + "\n";

                                if (queueMes.Length + addMes.Length > 2000)
                                {
                                    await e.Channel.SendMessage(queueMes);
                                    queueMes = addMes;
                                }
                                else
                                {
                                    queueMes += addMes;
                                }

                                i++;
                            }
                            await e.Channel.SendMessage(queueMes);
                        });

                group.CreateCommand("enqueue")
                        .Description("Adds songs to audio queue.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Parameter("Song URL")
                        .Do(async e =>
                        {
                            await AddToQueue(e);
                            RunQueue(e.Server.GetAudioClient());
                        });

                group.CreateCommand("Speak")
                        .Description("Talk using FonixTalk")
                        .Parameter("Words", Discord.Commands.ParameterType.Multiple)
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();

                            if (!aI.stopped)
                            {
                                string speechText = String.Join(" ", e.Args);
                                Console.WriteLine("Saying " + speechText + " using FonixTalk");
                                tts.SpeakToWavFile(@"..\sounds\voice.wav", speechText);

                                aI.songPath = @"..\sounds\voice.wav";

                                await e.Message.Delete();
                                await e.Channel.SendMessage(":speech_balloon: " + speechText);

                                await SendMusic(e.Server.GetAudioClient(), e.Server.GetAudioClient().CancelToken);
                            }
                        });
                group.CreateCommand("Queue Clear")
                        .Description("Empties the queue")
                        .MinPermissions((int)PermissionLevel.ServerAdmin)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();

                            aI.QueuedSongs.Clear();
                            aI.SongNames.Clear();

                            await e.Message.Delete();
                            await e.Channel.SendMessage("**Queue Cleared**");
                        });

                group.CreateCommand("Shuffle")
                        .Description("Shuffles the queue")
                        .MinPermissions((int)PermissionLevel.ServerAdmin)
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();

                            List<string> QueuedSongsList = aI.QueuedSongs.ToList();
                            List<string> QueuedSongNameList = aI.SongNames.ToList();
                            aI.QueuedSongs.Clear();
                            aI.SongNames.Clear();

                            Random ran = new Random();

                            while (QueuedSongsList.Count > 0)
                            {
                                int random = ran.Next(0, QueuedSongsList.Count);
                                aI.QueuedSongs.Enqueue(QueuedSongsList[random]);
                                aI.SongNames.Enqueue(QueuedSongNameList[random]);
                                QueuedSongsList.Remove(QueuedSongsList[random]);
                                QueuedSongNameList.Remove(QueuedSongNameList[random]);
                            }
                            await e.Message.Delete();
                            await e.Channel.SendMessage("**Queue Shuffled**");
                        });

                group.CreateCommand("Airhorn")
                        .Alias("Airhorm")
                        .MinPermissions((int)PermissionLevel.User)
                        .Description("For you tony. <3")
                        .Do(async e =>
                        {
                            AudioInfo aI = audioInfos.Where(x => x.id.Equals(e.Server.Id)).FirstOrDefault();

                            string path = "..\\sounds\\airhorn.mp3";
                            aI.songPath = path;

                            aI.playing = true;
                            await SendMusic(e.Server.GetAudioClient(), e.Server.GetAudioClient().CancelToken);

                            aI.playing = false;
                            _client.Log.Debug("voice", "Finished sending voice");
                        });
            });
        }





        /* VOICE UTILITY FUNCTIONS */
        private static byte[] AdjustVolume(byte[] audioSamples, float volume)
        {
            if (Math.Abs(volume - 1.0f) < 0.01f)
                return audioSamples;
            var array = new byte[audioSamples.Length];
            for (var i = 0; i < array.Length; i += 2)
            {

                // convert byte pair to int
                short buf1 = audioSamples[i + 1];
                short buf2 = audioSamples[i];

                buf1 = (short)((buf1 & 0xff) << 8);
                buf2 = (short)(buf2 & 0xff);

                var res = (short)(buf1 | buf2);
                res = (short)(res * volume);

                // convert back
                array[i] = (byte)res;
                array[i + 1] = (byte)(res >> 8);

            }
            return array;
        }

        public async Task SendMusic(IAudioClient _aClient, CancellationToken cancelToken)
        {
            AudioInfo aI = audioInfos.Where(x => x.id.Equals(_aClient.Server.Id)).FirstOrDefault();

            var bufferTask = ReadFromStream(_aClient.Server.Id ,cancelToken);
            await Task.Delay(2000, cancelToken);
            const int blockSize = 3840;
            var buffer = new byte[blockSize];
            int lag = 0;
            int lagged = 0;
            while (true)
            {
                var read = aI.ringBuffer.Read(buffer, blockSize);
                if (read == 0) { lag++; lagged++; } else { if (lag >= 0) { lag--; lagged = 0; } }
                if (lagged > 35000)
                {
                    Console.WriteLine("Terminating sound, too much lag.");
                    break;
                }
                if (lag > 10000)
                {
                    lag = 0;
                    await Task.Delay(10000);
                }
                if (aI.skip)
                {
                    continue;
                }
                if (aI.stopped) continue;
                while (aI.softpause) await Task.Delay(10);
                buffer = AdjustVolume(buffer, (float)aI.volume / 100);
                _aClient.Send(buffer, 0, read);
            }
            _client.Log.Debug("Finished playing song.", null);
            aI.playing = false;
        }

        void RunQueue(IAudioClient _aClient)
        {
            AudioInfo aI = audioInfos.Where(x => x.id.Equals(_aClient.Server.Id)).FirstOrDefault();

            aI.stopped = false;
            if (aI.qThread == null)
                aI.qThread = new Thread(async x => await SendQueue(_aClient));
            if (!aI.qThread.ThreadState.Equals(System.Threading.ThreadState.Running))
                aI.qThread.Start();
        }

        public async Task AddToQueue(CommandEventArgs e)
        {
            ulong serverId = e.Server.Id;
            AudioInfo aI = audioInfos.Where(x => x.id.Equals(serverId)).FirstOrDefault();

            await e.Message.Delete();
            string title = e.Args[0];
            string path = e.Args[0];
            if (e.Args[0].StartsWith("http"))
            {
                new Task(async () =>
                {
                    VideoInfo vInfo = DownloadUrlResolver.GetDownloadUrls(e.Args[0]).First();
                    title = vInfo.Title;
                    await e.Channel.SendMessage("**ADDED TO QUEUE:**\n" + "*" + title + "*\n");
                    _client.SetGame(title);
                }).RunSynchronously();
            }
            else
            {
                await e.Channel.SendMessage("**ADDED TO QUEUE:**\n" + "*" + e.Args[0] + "*\n");
                path = "..\\sounds\\" + e.Args[0];
                path.Replace('/', '\\');
            }
            aI.QueuedSongs.Enqueue(path);
            aI.SongNames.Enqueue(title);
            if ((queueThread == null || !queueThread.IsAlive) && !aI.playing)
                RunQueue(e.Server.GetAudioClient());
        }

        public async Task SendQueue(IAudioClient _aClient)
        {
            AudioInfo aI = audioInfos.Where(x => x.id.Equals(_aClient.Server.Id)).FirstOrDefault();

            aI.playing = true;
            aI.stopped = false;
            aI.softpause = false;
            aI.skip = false;

            while (aI.playing) { }

            while (aI.QueuedSongs.Count > 0)
            {
                _client.Log.Debug("Moved to next song in queue.", null);
                if (!aI.stopped)
                {
                    string url = aI.QueuedSongs.Dequeue();
                    string title = aI.SongNames.Dequeue();
                    if (aI.skip)
                        aI.skip = !aI.skip;

                    new Task(async () =>
                    {
                        _client.SetGame(title);
                        await _aClient.Channel.Server.DefaultChannel.SendMessage("**NOW PLAYING:**\n" +
                                              "*" + title + "*\n");
                    }).RunSynchronously();
                    aI.songPath = ResolveVideoUri(url);
                    await SendMusic(_aClient, _aClient.CancelToken);
                }
            }
            aI.stopped = true;
            aI.playing = false;
        }


        /* YOUTUBE URL FUNCTIONS */
        string ResolveVideoUri(string url)
        {
            var youtube = VideoLibrary.YouTube.Default;
            return youtube.GetVideo(url).Uri;
        }

        private async Task<List<string>> enqueueUrlsFromPlaylist(ulong serverId,string play)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyAaTefrLXSr6d2-fHTZRH6Tv7QhzTdFqSw",
                ApplicationName = "DiscordBot"
            });

            Console.WriteLine("Got youtube service.");


            var nextPageToken = "";
            while (nextPageToken != null)
            {
                AudioInfo aI = audioInfos.Where(x => x.id.Equals(serverId)).FirstOrDefault();

                var playListItems = youtubeService.PlaylistItems.List("snippet,contentDetails");
                playListItems.PlaylistId = play;
                playListItems.MaxResults = 50;
                playListItems.PageToken = nextPageToken;


                var playListResponse = await playListItems.ExecuteAsync();
                Console.WriteLine("Got playlist response.");



                foreach (PlaylistItem vidId in playListResponse.Items)
                {
                    Console.WriteLine("Starting to process video id.");
                    VideosResource.ListRequest videoR = youtubeService.Videos.List("snippet");
                    videoR.Id = vidId.ContentDetails.VideoId;
                    Console.WriteLine("Getting video details.");
                    var responseV = await videoR.ExecuteAsync();
                    string videoId = responseV.Items[0].Id;

                    aI.QueuedSongs.Enqueue("https://www.youtube.com/watch?v=" + videoId);
                    aI.SongNames.Enqueue(responseV.Items[0].Snippet.Title);
                    Console.WriteLine("Enqueued " + "https://www.youtube.com/watch?v=" + videoId);
                }
                nextPageToken = playListResponse.NextPageToken;
            }
            Console.WriteLine("finished getting playlist");
            return null;
        }

        private string getVideo(string url)
        {
            string link = url;
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
            VideoInfo video = videoInfos.Where(info => info.CanExtractAudio).OrderByDescending(info => info.AudioBitrate).First();

            string path = "..\\sounds\\" + video.Title + video.AudioExtension;
            if (!File.Exists(path))
            {

                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }

                var audioDownloader = new AudioDownloader(video, "..\\sounds\\" + video.Title + video.AudioExtension);
                audioDownloader.DownloadProgressChanged += (send, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
                audioDownloader.AudioExtractionProgressChanged += (send, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);
                audioDownloader.Execute();
            }
            return path;
        }




        /* BUFFER UTILITY STUFF */
        public class PBuffer
        {
            private readonly byte[] ringBuffer;

            public int WritePosition { get; private set; } = 0;
            public int ReadPosition { get; private set; } = 0;

            public int ContentLength => (WritePosition >= ReadPosition ?
                                         WritePosition - ReadPosition :
                                         (BufferSize - ReadPosition) + WritePosition);

            public int BufferSize { get; }

            private readonly object readWriteLock = new object();

            public PBuffer(int size)
            {
                if (size <= 0)
                    throw new ArgumentException();
                BufferSize = size;
                ringBuffer = new byte[size];
            }

            public int Read(byte[] buffer, int count)
            {
                if (buffer.Length < count)
                    throw new ArgumentException();
                //Console.WriteLine($"***\nRead: {ReadPosition}\nWrite: {WritePosition}\nContentLength:{ContentLength}\n***");
                lock (readWriteLock)
                {
                    //read as much as you can if you're reading too much
                    if (count > ContentLength)
                        count = ContentLength;
                    //if nothing to read, return 0
                    if (WritePosition == ReadPosition)
                        return 0;
                    // if buffer is in the "normal" state, just read
                    if (WritePosition > ReadPosition)
                    {
                        Buffer.BlockCopy(ringBuffer, ReadPosition, buffer, 0, count);
                        ReadPosition += count;
                        //Console.WriteLine($"Read only normally1 {count}[{ReadPosition - count} to {ReadPosition}]");
                        return count;
                    }
                    //else ReadPos <Writepos
                    // buffer is in its inverted state
                    // A: if i can read as much as possible without hitting the buffer.length, read that

                    if (count + ReadPosition <= BufferSize)
                    {
                        Buffer.BlockCopy(ringBuffer, ReadPosition, buffer, 0, count);
                        ReadPosition += count;
                        //Console.WriteLine($"Read only normally2 {count}[{ReadPosition - count} to {ReadPosition}]");
                        return count;
                    }
                    // B: if i can't read as much, read to the end,
                    var readNormaly = BufferSize - ReadPosition;
                    Buffer.BlockCopy(ringBuffer, ReadPosition, buffer, 0, readNormaly);

                    //Console.WriteLine($"Read normaly {count}[{ReadPosition} to {ReadPosition + readNormaly}]");
                    //then read the remaining amount from the start

                    var readFromStart = count - readNormaly;
                    Buffer.BlockCopy(ringBuffer, 0, buffer, readNormaly, readFromStart);
                    //Console.WriteLine($"Read From start {readFromStart}[{0} to {readFromStart}]");
                    ReadPosition = readFromStart;
                    return count;
                }
            }

            public async Task WriteAsync(byte[] buffer, int count, CancellationToken cancelToken)
            {
                if (count > buffer.Length)
                    throw new ArgumentException();
                while (ContentLength + count > BufferSize)
                {
                    await Task.Delay(20, cancelToken);
                    if (cancelToken.IsCancellationRequested)
                        return;
                }
                //the while above assures that i cannot write past readposition with my write, so i don't have to check
                // *unless its multithreaded or task is not awaited
                lock (readWriteLock)
                {
                    // if i can just write without hitting buffer.length, do it
                    if (WritePosition + count < BufferSize)
                    {
                        Buffer.BlockCopy(buffer, 0, ringBuffer, WritePosition, count);
                        WritePosition += count;
                        //Console.WriteLine($"Wrote only normally {count}[{WritePosition - count} to {WritePosition}]");
                        return;
                    }
                    // otherwise, i have to write to the end, then write the rest from the start

                    var wroteNormaly = BufferSize - WritePosition;
                    Buffer.BlockCopy(buffer, 0, ringBuffer, WritePosition, wroteNormaly);

                    //Console.WriteLine($"Wrote normally {wroteNormaly}[{WritePosition} to {BufferSize}]");

                    var wroteFromStart = count - wroteNormaly;
                    Buffer.BlockCopy(buffer, wroteNormaly, ringBuffer, 0, wroteFromStart);

                    //Console.WriteLine($"and from start {wroteFromStart} [0 to {wroteFromStart}");

                    WritePosition = wroteFromStart;
                }
            }
        }

        private async Task ReadFromStream(ulong serverId, CancellationToken cancelToken)
        {
            AudioInfo aI = audioInfos.Where(x => x.id.Equals(serverId)).FirstOrDefault();

            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {aI.songPath} -f s16le -ar 48000 -ac 2 pipe:1 -loglevel quiet",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            });
            const int blockSize = 3840; // this can really be anything
            int zBytes = 0;
            while (!cancelToken.IsCancellationRequested && !aI.skip && !aI.stopped)
            {
                var buffer = new byte[blockSize];
                var read = p.StandardOutput.BaseStream.Read(buffer, 0, blockSize);
                if (read == 0)
                {

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Read 0 bytes, terminating song. {BUFFER}");

                    zBytes++;
                }
                else
                {
                    zBytes = 0;
                }
                if (zBytes > 10)
                {
                    break;
                }
                if (aI.skip) continue;
                if (aI.stopped) continue;
                while (aI.softpause) await Task.Delay(10);
                await aI.ringBuffer.WriteAsync(buffer, read, cancelToken);
            }
            p.Kill();
        }
    }
}
