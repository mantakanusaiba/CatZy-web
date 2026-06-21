# CatZy 🐱

CatZy is an ASP.NET MVC web application built around cat adoption, rescue, and care. It connects four kinds of users — adopters, cat owners, veterinarians, and admins — through a single platform for adopting cats, reporting strays for rescue, booking vet appointments, and shopping for pet supplies.

## Features

### Accounts & Access
- Sign up with a username, email, password, and role (`User`, `Vet`, or `Admin`); duplicate emails are rejected.
- Log in with email/password; on success the username, email, and role are stored in `Session` and the user is redirected based on role — Admins go to the admin dashboard, Vets go to their credentials page (or dashboard once approved), everyone else lands on the user dashboard.
- Logout clears the session entirely.
- Every role-specific controller action checks `Session["Role"]` and bounces unauthenticated or wrong-role users back to the login page.

### Cat Adoption (Users)
- **Browse listings:** The adoption page shows only admin-approved cat posts, with server-side filtering by age, gender, color, and breed (all combinable, all optional).
- **Cat detail page:** Each approved post has its own detail view with full description and photo before deciding to adopt.
- **Adopt flow:** Selecting "Adopt" opens a form pre-filled with the cat's info; submitting it creates a `Pending` `AdoptionRequest` record (applicant name, email, phone, address) tied to that cat, then shows a confirmation page with a personalized thank-you message.
- **Post a cat for adoption:** Logged-in users can submit their own cat (name, age, gender, color, breed, description, photo upload) for admin review. New posts start as `Pending` and only appear publicly once an admin sets them to `Approved`. Uploaded photos are saved with a unique timestamp-based filename to avoid collisions.
- **Pet Diary:** A personal space for logged-in users (view-only placeholder for journaling about their pet).
- **Notifications:** The user dashboard and adoption page pull unread notifications for the logged-in username from the database and let the user dismiss them individually.

