public static class PlayerInputNames
{
    public const string Player = "Player";

    public const string Move = "Move";
    public const string Sprint = "Sprint";
    public const string Jump = "Jump";
    public const string Crouch = "Crouch";
    public const string Prone = "Prone";

    public const string Look = "Look";
    public const string FreeLook = "FreeLook";

    public const string PrimaryAction = "PrimaryAction";
    public const string SecondaryAction = "SecondaryAction";
    
    public static readonly string[] Vector2Actions = { Move, Look };
    public static readonly string[] FloatActions = { Sprint };
    public static readonly string[] BoolActions = { Jump, Crouch, Prone, FreeLook, PrimaryAction, SecondaryAction };
}