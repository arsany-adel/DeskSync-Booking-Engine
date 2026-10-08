using System;
using ErrorOr;

namespace DeskSync.Api.Constants;

public static class DomainErrors
{
    public static class General
    {
        public static readonly Error Unauthorized = Error.Unauthorized(
            code: "General.Unauthorized",
            description: "You do not have permission to perform this action."
        );
    }

    public static class Room
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound(
                code: "Room.NotFound",
                description: $"Room with ID '{id}' was not found."
            );
    }

    public static class Reservation
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound(
                code: "Reservation.NotFound",
                description: $"Reservation with ID '{id}' was not found."
            );

        public static Error InvalidTimezone(string timezoneId) =>
            Error.Validation(
                code: "Reservation.InvalidTimezone",
                description: $"Invalid timezone ID: '{timezoneId}'."
            );

        public static readonly Error InvalidTimeRange = Error.Validation(
            code: "Reservation.InvalidTimeRange",
            description: "End time must be strictly after start time."
        );

        public static readonly Error RoomNotAvailable = Error.Conflict(
            code: "Reservation.RoomNotAvailable",
            description: "The room is already booked for the selected time slot."
        );

        public static readonly Error CannotDeleteStarted = Error.Validation(
            code: "Reservation.CannotDeleteStarted",
            description: "Cannot delete a reservation that has already started. Historical records must be preserved."
        );
    }

    public static class Notification
    {
        public static readonly Error NotFound = Error.NotFound(
            code: "Notification.NotFound",
            description: "Notification not found."
        );

        public static readonly Error Unauthorized = Error.Unauthorized(
            code: "Notification.Unauthorized",
            description: "You do not have permission to modify this notification."
        );
    }

    public static class Email
    {
        public static readonly Error SendFailed = Error.Failure(
            code: "Email.SendFailed",
            description: "Failed to send email. Please try again later."
        );
    }
}