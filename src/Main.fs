module CommitLinter.Main

open Spectre.Console.Cli
open CommitLinter.Commands.Lint

[<EntryPoint>]
let main args =

    let app = CommandApp<LintCommand>()

    app
        .WithDescription(
            "Lint your commit message based on EasyBuild.CommitLinter conventions

Learn more at https://github.com/easybuild-org/EasyBuild.CommitLinter"
        )
        .Configure(fun config ->
            config.Settings.ApplicationName <- "commit-linter"

            config
                .AddCommand<LintCommand>("lint")
                .WithDescription("Lint your commit message")
                .IsHidden()
            |> ignore
        )

    app.Run(args)
