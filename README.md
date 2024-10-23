
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


3. Apply Database Migrations (Windows/PowerShell):

Antes de aplicar as migrações, certifique-se de que o **container PostgreSQL** está em execução e acessível.

### Passos:

1. **Obter o IP do WSL2 (se necessário):**

   Se você estiver usando o WSL2, execute o seguinte comando para descobrir o IP do sistema WSL:

   ```powershell
   wsl hostname -I
   ```

   Use o IP retornado como `Host` na string de conexão abaixo.

2. **Executar as migrações usando PowerShell:**

   No PowerShell, defina a variável de ambiente para a **string de conexão** e aplique as migrações com os seguintes comandos:

   ```powershell
   $Env:ConnectionStrings__DefaultConnection = "Host=<ip_wsl2>;Port=5432;Database=gamestore;Username=postgres;Password=password"
   dotnet ef database update --context DataContext --project ./src/GameStore.Infrastructure/GameStore.Infrastructure.csproj --startup-project ./src/GameStore.API/GameStore.API.csproj
   dotnet ef database update --context ApplicationDbContext --project ./src/GameStore.Infrastructure/GameStore.Infrastructure.csproj --startup-project ./src/GameStore.API/GameStore.API.csproj
   ```

   ou

   ```powershell
   dotnet ef database update --context DataContext --connection "Host=<ip_wsl2>;Port=5432;Database=game_store;Username=postgres;Password=password" --project ./src/GameStore.Infrastructure/GameStore.Infrastructure.csproj --startup-project ./src/GameStore.API/GameStore.API.csproj  
   dotnet ef database update --context ApplicationDbContext --connection "Host=<ip_wsl2>;Port=5432;Database=game_store;Username=postgres;Password=password" --project ./src/GameStore.Infrastructure/GameStore.Infrastructure.csproj --startup-project ./src/GameStore.API/GameStore.API.csproj
   ```

   Substitua `<ip_wsl2>` pelo IP retornado no comando anterior.

### Nota:

- Se o **PostgreSQL** e a aplicação estiverem na **mesma rede Docker**, você pode tentar usar `localhost` em vez do IP do WSL2.
- Certifique-se de que a porta **5432** está liberada e o banco de dados está acessível.
- Se necessário, permita a porta no firewall do Windows com o comando abaixo:

   ```powershell
   New-NetFirewallRule -DisplayName "PostgreSQL 5432" -Direction Inbound -LocalPort 5432 -Protocol TCP -Action Allow
   ```

Com esses passos, você conseguirá aplicar as migrações corretamente, conectando-se ao banco de dados dentro do container.

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


## API Documentation using Postman

The API documentation is available via a Postman collection, which provides detailed information about the available endpoints, their usage, and request/response examples.

### How to Use the Postman Collection

