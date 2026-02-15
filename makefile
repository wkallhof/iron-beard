build:
	dotnet build ./src

watch: 
	dotnet run --project ./src/IronBeard.Cli -- watch -i ./samples/razor-markdown-sample

serve:
	cd ./samples/razor-markdown-sample/www && dotnet-serve

example:
	dotnet run --project ./src/IronBeard.Cli -- generate -i ./samples/razor-markdown-sample

test:
	dotnet run --project ./src/IronBeard.Core.Tests
	dotnet run --project ./src/IronBeard.Cli.IntegrationTests

test-unit:
	dotnet run --project ./src/IronBeard.Core.Tests

test-integration:
	dotnet run --project ./src/IronBeard.Cli.IntegrationTests
