using UnityEngine;
using UnityEditor.SceneManagement;
//using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace Yakno.Discord
{

    public static class YaknoDiscordPresence 
    {
        private const string funAppID = "805401847850205204";
        private const string funAppSecret = "amoklywgLYeGJDcPyFD3dvLLUy0fX_Ux";

        public static Discord Discord { get; private set; }


        public static string projectName { get; private set; }
        public static string sceneName { get; private set; }
        public static bool showSceneName = true;
        public static bool showProjectName = true;
        public static bool resetOnSceneChange = false;
        public static bool debugMode = false;
        public static bool EditorClosed = true;
        public static long lastTimestamp = 0;
        public static string lastSessionID = "";
        public static bool Error;
        public static bool IsActive = true;
        public static float ToggleInterval = 0f;
        public static DiscordState State { get; set; } = DiscordState.Stopped;
        public enum DiscordState
        {
            Stopped,
            Connecting,
            Ready,
            Cooldown,
        }
        static YaknoDiscordPresence()
        {

        }
        public static bool IsInitialized { get; private set; } = false;
        public static void Initialize()
        {
            try
            {
                
                Discord = new Discord(long.Parse(funAppID), (long)CreateFlags.Default);
            }
            catch (Exception e)
            {
                State = DiscordState.Stopped;
                Start();
                Debug.Log("Reinitializing Discord Presence...");
                return;
            }
            if (Discord != null)
            {
                Discord.SetLogHook(LogLevel.Debug, OnDiscordLogger);
                State = DiscordState.Connecting;
                IsInitialized = true;
            }
            else
            {
                IsInitialized = false;
            }
            if (!resetOnSceneChange || Application.productName != lastSessionID)
            {
                lastTimestamp = GetTimestamp();
            }

            lastSessionID = Application.productName;

            projectName = GetState();
            sceneName = GetDetails();

            OnActivityUpdate();
                //EditorSceneManager.GetActiveScene().name;

            //EditorApplication.update += Update;
            //EditorSceneManager.sceneOpened += SceneOpened;
            //Log("Started!");
        }
        public static int CurrentStateIndex = 0;
        public static int CurrentDetailsIndex = 0;
        public static int CurrentSmallTipsIndex = 0;
        public static int CurrentBigTipsIndex = 0;

        public static float MaxUpdateRate { get; private set; } = 20f;//Dont go below 20 secs
        public static float LastActivityUpdate { get; private set; } = 0f;//Dont go below 20 secs
        public static int ActivityUpdatesRemaining = 5;//Reset every rate
        public static string LastLoggedState { get; private set; } = string.Empty;
        public static void OnDiscordLogger(LogLevel logLvl_, string msg_)
        {
            LastLoggedState = msg_;
            Debug.Log(string.Format("{2}[DiscordLogger]{0}: {1}", logLvl_, msg_,Time.realtimeSinceStartup));
            switch(msg_)
            {
                case "Ok"://All good
                    State = DiscordState.Ready;
                    break;
                case "ServiceUnavailable"://Discord isn't working
                case "OAuth2Error"://the OAuth2 process failed at some point
                case "NotAuthenticated"://the internal auth call failed for the user, and you can't do this
                case "InvalidAccessToken"://the user's bearer token is invalid
                case "ApplicationMismatch"://access token belongs to another application
                case "InvalidEntitlement"://	the user does not have the right entitlement for this game
                case "NotInstalled"://	Discord is not installed
                case "NotRunning"://Discord is not running
                    State = DiscordState.Stopped;
                    break;
                case "RateLimited"://	you are calling that method too quickly
                    State = DiscordState.Cooldown;
                    break;
                case "LockFailed"://an internal error on transactional operations
                case "InternalError"://something on our side went wrong
                case "InvalidPayload"://the data you sent didn't match what we expect
                case "InvalidCommand"://that's not a thing you can do
                case "InvalidPermissions"://you aren't authorized to do that
                case "NotFetched"://couldn't fetch what you wanted
                case "NotFound"://what you're looking for doesn't exist
                case "Conflict"://user already has a network connection open on that channel
                case "InvalidSecret"://activity secrets must be unique and not match party id
                case "InvalidJoinSecret"://join request for that user does not exist
                case "NoEligibleActivity"://you accidentally set an ApplicationId in your UpdateActivity() payload
                case "InvalidInvite"://your game invite is no longer valid
                case "InvalidDataUrl"://something internally went wrong fetching image data
                case "InvalidBase64"://not valid Base64 data
                case "NotFiltered"://you're trying to access the list before creating a stable list with Filter()
                case "LobbyFull"://the lobby is full
                case "InvalidLobbySecret"://the secret you're using to connect is wrong
                case "InvalidFilename"://file name is too long
                case "InvalidFileSize"://file is too large
                case "InsufficientBuffer"://insufficient buffer space when trying to write
                case "PurchaseCancelled"://user cancelled the purchase flow
                case "InvalidGuild"://Discord guild does not exist
                case "InvalidEvent"://the event you're trying to subscribe to does not exist
                case "InvalidChannel"://	Discord channel does not exist
                case "InvalidOrigin"://	the origin header on the socket does not match what you've registered (you should not see this)
                case "SelectChannelTimeout"://the user took too long selecting a channel for an invite
                case "GetGuildTimeout"://	took too long trying to fetch the guild
                case "SelectVoiceForceRequired	"://push to talk is required for this channel
                case "CaptureShortcutAlreadyListening"://that push to talk shortcut is already registered
                case "UnauthorizedForAchievement"://your application cannot update this achievement
                case "InvalidGiftCode"://the gift code is not valid
                case "PurchaseError"://something went wrong during the purchase flow
                case "TransactionAborted"://purchase flow aborted because the SDK is being torn down
                case "InvalidVersion"://Outdated Discord SDK
                default:
                    State = DiscordState.Connecting;
                    Discord.Dispose();
                    Discord = null;
                    IsInitialized = false;
                    break;
            }
        }

        public static YaknoDiscordConfiguration LoadedConfiguration;
        public static YaknoDiscordSettings ActivitySettings
        {
            get
            {
                if (LoadedConfiguration == null)
                    return defaultSettings;
                else
                    return LoadedConfiguration.Settings;
            }
        }
        private static YaknoDiscordSettings defaultSettings = new YaknoDiscordSettings();
        public static string GetState()
        {
            if(ActivitySettings.SecondaryMessages.Count > 0)
            {
                CurrentStateIndex++;
                if (CurrentStateIndex >= ActivitySettings.SecondaryMessages.Count)
                    CurrentStateIndex = 0;
                return ActivitySettings.SecondaryMessages[CurrentStateIndex];
            }
            return "Dencho was here!";
        }
        public static string GetDetails()
        {
            if (ActivitySettings.PrimaryMessages.Count > 0)
            {
                CurrentDetailsIndex++;
                if (CurrentDetailsIndex >= ActivitySettings.PrimaryMessages.Count)
                    CurrentDetailsIndex = 0;
                return ActivitySettings.PrimaryMessages[CurrentDetailsIndex];
            }
            return "Coded by Dencho!";
        }
        
        public static string GetBigTooltip()
        {
            if (ActivitySettings.PrimaryMessages.Count > 0)
            {
                CurrentBigTipsIndex++;
                if (CurrentBigTipsIndex >= ActivitySettings.BigTooltips.Count)
                    CurrentBigTipsIndex = 0;
                return ActivitySettings.BigTooltips[CurrentBigTipsIndex];
            }
            return "Dencho is over there!";
        }
        public static string GetSmallTooltip()
        {
            if (ActivitySettings.SmallTooltips.Count > 0)
            {
                CurrentSmallTipsIndex++;
                if (CurrentSmallTipsIndex >= ActivitySettings.SmallTooltips.Count)
                    CurrentSmallTipsIndex = 0;
                return ActivitySettings.SmallTooltips[CurrentSmallTipsIndex];
            }
            return "Dencho was not here";
        }
        public static ActivityManager ActivityManager
        {
            get => Discord.GetActivityManager();
        }
        public static Result LastActivityResult = Result.NotRunning;
        public static Activity CurrentActivity;
        private static float lastToggle = 0f;
        public static string LastActivityStatus { get; private set; } = string.Empty;
        public static string LastActivityDetails { get; private set; } = string.Empty;
        public static string LastActivityBigTip { get; private set; } = string.Empty;
        public static string LastActivitySmallTip { get; private set; } = string.Empty;
        public static bool IsUpdatingActivity { get; private set; } = false;
        private static void UpdateCurrentActivity()
        {
            
            LastActivityStatus = GetState();
            LastActivityDetails = GetDetails();
            LastActivityBigTip = GetBigTooltip();
            LastActivitySmallTip = GetSmallTooltip();
        }
        public static void OnActivityUpdate()
        {
            
            if (LastActivityUpdate < Time.realtimeSinceStartup)
            {
                IsUpdatingActivity = true;
                LastActivityUpdate = Time.realtimeSinceStartup + (MaxUpdateRate / 4.5f);
            }
            if(IsUpdatingActivity)
            {
                UpdateCurrentActivity();
                Activity nextActivity = new Activity()
                {
                    State = LastActivityStatus,
                    Details = LastActivityDetails,
                    Timestamps = new ActivityTimestamps
                    {
                        Start = lastTimestamp
                    },
                    Assets =
                    { 
                        LargeImage = "gamelogo",
                        LargeText = LastActivityBigTip,
                        SmallImage = "controller",
                        SmallText = LastActivitySmallTip
                    },
                    Instance = true
                };
                CurrentActivity = nextActivity;
                //CurrentActivity.
                //CurrentActivity.State = LastActivityStatus;
                //CurrentActivity.Details = LastActivityDetails;
                //CurrentActivity.Timestamps = new ActivityTimestamps
                //{
                //    Start = lastTimestamp
                //};
                //CurrentActivity.Assets.LargeImage = "gamelogo";
                //CurrentActivity.Assets.LargeText = LastActivityBigTip;
                //CurrentActivity.Assets.SmallImage = "controller";
                //CurrentActivity.Assets.SmallText = LastActivitySmallTip;
                //CurrentActivity.Instance = true;
                ActivityManager.UpdateActivity(nextActivity, result =>
                {
                    LastActivityResult = result;
                    if(LastActivityResult == Result.Ok)
                    {
                        ActivityUpdatesRemaining--;
                        if (ActivityUpdatesRemaining <= 0)
                            ActivityUpdatesRemaining = 5;
                    }
                    //Debug.Log("[DiscordLogger]: Activity Updated" + LastActivityResult);
                });
                IsUpdatingActivity = false;
            }

            //Activity nextActivity = new Activity
            //{
            //    State = GetState(),
            //    Details = GetDetails(),
            //    Timestamps =
            //    {
            //        Start = lastTimestamp
            //    },
            //    Assets =
            //    {
            //        LargeImage = "logo",
            //        LargeText = GetLargeTooltip(),
            //        SmallImage = "marshmello",
            //        SmallText = GetSmallTooltip(),
                    
            //    },
                
            //};

            //if (LastActivityResult == Result.Ok)
            //    State = DiscordState.Ready;
        }
        public static async void Start(int delay_ = 1000)
        {
            State = DiscordState.Connecting;
            await Task.Delay(delay_);
            Initialize();
        }
        private static float lastClearAttempt = 0f;
        public static void OnEditorUpdate()
        {
            //if (Discord.LobbyManagerInstance == null) return;
            //if (Discord.LobbyManagerInstance.LobbyCount() < 1) return;

            UpdateActiveToggle(out IsActive);
            if (!IsActive)
            {
                if (IsInitialized)
                {
                    if(Discord != null)
                    {
                        if(lastClearAttempt > Time.realtimeSinceStartup)
                        {
                            
                            Discord.ActivityManagerInstance.ClearActivity(result => 
                            {
                                Debug.Log(string.Format("Clearing Discord Activity... System Says: {0}", result.ToString()));
                                if(result == Result.Ok)
                                {
                                    if(Discord != null)
                                    {
                                        Debug.Log(string.Format("Discord Disposed!"));
                                        Discord.Dispose();
                                        //Discord = null;
                                    }
                                    IsInitialized = false;
                                }
                            });
                            lastClearAttempt = Time.realtimeSinceStartup + 5f;
                        }
                    }
                    else
                    {
                        IsInitialized = false;
                    }
                }
                return;
            }
            else
            {
               
                if (!IsInitialized)
                {
                    Initialize();
                    return;
                }
            }

            OnActivityUpdate();
            if (!IsInitialized || !IsActive) return;
            
            Discord.RunCallbacks();
        }
        private static void UpdateActiveToggle(out bool active_)
        {
            active_ = IsActive;
            if (ToggleInterval > 0f)
            {
                if (lastToggle < Time.realtimeSinceStartup)
                {
                    active_ = !active_;
                    lastToggle = Time.realtimeSinceStartup + ToggleInterval;
                }
            }

        }
        public static long GetTimestamp()
        {
            if (!resetOnSceneChange)
            {
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Time.realtimeSinceStartup);
                long timestamp = DateTimeOffset.Now.Add(timeSpan).ToUnixTimeSeconds();
                return timestamp;
            }
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            return unixTimestamp;
        }
    }

  
}