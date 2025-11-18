# Architecture-distribuee-avec-Event-Hub-Worker_DB
## Objectifs du projet
- Remplacer l’écriture directe en base de données par un **Event Hub**.
- Introduire un nouveau **Worker Service `Worker_DB`** qui devient **le seul service autorisé à écrire dans la base de données**.
- Conserver les **messages queues** existantes pour l’échange de tâches entre services.
- Intégrer le nouveau worker dans **`docker-compose`** et dans l’**infrastructure cloud** (ARM template).
## Architecture
Le projet est organisé en plusieurs services:
- **MVC**  
  Application ASP.NET Core MVC (front-end) permettant de créer et gérer des posts
- **API**  
  API REST exposant les opérations sur les ressources (posts, commentaires, images, etc.).  
  Elle ne parle plus directement à la base de données pour les modifications:  
  elle publie des événements dans l’Event Hub.
- **Worker_Content**  
  Worker Service chargé du traitement asynchrone du contenu texte  
  (validation, enrichissement, etc.) en consommant des messages dans les queues.
- **Worker_Image**  
  Worker Service chargé du traitement des images  
  (redimensionnement, validation, etc.) et il est aussi branché sur les queues.
- **Worker_DB**  
  Nouveau Worker Service introduit dans cette itération.  
  **Rôle :** écouter l’Event Hub, désérialiser les événements et appliquer les modifications dans la base de données (Cosmos DB) et le stockage (Blob Storage).
- **CloudInfrastructure**  
  Projet de déploiement ARM (`azuredeploy.json`) pour créer les ressources Azure nécessaires:  
  - Event Hub  
  - Service Bus (queues)  
  - Cosmos DB  
  - Storage Account / Blob Containers  
  - Key Vault  
  - App Configuration  
  - Application Insights / Log Analytics, etc.
## Flux d’événements
1. **MVC / API** reçoit une action utilisateur (création, modification, commentaire, image, etc.).
2. L’application construit un objet **`Event`** (classe sérialisable) contenant:
   - le type de média (`MediaType`),
   - l’action (`EventAction` : Submitted, Resized, Validated, Refused, Deleted),
   - l’identifiant du post / commentaire,
   - des données complémentaires.
3. Cet événement est sérialisé (JSON) et envoyé vers l’**Event Hub** via les classes :
   - `MVC/Models/Event.cs`, `MVC/Models/EventHubController.cs`
   - `API/Models/Event.cs`, `API/Models/EventHubController.cs`
   - `Worker_Content/Business/EventHubService.cs`
   - `Worker_Image/Business/EventHubService.cs`
4. **Worker_DB** écoute l’Event Hub:
   - `Worker_DB/Worker.cs`
   - `Worker_DB/Services/EventHubListener.cs`
   - `Worker_DB/Event.cs`
5. À la réception, l’événement est désérialisé et **Worker_DB applique la modification** dans:
   - **Cosmos DB** (données métier),
   - **Blob Storage** (fichiers / médias associés).
Les **messages queues** (Service Bus) restent utilisées pour la distribution des tâches (traitements de contenu / d’images) entre `API`, `MVC`, `Worker_Content` et `Worker_Image`.  
Seules les **modifications de la base de données** passent par l’Event Hub et `Worker_DB`
## Déploiement dans Azure
J'ai déployé l’intégralité de la solution dans Azure en utilisant le dossier **CloudInfrastructure**, qui contient un template ARM (`azuredeploy.json`).  
Le déploiement crée automatiquement toutes les ressources nécessaires:
- Event Hub (communication des événements pour les modifications en base)
- Service Bus et ses queues (échange des tâches entre workers)
- Cosmos DB (stockage des données)
- Storage Account / Blob Containers (médias et fichiers)
- Key Vault (stockage sécurisé des secrets)
- App Configuration (stockage des configurations centralisées)
- Log Analytics / Application Insights (monitoring et logs)
Les services **API**, **MVC**, **Worker_Content**, **Worker_Image** et **Worker_DB** ont ensuite été configurés pour utiliser **App Configuration** et **Key Vault** pour récupérer dynamiquement les secrets et chaînes de connexion Azure.
L’environnement Azure pour le déploiement était entièrement fonctionnel et les communications entre services (Event Hub, Service Bus, Cosmos DB et Blob Storage) ont été validées.
## Technologies utilisées
- **.NET 9.0**
  - ASP.NET Core MVC (`MVC`)
  - ASP.NET Core Web API (`API`)
  - Worker Services (`Worker_Content`, `Worker_Image`, `Worker_DB`)
- **Azure**
  - Azure Event Hubs
  - Azure Service Bus
  - Azure Cosmos DB
  - Azure Storage Blobs
  - Azure Key Vault
  - Azure App Configuration
  - Application Insights
- **Conteneurisation**
  - Docker
  - Docker Compose
- **Tests**
  - Projet `IntegrationTesting` (tests d’intégration de bout en bout).
