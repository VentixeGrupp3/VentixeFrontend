# Ventixe Frontend

A unified frontend application built with ASP.NET Core MVC that provides authentication, event booking, and invoice management by consuming multiple backend microservices. This project demonstrates secure authentication flows, microservice integration, and modular UI components. Created as a part of the ASP .NET 2 course at Nackademin.

## Overview

This solution delivers key frontend capabilities for the Ventixe platform, including:

- **Authentication Flows**: User signup, login, and logout via the Auth API.  
- **Microservice Integration**: Seamless connection to Events, Bookings, and Invoices microservices through typed HTTP clients.  
- **Role-Based UI**: Dynamic views and navigation based on user roles (User, Admin).  
- **Modular MVC Design**: Razor views and partials to compose pages for each domain.  
- **Responsive Layout**: Built with mobile-first design.  
- **Dependency Injection**: Typed `HttpClient` instances and services registered in `Program.cs`.  

## Key Techniques and Features

### Authentication & Authorization

- **Signup & Login**: Forms that post credentials to `Auth/api/auth/register` and `Auth/api/auth/login`.  
- **JWT Handling**: Tokens issued by Auth API are stored in secure cookies or local storage and attached to subsequent API requests.  
- **Logout**: Clears authentication data and redirects to the login page.  
- **Role-Based Navigation**: Conditional rendering of menu items and pages based on JWT claims.

### Microservice Integration

- **Events**: Fetches upcoming events from the Events microservice via `GET /api/events`.  
- **Bookings**: Allows users to create and view bookings with `POST /api/bookings` and `GET /api/bookings/{userId}`.  
- **Invoices**: Displays and generates invoices by calling the Invoice microservice endpoints under `GET /api/invoices` and related routes.  

All API calls are made using typed `HttpClient` services, ensuring retry policies and centralized error handling.

### Modular MVC Architecture

- **Controllers**: Separate controllers for Auth, Events, Bookings, and Invoices domains.  
- **Views**: Razor views organized into feature-specific folders; shared layouts and partial views for headers, footers, and alerts.  
- **View Models**: DTO-based models for strongly typed rendering and form binding.

## Conclusion

The Ventixe Frontend showcases a secure, modular ASP.NET Core MVC application that ties together authentication, event booking, and invoice management. With role-aware navigation, robust microservice consumption, and responsive design, it exemplifies best practices for enterprise-scale web applications.
