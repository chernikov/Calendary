# Calendary Database Schema

This document outlines the database schema for the Calendary application. The schema is defined using Entity Framework Core, and the diagram below is generated from the `CalendaryDbContext`.

## Entity-Relationship Diagram (ERD)

The following ERD provides a visual representation of the database tables and their relationships.

```plantuml
!include db_schema.puml
```

## Entities

The database consists of the following entities:

*   **User**: Represents the users of the application.
*   **Role**: Defines the roles that can be assigned to users (e.g., Admin, User).
*   **UserRole**: A many-to-many relationship between Users and Roles.
*   **Order**: Represents a user's order for a calendar.
*   **OrderItem**: Represents an item within an order.
*   **PaymentInfo**: Contains information about the payment for an order.
*   **Calendar**: Represents a calendar created by a user.
*   **Image**: Stores images associated with a calendar.
*   **Holiday**: Represents a holiday.
*   **CalendarHoliday**: A many-to-many relationship between Calendars and Holidays.
*   **Country**: Represents a country, used for holidays and user settings.
*   **Language**: Represents a language, used for calendars and user settings.
*   **UserSetting**: Stores user-specific settings, such as preferred language and country.
*   **EventDate**: Represents a custom event date added by a user.
*   **Category**: Represents categories for various entities.
*   **FluxModel**: Represents a flux model.
*   **Job**: Represents a background job.
*   **JobTask**: Represents a task within a background job.
*   **MonoWebhookEvent**: Represents an event from the Mono webhook.
*   **Photo**: Represents a user's photo.
*   **Prompt**: Represents a prompt for generating content.
*   **PromptSeed**: Represents a seed for a prompt.
*   **PromptTheme**: Represents a theme for a prompt.
*   **ResetToken**: Represents a token for resetting a user's password.
*   **Synthesis**: Represents a synthesis job.
*   **Training**: Represents a training job.
*   **VerificationEmailCode**: Represents a code for verifying a user's email.
*   **VerificationPhoneCode**: Represents a code for verifying a user's phone number.
*   **WebHook**: Represents a webhook.
*   **WebHookFluxModel**: Represents a webhook flux model.
