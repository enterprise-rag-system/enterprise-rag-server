# Enterprise RAG System Backend

A robust, scalable microservices-based backend for an Enterprise Retrieval-Augmented Generation (RAG) system. This project provides a complete infrastructure for document management, semantic search, and AI-powered conversational interfaces using modern .NET technologies.

## 🎯 Overview

The Enterprise RAG System is built on a microservices architecture with the following key components:

- **API Gateway**: Central entry point for all client requests with authentication and routing
- **Auth Service**: JWT-based authentication and authorization management
- **Project Service**: Project and workspace management
- **Document Service**: Document upload, processing, and storage
- **Chat Service**: Real-time chat and conversation management
- **RAG Worker**: Background worker for RAG pipeline processing and vector embeddings
- **Shared**: Common models, events, and utilities

The system uses **PostgreSQL with pgvector** for vector embeddings and **RabbitMQ** for asynchronous messaging between services.

## ✨ Features

- 🔐 **Secure Authentication**: JWT-based auth with role-based access control
- 📄 **Document Management**: Upload, process, and manage documents with vector embeddings
- 🤖 **RAG Pipeline**: Retrieval-Augmented Generation for intelligent document-based responses
- 💬 **Chat Interface**: Real-time chat with conversation history and context awareness
- 🔄 **Async Processing**: Event-driven architecture using RabbitMQ
- 🐳 **Docker Support**: Fully containerized with Docker Compose for easy deployment
- 🧪 **Comprehensive Testing**: Unit tests for all services
- 📊 **Database Support**: PostgreSQL with pgvector for semantic search

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Applications                       │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│                    API Gateway (Port 5001)                   │
│              • Request Routing • Correlation ID              │
│         • Authentication • Rate Limiting                     │
└─┬──────────┬──────────┬──────────┬──────────┬───────────────┘
  │          │          │          │          │
  ▼          ▼          ▼          ▼          ▼
┌─────┐  ┌──────┐  ┌────────┐  ┌──────┐  ┌──────────┐
│Auth │  │Proj. │  │Document│  │Chat  │  │Rag       │
│Srv. │  │Srv.  │  │Srv.    │  │Srv.  │  │Worker    │
│5200 │  │5002  │  │5003    │  │5004  │  │(Worker)  │
└─────┘  └──────┘  └────────┘  └──────┘  └──────────┘
  │         │         │          │          │
  └─────────┼─────────┼──────────┼──────────┘
            │         │          │
    ┌───────▼─────────▼──────────▼────────┐
    │    PostgreSQL + pgvector (5432)     │
    │         • User Data                 │
    │         • Documents & Embeddings    │
    │         • Chat History              │
    └──────────────────────────────────────┘
            │
    ┌───────▼──────────────────────────────┐
    │   RabbitMQ (5672 / Management 15672) │
    │    • Async Event Processing          │
    │    • Service Communication           │
    └──────────────────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK or higher
- Docker & Docker Compose
- PostgreSQL (optional if using Docker)
- RabbitMQ (optional if using Docker)

### Setup with Docker Compose

The easiest way to get started:

```bash
# Clone the repository
git clone <repository-url>
cd Backend

# Start all services with Docker Compose
docker-compose up -d

# Wait for services to be healthy (check compose.yaml for health checks)
docker-compose ps
```

**Default Access Points:**
- API Gateway: `http://localhost:5001`
- RabbitMQ Management: `http://localhost:15672` (guest/guest)
- Database: `localhost:5432` (ersadmin/ers@1234)

### Local Development Setup

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build ERSbackend.sln

# Run migrations (for each service with database)
cd AuthService
dotnet ef database update

cd ../ProjectService
dotnet ef database update

cd ../DocumentService
dotnet ef database update

