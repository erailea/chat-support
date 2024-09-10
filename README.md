# ChatSupport

## Requirements

- .NET 8
- MongoDB
- RabbitMQ

## How to Run

### Locally

1. **Build the Project**

    ```bash
    dotnet build
    ```

2. **Run the Project**

    ```bash
    dotnet run --project ChatSupport/ChatSupport.csproj
    ```

### Docker

1. **Run Docker Compose**

    ```bash
    docker-compose up -d```

### Running Tests and Generating Code Coverage

#### Locally

1. **Run Tests and Collect Coverage**

    ```bash
    dotnet test ChatSupport.Tests/ChatSupport.Tests.csproj --collect:"XPlat Code Coverage"
    ```

2. **Generate Code Coverage Report**

    ```bash
    reportgenerator --settings coverlet.runsettings -reports:./ChatSupport.Tests/TestResults/{{coverage guid}}/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
    ```

