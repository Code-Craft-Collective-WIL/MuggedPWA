# Mugged - Progressive Web App 🎁☕️

![Mugged Logo](./assets/logo.png)  
*Customizable Gift Shop - Personalized Mugs & More*

**Mugged** is a customizable gift shop that specializes in selling personalized mugs and other gift items. This project is a **Progressive Web App (PWA)** developed as part of a Work Integrated Learning (WIL) project for a gift shop. The app is designed to provide users with a seamless and engaging shopping experience, allowing them to customize products with ease.

---

## Table of Contents 📚

- [Project Overview](#project-overview)
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Installation](#installation)
- [Usage](#usage)
- [CI/CD & DevOps](#cicd-devops)
- [Testing](#testing)
- [Contributors](#contributors)
- [License](#license)

---

## Project Overview 💻

This **PWA** is built using **ASP.NET MVC** for the backend and **HTML, CSS, and JavaScript** for the frontend, ensuring an interactive and responsive design. The application makes full use of **Azure DevOps** for version control, project management, and CI/CD pipelines, ensuring a smooth development process and continuous integration.  

The app allows users to browse and customize mugs, adding personalized text or images. Features include a product gallery, customizable options, and a secure checkout process.

---

## Features ✨

- **User-Friendly Interface**: Intuitive navigation for an easy shopping experience.
- **Product Customization**: Users can personalize mugs with text and images.
- **Responsive Design**: Optimized for all screen sizes (mobile, tablet, desktop).
- **Offline Functionality**: Available as a PWA, allowing offline use.
- **Secure Checkout**: Integration with payment gateways for a smooth checkout process.
- **Product Gallery**: Displays customizable mugs and other gift items.

---

## Technologies Used 🔧

- **Frontend**:
  - HTML5
  - CSS3 (Bootstrap & custom styles)
  - JavaScript (ES6+)
  - Service Workers for PWA functionality

- **Backend**:
  - C#
  - ASP.NET MVC
  - Azure SQL Database

- **DevOps**:
  - Azure DevOps (CI/CD Pipelines)
  - Azure Boards for task tracking
  - Git for version control

- **Testing**:
  - NUnit for backend testing
  - Azure Test Plans for automated tests

---

## Installation ⚙️

To run the project locally, follow these steps:

### Prerequisites:

- **Visual Studio 2022** or later
- **.NET Core SDK**
- **Azure DevOps account** (for CI/CD and project management)
- **Azure SQL Database** (for local database testing)

### Steps to Run the Project Locally:

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/mugged.git

   # Mugged - A Personalized Mug and Gift Shop

2. Open the Project in Visual Studio 2022
- Navigate to **File > Open > Project/Solution**.
- Select the `Mugged.sln` solution file.

3. Restore the Project Dependencies
- Right-click on the solution in the Solution Explorer and select **Restore NuGet Packages**.

4. Configure the Database Connection
- Ensure that your Azure SQL Database connection string is correctly set in the `appsettings.json` file.

5. Build and Run the Project
- Click **Run** in Visual Studio or press **F5** to start the application.

---

## Usage 🛍️

Once the app is running, you can:

- Browse available customizable products.
- Select a product to personalize (add text/images).
- Add the customized product to the cart.
- Proceed to the checkout page and complete the purchase.

The app is also available as a **PWA (Progressive Web App)**. Install it on your device for offline usage and access it directly from your home screen.

---

## CI/CD & DevOps 🚀

The development of this project integrates **Azure DevOps** for:

- **CI/CD Pipelines**: Automated testing and deployment processes.
- **Azure Boards**: Task management and sprint planning.
- **Version Control**: Git repositories for collaboration and code management.

All code is pushed to the main branch via pull requests, and every commit triggers the CI/CD pipeline to ensure automatic deployment to Azure.

---

## Testing 🧪

Testing is a key part of this project. The following testing strategies were used:

- **Unit Testing**: Back-end logic tested using **NUnit**.
- **Automated Tests**: Managed and executed via **Azure Test Plans**.
- **Manual Testing**: Front-end features and UI tested across various devices and browsers.

---

## Contributors 👥

This project was developed by the **Code Craft Collective** team:

- **Member 1**: Project Manager & Backend Developer
- **Member 2**: Software Developer Lead & Frontend Developer
- **Member 3**: Business Analyst & Software Designer
- **Member 4**: Database Administrator & DevOps Engineer
- **Member 5**: Frontend Developer & QA Tester
- **Member 6**: Frontend Developer
- **Member 7**: Backend Developer & QA Tester

---

## License 📜

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

🔗 Want to see the project in action? Visit our **[live demo on Azure](#)** ✨

