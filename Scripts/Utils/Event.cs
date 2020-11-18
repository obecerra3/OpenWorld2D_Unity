using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Event : ObserverPattern.ObserverSubject
{
    public string next_event;
    public States current_state;

    //does the computational crunch needed
    public virtual void load() {}

    //graphics
    public virtual void render() {}

    //graphics
    public virtual void hide() {}

    //enables graphics/ enables physics/ enables something to happen
    public virtual void enable() {}

    //erases graphics, disables physics/interactivity, frees memory
    public virtual void disable() {}

    //gameloop
    public virtual void update() {}

}
