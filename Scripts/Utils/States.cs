//State for an event
public enum States {
    // Event
    RENDER,
    DRAW,
    ACTIVE,
    HIDE,
    EXPIRED,

    // Title Screen
    COMPANY_INTRO,
    INTRO,
    TITLE,
    TITLE_OPTIONS,
    TUTORIAL,
    SETTINGS,

    // Game Screen
    IN_GAME,
    IN_DIALOGUE,
    PAUSED,

    // Player
    IDLE,
    WALK,
    RUN,
    IN_AIR,
    LAND,
    ROLL,
    SLIDE,
    NULL_STATE,

    // General Object Status
    ON_FIRE,
    FROZEN,
    SHAKE,
    BOUNCE,

}
