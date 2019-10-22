# PowerTradingOverview

## Task Description
The power traders require an intra-day report to give them their day ahead power position. The report should output the aggregated volume per hour to a CSV file.
### Requirements
1. Must be implemented as a Windows service using .Net 4.5+ & C#.
2. All trade positions must be aggregated per hour (local/wall clock time). Note that for a given day, the actual local start time of the day is 23:00 (11pm) on the previous day. Local time is in the London, UK time zone.
3. CSV output format must be two columns, Local Time (format 24 hour HH:MM e.g. 13:00) and Volume and the first row must be a header row.
4. CSV filename must be PowerPosition_YYYYMMDD_HHMM.csv where YYYYMMDD is year/month/day e.g. 20141220 for 20 Dec 2014 and HHMM is 24hr time hour and minutes e.g. 1837. The date and time are the local time of extract.
5. The location of the CSV file should be stored and read from the application configuration file.
6. An extract must run at a scheduled time interval; every X minutes where the actual interval X is stored in the application configuration file. This extract does not have to run exactly on the minute and can be within +/- 1 minute of the configured interval.
7. It is not acceptable to miss a scheduled extract.
8. An extract must run when the service first starts and then run at the interval specified as above.
9. It is acceptable for the service to only read the configuration when first starting and it does not have to dynamically update if the configuration file changes. It is sufficient to require a service restart when updating the configuration.
10. The service must provide adequate logging for production support to diagnose any issues

## Build instructions
1. Open PowerTradingOverview.sln
2. Restore nuget packages for solution
3. Build solution

## Solution structure
Solution contains following projects:
1. PowerTradingOverview 
- project represents all the implementation logic
2. PowerTradingOverview.Host 
- project represents Windows Service host
- project can be run as a console application and can be installed as a Windows Service (for details look at https://topshelf.readthedocs.io)
3. PowerTradingOverview.UnitTests 
- project contains unit tests
- to run nunit tests "NUnit Test Adapter" extension should be installed in Visual Studio
- unit tests checks logic of JobScheduler, IntervalSchedule, ReportBuilder
4. PowerTradingOverview.IntegrationTests 
- project contains integration tests
- to run nunit tests "NUnit Test Adapter" extension should be installed in Visual Studio
- to run integration tests Visual Studio should be run from admin
- integration tests checks that service is installed/uninstalled correctly and generates csv reports

## Implementation details
1. Application is based on C# .NET 4.6.1 (.NET 4.6.1 is minimum possible version, cause TradingPlatform is based on it)
2. Log4Net is used for logging (any other logging library could be used here). Logs are written to PowerTradingOverview.log
3. Job scheduling is implemented manually (I decided not to use Quarz here)
4. Topshelf is used in PowerTradingOverview.Host to run application as windows service or console application
5. AutoFac is used in host project to construct JobScheduler with all its dependencies
6. For tests following libraries are used: NUnit, Moq, FliuentAssertions
