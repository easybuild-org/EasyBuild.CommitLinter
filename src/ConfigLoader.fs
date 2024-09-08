module CommitLinter.ConfigLoader

open EasyBuild.CommitParser.Types
open System.IO
open Spectre.Console
open Thoth.Json.Newtonsoft

[<RequireQualifiedAccess>]
type LoadConfig =
    | Failed
    | Success of CommitParserConfig

let tryLoadConfig (configPath: string option) =
    match configPath with
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

            match Decode.fromString CommitParserConfig.decoder configContent with
            | Ok config -> config |> LoadConfig.Success
            | Error error ->
                AnsiConsole.MarkupLine(
                    $"[red]Error:[/] Failed to parse configuration file:\n\n{error}"
                )

                LoadConfig.Failed

    | None -> CommitParserConfig.Default |> LoadConfig.Success
