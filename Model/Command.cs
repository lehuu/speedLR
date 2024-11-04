﻿using System.Text.Json.Serialization;

namespace SpeedLR.Model
{
    public class Command
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("short")]
        public string Short { get; set; }
        [JsonPropertyName("commandName")]
        public string CommandName { get; set; }
    }
}