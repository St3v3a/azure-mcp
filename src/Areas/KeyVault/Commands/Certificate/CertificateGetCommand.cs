// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Areas.KeyVault.Options;
using AzureMcp.Areas.KeyVault.Options.Certificate;
using AzureMcp.Areas.KeyVault.Services;
using AzureMcp.Commands.KeyVault;
using AzureMcp.Commands.Subscription;
using AzureMcp.Services.Telemetry;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.KeyVault.Commands.Certificate;

public sealed class CertificateGetCommand(ILogger<CertificateGetCommand> logger) : SubscriptionCommand<CertificateGetOptions>
{
    private const string CommandTitle = "Get Key Vault Certificate";
    private readonly ILogger<CertificateGetCommand> _logger = logger;
    private readonly Option<string> _vaultOption = KeyVaultOptionDefinitions.VaultName;
    private readonly Option<string> _certificateOption = KeyVaultOptionDefinitions.CertificateName;

    public override string Name => "get";

    public override string Title => CommandTitle;

    public override string Description =>
        """
        Gets a certificate from an Azure Key Vault. This command retrieves and displays details
        about a specific certificate in the specified vault.
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_vaultOption);
        command.AddOption(_certificateOption);
    }

    protected override CertificateGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VaultName = parseResult.GetValueForOption(_vaultOption);
        options.CertificateName = parseResult.GetValueForOption(_certificateOption);
        return options;
    }

    [McpServerTool(Destructive = false, ReadOnly = true, Title = CommandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            context.Activity?.WithSubscriptionTag(options);

            var keyVaultService = context.GetService<IKeyVaultService>();
            var certificate = await keyVaultService.GetCertificate(
                options.VaultName!,
                options.CertificateName!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new CertificateGetCommandResult(
                    certificate.Name,
                    certificate.Id,
                    certificate.KeyId,
                    certificate.SecretId,
                    Convert.ToBase64String(certificate.Cer),
                    certificate.Properties.X509ThumbprintString,
                    certificate.Properties.Enabled,
                    certificate.Properties.NotBefore,
                    certificate.Properties.ExpiresOn,
                    certificate.Properties.CreatedOn,
                    certificate.Properties.UpdatedOn,
                    certificate.Policy.Subject,
                    certificate.Policy.IssuerName),
                KeyVaultJsonContext.Default.CertificateGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificate {CertificateName} from vault {VaultName}", options.CertificateName, options.VaultName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record CertificateGetCommandResult(string Name, Uri Id, Uri KeyId, Uri SecretId, string Cer, string Thumbprint, bool? Enabled, DateTimeOffset? NotBefore, DateTimeOffset? ExpiresOn, DateTimeOffset? CreatedOn, DateTimeOffset? UpdatedOn, string Subject, string IssuerName);
}
