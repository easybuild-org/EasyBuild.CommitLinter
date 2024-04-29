module CommitLinter.Config

open Thoth.Json.Core

type Type =
    {
        Name: string
        Description: string option
        SkipTagLine: bool
    }

module Type =

    let decoder: Decoder<Type> =
        Decode.object (fun get ->
            {
                Name = get.Required.Field "name" Decode.string
                Description = get.Optional.Field "description" Decode.string
                SkipTagLine =
                    get.Optional.Field "skipTagLine" Decode.bool |> Option.defaultValue true
            }
        )

type Config =
    {
        Types: Type list
        Tags: string list option
    }

module Config =

    let decoder: Decoder<Config> =
        Decode.object (fun get ->
            {
                Types = get.Required.Field "types" (Decode.list Type.decoder)
                Tags = get.Optional.Field "tags" (Decode.list Decode.string)
            }
        )

let defaultConfig =
    {
        Types =
            [
                {
                    Name = "feat"
                    Description = Some "A new feature"
                    SkipTagLine = true
                }
                {
                    Name = "fix"
                    Description = Some "A bug fix"
                    SkipTagLine = true
                }
                {
                    Name = "ci"
                    Description = Some "Changes to CI/CD configuration"
                    SkipTagLine = true
                }
                {
                    Name = "chore"
                    Description =
                        Some
                            "Changes to the build process or auxiliary tools and libraries such as documentation generation"
                    SkipTagLine = true
                }
                {
                    Name = "docs"
                    Description = Some "Documentation changes"
                    SkipTagLine = true
                }
                {
                    Name = "test"
                    Description = Some "Adding missing tests or correcting existing tests"
                    SkipTagLine = true
                }
                {
                    Name = "style"
                    Description =
                        Some
                            "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)"
                    SkipTagLine = true
                }
                {
                    Name = "refactor"
                    Description = Some "A code change that neither fixes a bug nor adds a feature"
                    SkipTagLine = true
                }
            ]
        Tags = None
    }
