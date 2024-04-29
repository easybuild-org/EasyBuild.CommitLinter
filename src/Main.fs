module CommitLinter.Main

open Spectre.Console.Cli
open CommitLinter.Commands

[<EntryPoint>]
let main args =

    let app = CommandApp<LintCommand>()

    app.Configure(fun config ->
        config.Settings.ApplicationName <- "commit-linter"

        config
            .AddCommand<LintCommand>("lint")
            .WithDescription("Lint your commit message")
            .IsHidden()
        |> ignore
    )

    app.Run(args)
