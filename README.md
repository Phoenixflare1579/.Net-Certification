UserManagementAPI
=================

Overview
--------
This repository contains a simple ASP.NET Core Web API (targeting .NET 10) for managing users. I assisted by scaffolding the project structure, adding a User model and controller endpoints, and implementing middleware to satisfy TechHive Solutions policies for logging, standardized error handling, and token-based authentication.

What Copilot changed / implemented
----------------------------
- Created a User model with data annotations to validate first name, last name, and email.
- Implemented CRUD API endpoints (in UsersController) using an in-memory store for demonstration.
- Added middleware components:
  - ErrorHandlingMiddleware: catches unhandled exceptions and returns a JSON error response.
  - TokenAuthenticationMiddleware: validates a simple bearer token from configuration (ApiSettings:Token) and returns 401 on failure.
  - RequestResponseLoggingMiddleware: logs HTTP method, request path + query, response status code, and elapsed time.
- Configured middleware pipeline in Program.cs in the following order: Error handling -> Authentication -> Logging -> HTTPS/Authorization -> Endpoints.
- Added a sample ApiSettings:Token to appsettings.json (change before production).

Available API endpoints
-----------------------
Base path: /api/users

- GET /api/users
  - Returns a paged list of users.
  - Optional query parameters: page (default 1), pageSize (default 50, max 100)

- GET /api/users/{id}
  - Returns a single user by GUID. 404 if not found.

- POST /api/users
  - Creates a new user.
  - Request JSON must include firstName, lastName, email. All fields are validated.
  - Returns 201 Created with Location header.

- PUT /api/users/{id}
  - Updates an existing user. Validates payload and prevents duplicate emails.
  - Returns 204 No Content on success or 404 if not found.

- DELETE /api/users/{id}
  - Deletes a user by GUID. Returns 204 No Content or 404 if not found.

Authentication and configuration
--------------------------------
- The token-based authentication is configuration-driven. Set the expected token in UserManagementAPI/appsettings.json under ApiSettings:Token or via environment variables.
- Clients must send the header: Authorization: Bearer {token}
- Requests without a valid token receive a 401 JSON response: { "error": "Unauthorized" }

Error responses
---------------
- Unhandled exceptions produce a 500 response with body: { "error": "Internal server error." }
- Validation errors return standard ASP.NET Core validation responses (400) with details.

Logging / Auditing
------------------
- All requests passing through the API are logged with method, path (and query), response status code, and elapsed time. Logs are emitted via ILogger and controlled by appsettings logging levels.

How to run
----------
1. From the solution root:
   dotnet run --project UserManagementAPI
2. Ensure ApiSettings:Token is set in appsettings.json or environment.
3. Call endpoints with the Authorization header.
