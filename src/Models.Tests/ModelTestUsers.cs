namespace Models.Tests;

internal static class ModelTestUsers
{
    public static AppUser Create() =>
        AppUser.Create(
            UserEmail.From("user@example.com"),
            DisplayName.From("User"),
            PasswordHash.From("hash"),
            LanguageCode.From("en"));
}
