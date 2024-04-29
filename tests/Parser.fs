module Parser

open Expecto
open Expecto.NoMessage
open CommitLinter

let private opiniatedTagsConfig: Config.Config =
    {
        Types =
            [
                {
                    Name = "feat"
                    Description = Some "A new feature"
                    SkipTagLine = false
                }
                {
                    Name = "fix"
                    Description = Some "A bug fix"
                    SkipTagLine = false
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
                    SkipTagLine = false
                }
                {
                    Name = "test"
                    Description = Some "Adding missing tests or correcting existing tests"
                    SkipTagLine = false
                }
                {
                    Name = "style"
                    Description =
                        Some
                            "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)"
                    SkipTagLine = false
                }
                {
                    Name = "refactor"
                    Description = Some "A code change that neither fixes a bug nor adds a feature"
                    SkipTagLine = false
                }
            ]
        Tags = None
    }

[<Tests>]
let tests =
    testList
        "Parser"
        [
            testList
                "validateFirstLine"
                [
                    test "works for 'feat' type with default config" {
                        let expected =
                            ({
                                Type = "feat"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }
                            : Parser.CommitMessage)
                            |> Ok

                        let actual =
                            "feat: <description>" |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "works for 'fix' type with default config" {
                        let expected =
                            ({
                                Type = "fix"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }
                            : Parser.CommitMessage)
                            |> Ok

                        let actual =
                            "fix: <description>" |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "works for 'ci' type with default config" {
                        let expected =
                            ({
                                Type = "ci"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }
                            : Parser.CommitMessage)
                            |> Ok

                        let actual =
                            "ci: <description>" |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "works for 'chore' type with default config" {
                        let expected =
                            ({
                                Type = "chore"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }
                            : Parser.CommitMessage)
                            |> Ok

                        let actual =
                            "chore: <description>" |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "works for 'docs' type with default config" {
                        let expected =
                            ({
                                Type = "docs"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }
                            : Parser.CommitMessage)
                            |> Ok

                        let actual =
                            "docs: <description>" |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "works for 'test' type with default config" {
                        let expected =
                            ({
                                Type = "test"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }
                            : Parser.CommitMessage)
                            |> Ok

                        let actual =
                            "test: <description>" |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "works for 'style' type with default config" {
                        let expected =
                            ({
                                Type = "style"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }
                            : Parser.CommitMessage)
                            |> Ok

                        let actual =
                            "style: <description>" |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "works for 'refactor' type with default config" {
                        let expected =
                            ({
                                Type = "refactor"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }
                            : Parser.CommitMessage)
                            |> Ok

                        let actual =
                            "refactor: <description>"
                            |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "works for 'invalid' type with default config" {
                        let expected =
                            "Invalid commit message format.

Expected a commit message with the following format: '<type>[optional scope]: <description>'.

Where <type> is one of the following:

- feat: A new feature
- fix: A bug fix
- ci: Changes to CI/CD configuration
- chore: Changes to the build process or auxiliary tools and libraries such as documentation generation
- docs: Documentation changes
- test: Adding missing tests or correcting existing tests
- style: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- refactor: A code change that neither fixes a bug nor adds a feature

Example:
-------------------------
feat: some description
-------------------------"
                            |> Error

                        let actual =
                            "invalid: <description>"
                            |> Parser.validateFirstLine Config.defaultConfig

                        Expect.equal actual expected
                    }

                ]

            testList
                "validateSecondLine"
                [
                    test "should works for empty line" {
                        let expected = Ok()

                        let actual = "" |> Parser.validateSecondLine

                        Expect.equal actual expected
                    }

                    test "should fail for non-empty line" {
                        let expected =
                            "Invalid commit message format.

Expected an empty line after the commit message.

Example:
-------------------------
feat: add new feature

-------------------------"

                            |> Error

                        let actual = "non empty line" |> Parser.validateSecondLine

                        Expect.equal actual expected
                    }
                ]

            testList
                "validateTagLine"
                [
                    test "works with an empty line if the type is flagged as 'SkipTagLine'" {
                        let commitMessage: Parser.CommitMessage =
                            {
                                Type = "feat"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }

                        let expected = None |> Ok

                        let actual = "" |> Parser.validateTagLine Config.defaultConfig commitMessage

                        Expect.equal actual expected
                    }

                    test "return the tag if the type is flagged as 'SkipTagLine'" {
                        let commitMessage: Parser.CommitMessage =
                            {
                                Type = "feat"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }

                        let expected = [ "converter" ] |> Some |> Ok

                        let actual =
                            "[converter]"
                            |> Parser.validateTagLine Config.defaultConfig commitMessage

                        Expect.equal actual expected
                    }

                    test "return the list of tags if the type is flagged as 'SkipTagLine'" {
                        let commitMessage: Parser.CommitMessage =
                            {
                                Type = "feat"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }

                        let expected = [ "converter"; "web" ] |> Some |> Ok

                        let actual =
                            "[converter][web]"
                            |> Parser.validateTagLine Config.defaultConfig commitMessage

                        Expect.equal actual expected
                    }

                    test "return an error if the type is not flagged as 'SkipTagLine'" {
                        let commitMessage: Parser.CommitMessage =
                            {
                                Type = "feat"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }

                        let expected =
                            Error
                                "Invalid commit message format.

Expected a tag line with the following format: '[tag1][tag2]...[tagN]'

Example:
-------------------------
feat: add new feature

[tag1][tag2]
-------------------------"

                        let actual = "" |> Parser.validateTagLine opiniatedTagsConfig commitMessage

                        Expect.equal actual expected
                    }

                    test
                        "return an error if tag line is in an invalid format and the type is not flagged as 'SkipTagLine'" {
                        let commitMessage: Parser.CommitMessage =
                            {
                                Type = "feat"
                                Scope = None
                                Description = "<description>"
                                BreakingChange = false
                            }

                        let expected =
                            Error
                                "Invalid commit message format.

Expected a tag line with the following format: '[tag1][tag2]...[tagN]'

Example:
-------------------------
feat: add new feature

[tag1][tag2]
-------------------------"

                        let actual =
                            "invalid tag line"
                            |> Parser.validateTagLine opiniatedTagsConfig commitMessage

                        Expect.equal actual expected
                    }
                ]

            testList
                "validateCommitMessage"
                [
                    test "works for short commit message only" {
                        let actual =
                            "feat: add new feature"
                            |> Parser.validateCommitMessage Config.defaultConfig

                        Expect.equal actual (Ok())
                    }

                    test "works for commit message / tag line" {
                        let actual =
                            "feat: add new feature

[converter]"
                            |> Parser.validateCommitMessage Config.defaultConfig

                        Expect.equal actual (Ok())
                    }

                    test "works for commit message / tag line / body message" {
                        let actual =
                            "feat: add new feature

[converter]

This is the body message"
                            |> Parser.validateCommitMessage Config.defaultConfig

                        Expect.equal actual (Ok())
                    }

                    test "works for commit message / body message if tag line is not required" {
                        let actual =
                            "feat: add new feature

This is the body message"

                            |> Parser.validateCommitMessage Config.defaultConfig

                        Expect.equal actual (Ok())
                    }

                    test
                        "returns an error if an empty line is missing between the commit message and body" {
                        let expected =
                            "Invalid commit message format.

Expected an empty line after the commit message.

Example:
-------------------------
feat: add new feature

-------------------------"
                            |> Error

                        let actual =
                            "feat: add new feature
This is the body message"

                            |> Parser.validateCommitMessage Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test
                        "returns an error if an empty line is missing between the commit message and tag line" {
                        let expected =
                            "Invalid commit message format.

Expected an empty line after the commit message.

Example:
-------------------------
feat: add new feature

-------------------------"
                            |> Error

                        let actual =
                            "feat: add new feature
[converter]"

                            |> Parser.validateCommitMessage Config.defaultConfig

                        Expect.equal actual expected
                    }

                    test "returns an error if an empty line is missing after the tag line and body" {
                        let expected =
                            "Invalid commit message format.

Expected an empty line after the tag line.

Example:
-------------------------
feat: add new feature

[tag1][tag2]

-------------------------"
                            |> Error

                        let actual =
                            "feat: add new feature

[converter]
This is the body message"

                            |> Parser.validateCommitMessage Config.defaultConfig

                        Expect.equal actual expected
                    }

                ]
        ]