1. Download and install [Postman](https://www.postman.com/downloads/) if you don't already have it.
2. Import the provided collection by following these steps:
   - Open Postman.
   - Click on **Import** at the top-left corner.
   - Select the `GameStore.postman_collection.json` file and import it.
3. Once the collection is imported, you can see all the available endpoints categorized under different controllers.
4. Use the environment variables and pre-configured requests to quickly interact with the API.

The collection includes endpoints for:

- **AuthController**: Registration, Login, and Role Management.
- **BoxController**: CRUD operations on Boxes.
- **OrderController**: Handling Orders including listing, creation, and deletion.
- **ProductController**: Managing Products in the store.
- **PackingController**: Processing Orders and selecting the correct boxes.

You can find the collection file at the following location in the repository:

```
GameStore.postman_collection.json
```

Make sure to update the Postman environment variables, such as **baseUrl** and **accessToken**, to correctly interact with the local or production instance of the API.

# API Documentation

## AuthController
### Register
- **Method**: POST
- **URL**: https://localhost:7224/api/v1/auth/register
- **Description**: ### Register User

This endpoint is used to register a new user.

#### Request Body

- email (string): The email of the user.
    
- password (string): The password for the user account.
    
- confirmPassword (string): The confirmation of the password.
    

#### Response

The response of this request is a JSON schema.
#### Request Body
```json
{
  "email": "user@example.com",
  "password": "string",
  "confirmPassword": "string"
}
```

### Login
- **Method**: POST
- **URL**: https://localhost:7224/api/v1/auth/login
- **Description**: ### Auth Login

HTTP POST request to authenticate and login a user.

#### Request Body

- Type: JSON
    
    - email (string, required): The email of the user.
        
    - password (string, required): The password of the user.
        

#### Response

The response of this request is a JSON schema.
#### Request Body
```json
{
  "email": "admin@example.com",
  "password": "P@ssw0rd!"
}
```

### Add Role
- **Method**: POST
- **URL**: https://localhost:7224/api/v1/auth/add-role
- **Description**: ### Add Role

This endpoint is used to add a new role.

#### Request Body

- Type: string
    
- Description: The name of the role to be added.
    

#### Response

The response will include the status of the request to add the role.
#### Request Body
```json
string
```

### Assign Roles and Claims
- **Method**: POST
- **URL**: https://localhost:7224/api/v1/auth/assign-roles-claims
- **Description**: ### Add Roles and Claims to User

This endpoint allows you to assign roles and claims to a user.

#### Request Body

- `userId` (string): The ID of the user to whom the roles and claims will be assigned.
    
- `roles` (array of strings): An array of role names to be assigned to the user.
    
- `claims` (array of objects):
    
    - `value` (string): The value of the claim.
        
    - `type` (string): The type of the claim.
        

Example:

``` json
{
  "userId": "string",
  "roles": ["string"],
  "claims": [
    {
      "value": "string",
      "type": "string"
    }
  ]
}

 ```

#### Response

The response will contain the result of the operation.
#### Request Body
```json
{
  "userId": "string",
  "roles": [
    "string"
  ],
  "claims": [
    {
      "value": "string",
      "type": "string"
    }
  ]
}
```

## BoxController
### Get Box by ID
- **Method**: GET
- **URL**: https://localhost:7224/api/v1/boxes/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### Retrieve Box Details

This endpoint makes an HTTP GET request to retrieve details of a specific box.

#### Request

The request does not contain a request body. The endpoint URL is:

```
https://localhost:7224/api/v1/boxes/44d69eb9-b591-4e37-8135-e0458522e2d5

 ```

#### Response

The response will include the details of the specified box.

### Update Box
- **Method**: PUT
- **URL**: https://localhost:7224/api/v1/boxes/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### Update Box Details

This endpoint allows updating the details of a specific box.

#### Request Body

- `name` (string): The name of the box.
    
- `height` (number): The height of the box.
    
- `width` (number): The width of the box.
    
- `length` (number): The length of the box.
    

#### Response

The response for this request is a JSON object with the updated details of the box, following the schema below:

``` json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string"
    },
    "name": {
      "type": "string"
    },
    "height": {
      "type": "number"
    },
    "width": {
      "type": "number"
    },
    "length": {
      "type": "number"
    }
  }
}

 ```
#### Request Body
```json
{
  "name": "string",
  "height": 0,
  "width": 0,
  "length": 0
}
```

### List Boxes
- **Method**: GET
- **URL**: https://localhost:7224/api/v1/boxes/list
- **Description**: ### Retrieve List of Boxes

This endpoint makes an HTTP GET request to retrieve a list of boxes.

#### Request

- Method: `GET`
    
- URL: `https://localhost:7224/api/v1/boxes/list`
    

#### Response

The response will be a JSON object with the following schema:

``` json
{
  "type": "object",
  "properties": {
    "boxList": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "boxId": {
            "type": "string"
          },
          "boxName": {
            "type": "string"
          },
          "boxType": {
            "type": "string"
          },
          "location": {
            "type": "string"
          }
        },
        "required": ["boxId", "boxName", "boxType", "location"]
      }
    }
  }
}

 ```

This endpoint makes a GET request to retrieve a list of boxes.

#### Request

- Method: `GET`
    
- URL: `https://localhost:7224/api/v1/boxes/list`
    
- Headers:
    
    - `accept: \\*/\\*`
        

#### Response

The response of this request can be documented as a JSON schema.

### Create Box
- **Method**: POST
- **URL**: https://localhost:7224/api/v1/boxes
- **Description**: ### Create a New Box

This endpoint allows you to create a new box by sending a POST request to the specified URL.

#### Request Body

- `name` (string): The name of the box.
    
- `height` (number): The height of the box.
    
- `width` (number): The width of the box.
    
- `length` (number): The length of the box.
    

#### Response Body

The response will contain the details of the newly created box.
#### Request Body
```json
{
  "name": "string",
  "height": 0,
  "width": 0,
  "length": 0
}
```

### Soft Delete Box
- **Method**: PATCH
- **URL**: https://localhost:7224/api/v1/boxes/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### Update Box Details

This endpoint is used to update the details of a specific box.

#### Request

- Method: `PATCH`
    
- URL: `https://localhost:7224/api/v1/boxes/44d69eb9-b591-4e37-8135-e0458522e2d5`
    
- Headers:
    
    - accept: _/_
        

#### Response

The response for this request can be represented as a JSON schema:

``` json
{
  "type": "object",
  "properties": {
    "status": {
      "type": "string"
    },
    "message": {
      "type": "string"
    }
  }
}

 ```

## OrderController
### Get Order by ID
- **Method**: GET
- **URL**: https://localhost:7224/api/v1/orders/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### Retrieve Order Details

This endpoint retrieves the details of a specific order.

#### Request

- Method: GET
    
- URL: `https://localhost:7224/api/v1/orders/44d69eb9-b591-4e37-8135-e0458522e2d5`
    
- Headers:
    
    - accept: _/_
        

#### Response

The response for this request can be documented as a JSON schema. However, since the response data is masked, the JSON schema cannot be generated accurately without the actual response data.

### Update Order
- **Method**: PUT
- **URL**: https://localhost:7224/api/v1/orders/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### Update Order Details

This endpoint allows updating the details of a specific order.

#### Request Body

- `id` (string): The unique identifier for the order.
    
- `customerId` (string): The unique identifier for the customer associated with the order.
    
- `orderDate` (string): The date and time of the order in ISO 8601 format.
    
- `totalPrice` (number): The total price of the order.
    
- `products` (array): An array of products included in the order, each containing the following fields:
    
    - `id` (string): The unique identifier for the product.
        
    - `name` (string): The name of the product.
        
    - `description` (string): The description of the product.
        
    - `height` (number): The height of the product.
        
    - `width` (number): The width of the product.
        
    - `length` (number): The length of the product.
        
    - `weight` (number): The weight of the product.
        
    - `price` (number): The price of the product.
        

#### Response Body

The response will contain the updated details of the order.
#### Request Body
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderDate": "2024-10-22T01:24:58.966Z",
  "totalPrice": 0,
  "products": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "string",
      "description": "string",
      "height": 0,
      "width": 0,
      "length": 0,
      "weight": 0,
      "price": 0
    }
  ]
}
```

### Soft Delete Order
- **Method**: PATCH
- **URL**: https://localhost:7224/api/v1/orders/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### Update Order Details

This endpoint is used to update the details of a specific order.

#### Request Body

- `order_number` (string): The new order number.
    
- `status` (string): The updated status of the order.
    

#### Response

The response will contain the updated details of the order.

### List Orders
- **Method**: GET
- **URL**: https://localhost:7224/api/v1/orders/list
- **Description**: ### Retrieve Orders List

This endpoint is used to retrieve a list of orders.

#### Request

- Method: GET
    
- URL: `https://localhost:7224/api/v1/orders/list`
    
