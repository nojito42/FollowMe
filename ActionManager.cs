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
        int Priority { get; }                  // Plus c’est bas, plus c’est prioritaire (0 = top)
        bool CanExecute();                     // Condition d’activation
        void Execute();                        // Ce que l’action fait
        TimeSpan Cooldown { get; }             // Délai d'attente entre deux exécutions
        string MutexKey { get; }               // Un identifiant de verrouillage pour éviter les conflits
    }


    public class ActionManager
    {
        private readonly List<IGameAction> actions = new();
        private readonly Dictionary<string, DateTime> lastExecutionByMutex = new();

        public void Register(IGameAction action)
        {
            if (!actions.Contains(action))
                actions.Add(action);
        }

        public void Tick()
        {
            // Tri par priorité (plus petit d’abord)
            var sorted = actions.OrderBy(a => a.Priority);

            foreach (var action in sorted)
            {
                var mutex = action.MutexKey ?? "global";

                if (!action.CanExecute()) continue;

                if (!lastExecutionByMutex.TryGetValue(mutex, out var lastExec) ||
                    DateTime.Now - lastExec >= action.Cooldown)
                {
                    action.Execute();
                    lastExecutionByMutex[mutex] = DateTime.Now;
                    break; // Exécute une seule action à la fois par Tick()
                }
            }
        }
    }


}