### Stray Rescue Reporting
- Anyone can file a rescue report describing the cat and its location, plus exact latitude/longitude (the form defaults to Dhaka's coordinates, intended for use with a map picker).
- The `RescueRequests` table is created automatically on first use if it doesn't already exist (including a spatial-style index on latitude/longitude).
- Submissions get a confirmation message with the new request's ID and redirect back to the user dashboard.
- Admins/staff can view all rescue reports in one list (`AllReports`) and delete resolved or invalid ones.

### Veterinary Appointments
- Users see a list of **approved** vets only, pulled live from `DoctorCredentials` (name, specialization, hospital, experience, consultation hours, profile photo).
- Booking a slot auto-assigns a time based on the vet's declared hours: morning vets get four 20-minute slots starting 10:00 AM, evening vets get five 20-minute slots starting 6:00 PM. If all slots for that doctor/date are already booked, the user is told the doctor is fully booked instead of double-booking.
- A `CheckAvailability` endpoint (AJAX/JSON) lets the booking form check in real time whether a doctor still has openings on a chosen date.
- Booking requires cat details (name, age, breed, symptoms) and owner contact info, alongside the appointment date.
- Users can view all booked appointments (`AppointmentList`); vets see only the appointments assigned to them.

### Veterinarian Onboarding
- Vets submit a credentials application: name, contact info, hospital name, specialization, consultation hours, years of experience, a certificate file upload, and a profile picture upload. The application is stored with `Pending` status.
- Vets can't access their dashboard until an admin approves the application — until then they're redirected back to the credentials page with a status message ("not approved yet" / "please submit your credentials first").
- Once approved, the vet dashboard shows their appointment schedule.

### Shop & Checkout
- Product catalog with category filtering, individual product detail pages, stock levels, and pricing; the `Products` table is created automatically if missing.
- **Guest-friendly cart:** Cart contents are tracked via a cookie-bound cart ID (no login required), with line items, quantities, and computed line totals.
- Add to cart, update quantities, or remove items, with quantities of zero automatically removing the item.
- **Checkout:** Collects shipping/contact details (name, email, phone, address, city, postal code) and a payment method choice, applies a flat shipping fee, and calculates subtotal + shipping = total.
- Placing an order is transactional: it writes the order header, line items, and then clears the cart in a single database transaction, so a failure can't leave a half-placed order.
- Logged-in users can review their **order history** and drill into the details of any past order (items, totals, shipping info).

### Admin Console
- **Vet approvals:** Review pending vet applications (with certificates and profile photo) and approve or reject each one.
- **Adoption oversight:** Search and filter adoption requests by status (Pending/Approved/Rejected) or by applicant name, email, or cat name; approve or reject requests.
- **Cat post moderation:** Approve a user-submitted cat post (making it publicly visible) or close it; admins can also add cats directly or delete existing ones.
- **Product management:** Add, update, or delete products in the shop catalog.
- **Rescue oversight:** View and manage all submitted stray rescue reports.
- A `/Debug/Tables` page lists every table in the connected database — handy during development, see *Known Limitations* below.

## Tech Stack

- **Framework:** ASP.NET MVC 5 (.NET Framework 4.7.2)
- **ORM:** Entity Framework 6
- **Database:** SQL Server LocalDB
- **Front end:** Razor views, Bootstrap 5, jQuery
- **IDE:** Visual Studio 2019/2022 (solution file: `CatZy.sln`)

## Project Structure

```
CatZy/
├── Controllers/        # Account, Admin, Home, Product, Rescue, Shop, User, Vet, Debug
├── Models/              # EF entities (User, Cat, CatPost, Appointment, Product, ...) and AppDbContext
├── Views/               # Razor views, organized by controller
├── Content/             # CSS, images, and user-uploaded cat photos
├── Scripts/             # jQuery, Bootstrap bundles
├── App_Start/           # Route, bundle, and filter configuration
├── App_Data/            # LocalDB database file (DefaultConnection.mdf)
├── Uploads/             # Vet certificates and profile pictures
└── Web.config           # App settings and connection string
```

## Getting Started

### Prerequisites

- Windows with Visual Studio 2019 or later (with the **ASP.NET and web development** workload)
- SQL Server Express LocalDB (installed automatically with the Visual Studio web workload)
- .NET Framework 4.7.2 Developer Pack

### Setup

1. Open `CatZy.sln` in Visual Studio.
2. Let NuGet restore the packages referenced in `packages.config` (EntityFramework, Microsoft.AspNet.Mvc, jQuery, Bootstrap, etc.). If they don't restore automatically, right-click the solution and choose **Restore NuGet Packages**.
3. The project ships with a LocalDB database file at `App_Data/DefaultConnection.mdf`, and the connection string in `Web.config` points to it:
   ```xml
   <connectionStrings>
     <add name="DefaultConnection"
          connectionString="Data Source=(localDB)\MSSQLLocalDB;Initial Catalog=CRUDDB;Integrated Security=True"
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```
   No EF migrations run automatically (`Database.SetInitializer<AppDbContext>(null)` in `Global.asax.cs`), so the database schema must already exist — either via the bundled `.mdf` file or by creating the `CRUDDB` database and its tables (`Users`, `Appointments`, `AdoptionRequests`, `CatPosts`, `DoctorCredentials`, `Products`, etc.) yourself.
4. Press **F5** (or **Ctrl+F5**) to build and run with IIS Express. The app starts at the login page (`/Account/Login`).
5. To verify the database connection, visit `/Debug/Tables` after running — it lists every table in the connected database.

### Default User Roles

New accounts are created via **Sign Up** with a `Role` of `Admin`, `Vet`, or a regular user role. Login redirects based on role:
- `Admin` → Admin dashboard
- `Vet` → Credentials page (until approved by an admin), then the vet dashboard
- Anything else → the regular user dashboard


