﻿using ESPresense.Models;
using ESPresense.Services;
using Microsoft.AspNetCore.Mvc;

namespace ESPresense.Controllers;

[Route("api/node")]
[ApiController]
public class NodeController(NodeSettingsStore nodeSettingsStore, State state) : ControllerBase
{
    [HttpGet("{id}/settings")]
    public NodeSettingsDetails Get(string id)
    {
        var nodeSettings = nodeSettingsStore.Get(id);
        var details = new List<KeyValuePair<string, string>>();
        if (nodeSettings?.Id != null && state.Nodes.TryGetValue(id, out var node))
            details.AddRange(node.GetDetails());
        return new NodeSettingsDetails(nodeSettings ?? new Models.NodeSettings(id), details);
    }

    [HttpPut("{id}/settings")]
    public Task Set(string id, [FromBody] Models.NodeSettings ds)
    {
        return nodeSettingsStore.Set(id, ds);
    }

    [HttpPost("{id}/update")]
    public async Task Update(string id)
    {
        await nodeSettingsStore.Update(id);
    }

    [HttpPost("{id}/restart")]
    public async Task Restart(string id)
    {
        await nodeSettingsStore.Restart(id);
    }

    public readonly record struct NodeSettingsDetails(Models.NodeSettings? settings, IList<KeyValuePair<string, string>> details);
}