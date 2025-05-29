
# Assignment Project

A .NET web application (PhoneBook) featuring API endpoints, data access layer, and business logic components.

## Table of Contents
- [Installation](#installation)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Development](#development)
- [Testing](#testing)
- [Contributing](#contributing)


## Installation

1. Clone the repository:
```bash
git clone https://github.com/your-username/assignment.git
cd assignment
```

2. Copy environment configuration (if applicable):
```bash
cp .env.example .env
```

3. Update configuration files as needed (see [Configuration](#configuration) section)

## Running the Application

### Using Docker Compose (Recommended)

1. Build and start the application:
```bash
docker-compose up --build
```

2. To run in detached mode:
```bash
docker-compose up -d
```
3. To stop the application:
```bash
docker-compose down
```

### Using Docker Only

1. Build the Docker image:
```bash
docker build -t assignment-app .
```

2. Run the container:
```bash
docker run -p 5000:80 assignment-app
```

## API Documentation

### Base URL
```
http://localhost:5000/api
```


### Endpoints

#### Example Endpoints (Update with your actual API)

**GET** `/api/health`
- Description: Health check endpoint
- Response: `200 OK`

GET /api/Contacts - Get all contacts

POST /api/Contacts - Create new contact

GET /api/Contacts/search - Search contacts

PUT /api/Contacts/{phoneNumber} - Update contact by phone

DELETE /api/Contacts/{phoneNumber} - Delete contact by phone

✅ Added Metrics API endpoint: GET /api/metrics
✅ Updated port numbers throughout the document

### Error Responses

All endpoints may return the following error responses:

- `400 Bad Request`: Invalid request data
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error



## Project Structure

```
├── Controllers/          # API controllers
├── Models/              # Data models and DTOs
├── DAL/                 # Data Access Layer
├── Contracts/           # Interfaces and contracts
├── BL/                  # Business Logic layer
├── UnitTests/           # Unit test files
├── Contacts/            # Contact-related functionality
├── docker-compose.yml   # Docker Compose configuration
├── Dockerfile          # Docker image configuration
├── .gitignore          # Git ignore rules
├── .gitattributes      # Git attributes
└── README.md           # This file
```

### Application Logs

Logs are written to:
- Console (development)
- Files in `/logs` directory (production)
- External logging service (if configured)

### Health Checks

The application includes health check endpoints:
- `/health` - Basic health check