cd ../ChatService
dotnet ef database update
```

Then run each service individually or use VS Code/Visual Studio to run them with debugging.

## 📁 Project Structure

```
Backend/
├── ApiGateway/                 # API Gateway (Port 5001)
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
├── AuthService/                # Authentication Service (Port 5200)
│   ├── Controllers/
│   ├── Services/
│   ├── DTOs/
│   └── Migrations/
├── ProjectService/             # Project Management (Port 5002)
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   └── Migrations/
├── DocumentService/            # Document Management (Port 5003)
│   ├── Controllers/
│   ├── Services/
│   ├── Helpers/
│   └── Migrations/
├── ChatService/                # Chat & Conversation (Port 5004)
│   ├── Controllers/
│   ├── Services/
│   ├── Messaging/
│   └── Migrations/
├── RagWorker/                  # Background RAG Processing
│   ├── Workers/
│   ├── Services/
│   ├── Consumer/
│   └── Providers/
├── Shared/                     # Shared Models & Events
│   ├── AIResponseGeneratedEvent.cs
│   ├── ChatMessageCreatedEvent.cs
│   └── DocumentUploadedEvent.cs
├── Tests/                      # Unit Tests for each service
└── compose.yaml               # Docker Compose configuration
```

## 🔌 API Endpoints

### Authentication (Auth Service)
```
POST   /api/auth/register              # User registration
POST   /api/auth/login                 # User login
POST   /api/auth/refresh               # Refresh JWT token
POST   /api/auth/logout                # User logout
```

### Projects (Project Service)
```
GET    /api/projects                   # List all projects
POST   /api/projects                   # Create new project
GET    /api/projects/{id}              # Get project details
PUT    /api/projects/{id}              # Update project
DELETE /api/projects/{id}              # Delete project
```

### Documents (Document Service)
```
POST   /api/documents/upload           # Upload document
GET    /api/documents                  # List documents
GET    /api/documents/{id}             # Get document details
DELETE /api/documents/{id}             # Delete document
GET    /api/documents/search           # Semantic search
```

### Chat (Chat Service)
```
POST   /api/chat/messages              # Send message
GET    /api/chat/conversations         # List conversations
GET    /api/chat/conversations/{id}    # Get conversation history
DELETE /api/chat/conversations/{id}    # Delete conversation
```

## 🛠️ Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Runtime | .NET | 8.0+ |
| API | ASP.NET Core | Latest |
| Database | PostgreSQL | 16+ |
| Vector DB | pgvector | Latest |
| Message Queue | RabbitMQ | 3.x |
| Authentication | JWT (JSON Web Tokens) | - |
| Containerization | Docker | Latest |
| Testing | xUnit/NUnit | Latest |

## 📋 Configuration

### Environment Variables

Each service reads configuration from `appsettings.json` and environment-specific files:

- `appsettings.json` - Base configuration
- `appsettings.Docker.json` - Docker-specific overrides

**Key Configuration Settings:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=ers_v1;User Id=ersadmin;Password=ers@1234;"
  },
  "Jwt": {
    "Issuer": "EnterpriseRag.Auth",
    "Audience": "EnterpriseRag.Client",
    "SecretKey": "your-secret-key-here",
    "ExpirationMinutes": 60
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

## 🧪 Testing

Run unit tests for individual services:

```bash
# Auth Service tests
dotnet test AuthService.Test/AuthService.Test.csproj

# Project Service tests
dotnet test ProjectService.Test/ProjectService.Test.csproj

# Document Service tests
dotnet test DocumentService.Test/DocumentService.Test.csproj

# Chat Service tests
dotnet test ChatService.Test/ChatService.Test.csproj

# RAG Worker tests
dotnet test RagWorker.Test/RagWorker.Test.csproj

# Run all tests
dotnet test ERSbackend.sln
```

## 🔐 Security

- **JWT Authentication**: Secure token-based authentication for all API endpoints
- **CORS**: Configurable cross-origin resource sharing
- **Environment Secrets**: Sensitive data managed via environment variables
- **Password Hashing**: Industry-standard bcrypt for password storage
- **Role-Based Access Control**: Fine-grained authorization policies

## 📦 Docker Deployment

### Build Images

```bash
# Build API Gateway image
docker build -t ers/api-gateway:1.1 -f ApiGateway/Dockerfile .

# Build Auth Service image
docker build -t ers/auth-service:1.1 -f AuthService/Dockerfile .

# Build all services
docker-compose build
```

### Deploy

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Remove all data and start fresh
docker-compose down -v
```

## 🐛 Troubleshooting

### Database Connection Issues
- Ensure PostgreSQL is running: `docker-compose logs postgres`
- Check credentials in connection string
- Verify network connectivity: `docker network ls`

### RabbitMQ Connection Issues
- Verify RabbitMQ is running: `docker-compose logs rabbitmq`
- Check RabbitMQ Management UI: `http://localhost:15672`
- Ensure service hostnames are correct in configuration

### Service Not Starting
- Check logs: `docker-compose logs <service-name>`
- Verify all dependencies are healthy: `docker-compose ps`
- Ensure required ports are available

## 📝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -am 'Add new feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Submit a pull request

### Code Standards
- Follow C# coding conventions (PascalCase for classes, camelCase for variables)
- Write unit tests for new features
- Document public APIs with XML comments
- Run tests before submitting PR

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Support

For issues, feature requests, or questions:
1. Check existing issues on GitHub
2. Create a new issue with detailed description
3. Contact the development team

## 🗂️ Related Projects

- Frontend: [Enterprise RAG System Frontend](link-to-frontend-repo)
- Documentation: [Complete API Documentation](link-to-docs)
- Blog: [Enterprise RAG Technical Blog](link-to-blog)

## 🎓 Learning Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [PostgreSQL with pgvector](https://github.com/pgvector/pgvector)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [JWT Authentication](https://jwt.io/)

---

**Last Updated**: January 2026
**Version**: 1.1
**Maintainer**: Development Team
