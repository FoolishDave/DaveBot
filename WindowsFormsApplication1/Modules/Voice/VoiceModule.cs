using Discord;
using Discord.Commands.Permissions.Visibility;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using YoutubeExtractor;
using System.IO;
using NAudio.Wave;
using NAudio.Lame;
using System.Threading;
using System.Diagnostics;
using VideoLibrary;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace DaveBot.Modules.Voice
{

    // <summary> Provides interaction with voice and such. </summary>
    internal class VoiceModule : IModule
    {
        
        private ModuleManager _manager;
        private DiscordClient _client;
        private IAudioClient ac;
        bool softpause = false;
        bool skip = false;
        bool stopped = false;
        bool playing = false;
        string songPath = "";
        int volume = 100;
        VoiceInfo iVoice = new SpeechSynthesizer().GetInstalledVoices().First().VoiceInfo;
        int voiceSpeed = 0;
        Queue<string> QueuedSongs = new Queue<string>();
        Queue<string> SongNames = new Queue<string>();
        Thread queueThread = null;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;


            manager.CreateCommands("", group =>
             {
                 group.PublicOnly();

                 group.CreateCommand("JOIN")
                        .Description("Summons Fuckboi to an audio channel.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            var voiceChannel = e.User.VoiceChannel;
                            this.ac = await _client.Services.Get<AudioService>().Join(voiceChannel);
                        });

                 group.CreateCommand("FuckHorn")
                        .Description("For you tony. <3")
                        .Do(async e =>
                        {
                            String path = "..\\sounds\\airhorn.mp3";
                            songPath = path;

                            playing = true;
                            await SendMusic(ac, ac.CancelToken);

                            playing = false;
                            _client.Log.Debug("voice", "Finished sending voice");
                        });

                 group.CreateCommand("VOLUME")
                        .Description("Sets how loud fuckboi can be.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Parameter("Level")
                        .Do(async e =>
                        {
                            volume = int.Parse(e.Args[0]);

                            string vol = "Volume: [";
                            for (int i = 0; i < (int) volume/10;i++)
                            {
                                vol = vol + ":black_medium_small_square:";
                            }
                            for (int i = 0; i < (int) (100 - volume)/10; i++)
                            {
                                vol = vol + ":white_medium_small_square:";
                            }
                            vol = vol + "] " + e.Args[0] + "%";

                            await e.Message.Delete();
                            await e.Channel.SendMessage(vol);
                        });

                 group.CreateCommand("ENQUEUE")
                        .Description("Adds to the list of songs fuckboi must play.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Parameter("URL")
                        .Do(async e =>
                        {
                            QueuedSongs.Enqueue(e.Args[0]);
                            await e.Message.Delete();
                            string title = DownloadUrlResolver.GetDownloadUrls(e.Args[0]).First().Title;
                            await e.Channel.SendMessage("**Enqueued Song:**\n" +
                                                   "  " + title);
                            SongNames.Enqueue(title);
                            if ((queueThread == null || !queueThread.IsAlive) && !playing)
                                RunQueue(ac);

                            //await e.Channel.SendMessage(":no_entry_sign: I refuse faggot. :no_entry_sign:");
                        });

                 group.CreateCommand("SAY")
                        .Description("Talk with fuckboi.")
                        .Parameter("Words",Discord.Commands.ParameterType.Multiple)
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            string speechText = String.Join(" ", e.Args);
                            await e.Message.Delete();
                            await e.Channel.SendMessage(":speech_balloon: " + speechText);
                            await SendVoice(ac, speechText);

                            
                            Console.WriteLine("Saying " + speechText);

                            
                        });

                 group.CreateCommand("VOICES")
                        .Description("A list of voices Fuckboi can have.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            SpeechSynthesizer speech = new SpeechSynthesizer();
                            var installedVoices = speech.GetInstalledVoices();
                            foreach (var voice in installedVoices)
                            {
                                await e.Channel.SendMessage("Voice: " + voice.VoiceInfo.Name + "\n" +
                                                            "    Culture: " + voice.VoiceInfo.Culture + "\n" +
                                                            "    Age: " + voice.VoiceInfo.Age + "\n" +
                                                            "    Gender: " + voice.VoiceInfo.Gender);
                            }
                        });

                 group.CreateCommand("SETVOICE")
                        .Description("Make fuckboi sound like even more of a fuckboi. Or a fuckgril if you really want to.")
                        .MinPermissions((int)PermissionLevel.User)
                        .Parameter("Voice Name",Discord.Commands.ParameterType.Multiple)
                        .Do(e =>
                        {
                            string voiceName = String.Join(" ", e.Args);
                            SpeechSynthesizer speech = new SpeechSynthesizer();
                            var installedVoices = speech.GetInstalledVoices();
                            try
                            {

                                iVoice = installedVoices.ToList().Find(x => x.VoiceInfo.Name.ToLower().Contains(voiceName.ToLower())).VoiceInfo;
                            } catch (Exception excep)
                            {

                            }
                        });

                 group.CreateCommand("VOICESPEED")
                        .Description("Set fuckboi's speed.")
                        .Parameter("Speed")
                        .Do(e =>
                        {
                            voiceSpeed = int.Parse(e.Args[0]);
                        });

                 group.CreateCommand("PLAY")
                        .Description("Plays a youtube song bitch.")
                        .Parameter("url")
                        .MinPermissions((int)PermissionLevel.User)
                        .Do(async e =>
                        {
                            if (ac != null)
                            {
                                string url = e.Args[0];
                                string path;

                                await e.Message.Delete();
                                stopped = false;

                                if (url.StartsWith("http"))
                                {
                                    //await SendVoice(ac, "Now playing song");

                                    new Task(async () =>
                                    {
                                        VideoInfo vInfo = DownloadUrlResolver.GetDownloadUrls(url).First();
                                        string song = vInfo.Title;
                                        await e.Channel.SendMessage("**NOW PLAYING:**\n" +
                                                              "*" + song + "*\n");
                                        await this.SendVoice(ac, "Now playing song: " + song);
                                        _client.SetGame(song);
                                        
                                    }).RunSynchronously();
                                    //+ //DownloadUrlResolver.GetDownloadUrls(url).First().Title);
                                    //_client.Log.Debug("voice", "Getting video from: " + url);
                                    //path = getVideo(url);
                                    path = ResolveVideoUri(url);
                                } else
                                {
                                    await SendVoice(ac, "Now Playing " + url);
                                    path = "..\\sounds\\" + url;
                                    path.Replace('/', '\\');
                                }
                                
                                _client.Log.Debug("voice","Got video, is at path: " + path);
                                songPath = path;

                                playing = true;
                                await SendMusic(ac, ac.CancelToken);

                                playing = false;
                                _client.Log.Debug("voice", "Finished sending voice");

                            }
                            
                        });
                 group.CreateCommand("PAUSE")
                        .Description("Stops currently playing song.")
                        .Alias("RESUME")
                        .Do(e =>
                        {
                            softpause = !softpause;

                        });

                 group.CreateCommand("QUEUE")
                        .Description("Displays the queue.")
                        .Do(async e =>
                        {
                            string queueMes = "";

                            List < string > videoList = QueuedSongs.ToList<string>();
                            List<string> nameList = SongNames.ToList<string>();
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
                                } else
                                {
                                    queueMes += addMes;
                                }

                                i++;
                            }
                            await e.Channel.SendMessage(queueMes);
                        });

                 group.CreateCommand("SKIP")
                        .Description("Skips to the next song.")
                        .Do(e =>
                        {
                            skip = true;
                        });

                 group.CreateCommand("PLAYLIST")
                        .Description("Enqueues a youtube playlist.")
                        .Parameter("Playlist ID")
                        .Do(async e =>
                        {
                            await enqueueUrlsFromPlaylist(e.Args[0]);
                            await e.Channel.SendMessage("Enqueued Playlist.");
                            RunQueue(ac);
                        });

             });
        }

        PBuffer ringBuffer = new PBuffer(2000000);

        void RunQueue(IAudioClient _aClient)
        {
            stopped = false;
            Thread qThread = new Thread(async x => await SendQueue(_aClient));
            this.queueThread = qThread;
            qThread.Start();
        }

        public async Task SendQueue(IAudioClient _aClient)
        {
            playing = true;
            while (QueuedSongs.Count > 0)
            {
                string url = QueuedSongs.Dequeue();
                string title = SongNames.Dequeue();
                if (skip)
                    skip = !skip;

                new Task(async () =>
                {
                    await this.SendVoice(ac, "Now playing song: " + title);
                    _client.SetGame(title);
                    await _aClient.Channel.Server.DefaultChannel.SendMessage("**NOW PLAYING:**\n" +
                                          "*" + title + "*\n");
                }).RunSynchronously();
                songPath = ResolveVideoUri(url);
                await SendMusic(_aClient, _aClient.CancelToken);
            }
            stopped = true;
            playing = false;
        }

        public async Task SendVoice(IAudioClient _aClient, string toSay)
        {
            SpeechSynthesizer speech = new SpeechSynthesizer();
            speech.Rate = voiceSpeed;
            MemoryStream ms = new MemoryStream();
            speech.SetOutputToWaveStream(ms);
            speech.SelectVoice(iVoice.Name);
            speech.Speak(toSay);

            songPath = "..\\sounds\\voice.mp3";

            ms.Seek(0, SeekOrigin.Begin);
            using (var retMs = new MemoryStream())
            using (var rdr = new WaveFileReader(ms))
            using (var wtr = new LameMP3FileWriter(songPath, rdr.WaveFormat, LAMEPreset.VBR_90))
            {
                rdr.CopyTo(wtr);
            }

            Console.WriteLine("Speech path: " + songPath);

            await SendMusic(ac, ac.CancelToken);
        }

        public async Task SendMusic(IAudioClient _aClient, CancellationToken cancelToken)
        {
            var bufferTask = ReadFromStream(cancelToken);
            await Task.Delay(2000, cancelToken);
            const int blockSize = 3840;
            var buffer = new byte[blockSize];
            int lag = 0;
            int lagged = 0;
            while (true)
            {
                var read = ringBuffer.Read(buffer, blockSize);
                if (read == 0) { lag++; lagged++; } else {  if(lag >= 0) { lag--; lagged = 0; } }
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
                if (skip) continue; 
                if (stopped) continue;
                while (softpause) await Task.Delay(10);
                buffer = AdjustVolume(buffer, (float)volume / 100);
                _aClient.Send(buffer, 0, read);
            }
        }


        private async Task ReadFromStream(CancellationToken cancelToken)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {songPath} -f s16le -ar 48000 -ac 2 pipe:1 -loglevel quiet",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            });
            const int blockSize = 3840; // this can really be anything
            int zBytes = 0;
            while (!cancelToken.IsCancellationRequested && !skip && !stopped)
            {
                var buffer = new byte[blockSize];
                var read = p.StandardOutput.BaseStream.Read(buffer, 0, blockSize);
                if (read == 0)
                {

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Read 0 bytes, terminating song. {BUFFER}");

                    zBytes++;
                } else
                {
                    zBytes = 0;
                }
                if (zBytes > 10)
                {
                    break;
                }
                if (skip) continue;
                if (stopped) continue;
                while (softpause) await Task.Delay(10);
                await ringBuffer.WriteAsync(buffer, read, cancelToken);
            }
            p.Kill();
        }

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

        string ResolveVideoUri(string url)
        {
            var youtube = YouTube.Default;
            return youtube.GetVideo(url).Uri;
        }


        private async Task<List<string>> enqueueUrlsFromPlaylist(string play)
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

                    QueuedSongs.Enqueue("https://www.youtube.com/watch?v=" + videoId);
                    SongNames.Enqueue(responseV.Items[0].Snippet.Title);
                    Console.WriteLine("Enqueued " + "https://www.youtube.com/watch?v=" + videoId);
                }
                nextPageToken = playListResponse.NextPageToken;
            }
            Console.WriteLine("finished getting playlist");
            return null;
        }


        private void SendVoiceFFmpeg(string file)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\ffmpeg\bin\ffmpeg.exe",
                Arguments = $"-i {file} -f s16le -ar 48000 -ac 2 pipe:1 -loglevel quiet",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            Thread.Sleep(2000);

            int blockSize = 3840;                                                                                                           // The size of bytes to read per frame; 1920 for mono
            byte[] buffer = new byte[blockSize];
            int byteCount;

            ac.VoiceSocket.SendSetSpeaking(true);

            while (true)                                                                                                                            // Loop forever, so data will always be read
            {
                byteCount = process.StandardOutput.BaseStream                                                   // Access the underlying MemoryStream from the stdout of FFmpeg
                        .Read(buffer, 0, blockSize);                                                                            // Read stdout into the buffer

                if (byteCount == 0)                                                                                                             // FFmpeg did not output anything
                    break;                                                                                                                          // Break out of the while(true) loop, since there was nothing to read.

                ac.Send(buffer, 0, byteCount);                                                                    // Send our data to Discord
            }
            ac.Wait();
        }

        private void SendVoice(string file, IAudioClient ac)
        {

            if (_client.Services.Get<AudioService>().Config.Mode != AudioMode.Outgoing)
            {
                Console.WriteLine("Client not set to outgoing audio mode.");
                _client.UsingAudio(x =>
                {
                    x.Mode = AudioMode.Outgoing;
                });
            }

            try
            {
                int channels = _client.Services.Get<AudioService>().Config.Channels;
                int samplerate = 48000;
                var outFormat = new WaveFormat(samplerate, 16, channels);

                int blockSize = outFormat.AverageBytesPerSecond / 50;
                byte[] buffer = new byte[blockSize];
                int byteCount;


                ac.VoiceSocket.SendSetSpeaking(true);
                if (file.EndsWith(".wav"))
                {
                    using (var waveReader = new WaveFileReader(file))
                    {
                        while ((byteCount = waveReader.Read(buffer, 0, blockSize)) > 0)
                        {
                            if (byteCount < blockSize)
                            {
                                // Incomplete Frame
                                for (int i = byteCount; i < blockSize; i++)
                                    buffer[i] = 0;
                            }
                            Console.WriteLine("Sending buffer, byteCount=" + byteCount + "/" + blockSize);
                            ac.Send(buffer, 0, blockSize);
                        }
                    }
                }
                else if (file.EndsWith(".mp3"))
                {
                    using (var mp3Reader = new MediaFoundationReader(file))
                    {
                        using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat) { ResamplerQuality = 60 })
                        {
                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                            {
                                if (byteCount < blockSize)
                                {
                                    // Incomplete Frame
                                    for (int i = byteCount; i < blockSize; i++)
                                    {
                                        buffer[i] = 0;
                                    } 
                                }
                                Console.WriteLine("Sending buffer, byteCount=" + byteCount + "/" + blockSize);
                                ac.Send(buffer, 0, blockSize);
                            }
                            //resampler.Dispose();
                            //mp3Reader.Dispose();

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            Console.WriteLine("Done sending voice.");
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
    }
}
