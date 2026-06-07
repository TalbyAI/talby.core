# https://just.systems

set shell := ["bash", "-uc"]
set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

# Show the help message by default.
default: help

# Show this help message.
help:
    @just --list

# Run PowerShell commands
powershell +command:
    {{command}}

# Run bash commands
bash +command:
    @bash -lc "{{command}}"

# Format C# code using CSharpier.
format:
    @dotnet csharpier format .

gitnexus-update:
    @gitnexus analyze --no-stats

# Restore the project dependencies.
restore:
    @dotnet restore

# Build the project.
build:
    @dotnet build

# Run the tests.
test:
    @dotnet test

# Run the tests with Coverlet coverage collection.
coverage:
    @dotnet test Talby.Core.slnx --collect:"XPlat Code Coverage" --results-directory ./TestResults/Coverage

# Run the tests and generate an HTML coverage summary.
coverage-report:
    @dotnet tool restore
    @dotnet test Talby.Core.slnx --collect:"XPlat Code Coverage" --results-directory ./TestResults/Coverage
    @dotnet tool run reportgenerator -reports:"./TestResults/Coverage/**/coverage.cobertura.xml" -targetdir:"./TestResults/Coverage/Report" "-reporttypes:HtmlSummary;TextSummary"

# Run the application.
run:
    @aspire run
