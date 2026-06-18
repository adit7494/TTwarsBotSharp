# 🏰 TTwarsBotSharp

**A powerful, open-source automation bot for Travian: Legends and TTWars servers**

[![Website](https://img.shields.io/badge/Website-Live-brightgreen?style=for-the-badge&logo=vercel)](https://ttwarsbot.vercel.app)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightblue?style=for-the-badge&logo=windows)]()
[![Release](https://img.shields.io/github/v/release/adit7494/TTwarsBotSharp?style=for-the-badge&logo=github)](https://github.com/adit7494/TTwarsBotSharp/releases)
![Uploading image.png…]()

🌐 **Website**: [https://ttwarsbot.vercel.app](https://ttwarsbot.vercel.app)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🏗️ **Auto Builder** | Automatically builds and upgrades buildings based on your configured plan |
| 🌾 **Auto Farm Lists** | Starts farm lists automatically to maximize resource income |
| ⚔️ **Auto Adventures** | Automatically starts hero adventures when available |
| 🎯 **Auto Quest** | Claims side quests and rewards automatically |
| 💰 **Auto NPC** | Converts resources via NPC merchant when needed |
| 🛡️ **Auto Train Troops** | Trains troops automatically based on your configuration |
| 👥 **Multi-Account** | Supports multiple accounts running simultaneously |
| 🌍 **Multi-Language** | Supports Indonesian and English server languages |
| 🔒 **Anti-Detection** | Built-in fingerprint spoofing and browser automation protection |

## 🚀 Getting Started

### Prerequisites

- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime** (if using framework-dependent build)

### Installation

1. **Download** the latest release from [Releases](https://github.com/adit7494/TTwarsBotSharp/releases)
2. **Extract** the ZIP file to your preferred location
3. **Run** `TTwarsBotSharp.exe`

### Building from Source

```bash
# Clone the repository
git clone https://github.com/adit7494/TTwarsBotSharp.git
cd TTwarsBotSharp

# Build the project
dotnet build --configuration Release

# Run the application
dotnet run --project WPFUI
```

## 📖 Usage

### Adding an Account

1. Launch TTwarsBotSharp
2. Click **"Add Account"**
3. Enter your server URL (e.g., `https://nor4.ttwars.com`)
4. Enter your username and password
5. Configure proxy settings (optional)
6. Click **"Save"**

### Configuring Build Plans

1. Select your account from the sidebar
2. Go to the **"Build"** tab
3. Add buildings to your build queue
4. Set target levels for each building
5. The bot will automatically build prerequisites

### Supported Servers

| Server Type | Status |
|-------------|--------|
| Travian: Legends | ✅ Supported |
| TTWars (Speed) | ✅ Supported |

## 🛠️ Tech Stack

- **Framework**: .NET 8.0, WPF
- **UI**: ReactiveUI, Material Design
- **Browser Automation**: Selenium WebDriver
- **Database**: SQLite, Entity Framework Core
- **Logging**: Serilog

## 📁 Project Structure

```
TTwarsBotSharp/
├── MainCore/           # Core business logic, parsers, commands
├── MainCore.Test/      # Unit tests
├── WPFUI/              # WPF user interface
├── web-source/         # Reference HTML for parser development
└── .github/            # GitHub Actions workflows
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is open source and available under the [MIT License](LICENSE).

## 🙏 Acknowledgments

- Built with ❤️ for the Travian community
- Thanks to all contributors and testers

---

<p align="center">
  Made with ❤️ by the TTwarsBotSharp Community
</p>
