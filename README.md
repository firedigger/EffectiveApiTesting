# EffectiveApiTesting

This project demonstrates a modern and pragmatic methodology for developing and testing Web APIs using **ASP.NET Core 9**. using built-in .NET class `WebApplicationFactory`. This approach enables realistic testing of HTTP behavior **without needing a deployed backend or complex setup**.

## ✅ Key Concepts Demonstrated

- **Controller-based APIs** using ASP.NET Core 9
- **Entity Framework Core** with optional SQLite backing
- **Role-based Authorization** and model validation
- **OpenApi specification** and Swagger UI endpoint
- **Central package management** via `Directory.Packages.props`
- **Effective testing with MSTest** via `WebApplicationFactory`
- **Real HTTP request testing**, including:
  - Route & query string binding
  - Authorization checks
  - Automatic model validation responses (e.g. 400s)

## 🧩 The benefits of `WebApplicationFactory`

Traditional unit tests mock controller dependencies and focus on logic isolation. However, this approach skips critical parts of the HTTP pipeline: Parameter binding, Authorization enforcement, Model validation, Route constraints. This makes unit tests fast but **less trustworthy** for verifying actual API behavior.  
Full integration tests (e.g. against a real or dockerized backend) are realistic but slower to run and require setup/teardown of external services (like a database).  
`Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<T>` enables **in-memory, realistic testing** of your entire API:
- No mocking required
- No HTTP server to stand up
- Full control of HTTP request sent for testing server behaviour
- Optional use of in-memory SQLite for lightweight data persistence