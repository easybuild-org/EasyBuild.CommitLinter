namespace CommitLinter.Commands

open Spectre.Console
open Spectre.Console.Cli
open System.ComponentModel
open System.IO
open CommitLinter
open Thoth.Json.Newtonsoft

type LintSettings() =
    inherit CommandSettings()

    [<CommandOption("-c|--config")>]
    [<Description("Path to the configuration file")>]
    member val Config: string option = None with get, set

    [<CommandArgument(0, "<commit-file>")>]
    [<Description("Path to the commit message file")>]
    // Could be replace with string option, but it seems like
    // Spectre.Console.Cli makes this argument required thanks to '<...>` syntax
    member val CommitFile = "" with get, set

[<RequireQualifiedAccess>]
type LoadConfig =
    | Failed
    | Success of Config.Config

type LintCommand() =
    inherit Command<LintSettings>()

    let tryLoadConfig (settings: LintSettings) =
        match settings.Config with
        | Some configFile ->
            let configFile =
                if Path.IsPathFullyQualified(configFile) then
                    configFile
                else
                    Path.Combine(Directory.GetCurrentDirectory(), configFile) |> Path.GetFullPath

            let configFile = FileInfo(configFile)

            if not configFile.Exists then
                AnsiConsole.MarkupLine(
                    $"[red]Error:[/] Configuration file '{configFile.FullName}' does not exist."
                )

                LoadConfig.Failed
            else

                let configContent = File.ReadAllText(configFile.FullName)

                match Decode.fromString Config.Config.decoder configContent with
                | Ok config -> config |> LoadConfig.Success
                | Error error ->
                    AnsiConsole.MarkupLine(
                        $"[red]Error:[/] Failed to parse configuration file:\n\n{error}"
                    )

                    LoadConfig.Failed

        | None -> Config.defaultConfig |> LoadConfig.Success

    interface ICommandLimiter<LintSettings>

    override __.Execute(context, settings) =

        let commitFilePath =
            if Path.IsPathFullyQualified(settings.CommitFile) then
                settings.CommitFile
            else
                Path.Combine(Directory.GetCurrentDirectory(), settings.CommitFile)
                |> Path.GetFullPath

        let commitFile = FileInfo(commitFilePath)

        if not commitFile.Exists then
            AnsiConsole.MarkupLine($"[red]Error:[/] File '{commitFile.FullName}' does not exist.")
            1
        else

            let commitMessage = File.ReadAllText(commitFile.FullName)

            match tryLoadConfig settings with
            | LoadConfig.Failed -> 1
            | LoadConfig.Success config ->
                match Parser.validateCommitMessage config commitMessage with
                | Ok _ ->
                    AnsiConsole.MarkupLine("[green]Success:[/] Commit message is valid.")
                    0
                | Error error ->
                    AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error)}")
                    1
