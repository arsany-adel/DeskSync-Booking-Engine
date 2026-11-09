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

    public static class Auth
    {
        public static readonly Error InvalidCredentials = Error.Unauthorized(
            code: "Auth.InvalidCredentials",
            description: "Invalid email or password."
        );

        public static readonly Error EmailAlreadyInUse = Error.Conflict(
            code: "Auth.EmailAlreadyInUse",
            description: "Email is already in use."
        );

        public static readonly Error UsernameAlreadyInUse = Error.Conflict(
            code: "Auth.UsernameAlreadyInUse",
            description: "Username is already in use."
        );

        public static readonly Error RefreshNotImplemented = Error.Failure(
            code: "Auth.RefreshNotImplemented",
            description: "Refresh token flow is not yet implemented."
        );

        public static readonly Error InvalidToken = Error.Unauthorized(
            code: "Auth.InvalidToken",
            description: "The provided token is invalid."
        );

        public static readonly Error TokenExpired = Error.Unauthorized(
            code: "Auth.TokenExpired",
            description: "The provided token has expired."
        );

        public static readonly Error EmailNotConfirmed = Error.Unauthorized(
            code: "Auth.EmailNotConfirmed",
            description: "Please confirm your email address before logging in."
        );

        public static readonly Error AccountHasNoPassword = Error.Validation(
            code: "Auth.AccountHasNoPassword",
            description: "This account was created via social login. Use 'Forgot Password' to set a password."
        );

                public static readonly Error TwoFactorNotEnabled = Error.Validation(
            code: "Auth.TwoFactorNotEnabled",
            description: "Two-factor authentication is not enabled for this account."
        );

        public static readonly Error InvalidTwoFactorCode = Error.Unauthorized(
            code: "Auth.InvalidTwoFactorCode",
            description: "The authenticator code is invalid or expired."
        );

        public static readonly Error TwoFactorAlreadyEnabled = Error.Conflict(
            code: "Auth.TwoFactorAlreadyEnabled",
            description: "Two-factor authentication is already enabled."
        );

        public static readonly Error TwoFactorNotActivated = Error.Validation(
            code: "Auth.TwoFactorNotActivated",
            description: "Two-factor authentication setup was not completed. Please try enabling again."
        );

        public static readonly Error AdminRequiresTwoFactor = Error.Unauthorized(
            code: "Auth.AdminRequiresTwoFactor",
            description: "Administrator accounts must have two-factor authentication enabled. Please set up 2FA before logging in."
        );

        public static readonly Error OAuthProviderNotSupported = Error.Validation(
            code: "Auth.OAuthProviderNotSupported",
            description: "The requested social login provider is not supported."
        );

        public static readonly Error OAuthEmailNotVerified = Error.Unauthorized(
            code: "Auth.OAuthEmailNotVerified",
            description: "Your social provider did not return a verified email address. Please verify your email with the provider first."
        );

        public static readonly Error OAuthFailed = Error.Failure(
            code: "Auth.OAuthFailed",
            description: "Social login failed. Please try again."
        );
    }
}