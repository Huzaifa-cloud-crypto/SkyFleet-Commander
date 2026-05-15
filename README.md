# 🚁 SkyFleet Commander (AGDRONE)

SkyFleet Commander is an advanced, centralized desktop application designed to manage, monitor, and coordinate a heterogeneous fleet of Unmanned Aerial Vehicles (UAVs). Built entirely in **C# using the .NET 10 Framework**, this system moves extremely beyond standard CRUD database management by incorporating **Swarm Intelligence algorithms** and strict Object-Oriented Programming (OOP) paradigms.

---

## ✨ Key Features

* **Real-Time Swarm Intelligence Coordination**
  Select multiple drones and trigger the "Deploy Swarm" protocol. The system uses automated mathematical calculations to arrange drones into geometric formations (e.g., V-Shape) and assigns polymorphic duties based on vehicle hardware.
* **Autonomous Collision Avoidance Algorithm** 
  The system utilizes Euclidean distance formulas to monitor live X/Y coordinate proximities in real-time. If two drones breach a 5.0 unit safety radius, the system autonomously overrides their coordinates, thrusting them to safety and logging a warning. 
* **Dynamic Fleet Specialization**
  Supports specialized hardware deployments, including Delivery (heavy lifting), Medical (urgent payload), Surveillance (4K recording), Agricultural, and High-Speed Racing drones.
* **Global Activity Feed (Mission Log)** 
  A live, color-coded console that tracks all telemetry, drone registrations, warning metrics, and active task delegations.
* **MS Access Integration (ADO.NET)**
  A persistent, automated internal relational database using OLEDB for storing fleet metrics, utilizing advanced parameterized queries and `INNER JOIN` relational mappings to reduce data redundancy.

---

## 💻 Tech Stack & Architecture

This project strictly adheres to clean OOP design architectures rather than relying on heavy third-party frameoworks.

* **Language:** C# 14.0
* **Framework:** .NET 10 WinForms (MDI Application)
* **Storage:** Microsoft Access Table Relationships (`.accdb`) 
* **Data Access:** ADO.NET using `System.Data.OleDb`

### 🏗️ Object-Oriented Principles Highlighted:
1. **Encapsulation:** The custom `GPSModule` class hides string-parsing logic and safely locks mathematical X/Y coordinates behind strict updates.
2. **Abstraction:** A single overarching abstract `Drone` template guarantees no "generic" unclassified drones can be operated.
3. **Inheritance & Generalization:** `AgriculturalDrone`, `DeliveryDrone`, etc. inherit behaviors from the base class, reducing duplicated code.
4. **Interfaces:** Using the `ICommunicator` interface loosely couples individual drones and entire swarms to guarantee data can be transmitted regardless of the entity type.
5. **Polymorphism:** The `DroneSwarm` delegates jobs dynamically. A single command (`PerformMission`) executes entirely different code algorithms depending on if the active unit is a Medical Drone vs a Surveillance Drone. 
6. **Operator Overloading:** The `<` and `>` native math operators are overloaded to natively compare advanced objects against one another based on internal battery constraints.

---

## 🚀 Getting Started

### Prerequisites
* Visual Studio 2022 (Community, Pro, or Enterprise)
* .NET 10.0 SDK Framework
* Windows OS

### Installation & Run
1. Clone the repository:
