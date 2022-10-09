using System;
using System.Collections.Generic;
using System.Linq;

namespace CYM.AI.FStateMachine
{
    /// <summary>
    /// Non-generic state interface.
    /// </summary>
    public interface IState
    {
        bool IsHaveState(StrHash name);
        bool IsInState(StrHash name);
        /// <summary>
        /// Parent state, or null if this is the root level state.
        /// </summary>
        IState Parent { get; set; }
        StrHash Hash { get; set; }
        string Name { get; }
        IState CurState { get; }

        /// <summary>
        /// Change to the state with the specified name.
        /// </summary>
        void ChangeState(StrHash stateName);
        void ChangeDefaultState();
        /// <summary>
        /// Push another state above the current one, so that popping it will return to the
        /// current state.
        /// </summary>
        void PushState(StrHash stateName);

        /// <summary>
        /// Exit out of the current state and enter whatever state is below it in the stack.
        /// </summary>
        void PopState();

        /// <summary>
        /// Update this state and its children with a specified delta time.
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Triggered when we enter the state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Triggered when we exit the state.
        /// </summary>
        void Exit();

        /// <summary>
        /// Trigger an event on this state or one of its children.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        void TriggerEvent(string name);

        /// <summary>
        /// Triggered when and event occurs. Executes the event's action if the 
        /// current state is at the top of the stack, otherwise triggers it on 
        /// the next state down.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        /// <param name="eventArgs">Arguments to send to the event</param>
        void TriggerEvent(string name, EventArgs eventArgs);
    }

    /// <summary>
    /// State with a specified handler type.
    /// </summary>
    public abstract class AbstractState : IState
    {
        //进入这个状态的时间
        public float Time { get;private set; }
        public int TimeInt => (int)Time;
        int LastTimeStep = 0;
        public bool CheckTimeStep(float step)
        {
            if (LastTimeStep == TimeInt)
                return false;
            if (TimeInt % step == 0)
            {
                LastTimeStep = TimeInt;
                return true;
            }
            return false;
        }

        private float timeOut = float.MaxValue;
        //状态超时
        private Action onTimeOut;
        /// <summary>
        /// Action called when we enter the state.
        /// </summary>
        private Action onEnter;

        /// <summary>
        /// Action called when the state gets updated.
        /// </summary>
        private Action<float> onUpdate;

        /// <summary>
        /// Action called when we exit the state.
        /// </summary>
        private Action onExit;

        private readonly IList<Condition> conditions = new List<Condition>();

        /// <summary>
        /// Parent state, or null if this is the root level state.
        /// </summary>
        public IState Parent { get; set; }
        public StrHash Hash { get; set; }
        public string Name => Hash.String;

        /// <summary>
        /// Stack of active child states.
        /// </summary>
        private readonly Stack<IState> activeChildren = new Stack<IState>();
        public IState CurState => activeChildren.Peek();

        /// <summary>
        /// Dictionary of all children (active and inactive), and their names.
        /// </summary>
        private readonly IDictionary<StrHash, IState> children = new Dictionary<StrHash, IState>();

        /// <summary>
        /// Dictionary of all actions associated with this state.
        /// </summary>
        private readonly IDictionary<string, Action<EventArgs>> events = new Dictionary<string, Action<EventArgs>>();

        public void ChangeDefaultState()
        {
            ChangeState(children.Keys.First());
        }
        /// <summary>
        /// Pops the current state from the stack and pushes the specified one on.
        /// </summary>
        public void ChangeState(StrHash stateName)
        {
            // Try to find the specified state.
            IState newState;
            if (!children.TryGetValue(stateName, out newState))
            {
                throw new ApplicationException("Tried to change to state \"" + stateName + "\", but it is not in the list of children.");
            }

            // Exit and pop the current state
            if (activeChildren.Count > 0)
            {
                activeChildren.Pop().Exit();
            }

            // Activate the new state
            activeChildren.Push(newState);
            newState.Enter();
        }

        /// <summary>
        /// Push another state from the existing dictionary of children to the top of the state stack.
        /// </summary>
        public void PushState(StrHash stateName)
        {
            // Find the new state and add it
            IState newState;
            if (!children.TryGetValue(stateName, out newState))
            {
                throw new ApplicationException("Tried to change to state \"" + stateName + "\", but it is not in the list of children.");
            }
            activeChildren.Push(newState);
            newState.Enter();
        }

        /// <summary>
        /// Remove the current state from the active state stack and activate the state immediately beneath it.
        /// </summary>
        public void PopState()
        {
            // Exit and pop the current state
            if (activeChildren.Count > 0)
            {
                activeChildren.Pop().Exit();
            }
            else
            {
                throw new ApplicationException("PopState called on state with no active children to pop.");
            }
        }

        /// <summary>
        /// Update this state and its children with a specified delta time.
        /// </summary>
        public void Update(float deltaTime)
        {
            // Only update the child at the end of the tree
            if (activeChildren.Count > 0)
            {
                activeChildren.Peek().Update(deltaTime);
                return;
            }

            Time += deltaTime;
            if (onUpdate != null)
            {
                onUpdate(deltaTime);
            }
            if (Time >= timeOut)
            {
                if (onTimeOut != null)
                {
                    onTimeOut();
                }
            }
            // Update conditions
            for (var i = 0; i < conditions.Count; i++)
            {
                if (conditions[i].Predicate())
                {
                    conditions[i].Action();
                }
            }
        }

