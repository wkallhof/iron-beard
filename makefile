build:
	dotnet build ./src

watch: 
	dotnet run --project ./src/IronBeard.Cli -- watch -i ./samples/razor-markdown-sample

serve:
	cd ./samples/razor-markdown-sample/www && dotnet-serve

example:
	dotnet run --project ./src/IronBeard.Cli -- generate -i ./samples/razor-markdown-sample
