.PHONY: all build test aot run schema schema-openai clean install help

SLN = starter.slnx
PROJECT = src/cli/cli.csproj
APP = cli

all: build test

## build: Build solution in Debug mode
build:
	dotnet build $(SLN) -c Debug

## test: Run unit & integration test suite
test:
	dotnet test $(SLN) -c Debug

## aot: Publish optimized Native AOT binary for host platform
aot:
	./scripts/build.sh

## run: Run the CLI project with .NET runtime
run:
	dotnet run --project $(PROJECT) -- $(filter-out $@,$(MAKECMDGOALS))

## schema: Export CLI JSON schema
schema:
	dotnet run --project $(PROJECT) -- schema

## schema-openai: Export OpenAI / AI Agent function definitions
schema-openai:
	dotnet run --project $(PROJECT) -- schema --format openai

## install: Install Native AOT binary to ~/.local/bin
install:
	./scripts/install.sh

## clean: Remove all build, obj, and publish artifacts
clean:
	dotnet clean $(SLN)
	rm -rf publish artifacts .nuget-cache

## help: Display this help message
help:
	@echo "Native AOT CLI Template - Developer Commands"
	@echo ""
	@grep -E '^## ' $(MAKEFILE_LIST) | sed -e 's/## //' | column -t -s ':'