- Headers:
    
    - accept: _/_
        

#### Response

The response for this request is a JSON object representing the list of orders. Below is the JSON schema for the response:

``` json
{
  "type": "object",
  "properties": {
    "orders": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "order_id": {
            "type": "string"
          },
          "customer_name": {
            "type": "string"
          },
          "total_amount": {
            "type": "number"
          },
          "order_date": {
            "type": "string",
            "format": "date-time"
          }
        },
        "required": ["order_id", "customer_name", "total_amount", "order_date"]
      }
    }
  },
  "required": ["orders"]
}

 ```

### Create Order
- **Method**: POST
- **URL**: https://localhost:7224/api/v1/orders
- **Description**: ### Create a New Order

This endpoint is used to create a new order.

#### Request Body

- `customerId` (string): The ID of the customer placing the order.
    
- `productIds` (array of strings): An array containing the IDs of the products being ordered.
    

Example:

``` json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f ...",
  "productIds": [
    "3fa85f64-5717-4562-b3fc-2c963f ..."
  ]
}

 ```

#### Response

The response will contain the details of the newly created order.
#### Request Body
```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "productIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  ]
}
```

## ProductController
### Get Product by ID
- **Method**: GET
- **URL**: https://localhost:7224/api/v1/products/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### Retrieve Product Details

This API endpoint makes an HTTP GET request to retrieve the details of a specific product.

#### Request

The request does not contain a request body. It includes the following parameters:

