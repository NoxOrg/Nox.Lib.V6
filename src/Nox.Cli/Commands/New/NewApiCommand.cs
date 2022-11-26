using System.ComponentModel;
using Spectre.Console.Cli;

namespace Nox.Cli.Commands.New;

[Description("Create a new Nox microservice Api project.")]
public class NewApiCommand: AsyncCommand<NewApiCommand.Settings>
{
    public sealed class Settings : NewSettings
    {
        
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        throw new NotImplementedException();
    }
}