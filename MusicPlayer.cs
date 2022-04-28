using OWML.ModHelper;
using System.Collections;
using System.Collections.Generic;
using Environment = System.Environment;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UAudioType = UnityEngine.AudioType;

namespace MusicPlayer
{
    public class MusicPlayer : ModBehaviour
    {
        private static MusicPlayer instance;
        public static MusicPlayer Instance => instance;

        private List<AudioClip> musicTracks = new List<AudioClip>();
        public AudioSource PlayerHeadsetAudioSource;
        public AudioSource ShipSpeakerAudioSource;
        public bool isPlaying = false;
        public bool playerIsPlaying = false;
        private double _audioVolume = 0.2;
        private double _shipAudioVolume = 0.5;
        private int currentSong = 0;
        private int songsAvailable;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            ModHelper.Console.WriteLine("Music player is loaded!", OWML.Common.MessageType.Success);
            LoadManager.OnCompleteSceneLoad += OnSceneLoaded;
        }

        private void Update()
        {
            if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                songsAvailable = musicTracks.Count;
                ModHelper.Console.WriteLine("Amount of loaded songs " + musicTracks.Count.ToString(), OWML.Common.MessageType.Success);
                if (playerIsPlaying)
                {
                    PlayerHeadsetAudioSource.Stop();
                    playerIsPlaying = false;
                }
                else
                {
                    OnPlaySong();
                    playerIsPlaying = true;
                }
            }
            if (Keyboard.current.altKey.wasPressedThisFrame)
            {
                if (isPlaying)
                {
                    ShipSpeakerAudioSource.Stop();
                    isPlaying = false;
                }
                else
                {
                    ShipOnPlaySong();
                    isPlaying = true;
                }
            }
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                if (playerIsPlaying)
                {
                    if (currentSong > songsAvailable)
                    {
                        PlayerHeadsetAudioSource.Stop();
                        ModHelper.Console.WriteLine("List Index : " + currentSong.ToString(), OWML.Common.MessageType.Success);
                        currentSong = 0;
                        PlayerHeadsetAudioSource.clip = musicTracks[currentSong];
                        PlayerHeadsetAudioSource.Play();
                    }
                    else
                    {
                        PlayerHeadsetAudioSource.Stop();
                        currentSong++;
                        PlayerHeadsetAudioSource.clip = musicTracks[currentSong];
                        PlayerHeadsetAudioSource.Play();
                        ModHelper.Console.WriteLine("List Index : " + currentSong.ToString(), OWML.Common.MessageType.Success);
                    }
                }
                if (isPlaying)
                {
                    if (currentSong > songsAvailable)
                    {
                        ShipSpeakerAudioSource.Stop();
                        ModHelper.Console.WriteLine("List Index : " + currentSong.ToString(), OWML.Common.MessageType.Success);
                        currentSong = 0;
                        ShipSpeakerAudioSource.clip = musicTracks[currentSong];
                        ShipSpeakerAudioSource.Play();
                    }
                    else
                    {
                        ShipSpeakerAudioSource.Stop();
                        currentSong++;
                        ShipSpeakerAudioSource.clip = musicTracks[currentSong];
                        ShipSpeakerAudioSource.Play();
                        ModHelper.Console.WriteLine("List Index : " + currentSong.ToString(), OWML.Common.MessageType.Success);
                    }
                }
                else
                    ModHelper.Console.WriteLine("Your not playing silly!", OWML.Common.MessageType.Success);
            }
            if (!Keyboard.current.leftArrowKey.wasPressedThisFrame)
                return;
            if (playerIsPlaying)
            {
                if (currentSong < 0)
                {
                    PlayerHeadsetAudioSource.Stop();
                    ModHelper.Console.WriteLine("List Index : " + currentSong.ToString(), OWML.Common.MessageType.Success);
                    currentSong = 0;
                    PlayerHeadsetAudioSource.clip = musicTracks[currentSong];
                    PlayerHeadsetAudioSource.Play();
                }
                else
                {
                    PlayerHeadsetAudioSource.Stop();
                    --currentSong;
                    PlayerHeadsetAudioSource.clip = musicTracks[currentSong];
                    PlayerHeadsetAudioSource.Play();
                    ModHelper.Console.WriteLine("List Index : " + currentSong.ToString(), OWML.Common.MessageType.Success);
                }
            }
            else if (isPlaying)
            {
                if (currentSong < 0)
                {
                    ShipSpeakerAudioSource.Stop();
                    ModHelper.Console.WriteLine("List Index : " + currentSong.ToString(), OWML.Common.MessageType.Success);
                    currentSong = 0;
                    ShipSpeakerAudioSource.clip = musicTracks[currentSong];
                    ShipSpeakerAudioSource.Play();
                }
                else
                {
                    ShipSpeakerAudioSource.Stop();
                    --currentSong;
                    ShipSpeakerAudioSource.clip = musicTracks[currentSong];
                    ShipSpeakerAudioSource.Play();
                    ModHelper.Console.WriteLine("List Index : " + currentSong.ToString(), OWML.Common.MessageType.Success);
                }
            }
            else
                ModHelper.Console.WriteLine("Your not playing silly!", OWML.Common.MessageType.Success);
        }

        private void OnPlaySong()
        {
            PlayerHeadsetAudioSource.clip = musicTracks[currentSong];
            PlayerHeadsetAudioSource.Play();
        }

        private void ShipOnPlaySong()
        {
            ShipSpeakerAudioSource.clip = musicTracks[currentSong];
            ShipSpeakerAudioSource.Play();
        }

        private void OnSceneLoaded(OWScene originalScene, OWScene loadScene)
        {
            StartCoroutine("LoadAllAudio");
            if (loadScene == OWScene.SolarSystem)
            {
                PlayerHeadsetAudioSource = GameObject.Find("Player_Body").AddComponent<AudioSource>();
                PlayerHeadsetAudioSource.volume = (float)_audioVolume;
                ShipSpeakerAudioSource = GameObject.Find("Ship_Body").AddComponent<AudioSource>();
                ShipSpeakerAudioSource.volume = (float)_shipAudioVolume;
                ShipSpeakerAudioSource.spatialBlend = 1f;
                ShipSpeakerAudioSource.rolloffMode = AudioRolloffMode.Linear;
                ShipSpeakerAudioSource.minDistance = 1f;
                ShipSpeakerAudioSource.maxDistance = 25f;
            }
        }

        private string MusicFilesPath => Path.Combine(ModHelper.Manifest.ModFolderPath, "MusicFiles");

        private IEnumerator LoadAllAudio()
        {
            DirectoryInfo dir = new DirectoryInfo(MusicFilesPath);
            if (!dir.Exists)
            {
                dir.Create();
                yield break;
            }
            FileInfo[] fileInfoArray = dir.GetFiles("*.*", SearchOption.AllDirectories);
            for (int index = 0; index < fileInfoArray.Length; ++index)
            {
                FileInfo fileInfo = fileInfoArray[index];
                ModHelper.Console.WriteLine($"Track Found in DIR : {fileInfo}", OWML.Common.MessageType.Success);
                string name = fileInfo.Name.Replace(fileInfo.Extension, "");
                if (!HasAudio(name) && fileInfo.Extension != ".meta")
                {
                    UAudioType audioType = UAudioType.UNKNOWN;
                    if (fileInfo.Extension.Equals(".ogg"))
                        audioType = UAudioType.OGGVORBIS;
                    else if (fileInfo.Extension.Equals(".wav"))
                        audioType = UAudioType.WAV;
                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileInfo.FullName, audioType))
                    {
                        yield return www.SendWebRequest();
                        AudioClip content = DownloadHandlerAudioClip.GetContent(www);
                        content.name = name;
                        Object.DontDestroyOnLoad(content);
                        musicTracks.Add(content);
                        ModHelper.Console.WriteLine($"Track Loaded : {content.length}", OWML.Common.MessageType.Success);
                    }
                }
            }
        }

        private bool HasAudio(string name)
        {
            foreach (Object musicTrack in musicTracks)
            {
                if (musicTrack.name == name)
                    return true;
            }
            return false;
        }
    }
}
