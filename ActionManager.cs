using FollowMe.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowMe
{
    public interface IGameAction
    {
        bool CanExecute();          // Vérifie les conditions
        void Execute();             // Lance l'action
        TimeSpan Cooldown { get; }  // Cooldown individuel
    }

    public class ActionManager
    {
        private readonly Queue<(IGameAction Action, DateTime ReadyTime)> actionQueue = new();
        private bool isRunning = false;

        public void Register(IGameAction action)
        {
            if (action.CanExecute())
                actionQueue.Enqueue((action, DateTime.Now + action.Cooldown));
        }

        public void Tick()
        {
            if (isRunning || actionQueue.Count == 0)
                return;

            var (action, readyTime) = actionQueue.Peek();
            if (DateTime.Now >= readyTime)
            {
                isRunning = true;
                try
                {
                    action.Execute();
                }
                finally
                {
                    actionQueue.Dequeue();
                    isRunning = false;
                }
            }
        }
    }


}
