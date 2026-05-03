# Delivery Management System

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?style=flat&logo=windows)
![Status](https://img.shields.io/badge/Status-Completed-success)
![License](https://img.shields.io/badge/License-MIT-green)

## Overview
A desktop application built with C# and .NET 8.0 for managing logistics, couriers, and delivery orders. The main complexity of this project lies in coordinating a real-time background simulation engine with a responsive UI, ensuring safe cross-thread operations and concurrent data access.

**Project Context:** This educational project was developed to apply solid architectural patterns (3-Tier Layered Architecture) and tackle real-world multithreading challenges in C#/.NET.

---

## How the Simulation Works

The simulation engine models fleet operations using a background tick-based loop. During each tick:

1. **Evaluate Active Deliveries:** Identify orders currently assigned or in transit.
2. **Time Calculation:** Calculate the remaining time until arrival based on the configured speed and distance.
3. **Status Transition:** Trigger discrete status changes (e.g., *Picked Up* → *In Transit* → *Delivered*) once the calculated duration has elapsed in simulation time.
4. **UI Notification:** Dispatch updates to the PL via observer callbacks, safely invoking the main thread to update the datagrids dynamically.

---

## Architecture
The system enforces a clean separation of concerns, utilizing Factory Methods for dependency instantiation and maintaining a strict unidirectional data flow: 

`UI Action → PL ViewModel → BL Validation → DAL Execution`
```text
┌─────────────────────────────────────────┐
│  Presentation Layer (PL)                │
│  WPF Desktop Application & UI Logic     │
├─────────────────────────────────────────┤
│  Business Logic Layer (BL)              │
│  Core Business Rules & Validation       │
├─────────────────────────────────────────┤
│  Data Access Layer (DAL)                │
│  Database Abstraction & Persistence     │
└─────────────────────────────────────────┘
```

---

## Design Decisions & Technical Challenges

* **Database Agnosticism:** The DAL is abstracted via interfaces. Data is persisted using XML serialization or In-Memory structures (`DalXml`, `DalList`). This adheres to the Open/Closed Principle, allowing future migration to a relational database like SQL Server without touching the Business Logic layer.
* **Concurrency & Race Conditions:** With the UI thread reading data and the simulation thread continuously writing updates, race conditions were imminent. Instead of complex concurrent collections, I utilized explicit `lock` mechanisms within the Singleton DAL instances. This ensured atomic transactions for order updates without incurring heavy performance overhead.
* **Decoupled UI via Data Binding:** To prevent tight coupling, the PL relies strictly on XAML Data Binding and the `INotifyPropertyChanged` interface. Backend updates automatically reflect in the UI without direct DOM-like manipulation.

---

## Technology Stack
* **Language:** C# 12
* **Framework:** .NET 8.0
* **UI:** WPF (Windows Presentation Foundation)
* **Data Storage:** XML Serialization / In-Memory Collections

---

## Project Structure
```text
Delivery-Management-System/
├── xml/                     # XML database files & system configuration
├── PL/                      # Presentation Layer (WPF)
│   ├── MainWindow.xaml      # Admin Dashboard / Entry Point
│   ├── LoginWindow.xaml     # Authentication UI
│   ├── Order/               # Order management views
│   ├── Courier/             # Courier management views
│   ├── Helpers/             # Converters & Utilities
│   └── Images/              # UI Assets
├── BL/                      # Business Logic Layer
│   ├── BlApi/               # Interfaces for BL operations
│   ├── BlImplementation/    # Core business logic implementation
│   ├── BO/                  # Business Objects (Models)
│   └── Helpers/             # BL utilities
├── DalFacade/               # Data Access Facade (Interfaces & DO)
├── DalList/                 # In-Memory DAL Implementation
├── DalXml/                  # XML-Based DAL Implementation
├── BlTest/                  # Business Logic Unit Testing
└── DalTest/                 # Data Access Unit Testing
```

---

## Getting Started

### Prerequisites
* Windows 10/11
* Visual Studio 2022 (or equivalent)
* .NET 8.0 SDK

### Installation & Run
1. Clone the repository:
   
```sh
   git clone [https://github.com/ori-levental/Delivery-Management-System.git](https://github.com/ori-levental/Delivery-Management-System.git)
   ```
2. Navigate to the project directory:
   ```sh
   cd Delivery-Management-System
   ```
3. Build the solution to restore dependencies:
   ```sh
   dotnet build dotNet5786_9587_3771.sln
   ```
4. Run the Presentation Layer:
   ```sh
   dotnet run --project PL/PL.csproj
   ```

### Demo Accounts
* **Admin:** `Username: 111111118` | `Password: nxNsj544bh@?`
* **Courier:** `Username: 350605200` | `Password: GxzHtnPQ1m1.B`

---

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Authors & Contact

**Ori Levental**
* LinkedIn: [linkedin.com/in/ori-levental](https://www.linkedin.com/in/ori-levental)
* Email: [orilevental@gmail.com](mailto:orilevental@gmail.com)
* GitHub: [@ori-levental](https://github.com/ori-levental)

**Mordechai Gitscher**
