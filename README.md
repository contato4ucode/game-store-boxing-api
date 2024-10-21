
# Game Store Boxing API

## Introduction

This project is a microservice designed to automate the packaging process for Seu Manoel's online game store. The API determines the most optimal way to pack products in predefined boxes to minimize the number of boxes used.

## Problem Description

Seu Manoel needs a web API that, given a set of orders with product dimensions, returns the appropriate boxes for each order and specifies which products go into each box.

## Available Box Sizes

- **Box 1**: 30 x 40 x 80 cm
- **Box 2**: 80 x 50 x 40 cm
- **Box 3**: 50 x 80 x 60 cm

## API Functionality

### Input

The API accepts a JSON input with multiple orders. Each order contains products with dimensions specified in centimeters (height, width, and length).

### Processing

The API calculates the optimal way to package products by selecting the appropriate boxes, aiming to minimize the number of boxes used per order.

### Output

The output is a JSON response listing the boxes used for each order and the products assigned to each box.

## Example Usage

- **Input Example**: [entrada.json](./entrada.json)
- **Output Example**: [saida.json](./saida.json)

## How to Run the Application

### Prerequisites

- Docker
- .NET 8 SDK
- PostgreSQL and Redis (configured via Docker)

### Steps to Run

1. **Clone the repository:**

   ```bash
   git clone <repository-url>
   cd game-store-boxing-api
   ```

2. **Build and Run the Application with Docker:**

   Make sure Docker is running, then execute:

   ```bash
   docker-compose up --build
   ```

3. **Apply Database Migrations:**

   Ensure the PostgreSQL container is up and running. Run the following command to apply migrations:

   ```bash
   docker exec -it <container_id> dotnet ef database update --context DataContext --startup-project ./src/GameStore.API/GameStore.API.csproj
   ```

4. **Access the API:**

   Once the containers are up and running, access the Swagger documentation at:

   [http://localhost:8081/swagger](http://localhost:8081/swagger)

## Testing

This project includes unit tests. To run the tests, execute:

```bash
dotnet test
```

## Configuration

Modify the `appsettings.json` file to configure:

- **Database Connection:** PostgreSQL settings
- **Redis Settings:** Redis host and port
- **RabbitMQ:** RabbitMQ connection settings

## Optional Requirements

- **Authentication:** JWT-based authentication can be implemented for security.
- **Unit Tests:** The project includes unit tests for critical components.

## Document Reference

Refer to the following document for additional details on the technical evaluation: [Avaliação Técnica .NET](https://docs.google.com/document/d/1R1MbtXEjHGccL0jO_15_3a6SkxaJ-fg7VGPSBu1sEaw/edit?tab=t.0)

## Conclusion

This project demonstrates a microservice-based approach using .NET to automate the packaging process for an online game store. The application leverages Docker for containerization and ensures optimal packaging with predefined box sizes.

