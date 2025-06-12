using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowMe.Actions;

public interface IFollowerAction
{
    string Name { get; }
    int Priority { get; } // pour la PriorityQueue plus tard
    TimeSpan MinInterval { get; }
    TimeSpan Timeout { get; }
    int MaxAttempts { get; }

    bool CanExecute();
    void Execute();
}
