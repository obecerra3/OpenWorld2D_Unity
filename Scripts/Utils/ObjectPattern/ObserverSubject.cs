using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public abstract class ObserverSubject : Observer
{
    private List<Observer> observers;

    public ObserverSubject()
    {
        observers = new List<Observer>();
    }

    public void notify(Notifications _notification, List<object> _data)
    {
        foreach (Observer observer in observers)
        {
            observer.onNotify(_notification, _data);
        }
    }

    public void addObserver(Observer _observer)
    {
        if (!observers.Contains(_observer))
        {
            observers.Add(_observer);
        }
    }

    public void removeObserver(Observer _observer)
    {
        if (!observers.Contains(_observer))
        {
            observers.Remove(_observer);
        }
    }

}
