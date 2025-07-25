// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using AzureMcp.Options;

namespace AzureMcp.Areas.Cosmos.Services;

public interface ICosmosService : IDisposable
{
    Task<List<string>> GetCosmosAccounts(
        string subscriptionId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListDatabases(
        string accountName,
        string subscriptionId,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListContainers(
        string accountName,
        string databaseName,
        string subscriptionId,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<JsonElement>> QueryItems(
        string accountName,
        string databaseName,
        string containerName,
        string? query,
        string subscriptionId,
        AuthMethod authMethod = AuthMethod.Credential,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
