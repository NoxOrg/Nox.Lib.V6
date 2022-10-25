using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.Loaders;
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
        private readonly ILogger<DynamicService> _logger;

        private readonly IConfiguration _configuration;

        private readonly ILoaderExecutor _loaderExecutor;

        public class Settings : CommandSettings
        {
        }

        public SyncCommand(ILogger<DynamicService> logger, IConfiguration configuration, ILoaderExecutor loaderExecutor)
        {
            _logger = logger;
            
            _configuration = configuration;
            
            _loaderExecutor = loaderExecutor;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var dynamicService = new DynamicService(_logger, _configuration, _loaderExecutor);

            _ = await dynamicService.ExecuteDataLoadersAsync();

            return 1;
        }
    }
}
