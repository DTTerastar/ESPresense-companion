﻿using System.Text.Json.Serialization;

namespace ESPresense.Models;

public record HassIoMqtt(
    [property: JsonPropertyName("addon")] string Addon,
    [property: JsonPropertyName("host")] string Host,
    [property: JsonPropertyName("port")] string Port,
    [property: JsonPropertyName("ssl")] bool Ssl,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("protocol")] string Protocol
);

public record HassIoError(
    [property: JsonPropertyName("result")] string Result,
    [property: JsonPropertyName("message")] string Message
);