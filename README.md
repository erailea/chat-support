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
dotnet test --settings tests.runsettings
    ```

2. **Generate Code Coverage Report**

    ```bash
reportgenerator -reports:"ChatSupport.Tests/TestResults/67cf9a54-2ad1-4303-bfa4-460bd7eb182c/coverage.cobertura.xml"     -targetdir:"coverage"   -reporttypes:Html
    ```

