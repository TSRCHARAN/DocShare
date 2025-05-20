# 📁 DocShare – Document Sharing App

**DocShare** is a secure and lightweight web application that allows users to upload, manage, and share documents, images, and videos. Each file is assigned a unique DocID that can be shared and used to retrieve the content.

---

## 🚀 Features

- 📤 Upload documents, images, and videos
- 🔗 Generate a unique DocID for sharing
- 📥 Download content using DocID
- 🔐 User authentication and session management
- 📄 View a list of uploaded documents
- ❌ Delete uploaded files
- ⚡ Redis caching support (optional)

---

## 👤 User Capabilities

- **Login**: Users must log in to access features
- **Upload**: Upload documents via the `/DocumentUpload/UploadDoc` route
- **List**: View uploaded documents on the dashboard
- **Delete**: Remove unwanted files from their account
- **Retrieve**: Enter a DocID at `/DocumentDownload/DownloadDoc` to access the original file

---

## 🌐 Web Interface Routes

- `/Login` – User login page
- `/DocumentUpload/UploadDoc` – Upload new document
- `/MyUploads` – View all uploaded documents
- `/DocumentDownload/DownloadDoc` – Retrieve document using DocID

---

## 🛠️ How to Run

```bash
git clone https://github.com/TSRCHARAN/DocShare.git
cd DocShare
dotnet restore
dotnet run
```

> The app runs on `https://localhost:{PORT}`. Replace `{PORT}` with the actual port from your terminal or `appsettings.json`.

---

## ⚙️ Configuration

1. Copy the template config:
   ```bash
   cp appsettings.Template.json appsettings.Development.json
   ```

2. Update `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
      "MVCConnection": "DB-Connection"
    },
    "Redis": {
      "ConnectionString": "Redis-Connection"
    }
   }
   ```

> Note: This file is excluded from version control for security.

---

## 🗃️ Database Schema

You have two options to set up the database:

### Option 1: Using Entity Framework Core

If the project includes EF Core migrations, just run:

```bash
dotnet ef database update
```

> Ensure EF Core CLI tools are installed:  
> `dotnet tool install --global dotnet-ef`

### Option 2: Manual SQL Script

You can use the provided `documentdb.sql` file inside DocumentsProject folder to manually set up your database tables.

---

## 🔁 Redis Caching

To improve performance:
- Enable Redis in the configuration
- Documents are cached on access using their DocID

---

## 📦 Technologies Used

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- C#
- Redis (optional)
- SQL Server / PostgreSQL / MySQL

---

## 🙋 Author

Made with ❤️ by [TSR Charan](https://github.com/TSRCHARAN)

---