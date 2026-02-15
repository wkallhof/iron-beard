build:
	dotnet build ./src

watch: 
	dotnet run --project ./src/IronBeard.Cli -- watch -i ./samples/razor-markdown-sample

serve:
	cd ./samples/razor-markdown-sample/www && dotnet-serve

example:
	dotnet run --project ./src/IronBeard.Cli -- generate -i ./samples/razor-markdown-sample

test:
	dotnet build ./src/IronBeard.Core.Tests --nologo -v q
	dotnet coverlet ./src/IronBeard.Core.Tests/bin/Debug/net10.0/IronBeard.Core.Tests.dll \
		--target dotnet \
		--targetargs "run --project ./src/IronBeard.Core.Tests --no-build" \
		--include "[IronBeard.Core]*"
	dotnet run --project ./src/IronBeard.Cli.IntegrationTests

test-unit:
	dotnet build ./src/IronBeard.Core.Tests --nologo -v q
	dotnet coverlet ./src/IronBeard.Core.Tests/bin/Debug/net10.0/IronBeard.Core.Tests.dll \
		--target dotnet \
		--targetargs "run --project ./src/IronBeard.Core.Tests --no-build" \
		--include "[IronBeard.Core]*"

test-integration:
	dotnet run --project ./src/IronBeard.Cli.IntegrationTests
