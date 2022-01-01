using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Yakno.Discord
{

            [InitializeOnLoad]
    public class YaknoDiscordWindow : EditorWindow
    {
        public YaknoDiscordSettings DiscordSettings
        {
            get => m_DiscordSettings;
        }
        public YaknoDiscordSettings m_DiscordSettings = null;
        public new GUIContent titleContent = new GUIContent("",null,"Dencho was here");

        private static string defaultTitle = "Syntek Discord Manager";
        private static string currentTitleTooltip = "Made by Denchyaknow";

        private static Texture passTexture = null;
        private static Texture connectingTexture = null;
        private static Texture failTexture = null;
        private static string passTexName = "greenLight";
        private static string connectingTexName = "yellowLight";
        private static string failTexName = "redLight";

        private static YaknoDiscordPresence.DiscordState currentState = YaknoDiscordPresence.DiscordState.Stopped;
        private static YaknoDiscordPresence.DiscordState lastState = YaknoDiscordPresence.DiscordState.Stopped;
        private static bool isConnecting = false;
        private static float lastInitTime = 0f;
        private static YaknoDiscordWindow DiscordWindow;
        static YaknoDiscordWindow()
        {
            if (!isInitialized)
            {
                //if (PlayerPrefs.HasKey("DiscordConfig"))
                //{
                //    CurrentConfig = (YaknoDiscordConfiguration)Resources.Load(PlayerPrefs.GetString("DiscordConfig"), typeof(YaknoDiscordConfiguration));
                //}
                EditorApplication.update += Update;
                isInitialized = true;
                Debug.Log(string.Format("Initialized Yakno Discord!"));
            }
        }

        [MenuItem("Tools/Discord")]
        private static void Init()
        {
            
            DiscordWindow = (YaknoDiscordWindow)GetWindow(typeof(YaknoDiscordWindow), false, "Editor Rich Presence");
            DiscordWindow.Show();
            
        }
        private static bool isInitialized = false;
        public static YaknoDiscordConfiguration CurrentConfig = null;
        private string newConfigName = "DefaultDiscordConfiguration";
        private const string HASH_ConfigKey = "SuperAwesomeRhino";
        private static bool NeedsDefaultConfig = false;
        private static bool CreatingDefaultConfig = false;
        SerializedProperty prop;
        private void OnGUI()
        {
            if (!isInitialized)
            {
                EditorApplication.update += Update;
                isInitialized = true;
            }
            GUILayout.Label(titleContent, EditorStyles.boldLabel);
            UpdateTitle();
           if(CreatingDefaultConfig)
            {
                CreatingDefaultConfig = false;
                NeedsDefaultConfig = false;
                if (newConfigName.Length < 1)
                    newConfigName = "DefaultDiscordConfiguration";
                CurrentConfig = (YaknoDiscordConfiguration)YaknoDiscordConfiguration.CreateInstance(typeof(YaknoDiscordConfiguration));
                CurrentConfig.name = newConfigName;
                AssetDatabase.CreateAsset(CurrentConfig, string.Format("Assets/Resources/{0}.asset", newConfigName));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return;
            }

            EditorGUI.BeginChangeCheck();
            if (NeedsDefaultConfig)
            {
                using(var HoriScope = new GUILayout.HorizontalScope())
                {
                    newConfigName = GUILayout.TextField(newConfigName, 25, EditorStyles.miniTextField);
                    if (GUILayout.Button("Create New Config", EditorStyles.miniButton))
                    {
                        CreatingDefaultConfig = true;
                    }
                }
            }

            CurrentConfig = (YaknoDiscordConfiguration)EditorGUILayout.ObjectField(CurrentConfig, typeof(YaknoDiscordConfiguration));
            
                //string path = AssetDatabase.GetAssetPath(CurrentConfig);
               // PlayerPrefs.SetString(HASH_ConfigKey,path);
                //Debug.Log(string.Format("Discord: Configuration set to {0}", path));
           

            //var prop = new SerializedProperty();
        
            if (YaknoDiscordPresence.ToggleInterval < 0)
                YaknoDiscordPresence.ToggleInterval = 0f;
            YaknoDiscordPresence.ToggleInterval = EditorGUILayout.FloatField("Toggle Interval", YaknoDiscordPresence.ToggleInterval);
            using (var activeScope = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(string.Format("Rich Presence is {0}",YaknoDiscordPresence.IsActive));
                if (GUILayout.Button(string.Format("{0} the Rich Discord Presence", YaknoDiscordPresence.IsActive ? "Disable" : "Enable")))
                {
                    YaknoDiscordPresence.IsActive = !YaknoDiscordPresence.IsActive;
                }
            }
            UpdateActivity();
            DrawActivityConfiguration();   
            
            if(EditorGUI.EndChangeCheck())
            {
                if (CurrentConfig != null)
                {
                    if(lastChangeCheck < Time.realtimeSinceStartup)
                    {

                        EditorUtility.SetDirty(CurrentConfig);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        lastChangeCheck = Time.realtimeSinceStartup + 2f;
                    }
                }
            }
        }
        private float lastChangeCheck = 0f;
        private float activityTime => YaknoDiscordPresence.LastActivityUpdate > Time.realtimeSinceStartup ? YaknoDiscordPresence.LastActivityUpdate - Time.realtimeSinceStartup : 0f;
        private void UpdateActivity()
        {
            //GUILayout.Label(string.Format("LastState: {0}\n{1}", activityTime.ToString("F3"), YaknoDiscordPresence.LastActivityResult),EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label(string.Format("Activity: {0}\nLast Activity Result: {1}\nLast Command Result: {2}", 
                activityTime.ToString("F3"), YaknoDiscordPresence.LastActivityResult,
                YaknoDiscordPresence.LastLoggedState),EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label(string.Format("Current Status: {0}", YaknoDiscordPresence.LastActivityStatus), EditorStyles.boldLabel);
            GUILayout.Label(string.Format("Current Details: {0}", YaknoDiscordPresence.LastActivityDetails), EditorStyles.boldLabel);
            GUILayout.Label(string.Format("Current Small: {0}", YaknoDiscordPresence.LastActivitySmallTip), EditorStyles.boldLabel);
            GUILayout.Label(string.Format("Current Large: {0}", YaknoDiscordPresence.LastActivityBigTip), EditorStyles.boldLabel);
            //, defaultTitle,time.ToString("F3"), SyntekDiscordPresence.LastActivityResult
        }
        
        private void DrawActivityConfiguration()
        {
            using(var ScrollCon = new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Presence Settings", EditorStyles.whiteLargeLabel);
                listScroller = GUILayout.BeginScrollView(listScroller);
                bool toggle = listToggles[0];
                var settings = YaknoDiscordPresence.ActivitySettings;
                var list = settings.PrimaryMessages;
                DrawStringList(string.Format("Primary Ticker"), ref toggle, ref list);
                settings.PrimaryMessages = list;
                listToggles[0] = toggle;

                toggle = listToggles[1];
                list = settings.SecondaryMessages;
                DrawStringList(string.Format("Secondary Ticker"), ref toggle, ref list);
                settings.SecondaryMessages = list;
                listToggles[1] = toggle;

                toggle = listToggles[2];
                list = settings.SmallTooltips;
                DrawStringList(string.Format("SmallTips Ticker"), ref toggle, ref list);
                settings.SmallTooltips = list;
                listToggles[2] = toggle;

                toggle = listToggles[3];
                list = settings.BigTooltips;
                DrawStringList(string.Format("BigTips Ticker"), ref toggle, ref list);
                settings.BigTooltips = list;
                listToggles[3] = toggle;

                GUILayout.EndScrollView();
            }
        }
        private bool[] listToggles = new bool[4];
        private Vector2 listScroller = Vector2.zero;
        private void DrawStringList(string name_,ref bool toggle_,ref List<string> list_)
        {
            
                using (var HoriCon = new GUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                   toggle_ = GUILayout.Toggle(toggle_, string.Format("{0}: {1}", toggle_?"Hide":"Show",name_),EditorStyles.toggle);
                }
                if(toggle_)
                {
            using(var VertCon = new GUILayout.VerticalScope(EditorStyles.helpBox,GUILayout.ExpandHeight(false)))
            {
                    for(int i = 0; i < list_.Count;i++)
                    {
                        bool del = false;
                        using(var HoriCon = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(i.ToString(), EditorStyles.miniBoldLabel, GUILayout.Width(15f));
                            var element = list_[i];
                            element = GUILayout.TextArea(element, 120, EditorStyles.textArea, GUILayout.ExpandWidth(true));
                            list_[i] = element;

                            if (GUILayout.Button("X", GUILayout.Width(25f)))
                            {
                                del = true;
                            }
                        }
                        if(del)
                        {
                            list_.RemoveAt(i);
                            break;
                        }
                    }
                    using(var HoriCon = new GUILayout.HorizontalScope())
                    {
                        if(GUILayout.Button("Add", list_.Count > 0?EditorStyles.miniButtonLeft:EditorStyles.miniButton))
                        {
                            list_.Add("...");
                        }
                        if(list_.Count > 0)
                        {
                            if(GUILayout.Button("Remove", EditorStyles.miniButtonRight))
                            {
                                list_.RemoveAt(list_.Count - 1);
                            }
                        }
                    }
                }
            }
        }
        private void UpdateTitle()
        {
                switch(YaknoDiscordPresence.State)
                {
                    case YaknoDiscordPresence.DiscordState.Stopped:
                        //titleContent.text = string.Format("{0} (Disconnected)", defaultTitle);
                        if (failTexture == null)
                            failTexture = EditorGUIUtility.FindTexture(failTexName);
                        titleContent.image = failTexture;
                        break;
                    case YaknoDiscordPresence.DiscordState.Ready:
                        //titleContent.text = string.Format("{0} (Connected)", defaultTitle);
                        if (passTexture == null)
                            passTexture = EditorGUIUtility.FindTexture(passTexName);
                        titleContent.image = passTexture;
                        break;
                    case YaknoDiscordPresence.DiscordState.Connecting:
                        //titleContent.text = string.Format("{0} (Reconnecting: {1})", defaultTitle, (lastInitTime > Time.realtimeSinceStartup ? ((lastInitTime - Time.realtimeSinceStartup) / 5) : 0).ToString("F3"));
                        if (connectingTexture == null)
                            connectingTexture = EditorGUIUtility.FindTexture(connectingTexName);
                        titleContent.image = connectingTexture;
                        break;
                }
                titleContent.text = string.Format("{0} Status:({1})", defaultTitle, YaknoDiscordPresence.LastActivityStatus);
                titleContent.tooltip = currentTitleTooltip;
            
        }
        
        private static float lastUpdate = 0f;
        private static void Update()
        {
            if (CurrentConfig == null)
            {
                if (YaknoDiscordPresence.LoadedConfiguration != null)
                {
                    CurrentConfig = YaknoDiscordPresence.LoadedConfiguration;
                    NeedsDefaultConfig = false;
                }
                else
                {
                    CurrentConfig = Resources.Load<YaknoDiscordConfiguration>("DefaultDiscordConfiguration");
                    //AssetDatabase.LoadAssetAtPath<YaknoDiscordConfiguration>(Application.persistentDataPath + "/DefaultDiscordConfiguration.asset");
                    //Resources.Load<YaknoDiscordConfiguration>("DefaultDiscordConfiguration");
                    if (CurrentConfig == null)
                    {
                        NeedsDefaultConfig = true;
                    }
                    else
                    {
                        NeedsDefaultConfig = false;
                    }
                }
            }
            else
            {
                NeedsDefaultConfig = false;
                CreatingDefaultConfig = false;
            }
            if (YaknoDiscordPresence.LoadedConfiguration != CurrentConfig)
            {
                YaknoDiscordPresence.LoadedConfiguration = CurrentConfig;
            }
            if (DiscordWindow == null)
            {
                if(focusedWindow != null && focusedWindow.GetType() == typeof(YaknoDiscordWindow))
                    DiscordWindow = (YaknoDiscordWindow)focusedWindow;//(SyntekDiscordWindow)GetWindow(typeof(SyntekDiscordWindow), false, "Discord");
            }
            else if(DiscordWindow != null)
            {
                DiscordWindow.Repaint();

            }
            //if (YaknoDiscordPresence.Discord == null)
            //{
            //    currentState = YaknoDiscordPresence.DiscordState.Stopped;

            //    if (!isConnecting && YaknoDiscordPresence.State == YaknoDiscordPresence.DiscordState.Stopped)
            //    {
            //        isConnecting = true;
            //        lastInitTime = Time.realtimeSinceStartup + 5;
            //        YaknoDiscordPresence.Start(5000);
            //    }
            //    else
            //    {
            //        if (Time.realtimeSinceStartup > lastInitTime)
            //        {
            //            currentState = YaknoDiscordPresence.DiscordState.Connecting;
            //            isConnecting = false;
            //        }
            //    }
            //    return;
            //}
            //if (SyntekDiscordPresence.Discord == null) return;
            //currentState = YaknoDiscordPresence.State;
            YaknoDiscordPresence.OnEditorUpdate();
            //if (lastUpdate > Time.realtimeSinceStartup) return;
            //lastUpdate = Time.realtimeSinceStartup + 2f;
        }
    }

}