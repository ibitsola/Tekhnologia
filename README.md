# Tekhnologia - Empowering Women in Tech 🚀

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![Blazor Server](https://img.shields.io/badge/Blazor-Server-purple)](https://blazor.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

> A comprehensive well-being and career growth platform specifically designed to support women in technology through evidence-based tools and AI-powered coaching.

## 🌟 Project Overview

**Tekhnologia** is a full-stack web application that addresses the unique challenges faced by women in the technology industry. Through a combination of personal development tools, AI-powered career coaching, and a supportive digital ecosystem, the platform aims to reduce barriers and accelerate career growth for women in tech.

### 🎯 Mission Statement
To create an inclusive, evidence-based platform that empowers women in technology to overcome imposter syndrome, build confidence, and achieve their career aspirations through structured goal-setting, reflective practices, and personalized AI guidance.

## 🏗️ Architecture & Technology Stack

### Backend Architecture
- **Framework**: ASP.NET Core Web API (.NET 9)
- **Authentication**: ASP.NET Core Identity + JWT Bearer hybrid system
- **Database**: SQLite with Entity Framework Core 9.0
- **AI Integration**: OpenAI GPT API for career coaching
- **Payment Processing**: Stripe API for digital resource marketplace
- **Architecture Pattern**: Service Layer with Dependency Injection

### Frontend Implementation
- **Framework**: Blazor Server with SignalR real-time updates
- **UI Framework**: Bulma CSS with 700+ lines of custom styling
- **Responsive Design**: Mobile-first approach with dark theme
- **Component Architecture**: 15+ interactive Blazor components
- **Authentication State**: Seamless auth state management across components

### Database Design
```
Users (ASP.NET Identity)
├── JournalEntries (1:N) - Personal reflection system
├── Goals (1:N) - Career goal tracking
├── VisionBoardItems (1:N) - Visual goal representation
└── Purchases (1:N) - Digital resource transactions
    └── DigitalResources (N:1) - Purchasable career resources
```

## ✨ Core Features

### 🔐 Secure Authentication System
- Hybrid ASP.NET Core Identity + JWT authentication
- Role-based authorization (Admin/User)
- Secure password management with industry standards
- Seamless Blazor Server integration

### 📝 Reflective Journaling
- **Evidence-Based**: Reduces stress and builds resilience (Cascio et al., 2016)
- Private, secure journal entries with rich text support
- Date-based organization and filtering
- User-specific data isolation

### 🎯 SMART Goal Tracking
- Career-focused goal creation and management
- Progress tracking with completion status
- Due date management and organization
- Visual progress indicators

### 🖼️ Digital Vision Board
- Upload and organize inspirational images
- Goal categorization and description system
- Secure local file storage with future cloud migration path
- Responsive image gallery interface

### 🤖 AI Career Coach (Eve)
- **OpenAI GPT Integration**: Personalized career coaching
- 24/7 availability for career guidance and support
- Specialized prompting for women in tech challenges
- Conversational interface with professional coaching context

### 💼 Digital Resource Marketplace
- Curated career development resources
- Stripe-integrated payment processing
- Admin panel for resource management
- Secure download system for purchased resources

### 👩‍💼 Administrative Panel
- User management and analytics
- Digital resource upload and management
- Role-based access control
- System monitoring and maintenance tools

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- Code editor (Visual Studio Code recommended)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/ibitsola/Tekhnologia.git
   cd Tekhnologia
   ```

2. **Navigate to the project directory**
   ```bash
   cd Tekhnologia
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Set up environment variables**
   
   Create user secrets for sensitive configuration:
   ```bash
   dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"
   dotnet user-secrets set "Stripe:SecretKey" "your-stripe-secret-key"
   ```

   Or set environment variables:
   ```bash
   # Windows PowerShell
   [System.Environment]::SetEnvironmentVariable("OPENAI_API_KEY", "your-key", "User")
   [System.Environment]::SetEnvironmentVariable("STRIPE_SECRET_KEY", "your-key", "User")
   ```

5. **Run database migrations**
   ```bash
   dotnet build
   # Migrations run automatically on first startup
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access the application**
   - Navigate to `https://localhost:7136`
   - Default admin user is created automatically

### 🔧 Configuration

The application uses a hybrid configuration approach:
- **Development**: User secrets and environment variables
- **Production**: Environment variables and secure key management

Key configuration areas:
- OpenAI API key for AI coaching functionality
- Stripe API keys for payment processing
- JWT signing keys for authentication
- Database connection strings

## 📚 API Documentation

### Authentication Endpoints
```http
POST /api/auth/register    # User registration
POST /api/auth/login       # User authentication
POST /api/auth/logout      # Secure logout
GET  /api/auth/profile     # User profile data
```

### Core Feature Endpoints
```http
# Journal Management
GET    /api/journal           # Get user entries
POST   /api/journal           # Create entry
PUT    /api/journal/{id}      # Update entry
DELETE /api/journal/{id}      # Delete entry

# Goal Management
GET    /api/goal              # Get user goals
POST   /api/goal              # Create goal
PUT    /api/goal/{id}/complete # Mark complete

# Vision Board
GET    /api/visionboard       # Get board items
POST   /api/visionboard       # Upload image + create item

# AI Coaching
POST   /api/chatbot/business-coach # Get AI response

# Digital Resources
GET    /api/digitalresource   # Browse resources
POST   /api/payment/create-payment-intent # Purchase resource
```

## 🧪 Testing

The application includes comprehensive testing infrastructure:

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter Category=Controllers
dotnet test --filter Category=Services
```

### Test Coverage
- **Controller Tests**: All 9 API controllers with full endpoint coverage
- **Service Layer Tests**: Business logic validation and mock integrations
- **Authentication Tests**: Security and authorization flow validation
- **Integration Tests**: End-to-end feature validation

## 🌐 Deployment

### Development Deployment
The application is designed for easy local development with SQLite and local file storage.

### Production Deployment
Recommended hosting solutions:
- **Railway.app**: Optimized for .NET applications with automatic deployments
- **Azure App Service**: Enterprise-grade hosting with integrated services
- **Render.com**: Cost-effective hosting with good .NET support

### Domain Configuration
The application is prepared for deployment to **Tekhnologia.co.uk** with:
- SSL/HTTPS enforcement
- Custom domain configuration
- CDN integration readiness

## 🎯 CS50 Final Project Requirements

### Distinctiveness and Complexity
**Tekhnologia** demonstrates significant complexity and distinctiveness through:

1. **Full-Stack Architecture**: Complete Blazor Server + Web API implementation with real-time features via SignalR
2. **Multiple Technology Integrations**: 
   - OpenAI GPT API for intelligent career coaching
   - Stripe payment processing for e-commerce functionality
   - File upload and management system
   - Hybrid authentication with JWT and Identity
3. **Complex Business Logic**: User-specific data isolation, role-based authorization, and comprehensive CRUD operations
4. **Professional UI/UX**: Responsive design with custom dark theme and 15+ interactive components
5. **Real-World Application**: Addresses genuine social issues in tech industry diversity

### Technical Specifications
- **Backend**: 2,000+ lines of C# code across 9 API controllers
- **Frontend**: 1,500+ lines of Blazor component code with 15+ interactive pages
- **Database**: Comprehensive Entity Framework model with 6 related entities
- **Styling**: 700+ lines of custom CSS with professional dark theme
- **Testing**: Complete test infrastructure with controller and service layer coverage

### Social Impact
The project addresses documented challenges in tech industry diversity:
- **Imposter Syndrome**: Evidence-based journaling features
- **Career Advancement**: AI-powered coaching and goal tracking
- **Skill Development**: Digital resource marketplace for career growth
- **Community Building**: Platform designed for scalable community features

## 💼 Funding & Future Development

### Government Funding Application Ready
This project is positioned for funding applications to support women in tech initiatives:

### 📊 Impact Metrics (Projection)
- **Target Users**: 10,000+ women in tech within first year
- **Career Advancement**: 25% increase in promotion rates among active users
- **Skill Development**: 500+ digital resources distributed monthly
- **AI Coaching Sessions**: 50,000+ coaching interactions annually

### 🔮 Roadmap
**Phase 1: MVP Launch** (Current)
- Core features functional and tested
- Domain acquisition and production deployment
- Initial user acquisition and feedback collection

**Phase 2: Community Expansion** (6 months)
- User community features and forums
- Advanced AI coaching with conversation history
- Mobile app development (MAUI)
- Integration with professional networks

**Phase 3: Scale & Impact** (12 months)
- Enterprise partnerships with tech companies
- Advanced analytics and career outcome tracking
- Mentorship matching system
- International expansion and localization

### 💰 Sustainability Model
- **Freemium Model**: Core features free, premium resources paid
- **Corporate Partnerships**: Enterprise licensing for companies
- **Grant Funding**: Government and foundation grants for social impact
- **Resource Marketplace**: Revenue sharing on digital resource sales

## 🤝 Contributing

We welcome contributions from the community! This project is particularly suited for:
- Women in tech who understand the target user challenges
- Developers interested in social impact technology
- AI/ML specialists for coaching feature enhancement
- UX/UI designers for accessibility improvements

### Development Guidelines
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 About the Creator

**Ira Bitsola** - Full-Stack Developer & Women in Tech Advocate

- **GitHub**: [@ibitsola](https://github.com/ibitsola)
- **Project Repository**: [Tekhnologia](https://github.com/ibitsola/Tekhnologia)
- **CS50**: Harvard CS50's Introduction to Computer Science Final Project

### Project Journey
This project represents a comprehensive year-long development journey, evolving from a simple concept to a production-ready platform. The development process included:
- **Architecture Evolution**: From basic MVC to sophisticated Blazor Server + API
- **Feature Expansion**: Growing from journaling to full career coaching platform
- **Technology Integration**: Adding AI, payment processing, and real-time features
- **Production Readiness**: Comprehensive testing, security, and deployment preparation

## 🙏 Acknowledgments

- **Harvard CS50** for providing the foundation and inspiration
- **OpenAI** for making advanced AI accessible to developers
- **Stripe** for democratizing payment processing
- **Microsoft** for the excellent .NET and Blazor frameworks
- **The women in tech community** for inspiration and feedback throughout development

---

## 📞 Support & Contact

For questions, feedback, or collaboration opportunities:
- **Issues**: [GitHub Issues](https://github.com/ibitsola/Tekhnologia/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ibitsola/Tekhnologia/discussions)
- **Email**: Available through GitHub profile

---

**Tekhnologia** - *Empowering the next generation of women in technology* 🚀✨