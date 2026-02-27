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

Demo send text message:

[text.webm](https://github.com/user-attachments/assets/1e5c020e-8571-49e7-9a4e-ae5fca26076d)

Demo send image message:

[send-image.webm](https://github.com/user-attachments/assets/da7a6162-477f-49e4-a200-05899d3c5a58)

Demo load older messages:

[load-older-message.webm](https://github.com/user-attachments/assets/40e57a77-56ca-4ead-91af-dae248d41183)

Demo search user:

[search-user.webm](https://github.com/user-attachments/assets/0eec8dbf-fad2-4956-93a9-f7907fd1435d)

