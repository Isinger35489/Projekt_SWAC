
# SIMS - Security Incident Management System

![Build Status](https://badgen.net/badge/build/passing/green)
![Version](https://badgen.net/badge/version/1.0.0/blue)
![License](https://badgen.net/badge/license/MIT/green)
![.NET](https://badgen.net/badge/.NET/7.0/purple)

## 📋 Beschreibung

SIMS (Security Incident Management System) ist ein System zum Protokollieren und Verwalten von IT-Sicherheitsvorfällen. Es ermöglicht die manuelle Erfassung von sicherheitsrelevanten Vorfällen, Eskalation an zuständige Bearbeiter, Benutzer- und Rollenverwaltung sowie Benachrichtigungen über verschiedene Kanäle.

## ✨ Features

- **Vorfall-Management**: Erstellen, Bearbeiten und Schließen von Security-Incidents
- **Eskalationssystem**: Automatische Weiterleitung mithilfe eines Chatbots
- **Benutzerverwaltung**: Rollenbasierte Zugriffskontrolle (z. B. Administrator, Benutzer), Nutzer aktivieren/deaktivieren
- **Logging**: Vollständige Protokollierung aller Systemaktivitäten
- **Session-Management**: Redis für Session-State, damit z. B. der Vorfall-Entwurf bei Abbruch weiterbearbeitet werden kann
- **API-Integration**: Authentifizierung und User-Management als Microservice
- **Notifizierungen**: Übermittlung mithilfe von Chatbot (BOT-Tom)
- **Dockerized**: Alle Hauptkomponenten laufen in eigenen Docker Containern in einem separaten Network (momentan nur SQL-DB & Redis)



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

# auf DB schreiben testen:
# - in CMD das hinzufügen nachdem der Container läuft:
curl -X POST "http://localhost:5013/api/session?key=testuser&value=john_doe"

# schauen obs funktioniert hat:
# - in CMD auf Container verbinden:
docker exec -it redis-1 redis-cli
get testuser
"john_doe"

# testen von Sessions in Redis schreiben:
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
    
-   Incident (Status, CVE, Zeitstempel, Handler, Reporter, Alert-Level (Escalation Level), Severity, System, Beschreibung)
    
-   Log (Usersessions)

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

## 🔒 Sicherheit

### Aktueller Stand

- **Authentifizierung & Sessions**
  - Login über die API, Session-Daten werden in Redis gespeichert (`RedisSessionService`).
  - Ein API-Key ist in der Konfiguration vorgesehen (`Security:ApiKey`), um geschützte Endpunkte abzusichern.

- **Passwortschutz**
  - Passwörter werden nicht im Klartext gespeichert, sondern vor dem Speichern gehasht (`PasswordHasher`).
  - Damit landen echte Passwörter weder in der Datenbank noch in Logs.

- **Datenbankzugriff**
  - Zugriff auf SQL Server erfolgt ausschließlich über Entity Framework Core (parametrisierte Zugriffe, keine selbstgebauten SQL-Strings) → reduziert das Risiko klassischer SQL-Injection.
  - Das Schema (User, Incident, Log) wird über EF-Migrations verwaltet.

- **Transport & Konfiguration**
  - Die API ist für HTTPS-Betrieb ausgelegt (Kestrel Dev-Zertifikat).
  - Sensible Werte wie ConnectionStrings, API-Key und Telegram-Bot-Token liegen in `appsettings*.json` und können für produktive Umgebungen über Environment-Variablen/Secret-Store gesetzt werden.

- **Nachvollziehbarkeit**
  - Incidents speichern Zeitstempel (CreatedAt/ClosedAt), Reporter/Handler und Severity.
  - Redis wird genutzt, um z. B. `last_access` oder `last_incident_created` für einfache Session-/Aktivitätsverfolgung zu halten.


### Mögliche Security-Erweiterungen

- **Rollen & Rechte schärfen**  
  Admin-Endpunkte klar trennen und nur für Admin-Rollen freigeben.

- **Login & Sessions absichern**  
  Rate-Limiting, Lockout nach mehreren Fehlversuchen, kürzere Session-Dauer.

- **Secrets sicher speichern**  
  DB-Passwort, API-Key, Bot-Token per Environment-Variablen / Secret-Store statt in `appsettings.json`.

- **Audit-Logs nutzen**  
  Log-Tabelle verwenden für wichtige Aktionen (Logins, Rollenänderungen, Incident-Eskalationen).

- **HTTP-Schnittstelle härten**  
  Security-Header setzen und technische Details in Fehlermeldungen nach außen vermeiden.

- **Automatisierte Code-Scans**  
  Semgrep regelmäßig in einer CI-Pipeline laufen lassen.


## 🔒 SAST

### Semgrep-Ergebnisse

**Semgrep Prüfung**

semgrep --config=auto .
-   **Code Smells vermeiden:**  Clean Code, Rollenprüfungen, Sicherstellung parametrisierter SQL-Queries (Dapper/EF), Authentifizierung mit Token (JWT)
    
-   **Findings dokumentieren:**  (z. B. 0 Critical, 2 Medium, 4 Low)
    
-   **XSS:**  Nicht relevant (keine Web-Oberfläche)
    
-   **Passwortschutz:**  Alle Passwörter gehasht

semgrep --config=auto .

**Findings**: 

──── ○○○ ────┐
│ Semgrep CLI │
└─────────────┘

                                                                                                                     
Scanning 86 files (only git-tracked) with:
                                      
✔ Semgrep OSS
  ✔ Basic security coverage for first-party code vulnerabilities.
                                              
✘ Semgrep Code (SAST)
  ✘ Find and fix vulnerabilities in the code you write with advanced scanning and expert security     
rules.                                                                                                               
                                                     
✘ Semgrep Supply Chain (SCA)
  ✘ Find and fix the reachable vulnerabilities in your OSS dependencies.
                                                                            
💎 Get started with all Semgrep products via `semgrep login`.
✨ Learn more at https://sg.run/cloud.                        
                                                                            
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 0:00:00                                                                                                                     
                   
                   
┌─────────────────┐
│ 4 Code Findings │
└─────────────────┘
                                                            
    SIMS.API/Controllers/SessionController.cs
    ❯❱ csharp.dotnet.security.mvc-missing-antiforgery.mvc-missing-antiforgery
          Set is a state-changing MVC method that does not validate the antiforgery token or do strict     
          content-type checking. State-changing controller methods should either enforce antiforgery tokens
          or do strict content-type checking to prevent simple HTTP request types from bypassing CORS      
          preflight controls.                                                                              
          Details: https://sg.run/Y0Jy                                                                     
                                                                                                           
           16┆ [HttpPost]
           17┆ public IActionResult Set([FromQuery] string key, [FromQuery] string value)
           18┆ {
           19┆     _service.SetSession(key, value);
           20┆     return Ok();
           21┆ }
                                      
    SIMS.API/Dockerfile
   ❯❯❱ dockerfile.security.missing-user-entrypoint.missing-user-entrypoint
          By not specifying a USER, a program in the container may run as 'root'. This is a security  
          hazard. If an attacker can control a process running as root, they may have control over the
          container. Ensure that the last USER in a Dockerfile is a USER other than 'root'.           
          Details: https://sg.run/k281                                                                
                                                                                                      
           ▶▶┆ Autofix ▶ USER non-root ENTRYPOINT ["dotnet", "SIMS.API.dll"]
           29┆ ENTRYPOINT ["dotnet", "SIMS.API.dll"]
                                            
    SIMS.API/appsettings.json
   ❯❯❱ generic.secrets.security.detected-telegram-bot-api-key.detected-telegram-bot-api-key
          Telegram Bot API Key detected
          Details: https://sg.run/nd4b 
                                       
           26┆ "BotToken": "8213041452:AAGWnzP24LhV57jRdoaP0IA-JOcpuDCrtik",
                                      
    SIMS.Web/Dockerfile
   ❯❯❱ dockerfile.security.missing-user-entrypoint.missing-user-entrypoint
          By not specifying a USER, a program in the container may run as 'root'. This is a security  
          hazard. If an attacker can control a process running as root, they may have control over the
          container. Ensure that the last USER in a Dockerfile is a USER other than 'root'.           
          Details: https://sg.run/k281                                                                
                                                                                                      
           ▶▶┆ Autofix ▶ USER non-root ENTRYPOINT ["dotnet", "SIMS.Web.dll"]
           30┆ ENTRYPOINT ["dotnet", "SIMS.Web.dll"]

                
                
┌──────────────┐
│ Scan Summary │
└──────────────┘
✅ Scan completed successfully.
 • Findings: 4 (4 blocking)
 • Rules run: 133
 • Targets scanned: 86
 • Parsed lines: ~100.0%
 • Scan was limited to files tracked by git
 • For a detailed list of skipped files and lines, run semgrep with the --verbose flag
Ran 133 rules on 86 files: 4 findings.
💎 Missed out on 1390 pro rules since you aren't logged in!
⚡ Supercharge Semgrep OSS when you create a free account at https://sg.run/rules.

⏫ A new version of Semgrep is available. See https://semgrep.dev/docs/upgrading

Fazit zu den Findings:

Es wurden 4 Findings gefunden, keine davon kritisch, aber alle sicherheitsrelevant.
SessionController: POST /api/session ändert Serverzustand ohne CSRF-/Antiforgery-Schutz oder strikte Content-Type-Prüfung → in Produktion absichern oder entfernen.
Dockerfiles (API & Web): Container laufen aktuell als root → künftig eigenen, nicht-privilegierten User verwenden.
Telegram-Bot-Token liegt in appsettings.json → Token rotieren und in Zukunft nur über Environment-Variablen / Secret-Store, nicht im Git-Repo. 


## 📊 Versionshistorie

### Version 1.0.0 (2025-11-08)
- Fertiges, dockerisiertes System für SQL Datenbank und Redis
- Entity Framework Core Integration / EF-Migration
- Vorfallmanagement & Logging
- Benutzerverwaltung & Authentifizierung (REST)
- Docker-Integration (Dockerfiles für API, Web noch in Weiterentwicklung)
- Redis Session-Management integriert
- Web-Frontend mit Login & Logout mit Web-Sessions
- Passwort Hashing
- Chatbot (BOT-Tom)
- Semgrep-Check

## 🗺️ Roadmap

### Version 1.1.0 (geplant Q1 2026)
- [ ] komplette Docker-Integration
- [ ] Erweiterung der Web-Applikaton (Einbau von Chatbot-Assistent)


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

- Esra Aktas - Frontend Entwicklung, Klassendiagramm, Read Me, Docker
- Sophie Stereb - API-Entwicklung/Authentifizierung, Datenbank(SQL), Redis
- Sasa Vladuljevic - Chat-Bot, Security Maßnahmen, Docker

## 🔗 Links

- **GIT Repository**: https://github.com/Isinger35489/Projekt_SWAC/
- **Issue Tracker**: https://github.com/Isinger35489/Projekt_SWAC/issues

## 📞 Support (in Außnahmefällen)

Bei Fragen oder Problemen: sims-support@ustp-students.at


## Anleitung zur Passworthash Migration von bestehenden SQL Datenbanken:

- in der SIMS.API Applikation im Program.cs Zeilen 67-110 wieder reinkommentieren
- die Zeilen 67-110 in Program.cs sorgen dafür, dass alle bestehenden User Passwörter gehashed werden
- Anschließend die Applikation einmal starten, damit die Migration durchgeführt wird
- die Zeilen 67-110 sollen nur für die einmalige Passworthash Migration aktiviert werden
- danach wieder auskommentieren, damit die Passwörter nicht bei jedem Start erneut gehashed werden
