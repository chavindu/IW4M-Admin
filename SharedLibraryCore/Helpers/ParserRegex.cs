﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraryCore.Interfaces
{
    public sealed class ParserRegex
    {
        public enum GroupType
        {
            EventType,
            OriginNetworkId,
            TargetNetworkId,
            OriginClientNumber,
            TargetClientNumber,
            OriginName,
            TargetName,
            OriginTeam,
            TargetTeam,
            Weapon,
            Damage,
            MeansOfDeath,
            HitLocation,
            Message,
            RConClientNumber = 100,
            RConScore = 101,
            RConPing = 102,
            RConNetworkId = 103,
            RConName = 104,
            RConIpAddress = 105,
            RConDvarName = 106,
            RConDvarValue = 107,
            RConDvarDefaultValue = 108,
            RConDvarLatchedValue = 109,
            RConDvarDomain = 110,
            AdditionalGroup = 200
        }
        public string Pattern { get; set; }
        public Dictionary<GroupType, int> GroupMapping { get; private set; }

        public void AddMapping(object mapKey, object mapValue)
        {
            if (int.TryParse(mapKey.ToString(), out int key) && int.TryParse(mapValue.ToString(), out int value))
            {
                if (GroupMapping.ContainsKey((GroupType)key))
                {
                    GroupMapping[(GroupType)key] = value;
                }

                else
                {
                    GroupMapping.Add((GroupType)key, value);
                }
            }
        }

        public ParserRegex()
        {
            GroupMapping = new Dictionary<GroupType, int>();
        }
    }
}
