💬 Simple Chat App

A lightweight, real-time messaging application built with .NET Core. This project demonstrates how to handle instant text communication, media sharing, and user discovery.

🚀 Features
1. Instant Text Messaging
Real-time delivery: Low-latency messaging using SignalR.

3. Image Sharing
Media Support: Send snapshots or gallery images directly in the chat.

Preview: High-quality image rendering within the message bubbles.

3. User Discovery (Search)
Find Friends: Search for users by username.

🛠 Tech Stack
Backend: .NET Core (ASP.NET Core MVC)

Real-time Engine: SignalR

Database: Firebase

Frontend: Java Script

📦 Getting Started
Prerequisites
.NET SDK (latest version)

Your favorite IDE (Visual Studio, VS Code, or JetBrains Rider)

Installation
Clone the repository:

Bash
git clone https://github.com/hoang9498/chatApp.git
cd chat-app
Restore dependencies:

Bash
dotnet restore
Update Database:

Bash
dotnet ef database update
Run the application:

Bash
dotnet run
📂 Project Structure
ChatApp.Controller/ - The core logic and controllers.

ChatApp.Models/ - Data structures and DTOs.

ChatApp.Views/ - The frontend client application.

ChatApp.Service/ - For Firebase Service, Image Checking, Cache

ChatApp.Hub/  - SignalR
