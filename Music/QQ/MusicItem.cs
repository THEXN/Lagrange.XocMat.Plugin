﻿using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Music.QQ;

public class MusicItem
{
    /// <summary>
    /// 稻香
    /// </summary>
    [JsonProperty("song")]
    public string Song { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("singer")]
    public List<string> Singer { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("mid")]
    public string Mid { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("songid")]
    public int Songid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("picture")]
    public string Picture { get; set; } = string.Empty;
}