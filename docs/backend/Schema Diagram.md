DeskSync Database Schema (V1)

This document outlines the core Entity Relationship Diagram (ERD) for the DeskSync booking engine. The schema is designed to be fully relational and supports multiple social logins per user, precise hourly booking, and asynchronous background notifications.

Entity Relationship Diagram

DeskSync Schema Diagram

Schema Breakdown

1. Identity & Access (User, UserLogin)

The identity system separates the core user profile from their authentication methods. The User table holds personal data and a nullable password (for standard email logins). The UserLogin table holds external OAuth subject IDs (e.g., Google's sub claim), allowing a single user to safely link multiple social accounts to one profile.

1. Physical Resources (Workspace, Room)

A Workspace represents the physical building or geographic location, while Rooms are the specific bookable assets inside. The Room table uses strict numerical and boolean types (e.g., no_of_chairs, has_projector) to allow the frontend to easily filter resources via SQL queries. It also includes an operational status to prevent bookings during maintenance.

1. The Booking Engine (Reservation)

The Reservation table connects a User to a specific Room. To support the PRD requirement of tracking hourly limits (max 6 hours), the start_date and end_date are strictly typed as timestamp to ensure precise tracking of hours and minutes.

1. Asynchronous State (Notifications, Bookmarks)

Bookmarks acts as a pure junction table resolving the many-to-many relationship between Users and their favorite Rooms. Notifications serves a dual purpose: it acts as a persistent inbox for the React frontend to display historical alerts, and simultaneously functions as an outbox for the .NET Background Service to track pending email deliveries via the status enum.