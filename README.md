# Server-Side Assignment

This project is a server-side application that processes weather data and generates images with weather information. It utilizes Azure Functions, Azure Blob Storage, and Azure Queue Storage.

## Project Structure

- **StartJobTrigger/**: Contains the Azure Function that triggers the job processing.
    - `StartJobTrigger.cs`: Azure Function that triggers the job processing.

- **ProcessWeatherDataQueue/**: Contains the Azure Function that processes the weather data queue.
    - `ProcessWeatherDataQueue.cs`: Azure Function that processes the weather data queue.

- **ProcessJobQueue/**: Contains the Azure Function that processes the image job queue.
    - `ProcessImageJobQueue.cs`: Azure Function that processes the image job queue.

- **GetImagesTrigger/**: Contains the Azure Function that retrieves images from Blob Storage.
    - `GetImagesTrigger.cs`: Azure Function that retrieves images from Blob Storage.

- **services/**: Contains the service classes for image editing, weather data processing, and blob storage operations.
    - `ImageService.cs`: Handles image processing tasks.
    - `WeatherService.cs`: Manages weather data retrieval and processing.
    - `BlobService.cs`: Manages interactions with Azure Blob Storage.
    - `JobStatusStore.cs`: Manages job status storage.

- **models/**: Contains the data models used in the application.
    - `WeatherDataResponse.cs`: Represents the response structure for weather data.
    - `StationMeasurement.cs`: Represents a weather station measurement.
    - `Actual.cs`: Represents actual weather data.

- **Properties/**: Contains project properties.
    - `launchSettings.json`: Defines launch settings for the project.

- **Deployment Files**:
    - `template.bicep`: Defines the Azure resources for the project.
    - `deployment.ps1`: PowerShell script for deploying the project to Azure.

- **Configuration Files**:
    - `local.settings.json`: Local settings for Azure Functions.
    - `host.json`: Configuration for the Azure Functions host.

- **Project Files**:
    - `Program.cs`: Entry point for the application.
    - `serverside-assignment.csproj`: Project file for the .NET application.
    - `serverside-assignment.sln`: Solution file for the project.

## Getting Started

### Prerequisites

- .NET SDK 8.0
- Azure CLI
- Visual Studio Code or Visual Studio
- Azurite (optional, for local development)

### Setup

1. Clone the repository

2. Install the required .NET packages

3. Configure local settings:
     - Update `local.settings.json` with your Azure Storage connection string.

4. Deploy the project to Azure:
     ```sh
     ./deployment.ps1
     ```

### Running the Application (Local)

1. Build the project:
     ```sh
     dotnet build
     ```

2. Run the project:
     ```sh
     dotnet run
     ```

## License

This project is licensed under the Shadow Wizard Money Gang License.