- Path parameter:
    
    - `product_id` (string): The unique identifier of the product to be retrieved.
        

Example:

``` plaintext
GET https://localhost:7224/api/v1/products/44d69eb9-b591-4e37-8135-e0458522e2d5

 ```

#### Response

The response will be in JSON format and will adhere to the following schema:

``` json
{
  "type": "object",
  "properties": {
    "product_id": {
      "type": "string"
    },
    "name": {
      "type": "string"
    },
    "description": {
      "type": "string"
    },
    "price": {
      "type": "number"
    },
    "category": {
      "type": "string"
    },
    "in_stock": {
      "type": "boolean"
    }
  }
}

 ```

### Update Product
- **Method**: PUT
- **URL**: https://localhost:7224/api/v1/products/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### Update Product Details

This endpoint allows updating the details of a specific product.

#### Request Body

- `name` (string) - The name of the product.
    
- `description` (string) - The description of the product.
    
- `height` (number) - The height of the product.
    
- `width` (number) - The width of the product.
    
- `length` (number) - The length of the product.
    
- `weight` (number) - The weight of the product.
    
- `price` (number) - The price of the product.
    

#### Response

The response will contain the updated details of the product.
#### Request Body
```json
{
  "name": "string",
  "description": "string",
  "height": 0,
  "width": 0,
  "length": 0,
  "weight": 0,
  "price": 0
}
```

### Soft Delete Product
- **Method**: PATCH
- **URL**: https://localhost:7224/api/v1/products/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: This API endpoint sends an HTTP PATCH request to update the product with the specified ID. The request URL is `https://localhost:7224/api/v1/products/44d69eb9-b591-4e37-8135-e0458522e2d5`. The request body should contain the updated product information. Upon successful update, the endpoint will return a response with the updated product details.

### List Products
- **Method**: GET
- **URL**: https://localhost:7224/api/v1/products/list
- **Description**: ### Retrieve Products List

This endpoint retrieves a list of products.

#### HTTP Request

``` http
GET https://localhost:7224/api/v1/products/list

 ```

#### Response

The response of this request is a JSON schema representing the structure of the products list.

This endpoint makes an HTTP GET request to retrieve a list of products.

#### Request Body

This request does not require a request body.

#### Response Body

The response will contain a list of products.

### Create Product
- **Method**: POST
- **URL**: https://localhost:7224/api/v1/products
- **Description**: ### Create a Product

This endpoint is used to create a new product.

#### Request Body

- name (string): The name of the product.
    
- description (string): The description of the product.
    
- height (number): The height of the product.
    
- width (number): The width of the product.
    
- length (number): The length of the product.
    
- weight (number): The weight of the product.
    
- price (number): The price of the product.
    

#### Response

The response of this request can be documented as a JSON schema.

This endpoint allows you to create a new product by sending an HTTP POST request to the specified URL.

#### Request Body

- `name` (string): The name of the product.
    
- `description` (string): The description of the product.
    
- `height` (number): The height of the product.
    
- `width` (number): The width of the product.
    
- `length` (number): The length of the product.
    
- `weight` (number): The weight of the product.
    
- `price` (number): The price of the product.
    

#### Response

The response will contain the details of the newly created product.
#### Request Body
```json
{
  "name": "string",
  "description": "string",
  "height": 0,
  "width": 0,
  "length": 0,
  "weight": 0,
  "price": 0
}
```

## PackingController
### Process Order
- **Method**: POST
- **URL**: https://localhost:7224/api/v1/packing/process-order/44d69eb9-b591-4e37-8135-e0458522e2d5
- **Description**: ### HTTP POST /api/v1/packing/process-order/{order_id}

This endpoint is used to process an order with the specified order ID.

#### Request Body

The request body should contain the details of the order to be processed. The structure of the request body is as follows:

- `order_details`: (text) The details of the order to be processed.
    

#### Response

Upon successful processing of the order, the response will contain the details of the processed order along with a success message. The structure of the response body is as follows:

- `order_id`: The ID of the processed order.
    
- `processed_items`: The items that have been processed.
    
- `message`: A success message indicating the successful processing of the order.
    

Example Request Body:

``` json
{
  "order_details": "Sample order details"
}

 ```

Example Response:

``` json
{
  "order_id": "44d69eb9-b591-4e37-8135-e0458522e2d5",
  "processed_items": ["item1", "item2"],
  "message": "Order processed successfully"
}

 ```

