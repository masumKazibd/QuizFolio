# 📚 QuizFolio

A dynamic, customizable web application for creating, managing, and filling out quizzes, tests, surveys, and questionnaires — similar in concept to Google Forms.  
Built with **.NET Core (ASP.NET)**, **Bootstrap**, and **SQL Server**.

---

## 📖 Project Overview

**QuizFolio** enables authenticated users to:
- **Create and manage quiz templates** containing customizable questions.
- **Fill out forms** based on these templates.
- **View results** and manage submitted answers.

Meanwhile, **Admins** have full control over:
- All templates and forms.
- User management (block, unblock, promote, demote, delete users, and even remove their own admin rights).

Non-authenticated users can:
- **Browse public templates** in read-only mode.
- **Search templates** via a global search feature.

---

## 🎯 Features

✅ User Registration & Authentication  
✅ Role-based Access (User & Admin)  
✅ Customizable Form/Quiz/Survey Templates  
✅ Public and Private Templates  
✅ Form Submission & Answer Management  
✅ Global Full-text Search (Search templates by title, questions, and comments)  
✅ Admin Panel with Full User Management  
✅ Dynamic UI (toolbars, context actions — no N+1 button clutter)  
✅ Secure & Relational Data Storage (SQL Server)

---

## 🛠️ Tech Stack

| Category       | Tech                      |
|:---------------|:--------------------------|
| Frontend       | Bootstrap                  |
| Backend        | .NET Core (ASP.NET)         |
| Database       | SQL Server                  |
| ORM            | Entity Framework Core       |
| Authentication | ASP.NET Identity             |

---

## 🖥️ Pages & Access Rules

| Page                      | Visitor | Authenticated User | Admin |
|:--------------------------|:---------|:--------------------|:--------|
| View Templates             | ✅        | ✅                     | ✅       |
| Search Templates           | ✅        | ✅                     | ✅       |
| Register/Login             | ✅        | ❌                     | ✅       |
| Create Templates           | ❌        | ✅                     | ✅       |
| Fill Forms                 | ❌        | ✅                     | ✅       |
| Manage Own Templates       | ❌        | ✅                     | ✅       |
| Admin Panel (User Management) | ❌    | ❌                     | ✅       |
| Edit/Manage All Content    | ❌        | ❌                     | ✅       |

---

## ⚙️ Admin Functionalities

- **View, block, unblock, delete users**
- **Promote or demote admins**
- **Remove own admin rights**
- **Edit/delete any template or form**
- **Manage system-wide content**

---

## 📊 Database Structure Overview

- **Users**
- **Templates**
- **Questions**
- **Forms (Filled templates)**
- **Answers**
- **Comments**
- **Roles (Admin/User)**

---

## ✨ Author

**Md. Masum Kazi**  
[![LinkedIn](https://img.shields.io/badge/-LinkedIn-0A66C2?style=flat&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/masumkazi)
[![GitHub](https://img.shields.io/badge/-GitHub-181717?style=flat&logo=github&logoColor=white)](https://github.com/masumkazibd)
[![Portfolio](https://img.shields.io/badge/-Portfolio-FF5722?style=flat&logo=Firefox&logoColor=white)](https://masumkazi.com)
