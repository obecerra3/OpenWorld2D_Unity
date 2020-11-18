using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObserverPattern {

    // ========================================
    // OBSERVER
    // ========================================
    public interface Observer {
        void onNotify(Notifications notification, List<object> data);
    }

    // ========================================
    // SUBJECT
    // ========================================
    [System.Serializable]
    public abstract class Subject {
        private List<Observer> observers;

        public Subject() {
            observers = new List<Observer>();
        }

        public void notify(Notifications notification, List<object> data) {
            foreach (Observer observer in observers) {
                observer.onNotify(notification, data);
            }
        }

        public void addObserver(Observer observer) {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void removeObserver(Observer observer) {
            if (!observers.Contains(observer))
                observers.Remove(observer);
        }
    }

    // ========================================
    // OBSERVER_SUBJECT
    // ========================================
    [System.Serializable]
    public abstract class ObserverSubject : Subject, Observer {
        public void onNotify(Notifications notification, List<object> data) {}
    }

    // ========================================
    // MONO_OBSERVER
    // ========================================
    [System.Serializable]
    public abstract class MonoObserver : MonoBehaviour, Observer {
        public virtual void onNotify(Notifications notification, List<object> data) {}
    }

    // ========================================
    // MONO_OBSERVER_SUBJECT
    // ========================================
    [System.Serializable]
    public abstract class MonoObserverSubject : MonoObserver {
        private List<Observer> observers;

        public MonoObserverSubject() {
            observers = new List<Observer>();
        }

        public void notify(Notifications notification, List<object> data) {
            foreach (Observer observer in observers) {
                observer.onNotify(notification, data);
            }
        }

        public void addObserver(Observer observer) {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void removeObserver(Observer observer) {
            if (!observers.Contains(observer))
                observers.Remove(observer);
        }
    }

}
