# equipment-state-management

# System design

## Problem analysis and functional requirements

This solution is designed to reduce manual work in a Lego brick manufacturing factory where workers need to change manually color of the magnet attached to the equipement used.
Solution is made assuming that we want to automate process in one factory where we can have hundreds pieces of equipement and that the system should be scalable, configurable and efficient in the case we add more equipement or we want to use it for multiple factories.

## Physical devices

##### Worker Devices:
 Workers need to have devices (Smartphones, tablets, desktop computers) where they can update the equipment state and see the overview of equipement. This affects the way we design our frontend in multiple ways.

##### Responsive User Interface:
The user interface (UI) needs to be responsive and optimized for different screen sizes and device capabilities. This will affect the frontend design, requiring a responsive web app. (or mobile native app)

##### Offline Mode and Local Storage:
We should cover the situation that mobile, tablet or other device can lose internet connectivity and ensure that data is still stored and not lost. When the worker updates the state of a equipement, the application should store the update locally on local storage (SQLite, localStorage). This will be used while device is offline.
Once device is reconnected backend should read from the local storage and update the devices accordingly.

##### Conflict resolution:
A potential challenge arises when multiple workers attempt to update the same equipment while offline. Design should offer solution for conflicts (Timestamp-based synchronization).

## Key Requirements:

- Real-Time Equipment Updates: Workers are able to update the state of equipment (green/yellow/red).
- Central Monitoring: There is a centralized system where workers can see the state of the equipement.
- Implement historical Data: Supervisors need access to historical data for equipment state changes.
- Scalability: explained in the arhitecutural considerations

## Arhitecture analysis and considerations

1. Scalability (increased number of workers and increased number of machines)- 
- Designed to support hundreds of machines and thousands of workers (number of machines and workers can grow).
- Scalable accross the factories
- Accountable for real time updates, efficient data storing and keeping the history.

Potential challenge which needs to be addressed with a solution is the case where potentialy multiple workers can try to update the state of the same machine - system must ensure that there is no race conditions or data loss.

2. Availability - system needs to track real time updates and to serve historical data
   No delays: Real time updates should be visible instantly and historical data should be easy to retreive.

Potential challenge can happen when updating large number of machines at the same time and updates need to be visible in real time to supervisors and workers - keeping low latency is an important part for this case.

3. Roles and security
   System must ensure that workers can update the machines they are assigned to (or all machines depending on the requirement but I assume that workers have machines they are assigned to).
   System must ensure that only supervisor can see historical data.

4. Reliability - Since system will have a lot of write operations, it must ensure that data is stored without any loss.
   Assuming that there will be a notification system about the state of the machines, implementation of real time alerts would be also important part of the system.

Retreival of large amount of historical data should be fast and instant when supervisors want to access it.

5. Event-Driven Architecture - where each state change triggers an event that updates the cache, pushes updates to supervisors, and logs the change in the database.

## High level system design:

#### 1. Frontend: A dashboard (web-based) that allows workers and supervisors to perform actions.

Framework which can be used is React for responsive design and interaction.

Worker part (Frontend):

- Allows workers to log in, view the equipment and update the state
- Provides real-time updates using WebSockets to track the status of each equipment.

Supervisor App (Frontend):
Implementation of the dashboard where supervisors can see and query historical data.

#### Microservice arhitecture:
2. Microservice Architecture:

2.1  Equipment Service: 
Manages the state of the equipment (green/yellow/red).

2.2 History Service: 
Managing historical changes (get historical data and write)

2.4 Authentication & Authorization Service:
JWT authentication for workers and supervisors and role based access
Handles user login and token validation for communication between services.

2.5 WebSocket Service:
Listens to events from the Equipment Service and creates real time updates.

2.6 Redis as a shared cache layer: 
Caches the latest state of each equipment.
Synchronizes with DynamoDB 

2.7 Messaging Service (RabbitMQ):
Manages the message queue for real-time changes.
The Equipment Service and History Service publish events to this service, and the WebSocket and Cache Services consume these events.

       +-------------------+              +---------------------+
       |  Equipment State   |----Publish-->|  Messaging service |
       |      Service       |              +--------------------+
       +-------------------+                       |
                |                                   |
                |                                   |
      +---------+---------+                 +------v-------+             +-----------------+
      |   Redis Cache      |                 | WebSocket     | <----------| Historical      |
      |  (Shared)          |                 |  Service      |             |  Data Service   |
      +-------------------+                 +--------------+             +-----------------+


#### 3. API:

Api exposes endpoints for routing to the services.
API is responsible for handling authentication and authorization for workers and supervisors.
API endpoints to be documented using Swagger standards to facilitate external integration.


#### 4. Database layer and cache layer:

- NoSQL Database (e.g., DynamoDB) for equipment state updates and historical state changes. This ensures scalability and high-speed writes since we will have write heavy system.
- Redis as a cache - to cache the latest state of each piece of equipement for fast retreival.
  Since mobile version will have local storage (SQLite..) to store the data when device is offline, Redis cache will need to be synchronized with the latest data.

(in the implementation I am using SQLLite DB to write updates, because of time constraint)

#### 5. Messaging Layer: RabbitMQ or Kafka for processing real time changes.

This layer should handle equipment state updates and ensure that updates are visible in real time.
Also, in case of huge load, it offers load balancing solution.

#### 6. Authentication: 
Using JWT token for authentication and role based system is needed for the roles (supervisor, worker).

#### 7. Deployment 
- Docker and Kubernetes
- CI/CD pipeline to continuously build, test, and deploy services

#### 8. Real time monitoring system and health check
Example: Prometheus + Grafana for real-time performance metrics and alerts.

# Development approach:

Since system needs to be highly available Test driven development is a good approach.

Current implementation focuses on managing equipment state updates in real-time using Redis, WebSocket, and a database (SQLite for now, but designed for DynamoDB).

### .NET Project Structure
#### Selected exercise: Live updates to applications when equipement states are changed

Controllers:
EquipmentController handles API requests to update the equipment state and retrieve the history of changes.

Services:
RedisService: Interactions with the Redis cache.
DynamoDbService: Manages reading and writing data to the database. ( Currently, it is using SQLite but is designed to work with DynamoDB.)
WebSocketHandler: Manages WebSocket connections and manages real time updates.

#### Redis

To run redis locally for testing run the command: docker run --name redis -p 6379:6379 -d redis
This will start Redis on localhost:6379

#### SQLite

Due to time constraints, I decided to use SQLite as the database for equipment state history.
Future Plan: Ideally, this would be replaced with Amazon DynamoDB for its scalability and ability to handle high-write

#### WebSocket Service

Websocket service is responsible for interactions between clients and the system. The app is using WebSocket middleware provided by .NET.

Test: Since there is no Frontend app which would check if the message is received on update, idea was to mock the client in the websocket test class and test if the message will be received on state update. (WebSocket_ShouldReceiveMessage_WhenEquipmentStateIsUpdated)

Due to time constraint some functions are not fully implemented and there is space for improvements.



