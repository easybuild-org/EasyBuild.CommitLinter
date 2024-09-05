namespace CommitLinter

open Spectre.Console.Cli
open CommitLinter.Commands.Lint
open CommitLinter.Commands.Interactive

module Main =

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

                config
                    .AddCommand<InteractiveCommand>("interactive")
                    .WithDescription("Interactively create a commit message")
                |> ignore

            )

        app.Run(args)
