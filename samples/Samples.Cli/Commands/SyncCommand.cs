using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Nox.Core.Interfaces;
using Nox.Etl;
using Nox.Microservice;

namespace Samples.Cli.Commands;

public class SyncCommand : AsyncCommand<SyncCommand.Settings>
{
    private readonly ILogger<DynamicService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEtlExecutor _etlExecutor;
    private readonly IDynamicService _dynamicService;

    public class Settings : CommandSettings
    {
    }

    public SyncCommand(
        ILogger<DynamicService> logger, 
        IConfiguration configuration, 
        IEtlExecutor etlExecutor,
        IDynamicService dynamicService)
    {
        _logger = logger;
        _configuration = configuration;
        _etlExecutor = etlExecutor;
        _dynamicService = dynamicService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        //var dynamicService = new DynamicService(_logger, _configuration, _loaderExecutor);

        _ = await _dynamicService.ExecuteDataLoadersAsync();

        return 1;
    }
}