        /// <summary>
        /// Create a new state as a child of the current state.
        /// </summary>
        public void AddChild(IState newState, StrHash stateName)
        {
            try
            {
                children.Add(stateName, newState);
                newState.Parent = this;
                newState.Hash = stateName;
            }
            catch (ArgumentException)
            {
                throw new ApplicationException("State with name \"" + stateName + "\" already exists in list of children.");
            }
        }

        /// <summary>
        /// Create a new state as a child of the current state and automatically derive 
        /// its name from its handler type.
        /// </summary>
        public void AddChild(IState newState)
        {
            var name = newState.GetType().Name;
            AddChild(newState,new StrHash(name));
        }

        /// <summary>
        /// Data structure for associating a condition with an action.
        /// </summary>
        private struct Condition
        {
            public Func<bool> Predicate;
            public Action Action;
        }

        /// <summary>
        /// Set an action to be called when the state is updated an a specified 
        /// predicate is true.
        /// </summary>
        public void SetCondition(Func<bool> predicate, Action action)
        {
            conditions.Add(new Condition() {
                Predicate = predicate,
                Action = action
            });
        }

        /// <summary>
        /// Action triggered on entering the state.
        /// </summary>
        public void SetEnterAction(Action onEnter)
        {
            this.onEnter = onEnter;
        }

        /// <summary>
        /// Action triggered on exiting the state.
        /// </summary>
        public void SetExitAction(Action onExit)
        {
            this.onExit = onExit;
        }

        /// <summary>
        /// Action which passes the current state object and the delta time since the 
        /// last update to a function.
        /// </summary>
        public void SetUpdateAction(Action<float> onUpdate)
        {
            this.onUpdate = onUpdate;
        }

        /// <summary>
        /// Sets an action to be associated with an identifier that can later be used
        /// to trigger it.
        /// Convenience method that uses default event args intended for events that 
        /// don't need any arguments.
        /// </summary>
        public void SetEvent(string identifier, Action<EventArgs> eventTriggeredAction)
        {
            SetEvent<EventArgs>(identifier, eventTriggeredAction);
        }

        /// <summary>
        /// Sets an action to be associated with an identifier that can later be used
        /// to trigger it.
        /// </summary>
        public void SetEvent<TEvent>(string identifier, Action<TEvent> eventTriggeredAction)
            where TEvent : EventArgs
        {
            events.Add(identifier, args => eventTriggeredAction(CheckEventArgs<TEvent>(identifier, args)));
        }

        public void SetTimeOutAction(float timeOut,Action action)
        {
            this.timeOut = timeOut;
            onTimeOut = action;
        }

        /// <summary>
        /// Cast the specified EventArgs to a specified type, throwing a descriptive exception if this fails.
        /// </summary>
        private static TEvent CheckEventArgs<TEvent>(string identifier, EventArgs args) 
            where TEvent : EventArgs
        {
            try
            {
                return (TEvent)args;
            }
            catch (InvalidCastException ex)
            {
                throw new ApplicationException("Could not invoke event \"" + identifier + "\" with argument of type " +
                    args.GetType().Name + ". Expected " + typeof(TEvent).Name, ex);
            }
        }

        /// <summary>
        /// Triggered when we enter the state.
        /// </summary>
        public void Enter()
        {
            Time = 0;
            if (onEnter != null)
            {
                onEnter();
            }
        }

        /// <summary>
        /// Triggered when we exit the state.
        /// </summary>
        public void Exit()
        {
            Time = 0;
            if (onExit != null)
            {
                onExit();
            }

            while (activeChildren.Count > 0)
            {
                activeChildren.Pop().Exit();
            }
        }

        /// <summary>
        /// Triggered when and event occurs. Executes the event's action if the 
        /// current state is at the top of the stack, otherwise triggers it on 
        /// the next state down.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        public void TriggerEvent(string name)
        {
            TriggerEvent(name, EventArgs.Empty);
        }

        /// <summary>
        /// Triggered when and event occurs. Executes the event's action if the 
        /// current state is at the top of the stack, otherwise triggers it on 
        /// the next state down.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        /// <param name="eventArgs">Arguments to send to the event</param>
        public void TriggerEvent(string name, EventArgs eventArgs)
        {
            // Only update the child at the end of the tree
            if (activeChildren.Count > 0)
            {
                activeChildren.Peek().TriggerEvent(name, eventArgs);
                return;
            }

            Action<EventArgs> myEvent;
            if (events.TryGetValue(name, out myEvent))
            {
                myEvent(eventArgs);
            }
        }

        public bool IsInState(StrHash name)
        {
            if (activeChildren.Count == 0)
                return false;
            IState inState;
            if (!children.TryGetValue(name, out inState))
            {
                return false;
            }
            return inState == activeChildren.Peek();
        }
        public bool IsHaveState(StrHash name)
        {
            if (!children.TryGetValue(name, out IState inState))
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// State with no extra functionality used for root of state hierarchy.
    /// </summary>
    public class State : AbstractState
    {
    }
}
