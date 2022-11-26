using System.ComponentModel;
using Spectre.Console.Cli;

namespace Nox.Cli.Commands.New;

public class NewSettings: CommandSettings
{
    [CommandArgument(0, "<Project Name>")]
    [Description("The name of your new Api project.")]
    public string? Name { get; set; }
}