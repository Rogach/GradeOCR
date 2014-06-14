using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grader.gui {
    public class EventManager {
        private HashSet<EventListener> listeners = new HashSet<EventListener>();
        public EventManager() {
        }

        public void AddEventListener(EventListener listener) {
            listeners.Add(listener);
        }

        public void AddEventListener(Action action) {
            listeners.Add(new ActionEventListener(action));
        }

        public void RemoveEventListener(EventListener listener) {
            listeners.Remove(listener);
        }

        public void Invoke() {
            foreach (var listener in listeners) {
                listener.EventHappened();
            }
        }
    }

    public interface EventListener {
        void EventHappened();
    }

    public class ActionEventListener : EventListener {
        private Action action;
        public ActionEventListener(Action action) {
            this.action = action;
        }
        public void EventHappened() {
            action.Invoke();
        }
    }
}
