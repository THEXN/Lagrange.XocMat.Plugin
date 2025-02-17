﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Music.QQ.Internal;

public class Req
{
    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("data")]
    public JToken? Data { get; set; }
}
