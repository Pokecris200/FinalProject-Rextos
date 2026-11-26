/// <summary>
/// Body parts the player can collect. The declaration order defines progression:
/// a higher value is a more advanced part (Arms > Legs > Torso).
/// Do not reorder or add values unless the design changes.
/// </summary>
public enum BodyPartType
{
    Torso,
    Legs,
    Arms
}