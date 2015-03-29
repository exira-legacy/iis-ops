# iis-ops

## Exira.IIS

Exira.IIS is the main application, designed as an API.

It uses [FSharp.Configuration](http://fsprojects.github.io/FSharp.Configuration/) to read its configuration out of the [Web.yaml](iis-ops/src/Exira.IIS/Web.yaml) Yaml file.

Currently the OWIN pipeline implements Web API and [GetEventStore](https://geteventstore.com/): [Startup.fs](iis-ops/src/Exira.IIS/Startup.fs)

Each controller method takes a command and hands it off to [the domain](#Exira.IIS.Domain).

## Exira.IIS.Domain

The only entry point to the domain is the command handler: [CommandHandler.fs](iis-ops/src/Exira.IIS.Domain/CommandHandler.fs)

After a check if a valid command has been used, it is dispatched to a specific command handler.

All possible commands are listed in [Commands.fs](iis-ops/src/Exira.IIS.Domain/Commands.fs)