# Use the official .NET runtime as base image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

# Set the working directory inside the container
WORKDIR /home/site/wwwroot

# Copy the published function app
COPY ./src/umuthi.Functions/output/ .

# Set environment variables for Azure Functions
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true