// The exact room is captured when an exit is offered, and carried into the load.
// No second roll occurs when interacting with a door.
public sealed class StagePortalChoice
{
    public readonly int ProgressIndex;
    public readonly StageRoomDefinition Room;

    public StagePortalChoice(int progressIndex, StageRoomDefinition room)
    {
        ProgressIndex = progressIndex;
        Room = room;
    }
}
