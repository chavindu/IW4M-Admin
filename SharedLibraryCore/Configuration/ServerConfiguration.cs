﻿using SharedLibraryCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedLibraryCore.Configuration
{
    public class ServerConfiguration : IBaseConfiguration
    {
        public string IPAddress { get; set; }
        public ushort Port { get; set; }
        public string Password { get; set; }
        public IList<string> Rules { get; set; }
        public IList<string> AutoMessages { get; set; }
        public string ManualLogPath { get; set; }
        public string CustomParserVersion { get; set; }
        public int ReservedSlotNumber { get; set; }

        private readonly IList<IRConParser> rconParsers;
        private readonly IList<IEventParser> eventParsers;

        public ServerConfiguration()
        {
            rconParsers = new List<IRConParser>();
            eventParsers = new List<IEventParser>();
        }

        public void AddRConParser(IRConParser parser) => rconParsers.Add(parser);
        public void AddEventParser(IEventParser parser) => eventParsers.Add(parser);

        public IBaseConfiguration Generate()
        {
            var loc = Utilities.CurrentLocalization.LocalizationIndex;

            while (string.IsNullOrEmpty(IPAddress))
            {
                string input = Utilities.PromptString(loc["SETUP_SERVER_IP"]);

                if (System.Net.IPAddress.TryParse(input, out System.Net.IPAddress ip))
                    IPAddress = input;
            }

            while(Port < 1)
            {
                string input = Utilities.PromptString(loc["SETUP_SERVER_PORT"]);
                if (UInt16.TryParse(input, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture, out ushort port))
                    Port = port;
            }

            Password = Utilities.PromptString(loc["SETUP_SERVER_RCON"]);
            AutoMessages = new List<string>();
            Rules = new List<string>();
            ReservedSlotNumber = loc["SETUP_SERVER_RESERVEDSLOT"].PromptInt(null, 0, 32);

            var parserVersions = rconParsers.Select(_parser => _parser.Version).ToArray();
            var selection = Utilities.PromptSelection(loc["SETUP_SERVER_RCON_PARSER_VERSION"], $"{loc["SETUP_PROMPT_DEFAULT"]} (IW4x)", null, parserVersions);

            if (selection.Item1 > 0)
            {
                CustomParserVersion = selection.Item2;
            }

            parserVersions = eventParsers.Select(_parser => _parser.Version).ToArray();
            selection = Utilities.PromptSelection(loc["SETUP_SERVER_EVENT_PARSER_VERSION"], $"{loc["SETUP_PROMPT_DEFAULT"]} (IW4x)", null, parserVersions);

            if (selection.Item1 > 0)
            {
                CustomParserVersion = selection.Item2;
            }

            return this;
        }

        public string Name() => "ServerConfiguration";
    }
}
