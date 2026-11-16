namespace DeskSync.Api.DTOs.Users;

public record EnableTwoFactorResponse(
    string SharedKey,      // The TOTP secret (for manual entry)
    string AuthenticatorUri // The otpauth:// URL (for QR code generation on frontend)
);