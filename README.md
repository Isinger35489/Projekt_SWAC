
# SIMS - Security Incident Management System

![Build Status](https://badgen.net/badge/build/passing/green)
![Version](https://badgen.net/badge/version/1.0.0/blue)
![License](https://badgen.net/badge/license/MIT/green)
![.NET](https://badgen.net/badge/.NET/7.0/purple)

## 📋 Beschreibung

SIMS (Security Incident Management System) ist ein System zum Protokollieren und Verwalten von IT-Sicherheitsvorfällen. Es ermöglicht die manuelle Erfassung von sicherheitsrelevanten Vorfällen, Eskalation an zuständige Bearbeiter, Benutzer- und Rollenverwaltung sowie Benachrichtigungen über verschiedene Kanäle.

## ✨ Features

- **Vorfall-Management**: Erstellen, Bearbeiten und Schließen von Security-Incidents
- **Eskalationssystem**: Automatische Weiterleitung an zuständige Bearbeiter mit Level-System
- **Benutzerverwaltung**: Rollenbasierte Zugriffskontrolle (z. B. Administrator, Benutzer), Nutzer aktivieren/deaktivieren
- **Logging**: Vollständige Protokollierung aller Systemaktivitäten
- **Session-Management**: Redis für Session-State, damit z. B. der Vorfall-Entwurf bei Abbruch weiterbearbeitet werden kann
- **API-Integration**: Authentifizierung und User-Management als Microservice, Schnittstellen-Datei (z. B. für Monitoring-Tools wie Nagios/Prometheus)
- **Notifizierungen**: Grundgerüst für Message-Bus, Email, SMS und weitere Kanäle
- **Dockerized**: Alle Hauptkomponenten laufen in eigenen Docker Containern in einem separaten Network



## 🚀 Systemvoraussetzungen

- **Betriebssystem**: Windows 11, Linux (Ubuntu 20.04+), macOS 11+
- **Runtime**: .NET 7.0 SDK
- **Docker**: Docker Desktop 4.0+ oder Docker Engine 20.10+
- **RAM**: Minimum 4 GB
- **Festplatte**: 2 GB freier Speicher
- **Git**: GIT-Account auf  [https://git.nwt.fhstp.ac.at](https://git.nwt.fhstp.ac.at/)  oder Github (Projektverwaltung)

## 📦 Installation und Start

### 1. Repository klonen

git clone GIT-REPO-URL
cd SIMS

### 2. Docker-Container starten

cd Docker
docker-compose up -d

# 2.1 Docker einzeln starten
# SQL Container für Projekt:
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name db-1 --hostname db-1 -d mcr.microsoft.com/mssql/server:2022-latest

# Redis Container fürs Projekt:
docker run -d --name redis-1 -p 6379:6379 redis:latest

# auf DB schrieben testen:
# -in CMD das hinzufügen nachdme der Container läuft:
curl -X POST "http://localhost:5013/api/session?key=testuser&value=john_doe"

# schauen obs funktionier hat:
# -in CMD auf container verbinden:
docker exec -it redis-1 redis-cli
get testuser
"john_doe"


- testen von Sessions in Redis schreiben:
curl -X POST http://localhost:5013/api/incidents -H "Content-Type: application/json" -d "{\"ReporterId\":1,\"HandlerId\":1,\"Description\":\"Test Incident\",\"Severity\":\"High\",\"Status\":\"Open\",\"CVE\":\"CVE-123\",\"EscalationLevel\":1,\"System\":\"WebServer\",\"CreatedAt\":\"2025-11-13T10:00:00\"}"



### 3. Datenbank initialisieren
### 3.1 SQL Container starten (falls nicht schon geschehen)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name db-1 --hostname db-1 -d mcr.microsoft.com/mssql/server:2022-latest



docker exec -it sims-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -i /docker-entrypoint-initdb.d/create_database.sql

### 4. Anwendung starten

docker exec -it sims-app dotnet SIMS.App.dll

## 🏗️ Architektur

### ER-Diagramm

```
+----------------------+
|        User          |
+----------------------+
| + Id : int           |
| + Username : string  |
| + PasswordHash : string |
| + Email : string     |
| + Role : string      |
| + Enabled : bool     |
| + CreatedAt : DateTime |
+----------------------+
           |
           | 1         (Reportet)
           |----------<  n
           |             |
           v             v
+----------------------+
|      Incident        |
+----------------------+
| + Id : int           |
| + ReporterId : int   | ---+  (FK -> User)
| + HandlerId : int    | ---+
| + Description: string|
| + Severity: string   |
| + Status: string     |
| + CVE: string        |
| + EscalationLevel: int|
| + System: string     |
| + CreatedAt: DateTime|
| + ClosedAt: DateTime?|
+----------------------+
           |
           | n
           |----------< 1
           |
           v
+----------------------+
|        Log           |
+----------------------+
| + Id : int           |
| + Timestamp: DateTime|
| + Loglevel: string   |
| + Message : string   |
| + UserId : int       | (FK -> User)
+----------------------+
```

-   User (Administrator, Benutzer, aktiviert/deaktiviert)
    
-   Incident (Schweregrad, Status, CVE, Zeitstempel, ... ggf. Handler)
    
-   Escalation (optional eigene Tabelle, sonst als Incident-Status)
    
-   Log (Aktionen, Fehler, User)

### Klassendiagramm
```
+----------------------------------------------------------+
|                      RoleType (enum)                     |
|----------------------------------------------------------|
| Admin, User                                              |
+----------------------------------------------------------+

+------------------+      +---------------+            +-----------+
|      User        |<>----|   Incident    |<>----------|   Log     |
+------------------+      +---------------+            +-----------+
| Id               |      | Id            |            | Id        |
| Username         |      | ReporterId    |            | Timestamp |
| PasswordHash     |      | HandlerId     |            | Loglevel  |
| Email            |      | Description   |            | Message   |
| Role: RoleType   |      | Severity      |            | UserId    |
| Enabled          |      | Status        |            +-----------+
| CreatedAt        |      | CVE           |     
+------------------+      | EscalationLvl |     (User meldet Incident)
| +Validate()      |      | System        |     (Incident erzeugt Log)
| +Disable()       |      | CreatedAt     |
| +Enable()        |      | ClosedAt      |
+------------------+      +---------------+
                                      
           +------------------------------+
           |      IRepository (Interface) |
           |------------------------------|
           | +GetById(id)                 |
           | +GetAll()                    |
           | +Add(entity)                 |
           | +Update(entity)              |
           | +Delete(id)                  |
           +--------------+---------------+
                          ^
           +--------------|---------------+
           |              |               |
    +----------------+ +----------------+ +--------------+
    | UserRepository | |IncidentRepository| |LogRepository|
    +----------------+ +----------------+ +--------------+

+-------------------------+      +------------------------+
| INotificationService    |      |   NotificationService  |
|-------------------------|      +------------------------+
| +Notify(userId, msg)    |<-----| implements             |
+-------------------------+      +------------------------+

+------------------+
|  AuthService     |
+------------------+
+----------------------+
| RedisSessionService  |
+----------------------+
````


### Docker-Architektur

┌─────────────┐
│ SIMS.App │
│ (Console) │
└──────┬──────┘
│
├──────────┐
│ │
┌──────▼──────┐ ┌▼──────────┐
│ SIMS.API │ │ Redis │
│ (REST API) │ │ (Session) │
└──────┬──────┘ └───────────┘
│
┌──────▼──────┐
│ SQL Server │
│ (SIMSDB) │
└─────────────┘


## 🔒 Security (SAST)

### Semgrep-Ergebnisse

**Semgrep Prüfung**

semgrep --config=auto .
-   **Code Smells vermeiden:**  Clean Code, Rollenprüfungen, Sicherstellung parametrisierter SQL-Queries (Dapper/EF), Authentifizierung mit Token (JWT)
    
-   **Findings dokumentieren:**  (z. B. 0 Critical, 2 Medium, 4 Low)
    
-   **XSS:**  Nicht relevant (keine Web-Oberfläche)
    
-   **Passwortschutz:**  Alle Passwörter gehasht

semgrep --config=auto .

**Findings**: 


## 📊 Versionshistorie

### Version 1.0.0 (2025-11-08)
- Fertiges, dockerisiertes System für alle Kernfeatueres
- Vorfallmanagement & Logging
- Benutzerverwaltung & Authentifizierung (REST)
- Docker-Integration
- Redis Session-Management integriert
- Semgrep-Check

## 🗺️ Roadmap

### Version 1.1.0 (geplant Q1 2026)
- [ ] Entity Framework Core Integration /EF-Migration
- [ ] Azure Service Bus /Message Queue für Notifications
- [ ] Web-Frontend

### Version 1.2.0 (geplant Q2 2026)
- [ ] LDAP/Active Directory Integration
- [ ] Advanced Reporting & Analytics
- [ ] Mobile App (MAUI)
- [ ] Automatische CVE-Datenbank-Integration

## 📄 Lizenz

MIT License

Copyright (c) 2025 SIMS Team

Permission is hereby granted, free of charge, to any person obtaining a copy...

## 👥 Mitwirkende

- Esra Aktas- Projektleitung & Backend-Entwicklung
- Sophie Stereb - API-Entwicklung/Authentifizierung
- Sasa Vladuljevic - Docker & DevOps

## 🔗 Links

- **GIT Repository**: https://git.nwt.fhstp.ac.at/username/SIMS
- **Issue Tracker**: https://git.nwt.fhstp.ac.at/username/SIMS/issues

## 📞 Support (in Außnahmefällen)

Bei Fragen oder Problemen: sims-support@ustp-students.at


## Anleitung zur Passworthash Migration von bestehenden SQL Datenbanken:

- in der SIMS.API Applikation im Program.cs Zeilen 67-110 wieder reinkommentieren
- die Zeilen 67-110 in Program.cs sorgen dafür, dass alle bestehenden User Passwörter gehashed werden
- Anschließend die Applikation einmal starten, damit die Migration durchgeführt wird
- die Zeilen 67-110 sollen nur für die einmalige Passworthash Migration aktiviert werden
- danach wieder auskommentieren, damit die Passwörter nicht bei jedem Start erneut gehashed werden
