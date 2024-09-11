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

Keep in mind that you need to have Docker and Docker Compose installed on your machine and configurations should consider rabbitmq and mongodb containers.

    ```bash
    docker-compose up -d

### Running Tests and Generating Code Coverage

#### Locally

1. **Run Tests and Collect Coverage**


    ```bash
    dotnet test --settings tests.runsettings
    

2. **Generate Code Coverage Report**


    ```bash
    reportgenerator 
        -reports:"ChatSupport.Tests/TestResults/2c663faa-9c57-4074-95b2-25210ec76ac5/coverage.cobertura.xml" 
        -targetdir:"coverage" 
        -reporttypes:Html