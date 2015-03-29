# iis-ops

## Exira.IIS

Exira.IIS is the main application, designed as an API.

It uses [FSharp.Configuration](http://fsprojects.github.io/FSharp.Configuration/) to read configuration out of the [Web.yaml](https://github.com/exira/iis-ops/blob/master/src/Exira.IIS/Web.yaml) file.

Currently the [OWIN pipeline](https://github.com/exira/iis-ops/blob/master/src/Exira.IIS/Startup.fs) implements Web API and [GetEventStore](https://geteventstore.com/).

Each controller method takes a command and hands it off to [the domain](#Exira.IIS.Domain).

## Exira.IIS.Domain

The only entry point to the domain is the command handler: [CommandHandler.fs](https://github.com/exira/iis-ops/blob/master/src/Exira.IIS.Domain/CommandHandler.fs)

After a check if a valid command has been used, it is dispatched to a specific command handler.

All possible commands are listed in [Commands.fs](https://github.com/exira/iis-ops/blob/master/src/Exira.IIS.Domain/Commands.fs)

## Notes

There seems to be a current issue on build where Paket's auto-restore is not fetching all references.

Running ```.paket\paket.exe install``` fixes this.