# Product Requirements Document: DeskSync (V1)

**Status:** Draft

**Author:** Arsany Adel

**Date:** Jul 8, 2026

## 1. Executive Summary & Problem Statement

- **The Problem:** Freelancers, university students, and event organizers struggle to find and reliably reserve available workspaces or meeting rooms without resorting to phone calls, emails, or navigating clunky manual systems.
    
- **The Solution:** DeskSync is a real-time, self-serve booking engine that empowers users to discover, view, and reserve co-working spaces seamlessly online.
    
- **Success Metric:** Achieving zero double-bookings under concurrent load and providing a frictionless, real-time UI experience.
    

## 2. Target Personas

- **The Standard User (B2C):** Typically a university student, digital nomad, or event organizer. They want a reliable place to work or collaborate in person. They need to see the real-time availability of rooms, understand the room's capacity and amenities, and book it quickly.
    
- **The Staff / Workspace Admin:** Monitors system health, tracks user bookings, and manages the physical status of the rooms (e.g., taking a room offline for maintenance).
    

## 3. Scope & Out of Scope

**In Scope (V1):**

- Interactive timeline/grid view for booked and available rooms.
    
- Detailed room profiles (images, capacity, amenities).
    
- User notification system (bookmarks and status changes).
    
- History backlog for user interactions and bookings.
    
- Filtering system (by date, room specifications).
    
- Admin dashboard to view room/user history and manage room statuses (e.g., Active, Maintenance).
    

**Out of Scope (V1):**

- Google Calendar integration.
    
- Payment gateway / Billing integration (Assume bookings are paid on-site for V1).
    
- Admin ability to edit user profiles or passwords.
    

## 4. Functional Requirements (User Stories)

### Epic 1: Authentication System

**Story 1.1: Standard User Login & Registration**

> **As a** standard user, **I want to** create a new account (via email or Google) and log in, **so that** I can securely access the system, manage my bookmarks, and make reservations.

**Acceptance Criteria:**

- [ ] User can register via a standard form (First Name, Last Name, Email, Password).
    
- [ ] System sends a verification email upon standard account creation.
    
- [ ] User can authenticate via Google OAuth (Social Login).
    
- [ ] System returns a clear error message for incorrect credentials or existing emails.
    

**Story 1.2: Staff Login**

> **As a** staff member, **I want to** log in with my designated admin credentials, **so that** I can access the administrative dashboard.

**Acceptance Criteria:**

- [ ] The system does not allow public registration for Staff roles (Admin accounts must be seeded or created by a Super Admin).
    
- [ ] Staff login flows strictly require Two-Factor Authentication (2FA) for security.
    

### Epic 2: The Booking Engine

**Story 2.1: Viewing Room Availability**

> **As a** standard user, **I want to** view all rooms and their real-time statuses in a calendar/grid layout, **so that** I can decide which room fits my schedule.

**Acceptance Criteria:**

- [ ] UI displays a timeline grid with clear visual distinction between Available, Booked, and Maintenance states.
    
- [ ] User can filter rooms based on specific dates and capacity.
    
- [ ] Booked/Unavailable time slots are disabled and cannot be clicked.
    

**Story 2.2: Viewing Room Details**

> **As a** standard user, **I want to** view the specific details of a room, **so that** I can ensure it meets my technical and spatial needs.

**Acceptance Criteria:**

- [ ] The UI displays a gallery of images for the selected room.
    
- [ ] The UI lists capacity, hourly price, and amenities (e.g., projector, whiteboard).
    

**Story 2.3: Submitting a Reservation (Core)**

> **As a** standard user, **I want to** select an available time block and confirm my booking, **so that** I can secure my workspace.

**Acceptance Criteria:**

- [ ] The user can select a contiguous block of time (maximum 6 hours per booking).
    
- [ ] Submitting the booking instantly updates the grid state for the user.
    
- [ ] The backend validates the timeslot to prevent race conditions before confirming.
    

**Story 2.4: Notification & Bookmarks System**

> **As a** standard user, **I want to** bookmark specific rooms and receive updates, **so that** I am notified if an unavailable room opens up or if my booking has issues.

**Acceptance Criteria:**

- [ ] User can toggle a "Bookmark" icon on any room, visible on their profile page.
    
- [ ] System triggers an email notification if the user's upcoming booked room is marked as "Under Maintenance" by an Admin.
    
- [ ] System sends an in-app or email reminder 24 hours before an upcoming booking.
    

### Epic 3: Admin Panel

**Story 3.1: Viewing User & Room History**

> **As a** staff member, **I want to** view a log of registered users and room utilization, **so that** I can monitor facility usage and report unusual activity.

**Acceptance Criteria:**

- [ ] Admin UI displays a data table of all registered users and their booking history.
    
- [ ] Admin cannot mutate (edit/delete) the personal data inside a user's account.
    

**Story 3.2: Managing Room Status**

> **As a** staff member, **I want to** change the operational status of a room, **so that** users cannot book a room that requires cleaning or repairs.

**Acceptance Criteria:**

- [ ] Admin can transition a room state from "Active" to "Maintenance".
    
- [ ] Rooms in "Maintenance" automatically block any future bookings during that period.
    

## 5. Non-Functional Requirements (NFRs)

- **Tech Stack:** React (Frontend), .NET 10 Web API (Backend), Entity Framework Core, SQL Server / PostgreSQL, Docker (Containerization).
    
- **Performance:** API queries for grid availability (`GET /api/availability`) must respond in under 200ms to ensure a snappy user experience.
    
- **Data Integrity (Concurrency):** The system must use optimistic concurrency (e.g., `RowVersion`) or serializable database transactions to guarantee zero double-bookings when two users click "Book" simultaneously.
    
- **Security:** JSON Web Tokens (JWT) for API route authorization. 2FA mandatory for Staff roles. Passwords must be hashed using BCrypt/Argon2.
    

## 6. Future Considerations (V2)

- Integration with Google Calendar to sync bookings directly to user schedules.
    
- Stripe API integration to handle upfront payments and deposits.
    
- Dynamic pricing based on high-demand hours.