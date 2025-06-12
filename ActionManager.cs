using FollowMe.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowMe
{
    public class ActionManager
    {
        private readonly List<IFollowerAction> actions = [];

        public void Register(IFollowerAction action) => actions.Add(action);

        public void Tick()
        {
            foreach (var action in actions.OrderByDescending(a => a.Priority))
            {
                if (action.CanExecute())
                {
                    action.Execute();
                    break; // Exécute une seule action par tick
                }
            }
        }
    }

}
