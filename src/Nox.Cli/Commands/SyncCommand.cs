using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Services;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Cli.Commands
{
    public class SyncCommand : AsyncCommand<SyncCommand.Settings>
    {
        private readonly ILogger<SyncCommand> _logger;

        private readonly IConfiguration _configuration;

        public class Settings : CommandSettings
        {
        }

        public SyncCommand(ILogger<SyncCommand> logger, IConfiguration configuration)
        {
            _logger = logger;
            
            _configuration = configuration;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var dynamicService = new DynamicService.Builder()
                .WithLogger(_logger)
                .WithConfiguration(_configuration)
                .FromRootFolder(_configuration["DefinitionRootPath"])
                .Build();

            _ = await dynamicService.ValidateDatabaseSchemaAsync();

            _ = await dynamicService.ExecuteDataLoadersAsync();

            return 1;
        }
    }
}
