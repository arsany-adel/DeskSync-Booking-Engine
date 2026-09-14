using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace DeskSync.Api.DTOs.Reservations;

public record UpdateReservationDto(
    [Required]
    LocalDateTime LocalStartTime,

    [Required]
    LocalDateTime LocalEndTime,

    [Required]
    [StringLength(100, ErrorMessage = "TimezoneId cannot exceed 100 characters.")]
    string TimezoneId,

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
    string? Notes
);