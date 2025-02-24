## Services

### 1. UserDataGeneration

**Description:**
The UserDataGeneration service is responsible for generating user data in JSON format and saving it to a specified directory. This service exposes an API endpoint to trigger the generation of user data.

**Endpoints:**

- `POST /generation/generate`: Generates a new user data JSON file and saves it to the specified directory.

**Dockerfile:**
The Dockerfile for this service builds and publishes the .NET application, setting the necessary environment variables and exposing port 5001.

### 2. ProcessData

**Description:**
The ProcessData service monitors the directory where the UserDataGeneration service saves the generated JSON files. It processes these files by reading the user data and inserting it into a MySQL database. After processing, the files are moved to a "processed" directory.

**Dockerfile:**
The Dockerfile for this service builds and publishes the .NET application, setting the necessary environment variables and exposing port 5002.

### 3. UI

**Description:**
The UI service provides a web interface to display the user data stored in the MySQL database. It allows users to generate new user data by interacting with the UserDataGeneration service and displays the list of users retrieved from the database.

**Endpoints:**

- `GET /`: Displays the list of users.
- `POST /Home/GenerateUser`: Triggers the generation of new user data.

**Dockerfile:**
The Dockerfile for this service builds and publishes the .NET application, setting the necessary environment variables and exposing port 5003.

### MySQL

**Description:**
MySQL is used as the database to store user data. The database is initialized with a table to store user information.

**Initialization Script:**
The `mysql-init` directory contains an SQL script (`init.sql`) that initializes the database and creates the necessary table.

## Docker Compose

The `docker-compose.yaml` file defines the services and their configurations. It sets up the following services:

- `userdatageneration`: Builds and runs the UserDataGeneration service.
- `processdata`: Builds and runs the ProcessData service.
- `ui`: Builds and runs the UI service.
- `mysql`: Runs the MySQL database with the necessary initialization.

### Volumes

- `data-generation`: Shared volume for storing generated user data.
- `data-processed`: Shared volume for storing processed user data.
- `mysql-data`: Volume for MySQL data storage.

### Ports

- `5001:5001`: Exposes the UserDataGeneration service.
- `5002:5002`: Exposes the ProcessData service.
- `80:5003`: Exposes the UI service on port 80.
- `3306:3306`: Exposes the MySQL database.

## How to Run

1. Ensure Docker and Docker Compose are installed on your machine.
2. Clone the repository.
3. Navigate to the project directory.
4. Run `docker-compose up -d --build` to build and start all services.
5. Access the UI service at `http://localhost`.

## Project Structure

```
.
├── ProcessData
│   ├── Controllers
│   ├── Models
│   ├── Properties
│   ├── Views
│   ├── Dockerfile
│   ├── ProcessData.csproj
│   ├── Program.cs
│   ├── Startup.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── ...
├── UI
│   ├── Controllers
│   ├── Models
│   ├── Properties
│   ├── Views
│   ├── Dockerfile
│   ├── UI.csproj
│   ├── Program.cs
│   ├── Startup.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── ...
├── UserDataGeneration
│   ├── Controllers
│   ├── Properties
│   ├── Dockerfile
│   ├── UserDataGeneration.csproj
│   ├── Program.cs
│   ├── Startup.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── ...
├── mysql-init
│   └── init.sql
├── docker-compose.yaml
└── README.md
```

This project demonstrates a simple microservices architecture using .NET, Docker, and MySQL. It can be extended and modified to fit more complex use cases and requirements.
