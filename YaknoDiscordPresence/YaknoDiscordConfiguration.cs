using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yakno.Discord
{

    [CreateAssetMenu(fileName = "DiscordConfiguration", menuName = "Yakno/Discord/Configuration", order = 1000)]
    public class YaknoDiscordConfiguration : ScriptableObject
    {
        public YaknoDiscordSettings Settings = new YaknoDiscordSettings();

    }

}