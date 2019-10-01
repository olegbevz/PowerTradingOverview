# PowerTradingOverview

## Build instructions
1. Open PowerTradingOverview.sln
2. Restore nuget packages for solution
3. Build solution

## Solution structure
Solution contains following projects:
### PowerTradingOverview
- project represents all the implementation logic
### PowerTradingOverview.Host 
- project represents Windows Service host
- project can be run as a console application and can be installed as a Windows Service (for details look at https://topshelf.readthedocs.io)
### PowerTradingOverview.UnitTests 
- project contains unit tests
- to run nunit tests "NUnit Test Adapter" extension should be installed in Visual Studio
- unit tests checks logic of JobScheduler, IntervalSchedule, ReportBuilder
### PowerTradingOverview.IntegrationTests 
- project contains integration tests
- to run nunit tests "NUnit Test Adapter" extension should be installed in Visual Studio
- to run integration tests Visual Studio should be run from admin
- integration tests checks that service is installed/uninstalled correctly and generates csv reports

## Implementation description
* Application is based on C# .NET 4.6.1 (.NET 4.6.1 is minimum possible version, cause TradingPlatform is based on it)
* Log4Net is used for logging (any other logging library could be used here). Logs are written to PowerTradingOverview.log
* Job scheduling is implemented manually (I decided not to use Quarz here)
* Topshelf is used in PowerTradingOverview.Host to run application as windows service or console application
* AutoFac is used in host project to construct JobScheduler with all its dependencies
* For tests following libraries are used: NUnit, Moq, FliuentAssertions
