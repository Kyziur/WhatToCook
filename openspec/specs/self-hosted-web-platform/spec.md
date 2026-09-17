# self-hosted-web-platform Specification

## Purpose

Defines browser access, relational persistence, and container-friendly Linux deployment for a local-network self-hosted application.

## Requirements

### Requirement: Application is available as a browser-based experience on the local network
The system SHALL be accessible from standard web browsers on devices connected to the same local network as the host so that users can use the application without installing a dedicated client.

#### Scenario: Open the application from a phone on the same network
- **WHEN** the user enters the application address from a phone connected to the same local network as the host
- **THEN** the system loads the web application and allows the user to navigate the recipe experience

#### Scenario: Open the application from another household device
- **WHEN** the user opens the application from another browser-capable device on the same local network
- **THEN** the system presents the same application with responsive layout adjustments for that device size

### Requirement: Application persists recipe data in a relational database
The system SHALL store recipe data in a relational database so that recipes remain available across application restarts and the data model can evolve safely over time.

#### Scenario: Restart the application host
- **WHEN** the application process or container is restarted after recipes have been saved
- **THEN** the system reconnects to the database and previously saved recipes remain available

#### Scenario: Save structured recipe data
- **WHEN** the user saves a recipe with metadata, ingredients, and steps
- **THEN** the system persists the structured recipe data in the configured database

### Requirement: Application supports Linux-friendly self-hosted deployment
The system SHALL support deployment in a Linux environment suitable for Raspberry Pi hosting, with a container-friendly runtime model, configuration externalized from code, and no embedded production credentials.

#### Scenario: Start the application with external configuration
- **WHEN** the application starts in a Linux deployment environment with database and runtime settings provided externally
- **THEN** the system uses the supplied configuration without requiring source-code changes

#### Scenario: Deploy the application in a container-based environment
- **WHEN** the application is deployed using a container-based workflow on a Linux host
- **THEN** the system starts successfully and exposes the web application to the local network

#### Scenario: Start without a database connection string
- **WHEN** the application starts without any configured database connection string
- **THEN** startup fails with an explicit configuration error instead of using a default credential
