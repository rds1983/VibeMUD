namespace VibeMUD.Networking;

public enum LoginState
{
    AskingName,
    AskingPassword,
    AskingCreateConfirmation,
    AskingNewPassword,
    ConfirmingPassword,
    AskingClass,
    Authenticated
}
