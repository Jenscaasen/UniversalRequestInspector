# Universal Request Inspector

A powerful web application for capturing, inspecting, and forwarding HTTP requests in real-time. Perfect for webhook development, API debugging, and integration testing.

![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=flat&logo=blazor&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-blue?style=flat)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)

## ✨ Features

### 🔍 **Request Inspection**
- **One-click URL generation** - Generate unique endpoints instantly
- **Universal HTTP support** - Accepts ANY HTTP method (GET, POST, PUT, DELETE, OPTIONS, PATCH, HEAD)
- **Wildcard paths** - Capture requests to any sub-path
- **Real-time updates** - See requests as they arrive using SignalR
- **Detailed inspection** - View headers, body, query parameters, client IP, and timestamps

### 🔄 **Request Forwarding**
- **Optional forwarding** - Set a target URL to forward requests to
- **Response capture** - See both the original request AND the forwarded response
- **Performance metrics** - Track forwarding duration
- **Error handling** - Graceful handling of forwarding failures
- **Transparent proxying** - Original response is returned to the client

### 🎨 **User Interface**
- **Clean, focused design** - No unnecessary navigation or clutter
- **Expandable request cards** - Detailed view of each request/response
- **JSON formatting** - Automatic pretty-printing for JSON content
- **Copy functionality** - One-click copy for URLs, headers, and bodies
- **Status indicators** - Color-coded HTTP status badges

## 🚀 Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Installation & Running

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd UniversalRequestInspector
   ```

2. **Navigate to the project directory**
   ```bash
   cd UniversalRequestInspector
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open your browser**
   Navigate to `http://localhost:5062` (or the URL shown in the terminal)

## 📖 How to Use

### Basic Request Inspection

1. **Start a new sink**
   - Click the "Start Now" button
   - A unique URL will be generated (e.g., `http://localhost:5062/requestsink/abc123def456`)

2. **Send requests**
   - Use any HTTP client (curl, Postman, browser, webhook, etc.)
   - Send requests to your unique URL or any sub-path
   - Example: `POST http://localhost:5062/requestsink/abc123def456/api/webhook`

3. **Inspect in real-time**
   - Requests appear instantly in the web interface
   - Click on any request to expand and see full details

### Request Forwarding

1. **Set a forward URL** (optional)
   - Enter a target URL in the "Forward URL" field
   - Example: `https://api.example.com`
   - Click "Set" to enable forwarding

2. **Send requests**
   - Requests will be automatically forwarded to your target URL
   - Both the original request and the response will be captured

3. **View responses**
   - Expand any forwarded request to see the response details
   - Response includes status code, headers, body, and timing information

## 🛠️ Use Cases

### Webhook Development
```bash
# Set your webhook URL as the forward URL
# Use the generated sink URL in your webhook configuration
# See exactly what data is being sent and how your endpoint responds
```

### API Debugging
```bash
# Forward requests to your API
# Inspect request/response flow
# Debug integration issues
```

### Integration Testing
```bash
# Capture requests from your application
# Verify request format and content
# Test different scenarios
```

## 🏗️ Architecture

### Technology Stack
- **Frontend**: Blazor Server with Bootstrap 5
- **Backend**: ASP.NET Core 9.0
- **Real-time**: SignalR for live updates
- **Storage**: In-memory with automatic cleanup
- **HTTP Client**: Built-in HttpClient for forwarding

### Key Components

```
├── Controllers/
│   └── RequestSinkController.cs    # Handles all HTTP requests to sinks
├── Hubs/
│   └── RequestNotificationHub.cs   # SignalR hub for real-time updates
├── Services/
│   ├── RequestStorageService.cs    # In-memory request storage
│   └── RequestForwardingService.cs # HTTP request forwarding
├── Models/
│   ├── RequestInfo.cs              # Request data model
│   ├── ResponseInfo.cs             # Response data model
│   └── RequestSink.cs              # Sink configuration model
└── Components/
    ├── Pages/Home.razor            # Main UI
    └── RequestDisplay.razor        # Request/response display component
```

## 🔧 Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Set to `Development` for detailed logging
- `ASPNETCORE_URLS` - Override default listening URLs

### Application Settings
- Request sinks automatically expire after 1 hour of inactivity
- Maximum 100 requests per sink (oldest requests are removed)
- CORS enabled for all origins on sink endpoints

## 🚀 Deployment

### Local Development
```bash
dotnet run
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY ./publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "UniversalRequestInspector.dll"]
```


## 🙏 Acknowledgments

- Built with [Blazor](https://blazor.net/) and [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- Icons by [Bootstrap Icons](https://icons.getbootstrap.com/)

---

**Happy debugging! 🐛🔍**