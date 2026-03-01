# Real-Time Voting System

## Overview

This project is a real-time voting system built using modern web technologies.

It allows users to vote for candidates under specific positions with live result updates and countdown synchronization.

The system supports:

- Real-time voting updates  
- Live countdown timer synchronization  
- Admin management dashboard  
- Secure authentication  
- Scalable architecture  

---

## System Architecture

Users
 ↓
Angular Frontend
 ↓
ASP.NET Core API
 ↓
SignalR WebSocket Layer
 ↓
MySQL Database

---

## Technologies Used

### Frontend
- Angular 17  
- HTML5  
- CSS3  
- TypeScript  

### Backend
- ASP.NET Core Web API  
- SignalR (Real-time communication)  

### Database
- MySQL  

### Real-Time Communication
- WebSocket via SignalR  

### Infrastructure (Optional)
- Load Balancer  
- Redis Backplane for scaling  

---

## Features

### User Features

- Login and registration  
- View voting positions  
- View candidates per position  
- Live countdown timer  
- Real-time vote results  
- Vote for one candidate per position  
- Prevent double voting  
- Voting history tracking  

---

### Admin Features

- Create / edit / delete positions  
- Create / edit / delete candidates  
- Manage voting timers  
- Extend or close voting period  
- View live analytics  
- User management  

---

##  Real-Time Features

- Live vote count updates  
- Live result dashboard  
- Countdown synchronization  
- Instant voting notifications  

Powered by **SignalR WebSockets**.

---

## 🗄 Database Schema (MySQL)

### Users Table
- Id  
- Username  
- Email  
- PasswordHash  
- Role  

### Positions Table
- Id  
- Name  
- StartTime  
- EndTime  
- Status  

### Candidates Table
- Id  
- Name  
- PhotoUrl  
- PositionId  

### Votes Table
- Id  
- UserId  
- CandidateId  
- PositionId  
- VotedAt  

---

## Installation Guide

### 1. Clone Repository
```bash
git clone https://github.com/R-JDiaz/Parallel-Voting-App-Finals.git
cd Parallel-Voting-App-Finals
``` 

### 2. Backend Setup

Navigate to backend project:
```bash
cd backend
``` 
Restore dependencies:
``` bash
dotnet restore
``` 
Run project:
``` bash
dotnet run
``` 
### 3. Frontend Setup

Navigate to Angular project:
``` bash
cd frontend
npm install
ng serve
``` 
Open browser:
``` bash
http://localhost:4200
```
### 4. Database Setup
Create MySQL database:
``` sql
CREATE DATABASE voting_system;
```
Run migrations:
``` bash
dotnet ef database update
```
##  Security Features

JWT Authentication

Role-based authorization

Vote validation rules

Input sanitization

HTTPS enforcement

## Future Improvements

⭐ Redis distributed caching
⭐ Multiple server load balancing
⭐ AI-based voting analytics
⭐ Mobile app integration
⭐ Blockchain vote verification

## Testing

Run backend tests:
``` bash
dotnet test
```
Run frontend tests:
``` bash
ng test
```
## Contributors

    1. Robert James Diaz
    2. Jay Entiliso
    3. Rhio Dela Cruz
