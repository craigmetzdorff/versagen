using System;

[Flags]
public enum EVersaPerms
{
    Standard = 0,
    Moderator = 1,
    StoryTeller = Moderator << 1,
    PermanentGM = StoryTeller << 1,
    GameDeveloper = PermanentGM << 1,
    SystemManager = GameDeveloper << 1,
}