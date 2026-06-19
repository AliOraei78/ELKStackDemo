# ELKStackDemo

An educational **ELK Stack** (Elasticsearch + Logstash + Kibana) sample project integrated with ASP.NET Core Web API.

This project is designed as a portfolio sample to demonstrate backend skills, structured logging, and advanced search capabilities.

## Project Goal

A hands-on, end-to-end learning implementation of the ELK Stack integrated into a .NET 8 application. This project is suitable for GitHub and LinkedIn portfolios and demonstrates experience with modern observability and search tooling.

## Technologies Used

* **Backend**: ASP.NET Core Web API (.NET 10)
* **Logging**: Serilog (Structured Logging)
* **Search Engine**: Elasticsearch 8.15
* **Visualization**: Kibana
* **Log Processing**: Logstash
* **Containerization**: Docker & Docker Compose
* **Client**: Elastic.Clients.Elasticsearch

## Project Progress (by Days)

### Day 1: Introduction and Core Concepts of ELK Stack

* Introduction to Elasticsearch, Logstash, and Kibana
* Understanding ELK Stack architecture
* Creating ASP.NET Core Web API project named **ELKStackDemo** in Visual Studio
* Setting up GitHub repository and initial push

### Day 2: Installing and Running ELK Stack with Docker

* Installing Docker Desktop
* Creating `docker-compose.yml` for Elasticsearch, Kibana, and Logstash
* Running services using `docker compose up -d`
* Verifying access to Elasticsearch (`http://localhost:9200`) and Kibana (`http://localhost:5601`)

### Day 3: ASP.NET Core Project Structuring

* Creating a professional project structure (Models, Controllers, Services, DTOs, Configuration folders)
* Implementing `HealthController` for API health checks
* Enabling Swagger UI
* Running initial project and testing health endpoint

### Day 4: Integrating Serilog for Structured Logging

* Installing Serilog packages (AspNetCore, Console, File, Http)
* Configuring Serilog in `Program.cs` and `appsettings.json`
* Setting up Console, File (JSON), and Http sinks
* Adding enrichers for enhanced logging context
* Updating `HealthController` to generate test logs

### Day 5: Connecting to Elasticsearch using Official Client

* Installing `Elastic.Clients.Elasticsearch` package
* Creating `Product` model
* Implementing `ElasticsearchService` with basic CRUD operations:

  * Index creation
  * Document indexing
  * Simple search (Match Query)
  * Get and delete operations
* Creating `ElasticsearchController`
* Full testing via Swagger UI

### Day 6: Document Indexing and Management

* Updated the `Product` model with mapping attributes
* Implemented explicit index mapping using the Fluent API
* Added bulk indexing functionality
* Created endpoints for mapping management and bulk document insertion
* Successfully tested document indexing operations

### Day 7: Basic Search Queries

* Implemented Match Query, Term Query, MultiMatch Query, and Bool Query
* Resolved the CS1660 compilation error in the Fluent API
* Added multiple search endpoints
* Performed comprehensive testing of basic search operations through Swagger UI

### Day 8: Advanced Search (Full-text, Aggregations)

* Implemented advanced Bool Query with Filters (category and price range)
* Added Sorting and Boosting in search queries
* Implemented Aggregations (Terms, Average, Range)
* Created Advanced Search and Aggregations endpoints
* Successfully tested complex search queries and statistical analysis

### Day 9: Logstash Pipeline Configuration

* Created `pipeline.conf` with HTTP Input, Grok + JSON Filters, and Elasticsearch Output
* Mounted configuration into Docker Compose setup
* Restarted the ELK Stack services
* Successfully tested full log pipeline flow from .NET → Logstash → Elasticsearch

## How to Run the Project

### Prerequisites

1. Visual Studio 2022 (or later)
2. Docker Desktop
3. .NET 10 SDK

### Running ELK Stack

```bash
cd docker
docker compose up -d
```
