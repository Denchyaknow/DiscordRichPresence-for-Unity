using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yakno.Discord
{
    // [CreateAssetMenu(fileName ="DiscordConfig", menuName = "Configs/Discord")]

    
    [System.Serializable]
    public class YaknoDiscordSettings
    {
        public YaknoDiscordSettings()
        {
            //StateMessages = new List<string>(5)
            //{
            //    "State 0",
            //    "State 1",
            //    "State 2",
            //    "Dencho is Here 3",
            //    "State 4"
            //};
            //DetailMessages = new List<string>(5)
            //{
            //    "Details 5",
            //    "Details 4",
            //    "Details 3",
            //    "Details 2",
            //    "Dencho is Gone",
            //};
            //BigTooltips = new List<string>(4)
            //{
            //    "Tooltip 0",
            //    "Tooltip 1",
            //    "Tooltip 2",
            //    "Tooltip 3",
            //};
            //SmallTooltips = new List<string>(4)
            //{
            //    "SmallTooltip 0",
            //    "SmallTooltip 1",
            //    "SmallTooltip 2",
            //    "SmallTooltip 3",
            //};
        }
        public string ApplicationID = "805401847850205204";
        public string ApplicationSecret = "amoklywgLYeGJDcPyFD3dvLLUy0fX_Ux";
        public string GameName = "AFunVRGame";
        public List<string> SecondaryMessages = new List<string>(5)
        {
            "State 0",
                "State 1",
                "State 2",
                "Dencho is Here 3",
                "State 4"
        };
        public List<string> PrimaryMessages = new List<string>(5)
             {
                "Details 5",
                "Details 4",
                "Details 3",
                "Details 2",
                "Dencho is Gone",
            };
        public List<string> BigTooltips = new List<string>(4)
              {
                "Tooltip 0",
                "Tooltip 1",
                "Tooltip 2",
                "Tooltip 3",
            };
        public List<string> SmallTooltips = new List<string>(4)
            {
                "SmallTooltip 0",
                "SmallTooltip 1",
                "SmallTooltip 2",
                "SmallTooltip 3",
            };
    }
}